using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalPlatform.Forms
{
    public static class TextBoxExtension
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd,
            int msg,
            int wParam,
            int[] lParam);

        public static void SetTabStops(
            this TextBox edit,
            int[] tabstops)
        {
            SendMessage(edit.Handle,
                API.EM_SETTABSTOPS,
                tabstops.Length,
                tabstops);
        }

        public static bool GetModify(this TextBox edit)
        {
            if ((int)API.SendMessage(edit.Handle,
                API.EM_GETMODIFY, 0, 0) == 0)
                return false;
            return true;
        }

        public static void SetModify(this TextBox edit,
            bool bModified)
        {
            API.SendMessage(edit.Handle,
                API.EM_SETMODIFY, Convert.ToInt32(bModified), 0);
        }

        public static int GetFirstVisibleLine(this TextBox edit)
        {
            return (int)API.SendMessage(edit.Handle,
                API.EM_GETFIRSTVISIBLELINE, 0, 0);
        }

        // 得到edit中行总数
        public static int GetLines(this TextBox edit)
        {
            /*
            // save the handle reference for the ExtToolBox
            // HandleRef hr = new HandleRef(this, edit.Handle );

            // Send the EM_LINEFROMCHAR message with the value of
            // -1 in wParam.
            // The return value is the zero-based line number 
            // of the line containing the caret.


            int nWholeLength = (int)SendMessage(edit.Handle,
                WM_GETTEXTLENGTH, 0, 0);

            return (int)SendMessage(edit.Handle,EM_LINEFROMCHAR, nWholeLength, 0) + 1;
            */

            return (int)API.SendMessage(edit.Handle,
                API.EM_GETLINECOUNT,
                UIntPtr.Zero, IntPtr.Zero);
        }

        public static void SetCurrentCaretPos(
            this TextBox edit,
            int x,
            int y,
            bool bScrollIntoView)
        {
            int nStart = (int)API.SendMessage(edit.Handle,
                API.EM_LINEINDEX,
                new UIntPtr((uint)y),
                IntPtr.Zero);

            API.SendMessage(edit.Handle,
                API.EM_SETSEL,
                nStart + x,
                nStart + x);

            if (bScrollIntoView == true)
            {
                API.SendMessage(edit.Handle,
                    API.EM_SCROLLCARET,
                    0,
                    0);
            }
        }

        // 得到edit caret当前行列位置
        public static void GetCurrentCaretPos(
            this TextBox edit,
            out int x,
            out int y)
        {
            // save the handle reference for the ExtToolBox
            // HandleRef hr = new HandleRef(this, edit.Handle );

            // Send the EM_LINEFROMCHAR message with the value of
            // -1 in wParam.
            // The return value is the zero-based line number 
            // of the line containing the caret.

            int l = (int)API.SendMessage(edit.Handle, API.EM_LINEFROMCHAR, new UIntPtr(0xffffffff), IntPtr.Zero);
            // Send the EM_GETSEL message to the ToolBox control.
            // The low-order word of the return value is the
            // character position of the caret relative to the
            // first character in the ToolBox control,
            // i.e. the absolute character index.
            int sel = (int)API.SendMessage(edit.Handle, API.EM_GETSEL, UIntPtr.Zero, IntPtr.Zero);
            // get the low-order word from sel
            int ai = sel & 0xffff;
            // Send the EM_LINEINDEX message with the value of -1
            // in wParam.
            // The return value is the number of characters that
            // precede the first character in the line containing
            // the caret.
            int li = (int)API.SendMessage(edit.Handle, API.EM_LINEINDEX, new UIntPtr(0xffffffff), IntPtr.Zero);
            // Subtract the li (line index) from the ai
            // (absolute character index),
            // The result is the column number of the caret position
            // in the line containing the caret.
            int c = ai - li;

            x = c;
            y = l;
            // cpt = new CharPoint(l+1,c+1);
        }

    }

}
