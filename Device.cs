using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Image_and_Video_Processor
{
    public class Device
    {
        private const int WM_CAP = 0x400;
        private const int WM_CAP_DRIVER_CONNECT = 0x40a;
        private const int WM_CAP_DRIVER_DISCONNECT = 0x40b;
        private const int WM_CAP_EDIT_COPY = WM_CAP + 30;
        private const int WM_CAP_SET_PREVIEW = 0x432;
        private const int WM_CAP_SET_OVERLAY = 0x433;
        private const int WM_CAP_SET_PREVIEWRATE = 0x434;
        private const int WM_CAP_SET_SCALE = 0x435;
        private const int WS_CHILD = 0x40000000;
        private const int WS_VISIBLE = 0x10000000;

        [DllImport("avicap32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr capCreateCaptureWindowA(
            string lpszWindowName,
            int dwStyle, int x, int y,
            int nWidth, int nHeight,
            IntPtr hWndParent, int nID);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter,
            int x, int y, int cx, int cy, int wFlags);

        private int index;
        private IntPtr deviceHandle = IntPtr.Zero;

        public Device() { }

        public Device(int index)
        {
            this.index = index;
        }

        public string Name { get; set; }
        public string Version { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        public void Init(int windowHeight, int windowWidth, IntPtr parentHandle)
        {
            deviceHandle = capCreateCaptureWindowA("CaptureWindow",
                WS_VISIBLE | WS_CHILD,
                0, 0, windowWidth, windowHeight,
                parentHandle, 0);

            if (deviceHandle != IntPtr.Zero)
            {
                if (SendMessage(deviceHandle, WM_CAP_DRIVER_CONNECT, index, 0) > 0)
                {
                    SendMessage(deviceHandle, WM_CAP_SET_SCALE, 1, 0);
                    SendMessage(deviceHandle, WM_CAP_SET_PREVIEWRATE, 66, 0); // ~15 fps
                    SendMessage(deviceHandle, WM_CAP_SET_PREVIEW, 1, 0);

                    SetWindowPos(deviceHandle, 1, 0, 0, windowWidth, windowHeight, 6);
                }
            }
        }

        public void ShowWindow(Control windowsControl)
        {
            Init(windowsControl.Height, windowsControl.Width, windowsControl.Handle);
        }

        public void Stop()
        {
            if (deviceHandle != IntPtr.Zero)
            {
                SendMessage(deviceHandle, WM_CAP_DRIVER_DISCONNECT, index, 0);
                DestroyWindow(deviceHandle);
                deviceHandle = IntPtr.Zero;
            }
        }

        public void CopyFrameToClipboard()
        {
            if (deviceHandle != IntPtr.Zero)
            {
                SendMessage(deviceHandle, WM_CAP_EDIT_COPY, 0, 0);
            }
        }
    }
}
