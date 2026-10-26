using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FlaUI.Core.AutomationElements;

namespace Ez3Di.Installer.Core
{
    public static class WindowHelper
    {
        public static string GetWindowTitle(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return "";
            var sb = new StringBuilder(512);
            NativeMethods.GetWindowText(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string GetClassName(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return "";
            var sb = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        /// Directly queries native Win32 child buttons of a dialog via EnumChildWindows.
        /// Bypasses UIA3 delays, DPI scaling, and tree traversal issues.
        /// </summary>
        public static Point? FindChildButtonCoordinate(IntPtr parentHwnd, params string[] buttonTexts)
        {
            if (parentHwnd == IntPtr.Zero) return null;

            Point? result = null;
            NativeMethods.EnumChildWindows(parentHwnd, (childHwnd, lParam) =>
            {
                string cls = GetClassName(childHwnd);
                if (cls.Equals("Button", StringComparison.OrdinalIgnoreCase))
                {
                    string text = GetWindowTitle(childHwnd).Trim();
                    int ctrlId = NativeMethods.GetDlgCtrlID(childHwnd);

                    foreach (var expected in buttonTexts)
                    {
                        bool match = text.Equals(expected, StringComparison.OrdinalIgnoreCase) ||
                                     text.Contains(expected, StringComparison.OrdinalIgnoreCase);

                        // If expected is Next or OK, ID 1 is standard Win32 IDOK / default button
                        if (!match && (expected.Contains("Next") || expected.Equals("OK", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (ctrlId == 1 && text.Contains("Next", StringComparison.OrdinalIgnoreCase))
                                match = true;
                        }

                        if (match)
                        {
                            if (NativeMethods.GetWindowRect(childHwnd, out NativeMethods.RECT r))
                            {
                                int cx = r.Left + (r.Right - r.Left) / 2;
                                int cy = r.Top + (r.Bottom - r.Top) / 2;
                                Console.WriteLine($"[WindowHelper] Win32 EnumChildWindows found button '{text}' (CtrlId: {ctrlId}) at ({cx}, {cy}).");
                                result = new Point(cx, cy);
                                return false; // Stop enumeration
                            }
                        }
                    }
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);

            return result;
        }

        public static Rectangle GetWindowBounds(Window window)
        {
            if (window == null) return new Rectangle(0, 0, 1920, 1080);

            IntPtr hwnd = GetHwnd(window);
            if (hwnd != IntPtr.Zero)
            {
                if (NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT rect))
                {
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;
                    return new Rectangle(rect.Left, rect.Top, width, height);
                }
            }

            try
            {
                var bounds = window.BoundingRectangle;
                return new Rectangle((int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height);
            }
            catch { }

            return new Rectangle(0, 0, 1920, 1080);
        }

        public static IntPtr GetHwnd(AutomationElement element)
        {
            if (element == null) return IntPtr.Zero;
            try { return element.FrameworkAutomationElement.NativeWindowHandle; } catch { }
            try { return element.Properties.NativeWindowHandle.ValueOrDefault; } catch { }
            return IntPtr.Zero;
        }

        public static Screen GetScreenForWindow(Window window)
        {
            IntPtr hwnd = GetHwnd(window);
            if (hwnd != IntPtr.Zero)
            {
                return Screen.FromHandle(hwnd);
            }

            var bounds = GetWindowBounds(window);
            return Screen.FromRectangle(bounds);
        }

        public static Screen GetScreenForHwnd(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
            {
                return Screen.FromHandle(hwnd);
            }
            return Screen.PrimaryScreen ?? Screen.AllScreens[0];
        }

        public static void CenterWindowOnMonitor(IntPtr hwnd, Screen? preferredScreen = null)
        {
            if (hwnd == IntPtr.Zero) return;
            try
            {
                var screen = preferredScreen ?? GetScreenForHwnd(hwnd);
                if (NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT rect))
                {
                    int winWidth = rect.Right - rect.Left;
                    int winHeight = rect.Bottom - rect.Top;
                    if (winWidth <= 0) winWidth = 503;
                    if (winHeight <= 0) winHeight = 388;

                    int centerX = screen.WorkingArea.Left + (screen.WorkingArea.Width - winWidth) / 2;
                    int centerY = screen.WorkingArea.Top + (screen.WorkingArea.Height - winHeight) / 2;

                    Console.WriteLine($"[WindowHelper] Centering window (0x{hwnd:X8}) on {screen.DeviceName} at ({centerX}, {centerY})...");
                    NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, centerX, centerY, winWidth, winHeight,
                        NativeMethods.SWP_SHOWWINDOW);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WindowHelper] Notice centering window: {ex.Message}");
            }
        }

        public static bool IsVisibleOnAnyScreen(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;
            if (!NativeMethods.IsWindowVisible(hwnd)) return false;

            if (NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT rect))
            {
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                if (width > 50 && height > 50)
                {
                    var windowRect = new Rectangle(rect.Left, rect.Top, width, height);
                    return windowRect.IntersectsWith(SystemInformation.VirtualScreen);
                }
            }

            return false;
        }
    }
}
