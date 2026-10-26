using System;
using System.Drawing;
using System.Threading;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Uninstall
{
    /// <summary>
    /// Handles the InstallShield Maintenance Mode dialog presented when rerunning
    /// the setup package on a machine where Ez3D-i is already installed.
    /// Step 2: Selects the "Remove" option (switching from default "Repair").
    /// Step 3: Clicks the [ Next > ] button.
    /// </summary>
    public static class HandleMaintenanceModeAction
    {
        public static void Execute(InstallerWizardScreen screen, Action<string>? onStepCallback = null, int timeoutSeconds = 30)
        {
            Console.WriteLine("[HandleMaintenanceModeAction] Waiting for InstallShield Maintenance Mode dialog...");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            bool removeSelected = false;

            while (sw.Elapsed.TotalSeconds < timeoutSeconds && !removeSelected)
            {
                var win = screen.WaitForWizardWindow(timeoutSeconds: 3);
                if (win != null)
                {
                    IntPtr hwnd = WindowHelper.GetHwnd(win);
                    InputDriver.ForceWindowForeground(hwnd);

                    // =========================================================================
                    // Step 2: Select the "Remove" radio button
                    // =========================================================================
                    var radioRemove = screen.FindRadioButton("Remove", "&Remove", "Remove all installed features");
                    if (radioRemove != null)
                    {
                        var r = radioRemove.BoundingRectangle;
                        int clickX = (int)(r.Left + 12);
                        int clickY = (int)(r.Top + r.Height / 2);

                        Console.WriteLine($"[Step 2] Found 'Remove' radio button via FlaUI. Clicking at physical coordinate ({clickX}, {clickY})...");
                        InputDriver.ClickAtPhysicalCoordinate(hwnd, clickX, clickY);
                        removeSelected = true;
                    }
                    else
                    {
                        // Fallback 1: Native Win32 child button coordinates
                        var ptRemove = WindowHelper.FindChildButtonCoordinate(hwnd, "Remove", "&Remove");
                        if (ptRemove != null)
                        {
                            Console.WriteLine($"[Step 2] Found 'Remove' radio button via Win32. Clicking at ({ptRemove.Value.X}, {ptRemove.Value.Y})...");
                            InputDriver.ClickAtPhysicalCoordinate(hwnd, ptRemove.Value.X, ptRemove.Value.Y);
                            removeSelected = true;
                        }
                        else
                        {
                            // Fallback 2: Geometric dialog offset for standard InstallShield SdOptionsButtons dialog
                            var bounds = WindowHelper.GetWindowBounds(win);
                            if (bounds.Width > 200 && bounds.Height > 200)
                            {
                                int clickX = bounds.Left + 62;
                                int clickY = bounds.Top + 272; // Standard Y offset for 'Remove' radio option
                                Console.WriteLine($"[Step 2] Using geometric coordinate fallback for 'Remove' radio at ({clickX}, {clickY})...");
                                InputDriver.ClickAtPhysicalCoordinate(hwnd, clickX, clickY);
                                removeSelected = true;
                            }
                        }
                    }

                    if (removeSelected)
                    {
                        Thread.Sleep(500); // Dwell time for Win32 BN_CLICKED event propagation
                        onStepCallback?.Invoke("Step2_RemoveSelected");

                        // =========================================================================
                        // Step 3: Click the [ Next > ] button
                        // =========================================================================
                        var ptNext = screen.FindButtonCoordinate("Next >", "Next", "&Next >", "&Next");
                        if (ptNext != null)
                        {
                            Console.WriteLine($"[Step 3] Clicking [ Next > ] button at ({ptNext.Value.X}, {ptNext.Value.Y})...");
                            InputDriver.ClickAtPhysicalCoordinate(hwnd, ptNext.Value.X, ptNext.Value.Y);
                        }
                        else
                        {
                            Console.WriteLine("[Step 3] Clicking [ Next > ] via OpenCV template...");
                            ImageCompare.Click("btn_next.png", hwnd, threshold: 0.80);
                        }

                        Thread.Sleep(500);
                        onStepCallback?.Invoke("Step3_NextClicked");
                        Thread.Sleep(500);
                        return;
                    }
                }

                Thread.Sleep(500);
            }

            if (!removeSelected)
            {
                Console.WriteLine("[HandleMaintenanceModeAction] WARNING: Could not find 'Remove' radio button within timeout.");
            }
        }
    }
}
