using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using Ez3Di.Installer.Actions.Discovery;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Install
{
    public static class LaunchInstallerAction
    {
        public static (Process? Process, Window? Window, InstallerPackageInfo Info) Execute(
            AutomationBase automation,
            InstallerWizardScreen screen,
            string? explicitInstallerPath = null)
        {
            var info = FindInstallerPackageAction.Execute(explicitInstallerPath);

            Console.WriteLine($"[LaunchInstallerAction] Preparing to launch installer: {info.FileName} (Version: {info.FileVersion})...");

            // Clean up any lingering setup instances
            ProcessHelper.KillProcessesByName("Setup", "_is", "msiexec");
            Thread.Sleep(500);

            var proc = ProcessHelper.StartElevated(info.FilePath, workingDirectory: Path.GetDirectoryName(info.FilePath));
            if (proc == null)
            {
                throw new InvalidOperationException($"Failed to launch installer process: '{info.FilePath}'");
            }

            Console.WriteLine($"[LaunchInstallerAction] Installer process launched with PID: {proc.Id}. Waiting for UI window...");

            var win = screen.WaitForWizardWindow(timeoutSeconds: 60);
            if (win == null)
            {
                throw new TimeoutException("InstallShield Wizard main window did not appear within 60 seconds.");
            }

            Console.WriteLine($"[LaunchInstallerAction] Installer window captured: '{win.Name}'");
            return (proc, win, info);
        }
    }
}
