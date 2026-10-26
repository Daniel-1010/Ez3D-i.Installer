using System;
using System.Threading;
using FlaUI.Core.AutomationElements;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Install
{
    public static class StepThroughSetupWizardAction
    {
        public static void Execute(InstallerWizardScreen screen, Action<string>? onStepCallback = null)
        {
            Console.WriteLine("[StepThroughSetupWizardAction] Stepping through InstallShield wizard pages...");

            int maxPages = 10;
            for (int page = 1; page <= maxPages; page++)
            {
                var win = screen.WaitForWizardWindow(timeoutSeconds: 15);
                if (win == null)
                {
                    Console.WriteLine("[StepThroughSetupWizardAction] Wizard window no longer visible or transitioning to progress.");
                    break;
                }

                IntPtr hwnd = WindowHelper.GetHwnd(win);
                InputDriver.ForceWindowForeground(hwnd);
                Thread.Sleep(500);

                // Check 1: Is there a License Agreement radio button on screen?
                var acceptRadio = screen.FindRadioButton("accept", "agree", "Yes");
                if (acceptRadio != null && !acceptRadio.IsChecked)
                {
                    Console.WriteLine($"[StepThroughSetupWizardAction] Found License Agreement radio: '{acceptRadio.Name}'. Selecting...");
                    try
                    {
                        acceptRadio.IsChecked = true;
                    }
                    catch
                    {
                        var r = acceptRadio.BoundingRectangle;
                        InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                    }
                    Thread.Sleep(400);
                    onStepCallback?.Invoke("LicenseAccepted");
                }

                // Check 2: Is there an [ Install ] button?
                var installBtn = screen.FindButton("Install", "&Install");
                if (installBtn != null && installBtn.IsEnabled)
                {
                    Console.WriteLine($"[StepThroughSetupWizardAction] Found [ Install ] button: '{installBtn.Name}'. Clicking to start installation...");
                    var r = installBtn.BoundingRectangle;
                    InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                    Thread.Sleep(1000);
                    onStepCallback?.Invoke("InstallButtonClicked");
                    break; // Progress monitoring takes over next
                }

                // Check 3: Is there a [ Next > ] button?
                var nextBtn = screen.FindButton("Next >", "Next", "&Next >", "&Next");
                if (nextBtn != null && nextBtn.IsEnabled)
                {
                    Console.WriteLine($"[StepThroughSetupWizardAction] (Page {page}) Found [ Next > ] button. Clicking...");
                    var r = nextBtn.BoundingRectangle;
                    InputDriver.ClickAtPhysicalCoordinate(hwnd, (int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
                    Thread.Sleep(1000);
                    onStepCallback?.Invoke($"Page_{page}_NextClicked");
                    continue;
                }

                // Check 4: If [ Finish ] appears early, we are done
                var finishBtn = screen.FindButton("Finish", "&Finish");
                if (finishBtn != null && finishBtn.IsEnabled)
                {
                    Console.WriteLine("[StepThroughSetupWizardAction] [ Finish ] button already displayed.");
                    break;
                }

                Thread.Sleep(1000);
            }
        }
    }
}
