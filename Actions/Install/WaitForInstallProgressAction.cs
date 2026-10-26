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
        public static bool Execute(InstallerWizardScreen screen, int timeoutMinutes = 15, Action<string>? onStepCallback = null)
        {
            Console.WriteLine($"[WaitForInstallProgressAction] Waiting for progress bar to run and complete (Timeout: {timeoutMinutes}m)...");

            var sw = Stopwatch.StartNew();
            int maxSeconds = timeoutMinutes * 60;
            int lastLoggedSeconds = 0;
            bool progressCaptured = false;

            while (sw.Elapsed.TotalSeconds < maxSeconds)
            {
                var win = screen.WaitForWizardWindow(timeoutSeconds: 3);
                if (win != null)
                {
                    // Check if 'InstallShield Wizard Complete' page is reached and Finish button is enabled
                    var finishBtn = screen.FindButton("Finish", "&Finish");
                    if (finishBtn != null && finishBtn.IsEnabled)
                    {
                        Console.WriteLine($"[WaitForInstallProgressAction] Progress finished in {sw.Elapsed.TotalSeconds:F1}s! 'InstallShield Wizard Complete' reached.");
                        onStepCallback?.Invoke("ProgressFinished");
                        return true;
                    }

                    if (!progressCaptured)
                    {
                        onStepCallback?.Invoke("ProgressBarRunning");
                        progressCaptured = true;
                    }

                    // Check for any modal driver or prerequisite dialogs (#32770)
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
                                    Console.WriteLine($"[WaitForInstallProgressAction] Confirming intermediate popup '{dlg.Name}'...");
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
                if (currentSec - lastLoggedSeconds >= 10)
                {
                    Console.WriteLine($"[WaitForInstallProgressAction] Progress bar running... Elapsed: {currentSec}s / {maxSeconds}s");
                    lastLoggedSeconds = currentSec;
                }

                Thread.Sleep(1500);
            }

            throw new TimeoutException($"Installation progress bar did not complete within {timeoutMinutes} minutes.");
        }
    }
}
