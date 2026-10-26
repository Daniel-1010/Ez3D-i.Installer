using System;
using System.Diagnostics;
using System.IO;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;

namespace Ez3Di.Installer.Actions.Uninstall
{
    public static class LaunchUninstallerAction
    {
        public static Process? Execute(UninstallerWizardScreen screen)
        {
            Console.WriteLine("[LaunchUninstallerAction] Discovering uninstaller command...");

            string uninstallCommand = "";
            if (RegistryHelper.IsInstalled(out _, out _, out string regUninst))
            {
                uninstallCommand = regUninst;
                Console.WriteLine($"[LaunchUninstallerAction] Found uninstaller in registry: '{uninstallCommand}'");
            }

            string exeToRun = "";
            string args = "";

            if (!string.IsNullOrEmpty(uninstallCommand))
            {
                uninstallCommand = uninstallCommand.Trim();
                if (uninstallCommand.StartsWith("\""))
                {
                    int nextQuote = uninstallCommand.IndexOf('"', 1);
                    if (nextQuote > 1)
                    {
                        exeToRun = uninstallCommand.Substring(1, nextQuote - 1);
                        args = uninstallCommand.Substring(nextQuote + 1).Trim();
                    }
                }
                else
                {
                    int spaceIdx = uninstallCommand.IndexOf(' ');
                    if (spaceIdx > 0)
                    {
                        exeToRun = uninstallCommand.Substring(0, spaceIdx);
                        args = uninstallCommand.Substring(spaceIdx + 1).Trim();
                    }
                    else
                    {
                        exeToRun = uninstallCommand;
                    }
                }
            }

            // Fallback: check program directory for uninstaller executable
            if (string.IsNullOrEmpty(exeToRun) || !File.Exists(exeToRun))
            {
                string localUninst = Path.Combine(InstallerConstants.DefaultInstallDirectory, "uninst.exe");
                if (File.Exists(localUninst))
                {
                    exeToRun = localUninst;
                    args = "";
                }
            }

            if (string.IsNullOrEmpty(exeToRun) || !File.Exists(exeToRun))
            {
                throw new FileNotFoundException($"Cannot locate uninstaller executable. Command was: '{uninstallCommand}'");
            }

            Console.WriteLine($"[LaunchUninstallerAction] Launching uninstaller: '{exeToRun}' {args}...");
            var proc = ProcessHelper.StartElevated(exeToRun, args);

            screen.WaitForUninstallerWindow(timeoutSeconds: 30);
            return proc;
        }
    }
}
