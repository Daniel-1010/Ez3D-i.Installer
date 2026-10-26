using System;
using Ez3Di.Installer.Actions.Verify;
using NUnit.Framework;

namespace Ez3Di.Installer.Tests
{
    [TestFixture]
    [Category("Verification")]
    [Order(2)]
    public class TC02_Post_Install_Verify : BaseInstallerFixture
    {
        [Test]
        public void Run_TC02_Post_Install_Verify()
        {
            Console.WriteLine("[TC02] Starting post-installation verification...");

            CaptureScreenshot("TC02_Step1_PreVerification");
            VerifyInstallationStateAction.Execute();
            CaptureScreenshot("TC02_Step2_PostVerification");

            Console.WriteLine("[TC02] PASS: Post-installation verification completed.");
        }
    }
}
