using System;
using System.Diagnostics;
using System.Threading;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Install
{
    public static class CompleteInstallationAction
    {
        public static void Execute(InstallerWizardScreen screen, Process? setupProcess, Action<string>? onStepCallback = null)
        {
            Console.WriteLine("[CompleteInstallationAction] Finalizing installation on 'InstallShield Wizard Complete' page...");

            var win = screen.WaitForWizardWindow(timeoutSeconds: 20);
            if (win == null)
            {
                Console.WriteLine("[CompleteInstallationAction] Wizard window already closed or exited.");
                return;
            }

            IntPtr hwnd = WindowHelper.GetHwnd(win);
            InputDriver.ForceWindowForeground(hwnd);
            Thread.Sleep(500);

            // 1. Uncheck 'Launch the program' or 'Launch Ez3D-i' if present
            var launchCb = screen.FindCheckBox("Launch", "Ez3D-i");
            if (launchCb != null && launchCb.IsChecked == true)
            {
                Console.WriteLine($"[CompleteInstallationAction] Unchecking launch checkbox: '{launchCb.Name}'...");
                try
                {
                    launchCb.IsChecked = false;
                }
                catch
                {
                    var r = launchCb.BoundingRectangle;
                    InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + 10), (int)(r.Top + r.Height / 2));
                }
                Thread.Sleep(300);
            }

            // 2. Select 'No, I will restart my computer later' if reboot prompt exists
            var restartLaterRadio = screen.FindRadioButton("restart my computer later", "No, I will restart", "later");
            if (restartLaterRadio != null && !restartLaterRadio.IsChecked)
            {
                Console.WriteLine($"[CompleteInstallationAction] Selecting 'Restart later' option: '{restartLaterRadio.Name}'...");
                try
                {
                    restartLaterRadio.IsChecked = true;
                }
                catch
                {
                    var r = restartLaterRadio.BoundingRectangle;
                    InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + 10), (int)(r.Top + r.Height / 2));
                }
                Thread.Sleep(300);
            }

            // 3. Click [ Finish ]
            var pt = screen.FindButtonCoordinate("Finish", "&Finish");
            if (pt != null)
            {
                Console.WriteLine($"[CompleteInstallationAction] Clicking [ Finish ] button at ({pt.Value.X}, {pt.Value.Y})...");
                InputDriver.ClickAtPhysicalCoordinate(hwnd, pt.Value.X, pt.Value.Y);
                onStepCallback?.Invoke("FinishButtonClicked");
                Thread.Sleep(1500);
            }
            else if (ImageCompare.See("07_wizard_complete_finish.png", threshold: 0.80))
            {
                Console.WriteLine("[CompleteInstallationAction] Clicking [ Finish ] button via Multi-Screen OpenCV visual detection...");
                ImageCompare.Click("07_wizard_complete_finish.png", hwnd, threshold: 0.80);
                onStepCallback?.Invoke("FinishButtonClicked");
                Thread.Sleep(1500);
            }
            else
            {
                throw new InvalidOperationException("Failed to locate enabled [ Finish ] button on 'InstallShield Wizard Complete' page.");
            }

            // 4. Wait for installer process to terminate
            if (setupProcess != null && !setupProcess.HasExited)
            {
                Console.WriteLine("[CompleteInstallationAction] Waiting up to 30s for setup process exit...");
                ProcessHelper.WaitForExit(setupProcess, timeoutSeconds: 30);
            }

            Console.WriteLine("[CompleteInstallationAction] PASS: Setup wizard dismissed with [ Finish ] cleanly.");
        }
    }
}
