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
                    var desktop = Automation.GetDesktop();
                    var windows = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window));

                    foreach (var w in windows)
                    {
                        try
                        {
                            string name = w.Name ?? "";
                            string cls = w.ClassName ?? "";

                            if (name.Contains("Ez3D-i", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains("Uninstall", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains("Confirm", StringComparison.OrdinalIgnoreCase) ||
                                cls == "#32770")
                            {
                                var win = w.AsWindow();
                                if (!win.IsOffscreen && win.BoundingRectangle.Width > 150)
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
    }
}
