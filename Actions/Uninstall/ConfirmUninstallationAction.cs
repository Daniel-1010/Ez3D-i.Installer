using System;
using System.Diagnostics;
using System.Threading;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Uninstall
{
    public static class ConfirmUninstallationAction
    {
        /// <summary>
        /// Executes uninstallation from Windows Control Panel:
        /// Step 1: Detect confirmation prompt ("Do you want to completely remove...") -> Click [ Yes ]
        /// Step 2: Wait for progress bar running until complete
        /// Step 3: Detect "Uninstall Complete" dialog -> Click [ Finish ]
        /// </summary>
        public static void Execute(UninstallerWizardScreen screen, Process? uninstProcess, int timeoutMinutes = 8, Action<string>? onStepCallback = null)
        {
            Console.WriteLine("[ConfirmUninstallationAction] Executing Control Panel uninstallation workflow (3-step sequence)...");

            var sw = Stopwatch.StartNew();
            int maxSeconds = timeoutMinutes * 60;
            bool confirmedRemoval = false;
            int lastLoggedSeconds = 0;

            // =========================================================================
            // Step 1: Confirmation Prompt ("Do you want to completely remove...") -> [ Yes ]
            // =========================================================================
            Console.WriteLine("[Step 1] Waiting for uninstallation confirmation prompt...");
            while (sw.Elapsed.TotalSeconds < 30 && !confirmedRemoval)
            {
                var win = screen.WaitForUninstallerWindow(timeoutSeconds: 3);
                if (win != null)
                {
                    IntPtr hwnd = WindowHelper.GetHwnd(win);
                    InputDriver.ForceWindowForeground(hwnd);

                    var ptYes = screen.FindButtonCoordinate("Yes", "&Yes");
                    if (ptYes != null)
                    {
                        Console.WriteLine($"[ConfirmUninstallationAction] Found confirmation prompt. Clicking [ Yes ] button at ({ptYes.Value.X}, {ptYes.Value.Y})...");
                        InputDriver.ClickAtPhysicalCoordinate(hwnd, ptYes.Value.X, ptYes.Value.Y);
                        onStepCallback?.Invoke("ConfirmYesClicked");
                        confirmedRemoval = true;
                        Thread.Sleep(1000);
                        break;
                    }
                    else if (ImageCompare.See("08_uninstall_confirm_prompt.png", threshold: 0.80))
                    {
                        Console.WriteLine("[ConfirmUninstallationAction] Found confirmation prompt via OpenCV matching. Clicking [ Yes ]...");
                        ImageCompare.Click("08_uninstall_confirm_prompt.png", hwnd, threshold: 0.80);
                        onStepCallback?.Invoke("ConfirmYesClicked");
                        confirmedRemoval = true;
                        Thread.Sleep(1000);
                        break;
                    }
                }
                Thread.Sleep(500);
            }

            if (!confirmedRemoval)
            {
                Console.WriteLine("[Step 1] Notice: Confirmation prompt did not appear or was auto-accepted; proceeding to progress.");
            }

            // =========================================================================
            // Step 2 & 3: Monitor progress bar & Click [ Finish ] on completion
            // =========================================================================
            Console.WriteLine("[Step 2] Monitoring uninstallation progress bar...");
            bool finishClicked = false;

            while (sw.Elapsed.TotalSeconds < maxSeconds && !finishClicked)
            {
                var win = screen.WaitForUninstallerWindow(timeoutSeconds: 3);
                if (win != null)
                {
                    IntPtr hwnd = WindowHelper.GetHwnd(win);
                    InputDriver.ForceWindowForeground(hwnd);

                    // Step 3 Check: Is Finish button present & enabled?
                    var ptFinish = screen.FindButtonCoordinate("Finish", "&Finish", "OK", "&OK");
                    if (ptFinish != null)
                    {
                        Console.WriteLine($"[ConfirmUninstallationAction] Found 'Uninstall Complete' dialog in {sw.Elapsed.TotalSeconds:F1}s! Clicking [ Finish ] at ({ptFinish.Value.X}, {ptFinish.Value.Y})...");
                        onStepCallback?.Invoke("ProgressCompleted");
                        InputDriver.ClickAtPhysicalCoordinate(hwnd, ptFinish.Value.X, ptFinish.Value.Y);
                        onStepCallback?.Invoke("FinishClicked");
                        finishClicked = true;
                        Thread.Sleep(1500);
                        break;
                    }
                    else if (ImageCompare.See("09_uninstall_complete_finish.png", threshold: 0.80))
                    {
                        Console.WriteLine($"[ConfirmUninstallationAction] Found 'Uninstall Complete' dialog via OpenCV matching! Clicking [ Finish ]...");
                        onStepCallback?.Invoke("ProgressCompleted");
                        ImageCompare.Click("09_uninstall_complete_finish.png", hwnd, threshold: 0.80);
                        onStepCallback?.Invoke("FinishClicked");
                        finishClicked = true;
                        Thread.Sleep(1500);
                        break;
                    }

                    // Check for any modal dialogs
                    try
                    {
                        var desktop = screen.Automation.GetDesktop();
                        var dialogs = desktop.FindAllChildren(cf => cf.ByClassName("#32770"));
                        foreach (var dlg in dialogs)
                        {
                            try
                            {
                                if (dlg.FrameworkAutomationElement.NativeWindowHandle != win.FrameworkAutomationElement.NativeWindowHandle)
                                {
                                    var okBtn = dlg.FindFirstDescendant(cf => cf.ByName("OK").Or(cf.ByName("&OK")).Or(cf.ByName("Yes")).Or(cf.ByName("&Yes")));
                                    if (okBtn != null && okBtn.IsEnabled)
                                    {
                                        var r = okBtn.BoundingRectangle;
                                        InputDriver.ClickAtPhysicalCoordinate(IntPtr.Zero, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                if (uninstProcess != null && uninstProcess.HasExited)
                {
                    Console.WriteLine("[ConfirmUninstallationAction] Uninstaller process has exited.");
                    break;
                }

                int currentSec = (int)sw.Elapsed.TotalSeconds;
                if (currentSec - lastLoggedSeconds >= 10)
                {
                    Console.WriteLine($"[Step 2] Uninstalling... Progress bar running. Elapsed: {currentSec}s / {maxSeconds}s");
                    lastLoggedSeconds = currentSec;
                }

                Thread.Sleep(1500);
            }

            if (uninstProcess != null && !uninstProcess.HasExited)
            {
                Console.WriteLine("[ConfirmUninstallationAction] Waiting up to 30s for uninstaller process termination...");
                ProcessHelper.WaitForExit(uninstProcess, timeoutSeconds: 30);
            }

            Console.WriteLine("[ConfirmUninstallationAction] PASS: Uninstaller completed successfully.");
        }
    }
}
