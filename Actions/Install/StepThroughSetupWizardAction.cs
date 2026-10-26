using System;
using System.Diagnostics;
using System.Threading;
using FlaUI.Core.AutomationElements;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Install
{
    public static class StepThroughSetupWizardAction
    {
        /// <summary>
        /// Steps through the 5-step Ez3D-i setup wizard sequence:
        /// Step 1: Click [ Next > ] on Welcome page
        /// Step 2: Select 'I accept the terms of the license agreement' radio button
        /// Step 3: Click [ Next > ] on License Agreement page
        /// Step 4: Click [ Next > ] on Choose Destination Location page
        /// Step 5: Click [ Install ] on Ready to Install the Program page
        /// </summary>
        public static void Execute(InstallerWizardScreen screen, Action<string>? onStepCallback = null)
        {
            Console.WriteLine("[StepThroughSetupWizardAction] Stepping through InstallShield wizard (5-step sequence)...");

            // =========================================================================
            // Step 1: Click [ Next > ] button on Welcome page
            // =========================================================================
            Console.WriteLine("[Step 1] Waiting for Welcome page and clicking [ Next > ]...");
            ClickNextWhenReady(screen, "Step 1 (Welcome page)");
            onStepCallback?.Invoke("Step1_Welcome_NextClicked");
            Thread.Sleep(800);

            // =========================================================================
            // Step 2: Select the "I accept the terms of the license agreement" radio button
            // =========================================================================
            Console.WriteLine("[Step 2] Selecting 'I accept the terms of the license agreement' radio button...");
            SelectLicenseAcceptRadio(screen);
            onStepCallback?.Invoke("Step2_LicenseAccepted");
            Thread.Sleep(600);

            // =========================================================================
            // Step 3: Click [ Next > ] button on License Agreement page
            // =========================================================================
            Console.WriteLine("[Step 3] Clicking [ Next > ] on License Agreement page...");
            ClickNextWhenReady(screen, "Step 3 (License page)");
            onStepCallback?.Invoke("Step3_License_NextClicked");
            Thread.Sleep(800);

            // =========================================================================
            // Step 4: Click [ Next > ] button on Choose Destination Location page
            // =========================================================================
            Console.WriteLine("[Step 4] Clicking [ Next > ] on Choose Destination Location page...");
            ClickNextWhenReady(screen, "Step 4 (Destination page)");
            onStepCallback?.Invoke("Step4_Destination_NextClicked");
            Thread.Sleep(800);

            // =========================================================================
            // Step 5: Click [ Install ] button on Ready to Install the Program page
            // =========================================================================
            Console.WriteLine("[Step 5] Clicking [ Install ] button on Ready to Install page...");
            ClickInstallWhenReady(screen);
            onStepCallback?.Invoke("Step5_InstallClicked");
            Thread.Sleep(1000);

            Console.WriteLine("[StepThroughSetupWizardAction] PASS: All 5 wizard steps executed successfully.");
        }

        private static void ClickNextWhenReady(InstallerWizardScreen screen, string contextName, int timeoutSeconds = 25)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                var win = screen.WaitForWizardWindow(timeoutSeconds: 3);
                if (win != null)
                {
                    IntPtr hwnd = WindowHelper.GetHwnd(win);
                    InputDriver.ForceWindowForeground(hwnd);

                    // Method A & B: Native Win32 child button or FlaUI UIA3
                    var pt = screen.FindButtonCoordinate("Next >", "Next", "&Next >", "&Next");
                    if (pt != null)
                    {
                        InputDriver.ClickAtPhysicalCoordinate(hwnd, pt.Value.X, pt.Value.Y);
                        Console.WriteLine($"[{contextName}] Clicked [ Next > ] button at ({pt.Value.X}, {pt.Value.Y}).");
                        return;
                    }

                    // Method C: OpenCV Multi-Screen Visual Matching
                    if (ImageCompare.See("btn_next.png", threshold: 0.80))
                    {
                        Console.WriteLine($"[{contextName}] Visual matching found 'btn_next.png'. Clicking...");
                        if (ImageCompare.Click("btn_next.png", hwnd, threshold: 0.80))
                        {
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
                            Console.WriteLine($"[{contextName}] Fallback to geometric coordinate click at ({fallbackX}, {fallbackY})...");
                            InputDriver.ClickAtPhysicalCoordinate(hwnd, fallbackX, fallbackY);
                            return;
                        }
                    }
                }
                Thread.Sleep(400);
            }

            throw new TimeoutException($"Failed to find an enabled [ Next > ] button for {contextName} within {timeoutSeconds}s.");
        }

        private static void SelectLicenseAcceptRadio(InstallerWizardScreen screen, int timeoutSeconds = 25)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                var win = screen.WaitForWizardWindow(timeoutSeconds: 3);
                if (win != null)
                {
                    IntPtr hwnd = WindowHelper.GetHwnd(win);
                    InputDriver.ForceWindowForeground(hwnd);

                    var acceptRadio = screen.FindRadioButton("accept the terms", "I accept", "accept");
                    if (acceptRadio != null && (acceptRadio.BoundingRectangle.Width > 0 || !acceptRadio.IsOffscreen))
                    {
                        var r = acceptRadio.BoundingRectangle;
                        // Click left edge where radio circle is located
                        int clickX = (int)(r.Left + 12);
                        int clickY = (int)(r.Top + r.Height / 2);

                        InputDriver.ClickAtPhysicalCoordinate(hwnd, clickX, clickY);

                        try
                        {
                            acceptRadio.IsChecked = true;
                        }
                        catch { }

                        Console.WriteLine($"[Step 2] Selected 'I accept...' radio button at ({clickX}, {clickY}).");
                        return;
                    }
                    else if (ImageCompare.See("02_license_agreement_unaccepted.png", threshold: 0.80))
                    {
                        Console.WriteLine("[Step 2] Selecting 'I accept...' radio button via Multi-Screen OpenCV visual detection...");
                        if (ImageCompare.Click("02_license_agreement_unaccepted.png", hwnd, threshold: 0.80))
                        {
                            return;
                        }
                    }
                }
                Thread.Sleep(400);
            }

            throw new TimeoutException($"Failed to find 'I accept...' radio button on License page within {timeoutSeconds}s.");
        }

        private static void ClickInstallWhenReady(InstallerWizardScreen screen, int timeoutSeconds = 25)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                var win = screen.WaitForWizardWindow(timeoutSeconds: 3);
                if (win != null)
                {
                    IntPtr hwnd = WindowHelper.GetHwnd(win);
                    InputDriver.ForceWindowForeground(hwnd);

                    var pt = screen.FindButtonCoordinate("Install", "&Install");
                    if (pt != null)
                    {
                        InputDriver.ClickAtPhysicalCoordinate(hwnd, pt.Value.X, pt.Value.Y);
                        Console.WriteLine($"[Step 5] Clicked [ Install ] button at ({pt.Value.X}, {pt.Value.Y}).");
                        return;
                    }
                    else if (ImageCompare.See("05_ready_to_install.png", threshold: 0.80))
                    {
                        Console.WriteLine("[Step 5] Clicking [ Install ] button via Multi-Screen OpenCV visual detection...");
                        if (ImageCompare.Click("05_ready_to_install.png", hwnd, threshold: 0.80))
                        {
                            return;
                        }
                    }
                }
                Thread.Sleep(400);
            }

            throw new TimeoutException($"Failed to find an enabled [ Install ] button on Ready to Install page within {timeoutSeconds}s.");
        }
    }
}
