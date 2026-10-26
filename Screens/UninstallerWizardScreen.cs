using System;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using Ez3Di.Installer.Core;
using Button = FlaUI.Core.AutomationElements.Button;

namespace Ez3Di.Installer.Screens
{
    public class UninstallerWizardScreen
    {
        public AutomationBase Automation { get; }
        public Window? CurrentWindow { get; private set; }

        public UninstallerWizardScreen(AutomationBase automation)
        {
            Automation = automation;
        }

        public Window? WaitForUninstallerWindow(int timeoutSeconds = 60)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine($"[UninstallerWizardScreen] Waiting up to {timeoutSeconds}s for uninstaller window...");

            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                try
                {
                    // 1. Instant Win32 check
                    IntPtr fastHwnd = NativeMethods.FindWindow(null, "Ez3D-i - InstallShield Wizard");
                    if (fastHwnd != IntPtr.Zero && WindowHelper.IsVisibleOnAnyScreen(fastHwnd))
                    {
                        var activeScreen = WindowHelper.GetScreenForHwnd(fastHwnd);
                        WindowHelper.CenterWindowOnMonitor(fastHwnd, activeScreen);
                        InputDriver.PinWindowOnTop(fastHwnd);

                        try
                        {
                            CurrentWindow = Automation.FromHandle(fastHwnd).AsWindow();
                            return CurrentWindow;
                        }
                        catch { }
                    }

                    // 2. Desktop search with strict exclusion of VS Code / IDE
                    var desktop = Automation.GetDesktop();
                    var windows = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window));

                    foreach (var w in windows)
                    {
                        try
                        {
                            string name = (w.Name ?? "").Trim();
                            string cls = (w.ClassName ?? "").Trim();

                            if (name.Contains("Visual Studio Code", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains(".Installer", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains(".csproj", StringComparison.OrdinalIgnoreCase) ||
                                cls == "Chrome_WidgetWin_1" ||
                                cls == "CabinetWClass")
                            {
                                continue;
                            }

                            bool isMatch = (cls == "#32770" || cls.Contains("InstallShield", StringComparison.OrdinalIgnoreCase)) &&
                                           (name.Contains("Ez3D-i", StringComparison.OrdinalIgnoreCase) ||
                                            name.Contains("Uninstall", StringComparison.OrdinalIgnoreCase) ||
                                            name.Contains("Confirm", StringComparison.OrdinalIgnoreCase));

                            if (isMatch)
                            {
                                var win = w.AsWindow();
                                IntPtr hwnd = WindowHelper.GetHwnd(win);
                                bool isVisible = hwnd != IntPtr.Zero ? WindowHelper.IsVisibleOnAnyScreen(hwnd) : (win.BoundingRectangle.Width > 150);

                                if (isVisible && win.BoundingRectangle.Width > 150)
                                {
                                    CurrentWindow = win;
                                    if (hwnd != IntPtr.Zero)
                                    {
                                        var activeScreen = WindowHelper.GetScreenForHwnd(hwnd);
                                        Console.WriteLine($"[UninstallerWizardScreen] Captured true uninstaller '{win.Name}' on monitor: {activeScreen.DeviceName} (Bounds: {activeScreen.Bounds})");
                                        WindowHelper.CenterWindowOnMonitor(hwnd, activeScreen);
                                        InputDriver.PinWindowOnTop(hwnd);
                                    }
                                    return CurrentWindow;
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                Thread.Sleep(500);
            }

            return null;
        }

        public System.Drawing.Point? FindButtonCoordinate(params string[] buttonTexts)
        {
            if (CurrentWindow == null) return null;

            IntPtr hwnd = WindowHelper.GetHwnd(CurrentWindow);
            if (hwnd != IntPtr.Zero)
            {
                var pt = WindowHelper.FindChildButtonCoordinate(hwnd, buttonTexts);
                if (pt != null) return pt;
            }

            var btn = FindButton(buttonTexts);
            if (btn != null)
            {
                var r = btn.BoundingRectangle;
                return new System.Drawing.Point((int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
            }

            return null;
        }

        public Button? FindButton(params string[] buttonTexts)
        {
            if (CurrentWindow == null) return null;

            try
            {
                var buttons = CurrentWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
                foreach (var expected in buttonTexts)
                {
                    foreach (var btn in buttons)
                    {
                        try
                        {
                            string name = (btn.Name ?? "").Trim();
                            if (name.Equals(expected, StringComparison.OrdinalIgnoreCase) ||
                                name.Contains(expected, StringComparison.OrdinalIgnoreCase))
                            {
                                if (btn.IsEnabled && (btn.BoundingRectangle.Width > 0 || !btn.IsOffscreen))
                                {
                                    return btn.AsButton();
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
