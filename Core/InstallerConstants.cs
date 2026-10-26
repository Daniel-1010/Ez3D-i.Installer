using System;
using System.IO;

namespace Ez3Di.Installer.Core
{
    public static class InstallerConstants
    {
        public static string SolutionRoot =>
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\"));

        public static string PackageDirectory =>
            Path.Combine(SolutionRoot, "Installation Package");

        public static string ResultDirectory =>
            Path.Combine(SolutionRoot, "Result");

        public const string DefaultInstallDirectory = @"C:\Program Files\VATECH\Ez3D-i";
        public const string DefaultExecutablePath   = @"C:\Program Files\VATECH\Ez3D-i\Ez3D-i64.exe";
        public const string DefaultDesktopShortcut  = @"C:\Users\Public\Desktop\Ez3D-i.lnk";

        public const string UninstallRegKeyWow64    = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
        public const string UninstallRegKey64       = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        public const string VatechRegKey            = @"SOFTWARE\WOW6432Node\VATECH\Ez3D-i";
    }
}
