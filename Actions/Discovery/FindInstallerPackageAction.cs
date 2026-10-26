using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ez3Di.Installer.Core;

namespace Ez3Di.Installer.Actions.Discovery
{
    public class InstallerPackageInfo
    {
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FileVersion { get; set; } = "";
        public string ProductName { get; set; } = "";
        public long FileSizeBytes { get; set; }
    }

    public static class FindInstallerPackageAction
    {
        public static InstallerPackageInfo Execute(string? explicitPath = null)
        {
            if (!string.IsNullOrEmpty(explicitPath) && File.Exists(explicitPath))
            {
                return CreateInfo(explicitPath);
            }

            string packageDir = InstallerConstants.PackageDirectory;
            Console.WriteLine($"[FindInstallerPackageAction] Scanning package directory: '{packageDir}'...");

            if (Directory.Exists(packageDir))
            {
                var files = Directory.GetFiles(packageDir, "*.exe", SearchOption.AllDirectories)
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToList();

                if (files.Count > 0)
                {
                    var selected = files.First();
                    Console.WriteLine($"[FindInstallerPackageAction] Found installer in 'Installation Package/': '{selected}'");
                    return CreateInfo(selected);
                }
            }

            // Fallback scan in D:\Test product\Ez3D-i\ if package folder is currently empty
            string fallbackDir = @"D:\Test product\Ez3D-i";
            if (Directory.Exists(fallbackDir))
            {
                Console.WriteLine($"[FindInstallerPackageAction] 'Installation Package/' is empty, checking fallback repository: '{fallbackDir}'...");
                var fallbackFiles = Directory.GetFiles(fallbackDir, "*Setup*.exe", SearchOption.AllDirectories)
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToList();

                if (fallbackFiles.Count > 0)
                {
                    var selected = fallbackFiles.First();
                    Console.WriteLine($"[FindInstallerPackageAction] Selected fallback installer: '{selected}'");
                    return CreateInfo(selected);
                }
            }

            throw new FileNotFoundException(
                $"No installer executable (*.exe) was found in '{packageDir}' or fallback '{fallbackDir}'. " +
                "Please place the Ez3D-i setup executable into the 'Installation Package/' directory.");
        }

        private static InstallerPackageInfo CreateInfo(string path)
        {
            var fi = new FileInfo(path);
            var vi = FileVersionInfo.GetVersionInfo(path);

            return new InstallerPackageInfo
            {
                FilePath = path,
                FileName = fi.Name,
                FileVersion = vi.FileVersion ?? vi.ProductVersion ?? "Unknown",
                ProductName = vi.ProductName ?? "Ez3D-i Setup",
                FileSizeBytes = fi.Length
            };
        }
    }
}
