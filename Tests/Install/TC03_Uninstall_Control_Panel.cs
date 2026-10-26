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
    [Order(3)]
    public class TC03_Uninstall_Control_Panel : BaseInstallerFixture
    {
        [Test]
        public void Run_TC03_Uninstall_Control_Panel()
        {
            Console.WriteLine("[TC03] Starting Uninstallation via Windows Control Panel test...");

            // Step 0: Prerequisite check - Ensure Ez3D-i is installed before attempting uninstallation
            Console.WriteLine("[TC03] Step 0: Checking if Ez3D-i is installed...");
            EnsureEz3DiInstalledAction.Execute(Automation!, InstallerScreen!, step => CaptureScreenshot($"TC03_{step}"));

            Process? uninstProc = null;
            try
            {
                // Step 1: Launch uninstaller via Registry UninstallString
                Console.WriteLine("[TC03] Step 1: Launching uninstaller from Control Panel command...");
                uninstProc = LaunchUninstallerAction.Execute(UninstallerScreen!);
                CaptureScreenshot("TC03_Step1_UninstallerLaunched");

                // Step 2: Confirm removal prompt, monitor progress bar, click finish
                Console.WriteLine("[TC03] Step 2: Confirming removal, monitoring progress, and finishing...");
                ConfirmUninstallationAction.Execute(UninstallerScreen!, uninstProc, timeoutMinutes: 8, step =>
                {
                    if (step == "ConfirmYesClicked") CaptureScreenshot("TC03_Step1_ConfirmYesClicked");
                    else if (step == "ProgressCompleted") CaptureScreenshot("TC03_Step2_ProgressCompleted");
                    else if (step == "FinishClicked") CaptureScreenshot("TC03_Step3_FinishClicked");
                });

                // Step 3: Verify clean system state (binaries, shortcuts, registry)
                Console.WriteLine("[TC03] Step 3: Verifying uninstallation cleanup...");
                VerifyUninstallationStateAction.Execute();
                CaptureScreenshot("TC03_Step4_CleanupVerified");

                Console.WriteLine("[TC03] PASS: Ez3D-i uninstalled cleanly via Control Panel.");
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
