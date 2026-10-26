using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Ez3Di.Installer.Core;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using Button = FlaUI.Core.AutomationElements.Button;
using TextBox = FlaUI.Core.AutomationElements.TextBox;

namespace Ez3Di.Installer.Screens
{
    public class QuickSettingScreen
    {
        public AutomationBase Automation { get; }
        public Window? QuickSettingWindow { get; private set; }
        public Window? SearchServerWindow { get; private set; }

        public QuickSettingScreen(AutomationBase automation)
        {
            Automation = automation;
        }

        /// <summary>
        /// Waits for and captures the "Quick Setting" window.
        /// Unconditionally enforces Z-order elevation, anti-hide past VS Code, and screen centering.
        /// </summary>
        public Window? WaitForQuickSettingWindow(int timeoutSeconds = 30)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine($"[QuickSettingScreen] Waiting up to {timeoutSeconds}s for 'Quick Setting' window (No taskbar icon)...");

            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                try
                {
                    // 1. Instant Win32 FindWindow lookup
                    IntPtr hwnd = NativeMethods.FindWindow(null, "Quick Setting");
                    if (hwnd == IntPtr.Zero)
                        hwnd = NativeMethods.FindWindow(null, "Quick Settings");

                    // 2. Win32 EnumWindows scan if exact title FindWindow didn't match
                    if (hwnd == IntPtr.Zero)
                    {
                        hwnd = FindTopLevelWindowByTitlePrefix("Quick Setting");
                    }

                    if (hwnd != IntPtr.Zero && WindowHelper.IsVisibleOnAnyScreen(hwnd))
                    {
                        Console.WriteLine($"[QuickSettingScreen] Found 'Quick Setting' window (HWND: 0x{hwnd:X8}). Elevating past VS Code...");
                        
                        // Enforce anti-hide and elevation
                        InputDriver.PinWindowOnTop(hwnd);
                        InputDriver.DemoteVsCodeZOrder(hwnd);
                        InputDriver.ForceWindowForeground(hwnd);
                        WindowHelper.CenterWindowOnMonitor(hwnd);

                        try
                        {
                            QuickSettingWindow = Automation.FromHandle(hwnd).AsWindow();
                            return QuickSettingWindow;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[QuickSettingScreen] FlaUI wrap warning: {ex.Message}. Retrying...");
                        }
                    }

                    // 3. Desktop FlaUI fallback search
                    var desktop = Automation.GetDesktop();
                    var windows = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window));
                    foreach (var w in windows)
                    {
                        try
                        {
                            string name = w.Name ?? "";
                            if (name.StartsWith("Quick Setting", StringComparison.OrdinalIgnoreCase))
                            {
                                IntPtr h = WindowHelper.GetHwnd(w);
                                if (h != IntPtr.Zero)
                                {
                                    InputDriver.PinWindowOnTop(h);
                                    InputDriver.DemoteVsCodeZOrder(h);
                                    InputDriver.ForceWindowForeground(h);
                                    WindowHelper.CenterWindowOnMonitor(h);
                                    QuickSettingWindow = w.AsWindow();
                                    return QuickSettingWindow;
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                Thread.Sleep(500);
            }

            return null;
        }

        /// <summary>
        /// Waits for the modal "Searching the IP Address of file server" dialog.
        /// </summary>
        public Window? WaitForSearchServerWindow(int timeoutSeconds = 15)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine($"[QuickSettingScreen] Waiting up to {timeoutSeconds}s for 'Searching the IP Address of file server' dialog...");

            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                try
                {
                    IntPtr hwnd = NativeMethods.FindWindow(null, "Searching the IP Address of file server");
                    if (hwnd == IntPtr.Zero)
                    {
                        hwnd = FindTopLevelWindowByTitlePrefix("Searching the IP Address");
                    }

                    if (hwnd != IntPtr.Zero && WindowHelper.IsVisibleOnAnyScreen(hwnd))
                    {
                        Console.WriteLine($"[QuickSettingScreen] Found Search Server dialog (HWND: 0x{hwnd:X8}). Elevating...");
                        InputDriver.PinWindowOnTop(hwnd);
                        InputDriver.ForceWindowForeground(hwnd);

                        try
                        {
                            SearchServerWindow = Automation.FromHandle(hwnd).AsWindow();
                            return SearchServerWindow;
                        }
                        catch { }
                    }

                    // Fallback FlaUI lookup
                    var desktop = Automation.GetDesktop();
                    var match = desktop.FindFirstChild(cf => cf.ByName("Searching the IP Address of file server"));
                    if (match != null)
                    {
                        SearchServerWindow = match.AsWindow();
                        return SearchServerWindow;
                    }
                }
                catch { }

                Thread.Sleep(400);
            }

            return null;
        }

        /// <summary>
        /// Locates the [ Search ] button on the Quick Setting window.
        /// Priority:
        /// 1. Visual OpenCV Template Match (btn_search.png)
        /// 2. Native Win32 child button search
        /// 3. FlaUI UIA3 lookup
        /// 4. Pixel-accurate Geometric Fallback (X = r.Left + 154, Y = r.Top + 188)
        /// </summary>
        public Point? FindSearchButtonCoordinate(Window win)
        {
            IntPtr hwnd = WindowHelper.GetHwnd(win);

            // 1. Visual OpenCV Template Match (btn_search.png)
            try
            {
                if (ImageCompare.See("btn_search.png", threshold: 0.80))
                {
                    var ptVis = ImageCompare.Find("btn_search.png", threshold: 0.80);
                    if (ptVis != null)
                    {
                        Console.WriteLine($"[QuickSettingScreen] Visual template match found 'btn_search.png' at ({ptVis.Value.X}, {ptVis.Value.Y}).");
                        return ptVis;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QuickSettingScreen] Visual search warning: {ex.Message}");
            }

            // 2. Native Win32 child button search
            var pt = WindowHelper.FindChildButtonCoordinate(hwnd, "Search", "&Search");
            if (pt != null) return pt;

            // 3. FlaUI UIA3 lookup
            try
            {
                var btn = win.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByName("Search")))?.AsButton();
                if (btn != null && !btn.IsOffscreen)
                {
                    var rect = btn.BoundingRectangle;
                    return new Point((int)(rect.Left + rect.Width / 2), (int)(rect.Top + rect.Height / 2));
                }
            }
            catch { }

            // 4. Pixel-accurate Geometric Fallback:
            // Quick Setting Dialog is ~582x749.
            // Server IP Address edit box is at [27, 140, 280, 162] (center: 154, 151).
            // Search button is at [27, 175, 281, 200] (center: 154, 188).
            if (hwnd != IntPtr.Zero && NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT r))
            {
                int btnX = r.Left + 154;
                int btnY = r.Top + 188;
                Console.WriteLine($"[QuickSettingScreen] Using calibrated geometric coordinate for [ Search ] button at ({btnX}, {btnY}).");
                return new Point(btnX, btnY);
            }

            return null;
        }

        /// <summary>
        /// Selects the IP Address row (e.g. 127.0.0.1) in the search server dialog.
        /// </summary>
        public Point? FindIpRowCoordinate(Window searchWin, string targetIp = "127.0.0.1")
        {
            IntPtr hwnd = WindowHelper.GetHwnd(searchWin);

            // 1. UIA search for text containing target IP
            try
            {
                var item = searchWin.FindFirstDescendant(cf => cf.ByName(targetIp));
                if (item != null && !item.IsOffscreen)
                {
                    var rect = item.BoundingRectangle;
                    int cx = (int)(rect.Left + rect.Width / 2);
                    int cy = (int)(rect.Top + rect.Height / 2);
                    Console.WriteLine($"[QuickSettingScreen] Located IP '{targetIp}' via UIA element at ({cx}, {cy}).");
                    return new Point(cx, cy);
                }
            }
            catch { }

            // 2. Calibrated geometric fallback: First row in the table (127.0.0.1 is row 1)
            // Dialog size ~412x340. Row 1 center is at (r.Left + 104, r.Top + 86).
            if (hwnd != IntPtr.Zero && NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT r))
            {
                int rowX = r.Left + 104;
                int rowY = r.Top + 86;
                Console.WriteLine($"[QuickSettingScreen] Using calibrated geometric coordinate for row '{targetIp}' at ({rowX}, {rowY}).");
                return new Point(rowX, rowY);
            }

            return null;
        }

        /// <summary>
        /// Locates the [ OK ] button on either the Quick Setting or Search Server dialog.
        /// </summary>
        public Point? FindOkButtonCoordinate(Window win, bool isSearchDialog = false)
        {
            IntPtr hwnd = WindowHelper.GetHwnd(win);

            // 1. Native Win32 child button search
            var pt = WindowHelper.FindChildButtonCoordinate(hwnd, "OK", "&OK");
            if (pt != null) return pt;

            // 2. FlaUI UIA3 lookup
            try
            {
                var btn = win.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByName("OK")))?.AsButton();
                if (btn != null && !btn.IsOffscreen)
                {
                    var rect = btn.BoundingRectangle;
                    return new Point((int)(rect.Left + rect.Width / 2), (int)(rect.Top + rect.Height / 2));
                }
            }
            catch { }

            // 3. Calibrated geometric fallback:
            // Search Dialog: center of OK button is (r.Left + 150, r.Top + 304)
            // Main Quick Setting Dialog: center of OK button is (r.Left + 200, r.Bottom - 22)
            if (hwnd != IntPtr.Zero && NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT r))
            {
                int okX = isSearchDialog ? r.Left + 150 : r.Left + 200;
                int okY = isSearchDialog ? r.Top + 304 : r.Bottom - 22;
                Console.WriteLine($"[QuickSettingScreen] Using calibrated geometric coordinate for [ OK ] button at ({okX}, {okY}).");
                return new Point(okX, okY);
            }

            return null;
        }

        /// <summary>
        /// Reads the text currently displayed in the Server IP Address edit box.
        /// </summary>
        public string GetServerIpAddressText(Window win)
        {
            try
            {
                var edits = win.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit));
                if (edits != null && edits.Length > 0)
                {
                    // The first edit box is Server IP Address (the second is Port 43112)
                    var serverIpEdit = edits[0].AsTextBox();
                    string text = serverIpEdit.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        return text.Trim();
                    }
                }
            }
            catch { }

            return "";
        }

        private static IntPtr FindTopLevelWindowByTitlePrefix(string prefix)
        {
            IntPtr matchedHwnd = IntPtr.Zero;

            NativeMethods.EnumWindows((hwnd, lParam) =>
            {
                string title = WindowHelper.GetWindowTitle(hwnd);
                if (!string.IsNullOrEmpty(title) && title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    string cls = WindowHelper.GetClassName(hwnd);
                    if (!cls.Contains("Chrome") && !cls.Contains("Code") && !title.Contains("Visual Studio"))
                    {
                        matchedHwnd = hwnd;
                        return false; // stop enumeration
                    }
                }
                return true;
            }, IntPtr.Zero);

            return matchedHwnd;
        }
    }
}
