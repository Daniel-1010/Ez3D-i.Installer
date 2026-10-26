using System;
using System.Diagnostics;
using System.Threading;
using FlaUI.Core.Definitions;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Install
{
    public static class WaitForInstallProgressAction
    {
        public static bool Execute(InstallerWizardScreen screen, int timeoutMinutes = 10, Action<string>? onStepCallback = null)
        {
            Console.WriteLine($"[WaitForInstallProgressAction] Monitoring installation progress (Timeout: {timeoutMinutes}m)...");

            var sw = Stopwatch.StartNew();
            int maxSeconds = timeoutMinutes * 60;
            int lastLoggedSeconds = 0;

            while (sw.Elapsed.TotalSeconds < maxSeconds)
            {
                var win = screen.WaitForWizardWindow(timeoutSeconds: 5);
                if (win != null)
                {
                    // Check if Finish button is visible & enabled
                    var finishBtn = screen.FindButton("Finish", "&Finish");
                    if (finishBtn != null && finishBtn.IsEnabled)
                    {
                        Console.WriteLine($"[WaitForInstallProgressAction] Installation finished in {sw.Elapsed.TotalSeconds:F1}s!");
                        onStepCallback?.Invoke("InstallProgressCompleted");
                        return true;
                    }

                    // Check for any modal driver / prerequisite dialogs (#32770) that might pop up
                    try
                    {
                        var desktop = screen.Automation.GetDesktop();
                        var dialogs = desktop.FindAllChildren(cf => cf.ByClassName("#32770"));
                        foreach (var dlg in dialogs)
                        {
                            try
                            {
                                var okBtn = dlg.FindFirstDescendant(cf => cf.ByName("OK").Or(cf.ByName("&OK")).Or(cf.ByName("Yes")).Or(cf.ByName("&Yes")));
                                if (okBtn != null && okBtn.IsEnabled)
                                {
                                    Console.WriteLine($"[WaitForInstallProgressAction] Confirming intermediate prerequisite popup '{dlg.Name}'...");
                                    var r = okBtn.BoundingRectangle;
                                    InputDriver.ClickAtPhysicalCoordinate(IntPtr.Zero, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                int currentSec = (int)sw.Elapsed.TotalSeconds;
                if (currentSec - lastLoggedSeconds >= 15)
                {
                    Console.WriteLine($"[WaitForInstallProgressAction] Installing... Elapsed: {currentSec}s / {maxSeconds}s");
                    lastLoggedSeconds = currentSec;
                }

                Thread.Sleep(2000);
            }

            throw new TimeoutException($"Installation did not complete within {timeoutMinutes} minutes.");
        }
    }
}
