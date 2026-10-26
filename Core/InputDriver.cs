using System;
using System.Threading;

namespace Ez3Di.Installer.Core
{
    public static class InputDriver
    {
        public static void PinWindowOnTop(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;
            try
            {
                if (NativeMethods.IsIconic(hwnd))
                    NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);

                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);

                uint thisThread   = NativeMethods.GetCurrentThreadId();
                uint targetThread = NativeMethods.GetWindowThreadProcessId(hwnd, out _);
                IntPtr fgWnd      = NativeMethods.GetForegroundWindow();
                uint fgThread     = fgWnd != IntPtr.Zero ? NativeMethods.GetWindowThreadProcessId(fgWnd, out _) : 0;

                if (fgThread != 0 && fgThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, fgThread, true);
                if (targetThread != 0 && targetThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, targetThread, true);

                NativeMethods.BringWindowToTop(hwnd);
                NativeMethods.SetForegroundWindow(hwnd);
                Thread.Sleep(200);

                if (targetThread != 0 && targetThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, targetThread, false);
                if (fgThread != 0 && fgThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, fgThread, false);
            }
            catch { }
        }

        public static void ForceWindowForeground(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;
            try
            {
                if (NativeMethods.IsIconic(hwnd))
                    NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);

                uint thisThread   = NativeMethods.GetCurrentThreadId();
                uint targetThread = NativeMethods.GetWindowThreadProcessId(hwnd, out _);
                IntPtr fgWnd      = NativeMethods.GetForegroundWindow();
                uint fgThread     = fgWnd != IntPtr.Zero ? NativeMethods.GetWindowThreadProcessId(fgWnd, out _) : 0;

                if (fgThread != 0 && fgThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, fgThread, true);
                if (targetThread != 0 && targetThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, targetThread, true);

                NativeMethods.BringWindowToTop(hwnd);
                NativeMethods.SetForegroundWindow(hwnd);
                Thread.Sleep(150);

                if (targetThread != 0 && targetThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, targetThread, false);
                if (fgThread != 0 && fgThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, fgThread, false);
            }
            catch { }
        }

        public static void ClickAtPhysicalCoordinate(IntPtr hwnd, int screenX, int screenY)
        {
            Console.WriteLine($"[InputDriver] Moving mouse & clicking physical coordinate ({screenX}, {screenY})...");

            if (hwnd != IntPtr.Zero)
                ForceWindowForeground(hwnd);

            NativeMethods.SetCursorPos(screenX, screenY);
            Thread.Sleep(150);

            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(100);
            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(300);
        }
    }
}
