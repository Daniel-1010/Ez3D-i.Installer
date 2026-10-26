using System;
using System.Diagnostics;
using System.IO;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Uninstall
{
    /// <summary>
    /// Guard action that checks whether Ez3D-i is currently installed before running a clean installation test.
    /// If clean, it returns immediately (0 overhead).
    /// If installed (e.g. from a prior test run that failed before teardown), it automatically performs
    /// a clean uninstallation first so the setup launcher will NOT boot into Maintenance Mode.
    /// </summary>
    public static class EnsureEz3DiUninstalledAction
    {
        public static void Execute(UninstallerWizardScreen screen, Action<string>? onStepCallback = null)
        {
            bool isRegInstalled = RegistryHelper.IsInstalled(out string displayName, out string displayVersion, out _);
            bool isFileInstalled = File.Exists(InstallerConstants.DefaultExecutablePath) ||
                                  File.Exists(InstallerConstants.FallbackExecutablePath);

            if (!isRegInstalled && !isFileInstalled)
            {
                Console.WriteLine("[EnsureEz3DiUninstalledAction] Host machine is clean. Ready for clean installation.");
                return;
            }

            Console.WriteLine($"[EnsureEz3DiUninstalledAction] Lingering instance detected ({displayName} v{displayVersion}). Automatically uninstalling to ensure clean state...");

            Process? uninstProc = null;
            try
            {
                // 1. Launch uninstaller via Registry UninstallString
                uninstProc = LaunchUninstallerAction.Execute(screen);
                onStepCallback?.Invoke("PrereqCleanup_UninstallerLaunched");

                // 2. Confirm removal, wait for progress, and click finish
                ConfirmUninstallationAction.Execute(screen, uninstProc, timeoutMinutes: 8, step => onStepCallback?.Invoke($"PrereqCleanup_{step}"));

                // 3. Verify clean system state
                VerifyUninstallationStateAction.Execute();
                onStepCallback?.Invoke("PrereqCleanup_Completed");

                Console.WriteLine("[EnsureEz3DiUninstalledAction] Machine successfully cleaned. System is now ready for clean installation.");
            }
            finally
            {
                if (uninstProc != null && !uninstProc.HasExited)
                {
                    try { uninstProc.Kill(); } catch { }
                }
            }
        }
    }
}
