using System;
using System.Diagnostics;
using System.IO;
using Ez3Di.Installer.Actions.Discovery;
using Ez3Di.Installer.Actions.Install;
using Ez3Di.Installer.Actions.Uninstall;
using Ez3Di.Installer.Actions.Verify;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;
using FlaUI.Core;

namespace Ez3Di.Installer.Actions.Upgrade
{
    /// <summary>
    /// Guard action ensuring that an older baseline version of Ez3D-i (e.g. v5.5.7.3 from 'Installation Package/Old Version/')
    /// is pre-installed on the host system before executing upgrade test fixtures.
    /// - If an older version is already installed: Bypasses with 0 overhead.
    /// - If the target release (e.g. v5.5.16.0) is installed: Uninstalls it first, then installs the old version.
    /// - If the machine is clean: Installs the old version.
    /// </summary>
    public static class EnsureEz3DiOldVersionInstalledAction
    {
        public static InstallerPackageInfo Execute(
            AutomationBase automation,
            InstallerWizardScreen screen,
            Action<string>? onStepCallback = null)
        {
            var oldVersionInfo = FindInstallerPackageAction.FindOldVersion();
            string expectedOldVer = oldVersionInfo.FileVersion;

            bool isRegInstalled = RegistryHelper.IsInstalled(out string displayName, out string displayVersion, out _);
            bool isFileInstalled = File.Exists(InstallerConstants.DefaultExecutablePath) ||
                                  File.Exists(InstallerConstants.FallbackExecutablePath);

            // Check if the currently installed application version matches the expected baseline version
            if (isRegInstalled && isFileInstalled)
            {
                if (displayVersion.Trim().StartsWith(expectedOldVer, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[EnsureEz3DiOldVersionInstalledAction] Baseline older version ({displayName} v{displayVersion}) is already installed. Ready for upgrade.");
                    return oldVersionInfo;
                }

                Console.WriteLine($"[EnsureEz3DiOldVersionInstalledAction] Current installation (v{displayVersion}) does not match expected baseline v{expectedOldVer}. Executing prerequisite uninstallation...");
                var uninstallerScreen = new UninstallerWizardScreen(automation);
                EnsureEz3DiUninstalledAction.Execute(uninstallerScreen, step => onStepCallback?.Invoke($"PrereqClean_{step}"));
            }

            Console.WriteLine($"[EnsureEz3DiOldVersionInstalledAction] Installing baseline older version ({oldVersionInfo.FileName} v{expectedOldVer})...");

            Process? setupProc = null;
            try
            {
                // 1. Launch baseline installer
                var (proc, win, info) = LaunchInstallerAction.Execute(automation, screen, oldVersionInfo.FilePath);
                setupProc = proc;
                onStepCallback?.Invoke("PrereqOldInstall_Launched");

                // 2. Language selection if prompted
                HandleLanguageSelectionAction.Execute(automation, screen);

                // 3. Step through 5-step clean wizard
                StepThroughSetupWizardAction.Execute(screen, step => onStepCallback?.Invoke($"PrereqOldInstall_{step}"));

                // 4. Wait for file copy progress bar
                WaitForInstallProgressAction.Execute(screen, timeoutMinutes: 15, step => onStepCallback?.Invoke($"PrereqOldInstall_{step}"));

                // 5. Complete installation
                CompleteInstallationAction.Execute(screen, setupProc, step => onStepCallback?.Invoke($"PrereqOldInstall_{step}"));
                setupProc = null;

                // 6. Verify state matches baseline version
                VerifyInstallationStateAction.Execute(info.FileVersion);
                onStepCallback?.Invoke("PrereqOldInstall_Completed");

                Console.WriteLine($"[EnsureEz3DiOldVersionInstalledAction] Baseline installation of v{expectedOldVer} completed successfully. Ready for upgrade testing.");
                return oldVersionInfo;
            }
            finally
            {
                if (setupProc != null && !setupProc.HasExited)
                {
                    try { setupProc.Kill(entireProcessTree: true); } catch { }
                    setupProc.Dispose();
                }
            }
        }
    }
}
