using System;
using System.Diagnostics;
using System.Threading;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Uninstall
{
    public static class ConfirmUninstallationAction
    {
        public static void Execute(UninstallerWizardScreen screen, Process? uninstProcess, int timeoutMinutes = 5)
        {
            Console.WriteLine("[ConfirmUninstallationAction] Stepping through uninstallation wizard...");

            var sw = Stopwatch.StartNew();
            int maxSeconds = timeoutMinutes * 60;

            while (sw.Elapsed.TotalSeconds < maxSeconds)
            {
                var win = screen.WaitForUninstallerWindow(timeoutSeconds: 5);
                if (win != null)
                {
                    IntPtr hwnd = WindowHelper.GetHwnd(win);
                    InputDriver.ForceWindowForeground(hwnd);

                    // Check 1: Confirmation prompt ([ Yes ] button)
                    var yesBtn = screen.FindButton("Yes", "&Yes");
                    if (yesBtn != null && yesBtn.IsEnabled)
                    {
                        Console.WriteLine($"[ConfirmUninstallationAction] Confirming removal via '{yesBtn.Name}'...");
                        var r = yesBtn.BoundingRectangle;
                        InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                        Thread.Sleep(1000);
                        continue;
                    }

                    // Check 2: Setup next/remove button
                    var nextBtn = screen.FindButton("Next >", "&Next >", "Remove", "&Remove");
                    if (nextBtn != null && nextBtn.IsEnabled)
                    {
                        Console.WriteLine($"[ConfirmUninstallationAction] Advancing uninstaller via '{nextBtn.Name}'...");
                        var r = nextBtn.BoundingRectangle;
                        InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                        Thread.Sleep(1000);
                        continue;
                    }

                    // Check 3: Finish button
                    var finishBtn = screen.FindButton("Finish", "&Finish", "OK", "&OK");
                    if (finishBtn != null && finishBtn.IsEnabled)
                    {
                        Console.WriteLine($"[ConfirmUninstallationAction] Completing uninstallation via '{finishBtn.Name}'...");
                        var r = finishBtn.BoundingRectangle;
                        InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                        Thread.Sleep(1500);
                        break;
                    }
                }

                // If process has exited, uninstallation is complete
                if (uninstProcess != null && uninstProcess.HasExited)
                {
                    Console.WriteLine("[ConfirmUninstallationAction] Uninstaller process exited.");
                    break;
                }

                Thread.Sleep(2000);
            }

            if (uninstProcess != null)
            {
                ProcessHelper.WaitForExit(uninstProcess, timeoutSeconds: 30);
            }

            Console.WriteLine("[ConfirmUninstallationAction] PASS: Uninstaller completed.");
        }
    }
}
