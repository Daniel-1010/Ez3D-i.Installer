using System;
using System.Diagnostics;
using Ez3Di.Installer.Actions.Install;
using Ez3Di.Installer.Actions.Uninstall;
using Ez3Di.Installer.Actions.Verify;
using NUnit.Framework;

namespace Ez3Di.Installer.Tests
{
    [TestFixture]
    [Category("Lifecycle")]
    [Order(4)]
    public class TC04_Full_Lifecycle : BaseInstallerFixture
    {
        [Test]
        public void Run_TC04_Full_Lifecycle()
        {
            Console.WriteLine("[TC04] Starting full installation lifecycle test...");

            Process? proc = null;
            try
            {
                // Phase 1: Installation
                Console.WriteLine("[TC04] Phase 1: Installation...");
                var (setupProc, win, info) = LaunchInstallerAction.Execute(Automation!, InstallerScreen!);
                proc = setupProc;
                CaptureScreenshot("TC04_Phase1_Launched");

                HandleLanguageSelectionAction.Execute(Automation!, InstallerScreen!);
                StepThroughSetupWizardAction.Execute(InstallerScreen!, step => CaptureScreenshot($"TC04_Phase1_{step}"));
                WaitForInstallProgressAction.Execute(InstallerScreen!, timeoutMinutes: 10);
                CompleteInstallationAction.Execute(InstallerScreen!, proc);
                proc = null;
                CaptureScreenshot("TC04_Phase1_Installed");

                // Phase 2: Verification
                Console.WriteLine("[TC04] Phase 2: Verification...");
                VerifyInstallationStateAction.Execute(info.FileVersion);
                CaptureScreenshot("TC04_Phase2_Verified");

                // Phase 3: Uninstallation
                Console.WriteLine("[TC04] Phase 3: Uninstallation...");
                var uninstProc = LaunchUninstallerAction.Execute(UninstallerScreen!);
                proc = uninstProc;
                CaptureScreenshot("TC04_Phase3_UninstallerLaunched");

                ConfirmUninstallationAction.Execute(UninstallerScreen!, proc, timeoutMinutes: 5);
                proc = null;
                CaptureScreenshot("TC04_Phase3_Uninstalled");

                // Phase 4: Final Cleanup Verification
                Console.WriteLine("[TC04] Phase 4: Post-Uninstall Cleanup Verification...");
                VerifyUninstallationStateAction.Execute();
                CaptureScreenshot("TC04_Phase4_CleanupVerified");

                Console.WriteLine("[TC04] PASS: Complete installation and uninstallation lifecycle verified successfully.");
            }
            finally
            {
                if (proc != null && !proc.HasExited)
                {
                    try { proc.Kill(); } catch { }
                }
            }
        }
    }
}
