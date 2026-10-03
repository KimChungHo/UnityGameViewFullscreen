#if UNITY_EDITOR_WIN
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UltimateTrpgSimulator.Editor.GameViewFullscreen
{
    // Win32 상수는 데이터 값이 아니라 운영체제 ABI입니다.
    internal static class GameViewFullscreenNative
    {
        private const int WindowStyleIndex = -16;
        private const int Caption = 0x00C00000;
        private const int ThickFrame = 0x00040000;
        private const int Maximized = 0x01000000;
        private const uint PositionFlags = 0x0004 | 0x0010 | 0x0020 | 0x0400; // NOZORDER, NOACTIVATE, FRAMECHANGED, NOSENDCHANGING
        private const uint MonitorNearest = 2;
        private const float WindowsLogicalDpi = 96f;

        internal sealed class WindowSnapshot
        {
            internal IntPtr Handle;
            internal int Style;
            internal NativeRect Bounds;
            internal WindowPlacement Placement;
            internal IntPtr OriginalRegion;
            internal Rect Monitor;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativePoint { internal int x, y; }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeRect
        {
            internal int left, top, right, bottom;
            internal Rect ToRect() => new(left, top, right - left, bottom - top);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowPlacement
        {
            internal int length, flags, showCommand;
            internal NativePoint minimizedPosition, maximizedPosition;
            internal NativeRect normalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInfo
        {
            internal int size;
            internal NativeRect monitor, workArea;
            internal uint flags;
        }

        internal static IntPtr MainWindowHandle
        {
            get
            {
                using (Process process = Process.GetCurrentProcess())
                    return process.MainWindowHandle;
            }
        }

        internal static bool IsKeyDown(int virtualKey) => GetAsyncKeyState(virtualKey) < 0;
        internal static bool HasForeground(IntPtr handle) => handle != IntPtr.Zero && GetForegroundWindow() == handle;
        internal static IntPtr ForegroundWindow => GetForegroundWindow();
        internal static float GetScale(IntPtr handle) => GetDpiForWindow(handle) / WindowsLogicalDpi;

        internal static Rect ReadBounds(IntPtr handle)
        {
            Check(GetWindowRect(handle, out NativeRect bounds));
            return bounds.ToRect();
        }

        internal static WindowSnapshot Capture()
        {
            IntPtr handle = MainWindowHandle;
            Check(handle != IntPtr.Zero && IsWindow(handle));
            var placement = new WindowPlacement { length = Marshal.SizeOf<WindowPlacement>() };
            Check(GetWindowPlacement(handle, ref placement));
            Check(GetWindowRect(handle, out NativeRect bounds));
            var monitor = new MonitorInfo { size = Marshal.SizeOf<MonitorInfo>() };
            Check(GetMonitorInfo(MonitorFromWindow(handle, MonitorNearest), ref monitor));
            IntPtr region = CreateRectRgn(0, 0, 0, 0);
            Check(region != IntPtr.Zero);
            if (GetWindowRgn(handle, region) == 0)
            {
                DeleteObject(region);
                region = IntPtr.Zero;
            }
            return new WindowSnapshot
            {
                Handle = handle, Style = GetWindowLong(handle, WindowStyleIndex), Bounds = bounds,
                Placement = placement, OriginalRegion = region, Monitor = monitor.monitor.ToRect()
            };
        }

        internal static void MakeBorderless(WindowSnapshot snapshot)
        {
            SetStyle(snapshot.Handle, snapshot.Style & ~(Caption | ThickFrame | Maximized));
            Move(snapshot.Handle, snapshot.Monitor);
        }

        internal static void Move(IntPtr handle, Rect bounds)
        {
            Check(SetWindowPos(handle, IntPtr.Zero, Mathf.RoundToInt(bounds.x), Mathf.RoundToInt(bounds.y),
                Mathf.RoundToInt(bounds.width), Mathf.RoundToInt(bounds.height), PositionFlags));
        }

        internal static void CropToMonitor(WindowSnapshot snapshot)
        {
            Rect window = ReadBounds(snapshot.Handle);
            Rect monitor = snapshot.Monitor;
            IntPtr region = CreateRectRgn(Mathf.RoundToInt(monitor.x - window.x),
                Mathf.RoundToInt(monitor.y - window.y), Mathf.RoundToInt(monitor.xMax - window.x),
                Mathf.RoundToInt(monitor.yMax - window.y));
            Check(region != IntPtr.Zero);
            if (SetWindowRgn(snapshot.Handle, region, true) == 0)
            {
                DeleteObject(region);
                Check(false);
            }
            // 성공하면 영역 핸들의 소유권은 Windows로 이전됩니다.
        }

        internal static void Restore(WindowSnapshot snapshot)
        {
            try
            {
                if (!IsWindow(snapshot.Handle))
                    return;
                Check(SetWindowRgn(snapshot.Handle, snapshot.OriginalRegion, true) != 0);
                snapshot.OriginalRegion = IntPtr.Zero;
                // 원래의 일반 창 위치를 복원하면서 다른 앱의 포커스를 가져오지 않습니다.
                WindowPlacement placement = snapshot.Placement;
                placement.showCommand = 4; // SW_SHOWNOACTIVATE
                Check(SetWindowPlacement(snapshot.Handle, ref placement));
                SetStyle(snapshot.Handle, snapshot.Style);
                Move(snapshot.Handle, snapshot.Bounds.ToRect());
            }
            finally
            {
                if (snapshot.OriginalRegion != IntPtr.Zero)
                {
                    DeleteObject(snapshot.OriginalRegion);
                    snapshot.OriginalRegion = IntPtr.Zero;
                }
            }
        }

        private static void SetStyle(IntPtr handle, int style)
        {
            SetLastError(0);
            int previous = SetWindowLong(handle, WindowStyleIndex, style);
            Check(previous != 0 || Marshal.GetLastWin32Error() == 0);
        }

        private static void Check(bool success)
        {
            if (!success)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        [DllImport("kernel32.dll")] private static extern void SetLastError(uint error);
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int key);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr handle);
        [DllImport("user32.dll")] private static extern uint GetDpiForWindow(IntPtr handle);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool GetWindowRect(IntPtr handle, out NativeRect rect);
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr handle, int index);
        [DllImport("user32.dll", SetLastError = true)] private static extern int SetWindowLong(IntPtr handle, int index, int value);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool SetWindowPos(IntPtr handle, IntPtr after, int x, int y, int width, int height, uint flags);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool GetWindowPlacement(IntPtr handle, ref WindowPlacement placement);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool SetWindowPlacement(IntPtr handle, ref WindowPlacement placement);
        [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);
        [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr handle);
        [DllImport("user32.dll")] private static extern int GetWindowRgn(IntPtr handle, IntPtr region);
        [DllImport("user32.dll", SetLastError = true)] private static extern int SetWindowRgn(IntPtr handle, IntPtr region, bool redraw);
    }
}
#endif
