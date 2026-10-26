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
            Console.WriteLine($"[FindInstallerPackageAction] Scanning primary package directory: '{packageDir}' (excluding 'Old Version')...");

            if (Directory.Exists(packageDir))
            {
                // Strict isolation: Scan ONLY root of Installation Package/ and explicitly exclude any Old Version folder
                var files = Directory.GetFiles(packageDir, "*.exe", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.Contains("Old Version", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToList();

                if (files.Count > 0)
                {
                    var selected = files.First();
                    Console.WriteLine($"[FindInstallerPackageAction] Found target installer in 'Installation Package/': '{selected}'");
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
                $"No installer executable (*.exe) was found in root '{packageDir}' or fallback '{fallbackDir}'. " +
                "Please place the target Ez3D-i setup executable into the 'Installation Package/' root directory.");
        }

        /// <summary>
        /// Dedicated discovery method for Upgrade test fixtures.
        /// Scans strictly 'Installation Package/Old Version/' for the baseline older setup package.
        /// </summary>
        public static InstallerPackageInfo FindOldVersion(string? explicitPath = null)
        {
            if (!string.IsNullOrEmpty(explicitPath) && File.Exists(explicitPath))
            {
                return CreateInfo(explicitPath);
            }

            string oldVersionDir = InstallerConstants.OldVersionPackageDirectory;
            Console.WriteLine($"[FindInstallerPackageAction] Scanning for baseline installer in: '{oldVersionDir}'...");

            if (Directory.Exists(oldVersionDir))
            {
                var files = Directory.GetFiles(oldVersionDir, "*.exe", SearchOption.AllDirectories)
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToList();

                if (files.Count > 0)
                {
                    var selected = files.First();
                    Console.WriteLine($"[FindInstallerPackageAction] Found baseline installer in 'Installation Package/Old Version/': '{selected}'");
                    return CreateInfo(selected);
                }
            }

            // Fallback scan in D:\Test product\Ez3D-i\ for an older version (e.g. 5.5.12)
            string fallbackDir = @"D:\Test product\Ez3D-i";
            if (Directory.Exists(fallbackDir))
            {
                var fallbackFiles = Directory.GetFiles(fallbackDir, "*Setup*.exe", SearchOption.AllDirectories)
                    .OrderBy(f => File.GetLastWriteTime(f)) // Pick oldest for baseline
                    .ToList();

                if (fallbackFiles.Count > 0)
                {
                    var selected = fallbackFiles.First();
                    Console.WriteLine($"[FindInstallerPackageAction] Selected fallback baseline installer: '{selected}'");
                    return CreateInfo(selected);
                }
            }

            throw new FileNotFoundException(
                $"No baseline installer executable (*.exe) was found in '{oldVersionDir}' or fallback '{fallbackDir}'. " +
                "Please place the older version setup executable into 'Installation Package/Old Version/'.");
        }

        private static InstallerPackageInfo CreateInfo(string path)
        {
            var fi = new FileInfo(path);
            var vi = FileVersionInfo.GetVersionInfo(path);

            string fileVer = (vi.FileVersion ?? vi.ProductVersion ?? "").Trim();
            string prodName = (vi.ProductName ?? "").Trim();
            string fileDesc = (vi.FileDescription ?? "").Trim();

            // Detect if PE header version belongs to the generic InstallShield Setup Launcher bootstrapper (e.g. 16.0.400)
            bool isInstallShieldLauncher = fileVer.StartsWith("16.0") ||
                                           fileDesc.Contains("InstallScript", StringComparison.OrdinalIgnoreCase) ||
                                           prodName.Contains("InstallShield", StringComparison.OrdinalIgnoreCase);

            string detectedVersion = fileVer;
            // Extract the actual application version from the installer package filename (e.g. "Ez3D-i V5.5.16.0 Setup(x64).exe" -> "5.5.16.0")
            var match = System.Text.RegularExpressions.Regex.Match(fi.Name, @"[vV]?(\d+\.\d+(\.\d+)*)");
            if (match.Success)
            {
                string parsedVer = match.Groups[1].Value;
                if (isInstallShieldLauncher || string.IsNullOrEmpty(detectedVersion))
                {
                    Console.WriteLine($"[FindInstallerPackageAction] Detected InstallShield launcher header ({fileVer}). Extracted actual application version from filename: '{parsedVer}'");
                    detectedVersion = parsedVer;
                }
            }

            return new InstallerPackageInfo
            {
                FilePath = path,
                FileName = fi.Name,
                FileVersion = detectedVersion,
                ProductName = !string.IsNullOrEmpty(prodName) && !isInstallShieldLauncher ? prodName : "Ez3D-i",
                FileSizeBytes = fi.Length
            };
        }
    }
}
