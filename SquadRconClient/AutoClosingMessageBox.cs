using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace SquadRconClient
{
    public class AutoClosingMessageBox
    {
        private System.Threading.Timer _timeoutTimer;
        private string _caption;
        private const int WM_CLOSE = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "MoveWindow")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern bool MoveWindow([System.Runtime.InteropServices.InAttribute()] IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)] bool bRepaint);


        AutoClosingMessageBox(string text, string caption, int timeout, MessageBoxButton button, MessageBoxImage error)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            using (_timeoutTimer)
            {
                MessageBox.Show(text, caption);
            }
        }

        public static void Show(string text, string caption, int timeout, MessageBoxButton button, MessageBoxImage error)
        {
            //Dictionary<string, object> data = new Dictionary<string, object>(1);
            //data["Caption"] = caption;
            //CreateParallelTimer(50, data).Start();
            new AutoClosingMessageBox(text, caption, timeout, button, error);
        }

        private static MoveTimedEvent CreateParallelTimer(int timeoutDelay, Dictionary<string, object> args)
        {
            MoveTimedEvent timedEvent = new MoveTimedEvent(timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += Move;
            return timedEvent;
        }

        private static void Move(MoveTimedEvent evt)
        {
            evt.Kill();
            var data = evt.Args;
            RECT rect = new RECT();
            RECT mainwindow = new RECT();

            IntPtr mbWnd = FindWindow("#32770", (string)data["Caption"]); // lpClassName is #32770 for MessageBox
            IntPtr mainwindowhandle = FindWindow(null, "SquadRconClient");
            if (mbWnd != IntPtr.Zero && mainwindowhandle != IntPtr.Zero)
            {
                GetWindowRect(mbWnd, ref rect);
                GetWindowRect(mainwindowhandle, ref mainwindow);
                MoveWindow(mbWnd, (rect.Right - rect.Left) * 2, (rect.Bottom - rect.Top) * 2, rect.Right - rect.Left, rect.Bottom - rect.Top, true);
            }
        }

        private void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
            if (mbWnd != IntPtr.Zero)
            {
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            _timeoutTimer.Dispose();
        }
    }
}
