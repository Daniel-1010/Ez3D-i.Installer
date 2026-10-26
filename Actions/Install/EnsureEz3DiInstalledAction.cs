using System;
using System.Diagnostics;
using System.IO;
using Ez3Di.Installer.Actions.Verify;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;
using FlaUI.Core;

namespace Ez3Di.Installer.Actions.Install
{
    /// <summary>
    /// Guard action that checks whether Ez3D-i is currently installed on the host machine.
    /// If installed, it returns immediately (0 overhead).
    /// If NOT installed (e.g. following a previous uninstallation test in a continuous suite),
    /// it automatically performs a prerequisite clean installation so uninstallation tests can proceed.
    /// </summary>
    public static class EnsureEz3DiInstalledAction
    {
        public static void Execute(AutomationBase automation, InstallerWizardScreen screen, Action<string>? onStepCallback = null)
        {
            bool isRegInstalled = RegistryHelper.IsInstalled(out string displayName, out string displayVersion, out _);
            bool isFileInstalled = File.Exists(InstallerConstants.DefaultExecutablePath) ||
                                  File.Exists(InstallerConstants.FallbackExecutablePath);

            if (isRegInstalled && isFileInstalled)
            {
                Console.WriteLine($"[EnsureEz3DiInstalledAction] Ez3D-i ({displayName} v{displayVersion}) is already installed. Ready for uninstallation.");
                return;
            }

            Console.WriteLine("[EnsureEz3DiInstalledAction] Ez3D-i is NOT installed. Automatically running prerequisite installation...");

            Process? setupProc = null;
            try
            {
                // 1. Launch installer
                var (proc, win, info) = LaunchInstallerAction.Execute(automation, screen);
                setupProc = proc;
                onStepCallback?.Invoke("PrereqInstall_Launched");

                // 2. Language prompt if shown
                HandleLanguageSelectionAction.Execute(automation, screen);

                // 3. Step through 5-step wizard
                StepThroughSetupWizardAction.Execute(screen, step => onStepCallback?.Invoke($"PrereqInstall_{step}"));

                // 4. Wait for file copy progress bar
                WaitForInstallProgressAction.Execute(screen, timeoutMinutes: 15, step => onStepCallback?.Invoke($"PrereqInstall_{step}"));

                // 5. Complete installation
                CompleteInstallationAction.Execute(screen, setupProc, step => onStepCallback?.Invoke($"PrereqInstall_{step}"));
                setupProc = null;

                // 6. Verify state
                VerifyInstallationStateAction.Execute(info.FileVersion);
                onStepCallback?.Invoke("PrereqInstall_Completed");

                Console.WriteLine("[EnsureEz3DiInstalledAction] Prerequisite installation finished successfully. System is now ready for uninstallation.");
            }
            finally
            {
                if (setupProc != null && !setupProc.HasExited)
                {
                    try { setupProc.Kill(); } catch { }
                }
            }
        }
    }
}
