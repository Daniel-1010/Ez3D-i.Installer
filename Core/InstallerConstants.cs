using System;
using System.IO;

namespace Ez3Di.Installer.Core
{
    public static class InstallerConstants
    {
        /// <summary>
        /// Dynamically locates the project root directory (containing Ez3D-i.Installer.csproj and Assets/)
        /// regardless of whether NUnit/dotnet test runs from bin/Debug, bin/Release, or AppData/Temp.
        /// </summary>
        public static string SolutionRoot
        {
            get
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var dir = new DirectoryInfo(baseDir);
                    while (dir != null)
                    {
                        if (File.Exists(Path.Combine(dir.FullName, "Ez3D-i.Installer.csproj")) ||
                            Directory.Exists(Path.Combine(dir.FullName, "Assets")))
                        {
                            return dir.FullName;
                        }
                        dir = dir.Parent;
                    }
                }
                catch { }

                return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
            }
        }

        public static string PackageDirectory =>
            Path.Combine(SolutionRoot, "Installation Package");

        public static string OldVersionPackageDirectory =>
            Path.Combine(PackageDirectory, "Old Version");

        public static string ResultDirectory =>
            Path.Combine(SolutionRoot, "Result");

        public const string DefaultInstallDirectory = @"C:\Program Files\VATECH\Ez3D-i";
        public const string DefaultExecutablePath   = @"C:\Program Files\VATECH\Ez3D-i\Bin\Ez3D-i64.exe";
        public const string FallbackExecutablePath  = @"C:\Program Files\VATECH\Ez3D-i\Ez3D-i64.exe";
        public const string DefaultDesktopShortcut  = @"C:\Users\Public\Desktop\Ez3D-i.lnk";

        public const string UninstallRegKeyWow64    = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
        public const string UninstallRegKey64       = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        public const string VatechRegKey            = @"SOFTWARE\WOW6432Node\VATECH\Ez3D-i";

        /// <summary>
        /// Common AppData configuration folder for Ewoosoft / Ez3D-i (C:\ProgramData\ewoosoft\common\config).
        /// Requires elevated Administrator privileges to modify or delete.
        /// </summary>
        public static string CommonAppDataEwoosoftConfigDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ewoosoft", "common", "config");

        /// <summary>
        /// Active server configuration file (C:\ProgramData\ewoosoft\common\config\VTServerConfig.ini).
        /// Controls the Quick Setting first-launch prompt.
        /// </summary>
        public static string CommonAppDataServerConfigFile =>
            Path.Combine(CommonAppDataEwoosoftConfigDirectory, "VTServerConfig.ini");

        /// <summary>
        /// Legacy fallback configuration file path under Program Files if present.
        /// </summary>
        public static string LegacyInstallFolderServerConfigFile =>
            Path.Combine(DefaultInstallDirectory, "Setting", "VTServerConfig.ini");
    }
}
