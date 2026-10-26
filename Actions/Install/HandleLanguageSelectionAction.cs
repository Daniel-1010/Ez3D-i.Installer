using System;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Install
{
    public static class HandleLanguageSelectionAction
    {
        public static bool Execute(AutomationBase automation, InstallerWizardScreen screen, int timeoutSeconds = 15)
        {
            Console.WriteLine("[HandleLanguageSelectionAction] Checking for language selection dialog...");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                try
                {
                    var desktop = automation.GetDesktop();
                    var dialog = desktop.FindFirstChild(cf => cf.ByName("Choose Setup Language")
                        .Or(cf.ByName("Select Language"))
                        .Or(cf.ByClassName("#32770")));

                    if (dialog != null && !dialog.IsOffscreen)
                    {
                        var win = dialog.AsWindow();
                        IntPtr hwnd = WindowHelper.GetHwnd(win);
                        InputDriver.ForceWindowForeground(hwnd);

                        Console.WriteLine($"[HandleLanguageSelectionAction] Found language dialog: '{dialog.Name}'. Clicking [ OK ]...");

                        // Locate [ OK ] button
                        var okBtn = dialog.FindFirstDescendant(cf => cf.ByName("OK").And(cf.ByControlType(ControlType.Button)))
                                 ?? dialog.FindFirstDescendant(cf => cf.ByName("&OK").And(cf.ByControlType(ControlType.Button)))
                                 ?? dialog.FindFirstDescendant(cf => cf.ByAutomationId("1"));

                        if (okBtn != null && okBtn.IsEnabled)
                        {
                            var r = okBtn.BoundingRectangle;
                            InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                            Thread.Sleep(800);
                            return true;
                        }
                    }
                }
                catch { }

                Thread.Sleep(500);
            }

            Console.WriteLine("[HandleLanguageSelectionAction] No separate language prompt detected; proceeding.");
            return false;
        }
    }
}
