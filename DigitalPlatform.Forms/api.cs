﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Forms
{
    public static class API
    {
        // SendMessage
        [DllImport("user32")]
        public static extern IntPtr
            SendMessage(IntPtr hWnd, uint Msg,
            UIntPtr wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern IntPtr
            SendMessage(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern IntPtr
            SendMessage(IntPtr hWnd, uint Msg,
            int wParam, int lParam);

        #region EM_?? 消息定义 和 Windows Edit 控件相关功能

        // EM_???
        public const int EM_GETSEL = 0x00b0;
        public const int EM_SETSEL = 0x00b1;
        public const int EM_LINESCROLL = 0x00B6;
        public const int EM_SCROLLCARET = 0x00B7;
        public const int EM_GETMODIFY = 0x00B8;
        public const int EM_SETMODIFY = 0x00B9;
        public const int EM_GETLINECOUNT = 0x00BA;
        public const int EM_LINEINDEX = 0x00bb;
        public const int EM_LINEFROMCHAR = 0x00c9;
        public const int EM_SETTABSTOPS = 0x00CB;

        public const int EM_GETFIRSTVISIBLELINE = 0x00CE;

        #endregion


        public const int WM_NCLBUTTONDOWN = 0x00a1;
        public const int WM_NCHITTEST = 0x0084;

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        [DllImport("User32", CharSet = CharSet.Auto)]
        public static extern int GetWindowLong(IntPtr hWnd, int Index);

        [DllImport("User32", CharSet = CharSet.Auto)]
        public static extern int SetWindowLong(IntPtr hWnd, int Index, int Value);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr handle);


        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public const int LVM_FIRST = 0x1000;
        public const int LVM_GETHEADER = (LVM_FIRST + 31);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(HandleRef hwnd, out RECT lpRect);



        /*
 * GetWindow() Constants
 */
        public const int GW_HWNDFIRST = 0;
        public const int GW_HWNDLAST = 1;
        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;
        public const int GW_OWNER = 4;
        public const int GW_CHILD = 5;
        public const int GW_ENABLEDPOPUP = 6;
        public const int GW_MAX = 6;

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindow",
     SetLastError = true)]
        public static extern IntPtr GetWindow(
    IntPtr hwnd,
    [MarshalAs(UnmanagedType.U4)] int wFlag);
    }

}
