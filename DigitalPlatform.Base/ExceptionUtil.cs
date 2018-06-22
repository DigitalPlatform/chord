﻿using System;
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

        /*
*
.NET Runtime 	Error 	2016/11/4 10:24:46
Application: dp2capo.exe
Framework Version: v4.0.30319
Description: The process was terminated due to an unhandled exception.
Exception Info: System.NullReferenceException
Stack:
   at DigitalPlatform.ExceptionUtil.GetAggregateExceptionText(System.AggregateException)
   at DigitalPlatform.ExceptionUtil.GetAggregateExceptionText(System.AggregateException)
   at DigitalPlatform.ExceptionUtil.GetExceptionText(System.Exception)
   at dp2Capo.ServerInfo.Echo(dp2Capo.Instance, Boolean)
   at dp2Capo.ServerInfo.BackgroundWork()
   at dp2Capo.DefaultThread.Worker()
   at DigitalPlatform.ThreadBase.ThreadMain()
   at System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)
   at System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)
   at System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object)
   at System.Threading.ThreadHelper.ThreadStart()

         * */
        public static string GetExceptionText(Exception ex)
        {
            if (ex is AggregateException)
                return GetAggregateExceptionText(ex as AggregateException);

            return ex.GetType().ToString() + ":" + ex.Message + "\r\n"
                + ex.StackTrace.ToString();
        }

        public static string GetAggregateExceptionText(AggregateException exception)
        {
            if (exception == null) // 2016/11/4
                return "";
            if (exception.InnerExceptions == null) // 2016/11/4
                return "";

            StringBuilder text = new StringBuilder();
            foreach (Exception ex in exception.InnerExceptions)
            {
                if (ex == null) // 2016/11/4
                    continue;

                if (ex is AggregateException)   // 2016/7/5
                    text.Append(GetAggregateExceptionText(ex as AggregateException) + "\r\n");
                else
                {
                    // 2016/11/4 巩固代码
                    Type type = ex.GetType();
                    text.Append((type == null ? "" : type.ToString()) 
                        + ":"
                        + (ex.Message == null ? "" : ex.Message)
                        + (ex.StackTrace == null ? "" : "\r\n{stack-trace-begin}" + ex.StackTrace + "{stack-trace-end}")
                        + "\r\n");
                    // text.Append(ex.ToString() + "\r\n");
                }
            }

            return text.ToString();
        }

        public static Exception FindInnerException(AggregateException exception,
            Type innerType)
        {
            StringBuilder text = new StringBuilder();
            foreach (Exception ex in exception.InnerExceptions)
            {
                if (ex == null) // 2016/11/4
                    continue;
                if (ex.GetType().Equals(innerType))
                    return ex;
            }

            return null;
        }
    }

}
