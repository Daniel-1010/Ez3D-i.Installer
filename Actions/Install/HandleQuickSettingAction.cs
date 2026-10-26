using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Ez3Di.Installer.Core;
using Ez3Di.Installer.Screens;
using FlaUI.Core;

namespace Ez3Di.Installer.Actions.Install
{
    /// <summary>
    /// Automates the Quick Setting configuration workflow on first Ez3D-i launch:
    /// 1. Detects the Quick Setting dialog and brings it to foreground (past VS Code).
    /// 2. Clicks [ Search ] button.
    /// 3. Detects modal dialog "Searching the IP Address of file server".
    /// 4. Selects row with "127.0.0.1".
    /// 5. Clicks [ OK ] button on the search dialog.
    /// 6. Verifies that Server IP Address edit box displays "127.0.0.1".
    /// 7. Clicks [ OK ] button on the main Quick Setting dialog.
    /// </summary>
    public static class HandleQuickSettingAction
    {
        /// <summary>
        /// Administratively purges the Quick Setting server configuration file (VTServerConfig.ini)
        /// from the system Common AppData directory (C:\ProgramData\ewoosoft\common\config\).
        /// This forces Ez3D-i to display the Quick Setting dialog on its next launch.
        /// 
        /// NOTE: Because C:\ProgramData is a protected system directory, this operation requires
        /// that the calling test process / VS Code be executed with elevated Administrator privileges.
        /// </summary>
        /// <returns>True if configuration was successfully purged or was already absent.</returns>
        public static bool PurgeQuickSettingConfigFile()
        {
            string primaryPath = InstallerConstants.CommonAppDataServerConfigFile;
            Console.WriteLine($"[HandleQuickSettingAction] Administratively purging Quick Setting config: '{primaryPath}'...");

            // 1. Purge primary Common AppData location (C:\ProgramData\ewoosoft\common\config\VTServerConfig.ini)
            try
            {
                if (File.Exists(primaryPath))
                {
                    var attr = File.GetAttributes(primaryPath);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(primaryPath, attr & ~FileAttributes.ReadOnly);
                    }

                    File.Delete(primaryPath);
                    Console.WriteLine($"[HandleQuickSettingAction] Successfully deleted: '{primaryPath}'");
                }
                else
                {
                    Console.WriteLine($"[HandleQuickSettingAction] Primary config file not present (already clean): '{primaryPath}'");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                string errMsg = $"[HandleQuickSettingAction] ACCESS DENIED: Unable to delete '{primaryPath}'.\n" +
                                "Deleting files from C:\\ProgramData requires elevated Administrator privileges.\n" +
                                "Please ensure VS Code or Windows Terminal is launched with 'Run as administrator'.\n" +
                                $"Technical error: {ex.Message}";
                Console.WriteLine(errMsg);
                throw new UnauthorizedAccessException(errMsg, ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HandleQuickSettingAction] Error deleting '{primaryPath}': {ex.Message}");
                throw;
            }

            // 2. Also purge legacy install folder setting if present (C:\Program Files\VATECH\Ez3D-i\Setting\VTServerConfig.ini)
            try
            {
                string legacyPath = InstallerConstants.LegacyInstallFolderServerConfigFile;
                if (File.Exists(legacyPath))
                {
                    var attr = File.GetAttributes(legacyPath);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(legacyPath, attr & ~FileAttributes.ReadOnly);
                    }
                    File.Delete(legacyPath);
                    Console.WriteLine($"[HandleQuickSettingAction] Successfully deleted legacy config: '{legacyPath}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HandleQuickSettingAction] Note: Could not delete legacy config: {ex.Message}");
            }

            // Verify deletion of primary path
            if (File.Exists(primaryPath))
            {
                throw new IOException($"[HandleQuickSettingAction] Verification failed: '{primaryPath}' still exists after deletion attempt.");
            }

            Console.WriteLine("[HandleQuickSettingAction] Quick Setting configuration state is clean. Ez3D-i will trigger Quick Setting on launch.");
            return true;
        }

        public static void Execute(QuickSettingScreen screen, Action<string>? onStepCallback = null, int timeoutSeconds = 30)
        {
            Console.WriteLine("[HandleQuickSettingAction] Starting Quick Setting automation workflow...");

            // =========================================================================
            // Step 1: Detect Quick Setting window & elevate past VS Code
            // =========================================================================
            var win = screen.WaitForQuickSettingWindow(timeoutSeconds);
            if (win == null)
            {
                throw new TimeoutException($"Failed to detect 'Quick Setting' window within {timeoutSeconds}s.");
            }

            IntPtr hwnd = WindowHelper.GetHwnd(win);
            onStepCallback?.Invoke("Step1_QuickSettingDisplayed");
            Thread.Sleep(600);

            // =========================================================================
            // Step 2: Click [ Search ] button
            // =========================================================================
            Console.WriteLine("[HandleQuickSettingAction] Step 2: Locating and clicking [ Search ] button...");
            var searchPt = screen.FindSearchButtonCoordinate(win);
            if (searchPt == null)
            {
                throw new InvalidOperationException("Failed to locate [ Search ] button coordinate on Quick Setting dialog.");
            }

            InputDriver.ForceWindowForeground(hwnd);
            Thread.Sleep(300);
            InputDriver.ClickAtPhysicalCoordinate(hwnd, searchPt.Value.X, searchPt.Value.Y);
            onStepCallback?.Invoke("Step2_SearchClicked");
            Thread.Sleep(800);

            // =========================================================================
            // Step 3: Wait for modal dialog "Searching the IP Address of file server"
            // =========================================================================
            Console.WriteLine("[HandleQuickSettingAction] Step 3: Waiting for 'Searching the IP Address of file server' dialog...");
            var searchWin = screen.WaitForSearchServerWindow(timeoutSeconds: 8);
            if (searchWin == null)
            {
                Console.WriteLine("[HandleQuickSettingAction] Search dialog not detected on first attempt. Refocusing and retrying [ Search ] click...");
                InputDriver.ForceWindowForeground(hwnd);
                Thread.Sleep(300);
                InputDriver.ClickAtPhysicalCoordinate(hwnd, searchPt.Value.X, searchPt.Value.Y);
                searchWin = screen.WaitForSearchServerWindow(timeoutSeconds: 15);
            }

            if (searchWin == null)
            {
                throw new TimeoutException("Failed to detect 'Searching the IP Address of file server' modal dialog.");
            }

            IntPtr searchHwnd = WindowHelper.GetHwnd(searchWin);
            onStepCallback?.Invoke("Step3_SearchDialogDisplayed");
            Thread.Sleep(600);

            // =========================================================================
            // Step 4: Select 127.0.0.1 in the server table
            // =========================================================================
            Console.WriteLine("[HandleQuickSettingAction] Step 4: Selecting '127.0.0.1' server row...");
            var ipPt = screen.FindIpRowCoordinate(searchWin, "127.0.0.1");
            if (ipPt == null)
            {
                throw new InvalidOperationException("Failed to locate '127.0.0.1' row coordinate in search server table.");
            }

            InputDriver.ClickAtPhysicalCoordinate(searchHwnd, ipPt.Value.X, ipPt.Value.Y);
            onStepCallback?.Invoke("Step4_127Selected");
            Thread.Sleep(600);

            // =========================================================================
            // Step 5: Click [ OK ] on the search dialog
            // =========================================================================
            Console.WriteLine("[HandleQuickSettingAction] Step 5: Clicking [ OK ] button on search dialog...");
            var okSearchPt = screen.FindOkButtonCoordinate(searchWin, isSearchDialog: true);
            if (okSearchPt == null)
            {
                throw new InvalidOperationException("Failed to locate [ OK ] button coordinate on search dialog.");
            }

            InputDriver.ClickAtPhysicalCoordinate(searchHwnd, okSearchPt.Value.X, okSearchPt.Value.Y);
            onStepCallback?.Invoke("Step5_SearchOkClicked");
            Thread.Sleep(1000);

            // =========================================================================
            // Step 6: Verify Server IP Address edit box displays 127.0.0.1
            // =========================================================================
            Console.WriteLine("[HandleQuickSettingAction] Step 6: Verifying Server IP Address edit box displays '127.0.0.1'...");
            InputDriver.ForceWindowForeground(hwnd);

            string displayedIp = "";
            var verifySw = Stopwatch.StartNew();
            while (verifySw.Elapsed.TotalSeconds < 8)
            {
                displayedIp = screen.GetServerIpAddressText(win);
                if (displayedIp.Contains("127.0.0.1"))
                {
                    break;
                }
                Thread.Sleep(400);
            }

            Console.WriteLine($"[HandleQuickSettingAction] Read Server IP Address: '{displayedIp}'");
            if (!displayedIp.Contains("127.0.0.1"))
            {
                throw new InvalidOperationException($"Server IP Address verification failed. Expected '127.0.0.1', but found: '{displayedIp}'");
            }

            onStepCallback?.Invoke("Step6_ServerIpVerified");
            Thread.Sleep(600);

            // =========================================================================
            // Step 7: Click [ OK ] on the main Quick Setting dialog
            // =========================================================================
            Console.WriteLine("[HandleQuickSettingAction] Step 7: Clicking [ OK ] button on Quick Setting dialog...");
            var okMainPt = screen.FindOkButtonCoordinate(win, isSearchDialog: false);
            if (okMainPt == null)
            {
                throw new InvalidOperationException("Failed to locate [ OK ] button coordinate on main Quick Setting dialog.");
            }

            InputDriver.ClickAtPhysicalCoordinate(hwnd, okMainPt.Value.X, okMainPt.Value.Y);
            onStepCallback?.Invoke("Step7_QuickSettingCompleted");
            Thread.Sleep(1200);

            Console.WriteLine("[HandleQuickSettingAction] PASS: Quick Setting configured successfully with Server IP 127.0.0.1.");
        }
    }
}
