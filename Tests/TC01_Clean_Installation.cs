using System;
using System.Diagnostics;
using Ez3Di.Installer.Actions.Install;
using NUnit.Framework;

namespace Ez3Di.Installer.Tests
{
    [TestFixture]
    [Category("Installation")]
    [Order(1)]
    public class TC01_Clean_Installation : BaseInstallerFixture
    {
        [Test]
        public void Run_TC01_Clean_Installation()
        {
            Console.WriteLine("[TC01] Starting clean installation test...");

            Process? setupProc = null;
            try
            {
                // Step 1: Launch installer from 'Installation Package/'
                Console.WriteLine("[TC01] Step 1: Launching Ez3D-i installer package...");
                var (proc, win, info) = LaunchInstallerAction.Execute(Automation!, InstallerScreen!);
                setupProc = proc;
                CaptureScreenshot("TC01_Step1_InstallerLaunched");

                // Step 2: Handle Language prompt if present
                Console.WriteLine("[TC01] Step 2: Checking language selection...");
                HandleLanguageSelectionAction.Execute(Automation!, InstallerScreen!);
                CaptureScreenshot("TC01_Step2_LanguageHandled");

                // Step 3: Step through wizard pages (Welcome, License, Components)
                Console.WriteLine("[TC01] Step 3: Stepping through wizard pages & clicking Install...");
                StepThroughSetupWizardAction.Execute(InstallerScreen!, step => CaptureScreenshot($"TC01_Step3_{step}"));

                // Step 4: Wait for progress bar completion
                Console.WriteLine("[TC01] Step 4: Waiting for installation progress...");
                WaitForInstallProgressAction.Execute(InstallerScreen!, timeoutMinutes: 10, step => CaptureScreenshot($"TC01_Step4_{step}"));

                // Step 5: Complete installation on Finish page
                Console.WriteLine("[TC01] Step 5: Completing installation...");
                CompleteInstallationAction.Execute(InstallerScreen!, setupProc);
                CaptureScreenshot("TC01_Step5_InstallationCompleted");

                Console.WriteLine($"[TC01] PASS: Ez3D-i version {info.FileVersion} installed successfully.");
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
