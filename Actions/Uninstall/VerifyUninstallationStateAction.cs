using System;
using System.IO;
using Ez3Di.Installer.Core;
using NUnit.Framework;

namespace Ez3Di.Installer.Actions.Uninstall
{
    public static class VerifyUninstallationStateAction
    {
        public static void Execute()
        {
            Console.WriteLine("[VerifyUninstallationStateAction] Verifying post-uninstallation cleanup...");

            // 1. Verify executable is removed
            string exePath = InstallerConstants.DefaultExecutablePath;
            bool exeExists = File.Exists(exePath);
            Console.WriteLine($"[VerifyUninstallationStateAction] Main executable '{exePath}' exists: {exeExists}");
            Assert.That(exeExists, Is.False, $"Main executable still exists at '{exePath}' after uninstallation!");

            // 2. Verify desktop shortcut is removed
            string shortcutPath = InstallerConstants.DefaultDesktopShortcut;
            bool shortcutExists = File.Exists(shortcutPath);
            Console.WriteLine($"[VerifyUninstallationStateAction] Desktop shortcut '{shortcutPath}' exists: {shortcutExists}");
            Assert.That(shortcutExists, Is.False, $"Desktop shortcut still exists at '{shortcutPath}' after uninstallation!");

            // 3. Verify Registry Uninstall entry is removed
            bool isRegistered = RegistryHelper.IsInstalled(out string displayName, out string displayVersion, out _);
            Console.WriteLine($"[VerifyUninstallationStateAction] Windows Registry uninstall entry exists: {isRegistered} (Name='{displayName}', Version='{displayVersion}')");
            Assert.That(isRegistered, Is.False, "Ez3D-i is still registered in Windows Registry Uninstall hive!");

            Console.WriteLine("[VerifyUninstallationStateAction] PASS: Uninstallation successfully verified.");
        }
    }
}
