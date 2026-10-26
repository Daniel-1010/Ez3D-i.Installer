using System;
using Microsoft.Win32;

namespace Ez3Di.Installer.Core
{
    public static class RegistryHelper
    {
        public static bool IsInstalled(out string displayName, out string displayVersion, out string uninstallString)
        {
            displayName = "";
            displayVersion = "";
            uninstallString = "";

            if (CheckUninstallHive(RegistryView.Registry32, out displayName, out displayVersion, out uninstallString))
                return true;

            if (CheckUninstallHive(RegistryView.Registry64, out displayName, out displayVersion, out uninstallString))
                return true;

            return false;
        }

        private static bool CheckUninstallHive(RegistryView view, out string displayName, out string displayVersion, out string uninstallString)
        {
            displayName = "";
            displayVersion = "";
            uninstallString = "";

            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var uninstKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if (uninstKey == null) return false;

                foreach (var subKeyName in uninstKey.GetSubKeyNames())
                {
                    using var subKey = uninstKey.OpenSubKey(subKeyName);
                    if (subKey == null) continue;

                    var name = subKey.GetValue("DisplayName") as string;
                    if (!string.IsNullOrEmpty(name) && name.Contains("Ez3D-i", StringComparison.OrdinalIgnoreCase))
                    {
                        displayName = name;
                        displayVersion = (subKey.GetValue("DisplayVersion") as string) ?? "";
                        uninstallString = (subKey.GetValue("UninstallString") as string) ?? "";
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegistryHelper] Warning reading registry: {ex.Message}");
            }

            return false;
        }

        public static string? GetVatechRegistryValue(string valueName)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                using var subKey = baseKey.OpenSubKey(InstallerConstants.VatechRegKey);
                return subKey?.GetValue(valueName) as string;
            }
            catch
            {
                return null;
            }
        }
    }
}
