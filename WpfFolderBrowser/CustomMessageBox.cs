﻿using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace WpfFolderBrowser
{
    public static class CustomMessageBox
    {
        static class Win32Native
        {
            [DllImport("user32.dll")]
            static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
            const int GWL_HINSTANCE = -6;

            public static IntPtr GetWindowHInstance(IntPtr hWnd) => GetWindowLong(hWnd, GWL_HINSTANCE);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetCurrentThreadId();

            [DllImport("user32.dll")]
            static extern IntPtr SetWindowsHookEx(int idHook, HOOKPROC lpfn, IntPtr hInstance, IntPtr threadId);
            const int WH_CBT = 5;
            public static IntPtr SetWindowsHookEx(HOOKPROC lpfn, IntPtr hInstance, IntPtr threadId) => SetWindowsHookEx(WH_CBT, lpfn, hInstance, threadId);

            [DllImport("user32.dll")]
            public static extern bool UnhookWindowsHookEx(IntPtr hHook);
            [DllImport("user32.dll")]
            public static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, IntPtr lParam);

            public delegate IntPtr HOOKPROC(int nCode, IntPtr wParam, IntPtr lParam);
            public const int HCBT_ACTIVATE = 5;

            [DllImport("user32.dll")]
            static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
            public struct RECT
            {
                public int Left, Top, Right, Bottom;
                public int Width => this.Right - this.Left;
                public int Height => this.Bottom - this.Top;
            }

            public static RECT GetWindowRect(IntPtr hWnd)
            {
                RECT rc;
                GetWindowRect(hWnd, out rc);
                return rc;
            }

            [DllImport("user32.dll")]
            static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOZORDER = 0x0004;
            const uint SWP_NOACTIVATE = 0x0010;

            public static bool SetWindowPos(IntPtr hWnd, int x, int y)
            {
                var flags = SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE;
                return SetWindowPos(hWnd, 0, x, y, 0, 0, flags);
            }
        }

        public static DialogResult Show(System.Windows.Forms.IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            hOwner = owner.Handle;
            var hInstance = Win32Native.GetWindowHInstance(owner.Handle);
            var threadId = Win32Native.GetCurrentThreadId();
            hHook = Win32Native.SetWindowsHookEx(new Win32Native.HOOKPROC(HookProc), hInstance, threadId);
            return System.Windows.Forms.MessageBox.Show(owner, text, caption, buttons, icon);
        }

        public static MessageBoxResult Show(string text)
        {
            return Show(text, "メッセージ");
        }

        public static MessageBoxResult Show(string text, string caption)
        {
            return Show(null, text, caption);
        }

        public static MessageBoxResult Show(Window owner, string text, string caption)
        {
            return Show(owner, text, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK, System.Windows.MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, System.Windows.MessageBoxOptions options)
        {
            if (owner != null)
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(owner);
                hOwner = helper.Handle;
                var hInstance = Win32Native.GetWindowHInstance(helper.Handle);
                var threadId = Win32Native.GetCurrentThreadId();
                hHook = Win32Native.SetWindowsHookEx(new Win32Native.HOOKPROC(HookProc), hInstance, threadId);
                return System.Windows.MessageBox.Show(owner, text, caption, button, icon, defaultResult, options);
            }
            else
            {
                return System.Windows.MessageBox.Show(text, caption, button, icon, defaultResult, options);
            }
        }

        private static IntPtr hOwner = (IntPtr)0;
        private static IntPtr hHook = (IntPtr)0;
        private static IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == Win32Native.HCBT_ACTIVATE)
            {
                var rcOwner = Win32Native.GetWindowRect(hOwner);
                var rcMsgBox = Win32Native.GetWindowRect(wParam);

                var x = rcOwner.Left + (rcOwner.Width - rcMsgBox.Width) / 2;
                var y = rcOwner.Top + (rcOwner.Height - rcMsgBox.Height) / 2;
                Win32Native.SetWindowPos(wParam, x, y);

                Win32Native.UnhookWindowsHookEx(hHook);
                hHook = (IntPtr)0;
            }
            return Win32Native.CallNextHookEx(hHook, nCode, wParam, lParam);
        }
    }
}
