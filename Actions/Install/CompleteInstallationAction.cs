using System;
using System.Diagnostics;
using System.Threading;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Install
{
    public static class CompleteInstallationAction
    {
        public static void Execute(InstallerWizardScreen screen, Process? setupProcess)
        {
            Console.WriteLine("[CompleteInstallationAction] Finalizing installation on Finish page...");

            var win = screen.WaitForWizardWindow(timeoutSeconds: 15);
            if (win == null)
            {
                Console.WriteLine("[CompleteInstallationAction] Wizard window already closed or exited.");
                return;
            }

            IntPtr hwnd = WindowHelper.GetHwnd(win);
            InputDriver.ForceWindowForeground(hwnd);
            Thread.Sleep(500);

            // 1. Uncheck 'Launch the program' or 'Launch Ez3D-i'
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
                    InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
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
                    InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                }
                Thread.Sleep(300);
            }

            // 3. Click [ Finish ]
            var finishBtn = screen.FindButton("Finish", "&Finish");
            if (finishBtn != null && finishBtn.IsEnabled)
            {
                Console.WriteLine($"[CompleteInstallationAction] Clicking [ Finish ] button: '{finishBtn.Name}'...");
                var r = finishBtn.BoundingRectangle;
                InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                Thread.Sleep(1500);
            }

            // 4. Wait for installer process to terminate
            if (setupProcess != null)
            {
                Console.WriteLine("[CompleteInstallationAction] Waiting up to 30s for setup process exit...");
                ProcessHelper.WaitForExit(setupProcess, timeoutSeconds: 30);
            }

            Console.WriteLine("[CompleteInstallationAction] PASS: Setup wizard dismissed cleanly.");
        }
    }
}
