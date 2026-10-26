using System;
using System.Diagnostics;
using System.Threading;

namespace Ez3Di.Installer.Core
{
    public static class InputDriver
    {
        /// <summary>
        /// Pins the installer window as HWND_TOPMOST and forces it to the foreground,
        /// bypassing Windows foreground locks to ensure it is never hidden behind VS Code.
        /// </summary>
        public static void PinWindowOnTop(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;
            try
            {
                if (NativeMethods.IsIconic(hwnd))
                    NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);

                // 1. Bypass Windows Foreground Lock
                NativeMethods.LockSetForegroundWindow(NativeMethods.LSFW_UNLOCK);
                NativeMethods.AllowSetForegroundWindow(NativeMethods.ASFW_ANY);

                // Pulse the ALT key to reset Windows foreground lock timer
                NativeMethods.keybd_event(NativeMethods.VK_MENU, 0, 0, UIntPtr.Zero);
                NativeMethods.keybd_event(NativeMethods.VK_MENU, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

                // 2. Attach thread input from current thread and active foreground thread
                uint thisThread   = NativeMethods.GetCurrentThreadId();
                uint targetThread = NativeMethods.GetWindowThreadProcessId(hwnd, out _);
                IntPtr fgWnd      = NativeMethods.GetForegroundWindow();
                uint fgThread     = fgWnd != IntPtr.Zero ? NativeMethods.GetWindowThreadProcessId(fgWnd, out _) : 0;

                if (fgThread != 0 && fgThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, fgThread, true);
                if (targetThread != 0 && targetThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, targetThread, true);

                // 3. Bring window to absolute top and set TOPMOST
                NativeMethods.SwitchToThisWindow(hwnd, true);
                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);

                NativeMethods.BringWindowToTop(hwnd);
                NativeMethods.SetForegroundWindow(hwnd);
                Thread.Sleep(150);

                if (targetThread != 0 && targetThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, targetThread, false);
                if (fgThread != 0 && fgThread != thisThread)
                    NativeMethods.AttachThreadInput(thisThread, fgThread, false);

                // 4. Demote any VS Code (Code.exe) window sharing the same display to ensure it stays behind
                DemoteVsCodeZOrder(hwnd);
            }
            catch { }
        }

        /// <summary>
        /// Demotes any running VS Code windows from the topmost layer to prevent them
        /// from obscuring the installer wizard.
        /// </summary>
        public static void DemoteVsCodeZOrder(IntPtr installerHwnd)
        {
            try
            {
                var codeProcs = Process.GetProcessesByName("Code");
                foreach (var proc in codeProcs)
                {
                    IntPtr vHwnd = proc.MainWindowHandle;
                    if (vHwnd != IntPtr.Zero && vHwnd != installerHwnd)
                    {
                        NativeMethods.SetWindowPos(vHwnd, NativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0,
                            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
                    }
                }
            }
            catch { }
        }

        public static void ForceWindowForeground(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;
            PinWindowOnTop(hwnd);
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
