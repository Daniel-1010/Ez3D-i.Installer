using System;
using System.Diagnostics;
using Ez3Di.Installer.Actions.Uninstall;
using NUnit.Framework;

namespace Ez3Di.Installer.Tests
{
    [TestFixture]
    [Category("Uninstallation")]
    [Order(3)]
    public class TC03_Uninstallation : BaseInstallerFixture
    {
        [Test]
        public void Run_TC03_Uninstallation()
        {
            Console.WriteLine("[TC03] Starting uninstallation test...");

            Process? uninstProc = null;
            try
            {
                // Step 1: Launch uninstaller
                Console.WriteLine("[TC03] Step 1: Launching uninstaller...");
                uninstProc = LaunchUninstallerAction.Execute(UninstallerScreen!);
                CaptureScreenshot("TC03_Step1_UninstallerLaunched");

                // Step 2: Confirm and step through uninstaller
                Console.WriteLine("[TC03] Step 2: Confirming removal and waiting for completion...");
                ConfirmUninstallationAction.Execute(UninstallerScreen!, uninstProc, timeoutMinutes: 5);
                CaptureScreenshot("TC03_Step2_UninstallerCompleted");

                // Step 3: Verify clean system state
                Console.WriteLine("[TC03] Step 3: Verifying uninstallation cleanup...");
                VerifyUninstallationStateAction.Execute();
                CaptureScreenshot("TC03_Step3_CleanupVerified");

                Console.WriteLine("[TC03] PASS: Ez3D-i uninstalled cleanly.");
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
