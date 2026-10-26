using System;
using System.Diagnostics;
using Ez3Di.Installer.Actions.Discovery;
using Ez3Di.Installer.Actions.Install;
using Ez3Di.Installer.Actions.Upgrade;
using Ez3Di.Installer.Actions.Verify;
using Ez3Di.Installer.Tests;
using NUnit.Framework;

namespace Ez3Di.Installer.Tests.Upgrade
{
    [TestFixture]
    [Category("Upgrade")]
    [Order(1)]
    public class TC01_InPlace_Upgrade : BaseInstallerFixture
    {
        [Test]
        public void Run_TC01_InPlace_Upgrade()
        {
            Console.WriteLine("[TC_UP01] Starting In-Place Application Upgrade Test...");

            // Step 0: Ensure baseline older version is installed on host
            Console.WriteLine("[TC_UP01] Step 0: Checking and establishing baseline older version...");
            var baselineInfo = EnsureEz3DiOldVersionInstalledAction.Execute(Automation!, InstallerScreen!, step => CaptureScreenshot($"TC_UP01_{step}"));

            Process? setupProc = null;
            try
            {
                // Step 1: Launch target release installer (v5.5.16.0) from 'Installation Package/'
                Console.WriteLine("[TC_UP01] Step 1: Launching target release setup package...");
                var (proc, win, targetInfo) = LaunchInstallerAction.Execute(Automation!, InstallerScreen!);
                setupProc = proc;
                CaptureScreenshot("TC_UP01_Step1_UpgradeLaunched");

                // Step 2: Handle setup language selection if prompted
                Console.WriteLine("[TC_UP01] Step 2: Checking language selection dialog...");
                HandleLanguageSelectionAction.Execute(Automation!, InstallerScreen!);
                CaptureScreenshot("TC_UP01_Step2_LanguageHandled");

                // Step 3: Handle Upgrade Welcome page ("...will update installed version...") and click [ Next > ]
                Console.WriteLine("[TC_UP01] Step 3: Detecting Upgrade Welcome page and clicking [ Next > ]...");
                HandleUpgradeWizardAction.Execute(InstallerScreen!, step => CaptureScreenshot($"TC_UP01_Step3_{step}"));

                // Step 4: Monitor Setup Status progress bar until upgrade completes
                Console.WriteLine("[TC_UP01] Step 4: Monitoring upgrade progress bar to completion...");
                WaitForInstallProgressAction.Execute(InstallerScreen!, timeoutMinutes: 15, step => CaptureScreenshot($"TC_UP01_Step4_{step}"));

                // Step 5: Complete upgrade on "Update Complete" page by clicking [ Finish ]
                Console.WriteLine("[TC_UP01] Step 5: Clicking [ Finish ] on Update Complete screen...");
                CompleteInstallationAction.Execute(InstallerScreen!, setupProc, step => CaptureScreenshot($"TC_UP01_Step5_{step}"));
                setupProc = null;
                CaptureScreenshot("TC_UP01_Step5_UpgradeCompleted");

                // Step 6: Verify post-upgrade state (Bin\Ez3D-i64.exe, shortcut, and registry DisplayVersion == 5.5.16.0)
                Console.WriteLine($"[TC_UP01] Step 6: Verifying upgraded installation state for v{targetInfo.FileVersion}...");
                VerifyInstallationStateAction.Execute(targetInfo.FileVersion);
                CaptureScreenshot("TC_UP01_Step6_UpgradeVerified");

                Console.WriteLine($"[TC_UP01] PASS: Ez3D-i successfully upgraded from baseline v{baselineInfo.FileVersion} to target release v{targetInfo.FileVersion}.");
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
