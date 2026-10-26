using System;
using System.Drawing;
using FlaUI.Core.AutomationElements;

namespace Ez3Di.Installer.Core
{
    public static class WindowHelper
    {
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
    }
}
