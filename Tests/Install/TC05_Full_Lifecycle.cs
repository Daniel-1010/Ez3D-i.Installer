using System;
using System.Diagnostics;
using Ez3Di.Installer.Actions.Install;
using Ez3Di.Installer.Actions.Uninstall;
using Ez3Di.Installer.Actions.Verify;
using Ez3Di.Installer.Tests;
using NUnit.Framework;

namespace Ez3Di.Installer.Tests.Install
{
    [TestFixture]
    [Category("Installation")]
    [Category("Lifecycle")]
    [Order(5)]
    public class TC05_Full_Lifecycle : BaseInstallerFixture
    {
        [Test]
        public void Run_TC05_Full_Lifecycle()
        {
            Console.WriteLine("[TC05] Starting full installation lifecycle test...");

            // Step 0: Ensure machine is clean so Phase 1 starts as a true clean install
            Console.WriteLine("[TC05] Step 0: Ensuring clean initial state...");
            EnsureEz3DiUninstalledAction.Execute(UninstallerScreen!);

            Process? proc = null;
            try
            {
                // Phase 1: Installation
                Console.WriteLine("[TC05] Phase 1: Installation...");
                var (setupProc, win, info) = LaunchInstallerAction.Execute(Automation!, InstallerScreen!);
                proc = setupProc;
                CaptureScreenshot("TC05_Phase1_Launched");

                HandleLanguageSelectionAction.Execute(Automation!, InstallerScreen!);
                StepThroughSetupWizardAction.Execute(InstallerScreen!, step => CaptureScreenshot($"TC05_Phase1_{step}"));
                WaitForInstallProgressAction.Execute(InstallerScreen!, timeoutMinutes: 15, step => CaptureScreenshot($"TC05_Phase1_{step}"));
                CompleteInstallationAction.Execute(InstallerScreen!, proc, step => CaptureScreenshot($"TC05_Phase1_{step}"));
                proc = null;
                CaptureScreenshot("TC05_Phase1_Installed");

                // Phase 2: Verification
                Console.WriteLine("[TC05] Phase 2: Verification...");
                VerifyInstallationStateAction.Execute(info.FileVersion);
                CaptureScreenshot("TC05_Phase2_Verified");

                // Phase 3: Uninstallation
                Console.WriteLine("[TC05] Phase 3: Uninstallation...");
                var uninstProc = LaunchUninstallerAction.Execute(UninstallerScreen!);
                proc = uninstProc;
                CaptureScreenshot("TC05_Phase3_UninstallerLaunched");

                ConfirmUninstallationAction.Execute(UninstallerScreen!, proc, timeoutMinutes: 8, step => CaptureScreenshot($"TC05_Phase3_{step}"));
                proc = null;
                CaptureScreenshot("TC05_Phase3_Uninstalled");

                // Phase 4: Final Cleanup Verification
                Console.WriteLine("[TC05] Phase 4: Post-Uninstall Cleanup Verification...");
                VerifyUninstallationStateAction.Execute();
                CaptureScreenshot("TC05_Phase4_CleanupVerified");

                Console.WriteLine("[TC05] PASS: Complete installation and uninstallation lifecycle verified successfully.");
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
