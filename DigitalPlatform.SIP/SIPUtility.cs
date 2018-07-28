using DigitalPlatform.SIP2.Request;
using DigitalPlatform.SIP2.Response;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

//[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace DigitalPlatform.SIP2
{
    public class SIPUtility
    {
        //统一用一个专门的LogManager
        //// public static ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //public static ILog Logger = log4net.LogManager.GetLogger("dp2SIPLogging");
        //public static void WriteLog(string message)
        //{
        //    Logger.Info(message);
        //}

        /// <summary>
        /// 将消息字符串 解析 成对应的消息对象
        /// </summary>
        /// <param name="cmdText">消息字符串</param>
        /// <param name="message">解析后的消息对象</param>
        /// <param name="error">氏族</param>
        /// <returns>
        /// true 成功
        /// false 出错
        /// </returns>
        public static int ParseMessage(string cmdText, out BaseMessage message, out string error)
        {
            message = new BaseMessage();
            error = "";

            if (cmdText.Length < 2)
            {
                error = "命令长度不够2位";
                return -1;
            }

            string cmdIdentifiers = cmdText.Substring(0, 2);
            //text = text.Substring(2);
            switch (cmdIdentifiers)
            {
                case "93":
                    {
                        message = new Login_93();
                        break;
                    }
                case "94":
                    {
                        message = new LoginResponse_94();
                        break;
                    }
                case "99":
                    {
                        message = new SCStatus_99();
                        break;
                    }
                case "98":
                    {
                        message = new ACSStatus_98();
                        break;
                    }
                case "11":
                    {
                        message = new Checkout_11();
                        break;
                    }
                case "12":
                    {
                        message = new CheckoutResponse_12();
                        break;
                    }
                case "09":
                    {
                        message = new Checkin_09();
                        break;
                    }
                case "10":
                    {
                        message = new CheckinResponse_10();
                        break;
                    }
                case "63":
                    {
                        message = new PatronInformation_63();
                        break;
                    }
                case "64":
                    {
                        message = new  PatronInformationResponse_64 ();
                        break;
                    }
                case "35":
                    {
                        message = new EndPatronSession_35();
                        break;
                    }
                case "36":
                    {
                        message = new EndSessionResponse_36();
                        break;
                    }
                case "17":
                    {
                        message = new ItemInformation_17();
                        break;
                    }
                case "18":
                    {
                        message = new ItemInformationResponse_18();
                        break;
                    }
                case "29":
                    {
                        message = new Renew_29();
                        break;
                    }
                case "30":
                    {
                        message = new RenewResponse_30();
                        break;
                    }
                case "37":
                    {
                        message = new FeePaid_37();
                        break;
                    }
                case "38":
                    {
                        message = new FeePaidResponse_38();
                        break;
                    }
                default:
                    error = "不支持的命令'" + cmdIdentifiers + "'";
                    return -1;
            }

            return message.parse(cmdText, out error);

        }



        #region 通用函数

        /// <summary>
        /// 当前时间
        /// </summary>
        public static string NowDateTime
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMdd    HHmmss");

            }
        }



        #endregion

    }
}
