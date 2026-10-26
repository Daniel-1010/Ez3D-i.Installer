using System;
using System.Diagnostics;
using System.Threading;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;
using FlaUI.Core.AutomationElements;

namespace Ez3Di.Installer.Actions.Upgrade
{
    /// <summary>
    /// Handles the InstallShield Update/Upgrade welcome wizard page:
    /// - Detects the update confirmation dialog ("...will update the installed version (...) of Ez3D-i to version 5.5.16.0.")
    /// - Advances past the welcome page by clicking [ Next > ].
    /// </summary>
    public static class HandleUpgradeWizardAction
    {
        public static void Execute(InstallerWizardScreen screen, Action<string>? onStepCallback = null, int timeoutSeconds = 30)
        {
            Console.WriteLine("[HandleUpgradeWizardAction] Detecting Upgrade Welcome wizard page...");

            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                var win = screen.WaitForWizardWindow(timeoutSeconds: 3);
                if (win != null)
                {
                    IntPtr hwnd = WindowHelper.GetHwnd(win);
                    InputDriver.ForceWindowForeground(hwnd);
                    InputDriver.PinWindowOnTop(hwnd);

                    // Method A & B: Native Win32 child button or FlaUI UIA3
                    var pt = screen.FindButtonCoordinate("Next >", "Next", "&Next >", "&Next");
                    if (pt != null)
                    {
                        InputDriver.ClickAtPhysicalCoordinate(hwnd, pt.Value.X, pt.Value.Y);
                        Console.WriteLine($"[HandleUpgradeWizardAction] Clicked [ Next > ] button at ({pt.Value.X}, {pt.Value.Y}).");
                        onStepCallback?.Invoke("UpgradeWelcome_NextClicked");
                        Thread.Sleep(800);
                        return;
                    }

                    // Method C: OpenCV Template Visual Matching
                    if (ImageCompare.See("btn_next.png", threshold: 0.80))
                    {
                        Console.WriteLine("[HandleUpgradeWizardAction] Visual matching found 'btn_next.png'. Clicking...");
                        if (ImageCompare.Click("btn_next.png", hwnd, threshold: 0.80))
                        {
                            onStepCallback?.Invoke("UpgradeWelcome_NextClicked");
                            Thread.Sleep(800);
                            return;
                        }
                    }

                    // Method D: Geometric Fallback on standard InstallShield dialog
                    if (hwnd != IntPtr.Zero && NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT r))
                    {
                        int width = r.Right - r.Left;
                        int height = r.Bottom - r.Top;
                        if (width >= 400 && height >= 300 && sw.Elapsed.TotalSeconds > 5)
                        {
                            int fallbackX = r.Left + (int)(width * 0.73);
                            int fallbackY = r.Top + height - 25;
                            Console.WriteLine($"[HandleUpgradeWizardAction] Geometric fallback coordinate click at ({fallbackX}, {fallbackY})...");
                            InputDriver.ClickAtPhysicalCoordinate(hwnd, fallbackX, fallbackY);
                            onStepCallback?.Invoke("UpgradeWelcome_NextClicked");
                            Thread.Sleep(800);
                            return;
                        }
                    }
                }

                Thread.Sleep(500);
            }

            throw new TimeoutException($"[HandleUpgradeWizardAction] Timed out waiting for [ Next > ] button on Upgrade Welcome page within {timeoutSeconds}s.");
        }
    }
}
