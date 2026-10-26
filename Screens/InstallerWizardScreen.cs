using System;
using System.Linq;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using Ez3Di.Installer.Core;
using Button = FlaUI.Core.AutomationElements.Button;
using RadioButton = FlaUI.Core.AutomationElements.RadioButton;
using CheckBox = FlaUI.Core.AutomationElements.CheckBox;

namespace Ez3Di.Installer.Screens
{
    public class InstallerWizardScreen
    {
        public AutomationBase Automation { get; }
        public Window? CurrentWindow { get; private set; }

        public InstallerWizardScreen(AutomationBase automation)
        {
            Automation = automation;
        }

        public Window? WaitForWizardWindow(int timeoutSeconds = 60)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine($"[InstallerWizardScreen] Waiting up to {timeoutSeconds}s for InstallShield Wizard window...");

            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                try
                {
                    // 1. Instant Win32 lookup (bypasses any UIA desktop search)
                    IntPtr fastHwnd = NativeMethods.FindWindow(null, "Ez3D-i - InstallShield Wizard");
                    if (fastHwnd == IntPtr.Zero)
                        fastHwnd = NativeMethods.FindWindow(null, "Choose Setup Language");

                    if (fastHwnd != IntPtr.Zero && WindowHelper.IsVisibleOnAnyScreen(fastHwnd))
                    {
                        Console.WriteLine($"[InstallerWizardScreen] Instant Win32 match found (HWND: 0x{fastHwnd:X8}).");
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

                    // 2. Fallback: Search desktop windows with strict exclusion of VS Code / IDE
                    var desktop = Automation.GetDesktop();
                    var windows = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window));

                    foreach (var w in windows)
                    {
                        try
                        {
                            string name = (w.Name ?? "").Trim();
                            string cls = (w.ClassName ?? "").Trim();

                            // EXCLUDE VS Code, browsers, and explorer
                            if (name.Contains("Visual Studio Code", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains(".Installer", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains(".csproj", StringComparison.OrdinalIgnoreCase) ||
                                cls == "Chrome_WidgetWin_1" ||
                                cls == "CabinetWClass")
                            {
                                continue;
                            }

                            // Match genuine InstallShield wizard dialog
                            bool isMatch = name.Contains("InstallShield Wizard", StringComparison.OrdinalIgnoreCase) ||
                                           name.Contains("Choose Setup Language", StringComparison.OrdinalIgnoreCase) ||
                                           (cls == "#32770" && name.StartsWith("Ez3D-i", StringComparison.OrdinalIgnoreCase));

                            if (isMatch)
                            {
                                var win = w.AsWindow();
                                IntPtr hwnd = WindowHelper.GetHwnd(win);
                                bool isVisible = hwnd != IntPtr.Zero ? WindowHelper.IsVisibleOnAnyScreen(hwnd) : (win.BoundingRectangle.Width > 180);

                                if (isVisible && win.BoundingRectangle.Width > 180 && win.BoundingRectangle.Height > 120)
                                {
                                    CurrentWindow = win;
                                    if (hwnd != IntPtr.Zero)
                                    {
                                        var activeScreen = WindowHelper.GetScreenForHwnd(hwnd);
                                        Console.WriteLine($"[InstallerWizardScreen] Captured true wizard '{win.Name}' on monitor: {activeScreen.DeviceName} (Bounds: {activeScreen.Bounds})");
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

        /// <summary>
        /// Locates a button by native Win32 child enumeration first, then FlaUI UIA3.
        /// Returns screen coordinates for physical cursor click.
        /// </summary>
        public System.Drawing.Point? FindButtonCoordinate(params string[] buttonTexts)
        {
            if (CurrentWindow == null) return null;

            IntPtr hwnd = WindowHelper.GetHwnd(CurrentWindow);

            // 1. Native Win32 child button lookup (fastest & most reliable)
            if (hwnd != IntPtr.Zero)
            {
                var pt = WindowHelper.FindChildButtonCoordinate(hwnd, buttonTexts);
                if (pt != null) return pt;
            }

            // 2. FlaUI UIA3 lookup
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

        public RadioButton? FindRadioButton(params string[] radioTexts)
        {
            if (CurrentWindow == null) return null;

            try
            {
                var radios = CurrentWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.RadioButton));
                foreach (var expected in radioTexts)
                {
                    foreach (var r in radios)
                    {
                        try
                        {
                            string name = (r.Name ?? "").Trim();
                            if (name.Contains(expected, StringComparison.OrdinalIgnoreCase))
                            {
                                return r.AsRadioButton();
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return null;
        }

        public CheckBox? FindCheckBox(params string[] checkboxTexts)
        {
            if (CurrentWindow == null) return null;

            try
            {
                var checkboxes = CurrentWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.CheckBox));
                foreach (var expected in checkboxTexts)
                {
                    foreach (var cb in checkboxes)
                    {
                        try
                        {
                            string name = (cb.Name ?? "").Trim();
                            if (name.Contains(expected, StringComparison.OrdinalIgnoreCase))
                            {
                                return cb.AsCheckBox();
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
