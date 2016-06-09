using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform
{
    /// <summary>
    /// 和异常处理有关的实用函数
    /// </summary>
    public static class ExceptionUtil
    {
        // 2015/9/30
        // 如果 Exceptoin 为 NullException 类型，则返回详细调用堆栈；否则只返回 e.Message 信息
        public static string GetAutoText(Exception e)
        {
            if (e is NullReferenceException)
                return GetDebugText(e);
            return e.Message;
        }

        // 返回详细调用堆栈
        public static string GetDebugText(Exception e)
        {
            StringBuilder message = new StringBuilder();

            Exception currentException = null;
            for (currentException = e; currentException != null; currentException = currentException.InnerException)
            {
                message.AppendFormat("Type: {0}\r\nMessage: {1}\r\nStack:\r\n{2}\r\n\r\n",
                                     currentException.GetType().FullName,
                                     currentException.Message,
                                     currentException.StackTrace);
            }

            return message.ToString();
        }

        public static string GetStackTraceText(StackTrace st)
        {
            string strText = "";
            StackFrame[] frames = st.GetFrames();

            for (int i = 0; i < frames.Length; i++)
            {
                StackFrame frame = frames[i];
                strText += frame.ToString() + "\r\n";
            }

            return strText;
        }

        public static string GetExceptionMessage(Exception ex)
        {
            string strResult = ex.GetType().ToString() + ":" + ex.Message;
            while (ex != null)
            {
                if (ex.InnerException != null)
                    strResult += "\r\n" + ex.InnerException.GetType().ToString() + ": " + ex.InnerException.Message;

                ex = ex.InnerException;
            }

            return strResult;
        }

        public static string GetExceptionText(Exception ex)
        {
            if (ex is AggregateException)
                return GetAggregateExceptionText(ex as AggregateException);

            return ex.GetType().ToString() + ":" + ex.Message + "\r\n"
                + ex.StackTrace;
        }

        public static string GetAggregateExceptionText(AggregateException exception)
        {
            StringBuilder text = new StringBuilder();
            foreach (Exception ex in exception.InnerExceptions)
            {
                text.Append(ex.GetType().ToString() + ":" + ex.Message + "\r\n");
                // text.Append(ex.ToString() + "\r\n");
            }

            return text.ToString();
        }
    }

}
