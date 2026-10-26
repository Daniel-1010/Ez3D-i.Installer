using System;
using System.Diagnostics;
using System.Threading;

namespace Ez3Di.Installer.Core
{
    public static class ProcessHelper
    {
        public static Process? StartElevated(string filePath, string arguments = "", string? workingDirectory = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas"
            };

            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }

            Console.WriteLine($"[ProcessHelper] Launching elevated process: '{filePath}' {arguments}");
            return Process.Start(psi);
        }

        public static void KillProcessesByName(params string[] processNames)
        {
            foreach (var name in processNames)
            {
                try
                {
                    var procs = Process.GetProcessesByName(name);
                    foreach (var proc in procs)
                    {
                        using (proc)
                        {
                            try
                            {
                                Console.WriteLine($"[ProcessHelper] Killing process: {proc.ProcessName} (PID: {proc.Id})...");
                                proc.Kill();
                                proc.WaitForExit(3000);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[ProcessHelper] Warning while killing {name}: {ex.Message}");
                            }
                        }
                    }
                }
                catch { }
            }
        }

        public static bool WaitForExit(Process? process, int timeoutSeconds)
        {
            if (process == null) return true;
            try
            {
                return process.WaitForExit(timeoutSeconds * 1000);
            }
            catch
            {
                return true;
            }
        }
    }
}
