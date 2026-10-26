using System;
using System.Diagnostics;
using Ez3Di.Installer.Actions.Install;
using Ez3Di.Installer.Actions.Uninstall;
using Ez3Di.Installer.Tests;
using NUnit.Framework;

namespace Ez3Di.Installer.Tests.Install
{
    [TestFixture]
    [Category("Installation")]
    [Category("Uninstallation")]
    [Order(4)]
    public class TC04_Uninstall_Rerun_Package : BaseInstallerFixture
    {
        [Test]
        public void Run_TC04_Uninstall_Rerun_Package()
        {
            Console.WriteLine("[TC04] Starting Uninstallation by Rerunning Setup Package test...");

            // Step 0: Prerequisite check - Ensure Ez3D-i is installed so rerunning setup enters Maintenance Mode
            Console.WriteLine("[TC04] Step 0: Checking if Ez3D-i is installed...");
            EnsureEz3DiInstalledAction.Execute(Automation!, InstallerScreen!, step => CaptureScreenshot($"TC04_{step}"));

            Process? proc = null;
            try
            {
                // Step 1: Launch the setup package on an already-installed machine
                Console.WriteLine("[TC04] Step 1: Rerunning setup package...");
                var (setupProc, win, info) = LaunchInstallerAction.Execute(Automation!, InstallerScreen!);
                proc = setupProc;
                CaptureScreenshot("TC04_Step1_PackageLaunched");

                // Step 2: Handle setup language dialog if prompted
                Console.WriteLine("[TC04] Step 2: Handling language selection...");
                HandleLanguageSelectionAction.Execute(Automation!, InstallerScreen!);
                CaptureScreenshot("TC04_Step2_LanguageHandled");

                // Step 2 & 3: Handle Maintenance Mode (Select 'Remove' radio button and click Next >)
                Console.WriteLine("[TC04] Steps 2 & 3: Selecting 'Remove' option in Maintenance Mode and clicking Next...");
                HandleMaintenanceModeAction.Execute(InstallerScreen!, step => CaptureScreenshot($"TC04_{step}"));

                // Step 4 & 5: Confirm removal, monitor uninstallation progress, and click Finish
                Console.WriteLine("[TC04] Steps 4 & 5: Confirming removal, monitoring progress, and finishing...");
                ConfirmUninstallationAction.Execute(UninstallerScreen!, proc, timeoutMinutes: 8, step =>
                {
                    if (step == "ConfirmYesClicked") CaptureScreenshot("TC04_Step4_ConfirmYesClicked");
                    else if (step == "ProgressCompleted") CaptureScreenshot("TC04_Step5_ProgressCompleted");
                    else if (step == "FinishClicked") CaptureScreenshot("TC04_Step5_FinishClicked");
                });
                proc = null;

                // Step 5: Verify post-uninstall cleanup
                Console.WriteLine("[TC04] Step 5: Verifying uninstallation cleanup...");
                VerifyUninstallationStateAction.Execute();
                CaptureScreenshot("TC04_Step6_CleanupVerified");

                Console.WriteLine("[TC04] PASS: Ez3D-i uninstalled cleanly via Setup Package Maintenance Mode.");
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
