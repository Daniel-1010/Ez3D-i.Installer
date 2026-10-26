using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using FlaUI.Core;
using FlaUI.UIA3;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;
using NUnit.Framework;

namespace Ez3Di.Installer.Tests
{
    [TestFixture]
    public abstract class BaseInstallerFixture
    {
        public AutomationBase? Automation { get; private set; }
        public InstallerWizardScreen? InstallerScreen { get; private set; }
        public UninstallerWizardScreen? UninstallerScreen { get; private set; }

        protected List<string> ScreenshotPaths { get; } = new();
        protected string TestResultDirectory { get; private set; } = "";

        [SetUp]
        public virtual void SetUp()
        {
            Automation = new UIA3Automation();
            InstallerScreen = new InstallerWizardScreen(Automation);
            UninstallerScreen = new UninstallerWizardScreen(Automation);
            ScreenshotPaths.Clear();

            string testName = TestContext.CurrentContext.Test.Name;
            TestResultDirectory = Path.Combine(InstallerConstants.ResultDirectory, testName);
            Directory.CreateDirectory(TestResultDirectory);
        }

        protected void CaptureScreenshot(string stepName)
        {
            try
            {
                string filePath = Path.Combine(TestResultDirectory, $"{stepName}.png");
                Rectangle bounds;
                var activeWindow = InstallerScreen?.CurrentWindow ?? UninstallerScreen?.CurrentWindow;
                if (activeWindow != null)
                {
                    var targetScreen = WindowHelper.GetScreenForWindow(activeWindow);
                    bounds = targetScreen.Bounds;
                }
                else
                {
                    bounds = SystemInformation.VirtualScreen;
                }

                using var bmp = new Bitmap(bounds.Width, bounds.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
                }

                bmp.Save(filePath, ImageFormat.Png);
                ScreenshotPaths.Add(filePath);
                Console.WriteLine($"[Screenshot] Saved: '{filePath}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Screenshot] Warning capturing '{stepName}': {ex.Message}");
            }
        }

        [TearDown]
        public virtual void TearDown()
        {
            var outcome = TestContext.CurrentContext.Result.Outcome.Status.ToString().ToUpper();
            string testName = TestContext.CurrentContext.Test.Name;
            string message = TestContext.CurrentContext.Result.Message ?? "";

            Console.WriteLine("==========================================================");
            Console.WriteLine($"TEST {testName}: {outcome} - {message}");
            Console.WriteLine("==========================================================");

            ResultWriter.WriteResult(testName, outcome, message, ScreenshotPaths);

            try
            {
                Automation?.Dispose();
            }
            catch { }
        }
    }
}
