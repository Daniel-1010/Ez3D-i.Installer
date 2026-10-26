using System;
using System.Diagnostics;
using System.IO;
using Ez3Di.Installer.Core;
using NUnit.Framework;

namespace Ez3Di.Installer.Actions.Verify
{
    public static class VerifyInstallationStateAction
    {
        public static void Execute(string? expectedVersion = null)
        {
            Console.WriteLine("[VerifyInstallationStateAction] Verifying post-installation state...");

            // 1. Verify executable exists in target directory
            string exePath = InstallerConstants.DefaultExecutablePath;
            Console.WriteLine($"[VerifyInstallationStateAction] Checking executable: '{exePath}'...");
            Assert.That(File.Exists(exePath), Is.True, $"Main executable not found at: '{exePath}'");

            var vi = FileVersionInfo.GetVersionInfo(exePath);
            Console.WriteLine($"[VerifyInstallationStateAction] Executable verified: FileVersion={vi.FileVersion}, ProductVersion={vi.ProductVersion}");

            if (!string.IsNullOrEmpty(expectedVersion))
            {
                Console.WriteLine($"[VerifyInstallationStateAction] Comparing version against expected '{expectedVersion}'...");
                bool versionMatches = (vi.FileVersion != null && vi.FileVersion.Contains(expectedVersion)) ||
                                     (vi.ProductVersion != null && vi.ProductVersion.Contains(expectedVersion));
                Assert.That(versionMatches, Is.True, $"Installed version ({vi.FileVersion}) does not match expected '{expectedVersion}'.");
            }

            // 2. Verify Desktop Shortcut
            string shortcutPath = InstallerConstants.DefaultDesktopShortcut;
            Console.WriteLine($"[VerifyInstallationStateAction] Checking Desktop shortcut: '{shortcutPath}'...");
            bool shortcutExists = File.Exists(shortcutPath);
            Console.WriteLine($"[VerifyInstallationStateAction] Desktop shortcut present: {shortcutExists}");

            // 3. Verify Windows Registry Uninstall Entry
            Console.WriteLine("[VerifyInstallationStateAction] Checking Windows Registry uninstall key...");
            bool isRegistered = RegistryHelper.IsInstalled(out string displayName, out string displayVersion, out string uninstallString);
            Assert.That(isRegistered, Is.True, "Ez3D-i is not registered in Windows Registry Uninstall hive.");
            Console.WriteLine($"[VerifyInstallationStateAction] Registry Verified: Name='{displayName}', Version='{displayVersion}', UninstallString='{uninstallString}'");

            Console.WriteLine("[VerifyInstallationStateAction] PASS: Post-installation state successfully verified.");
        }
    }
}
