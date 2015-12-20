using System;
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

    }

}
