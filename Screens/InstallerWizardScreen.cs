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
                    var desktop = Automation.GetDesktop();
                    var windows = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window));

                    foreach (var w in windows)
                    {
                        try
                        {
                            string name = w.Name ?? "";
                            string cls = w.ClassName ?? "";

                            // InstallShield wizards typically have titles like 'Ez3D-i - InstallShield Wizard', 'Choose Setup Language', etc.
                            if (name.Contains("Ez3D-i", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains("InstallShield", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains("Setup", StringComparison.OrdinalIgnoreCase) ||
                                cls.Contains("InstallShield", StringComparison.OrdinalIgnoreCase) ||
                                cls == "#32770")
                            {
                                var win = w.AsWindow();
                                if (!win.IsOffscreen && win.BoundingRectangle.Width > 200 && win.BoundingRectangle.Height > 150)
                                {
                                    CurrentWindow = win;
                                    IntPtr hwnd = WindowHelper.GetHwnd(win);
                                    if (hwnd != IntPtr.Zero)
                                    {
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
                                if (btn.IsEnabled && !btn.IsOffscreen)
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
