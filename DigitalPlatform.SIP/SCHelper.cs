using DigitalPlatform;
using DigitalPlatform.SIP2;
using DigitalPlatform.SIP2.Request;
using DigitalPlatform.SIP2.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DigitalPlatform.SIP2
{
    public class SCHelper
    {



        #region 单一实例

        static SCHelper _instance;
        private SCHelper()
        {
        }
        private static object _lock = new object();
        static public SCHelper Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_lock)  //线程安全的
                    {
                        _instance = new SCHelper();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region 连接服务器

        private TcpClientWrapper _clientWrapper = null;

        public bool Connection(string serverUrl, int port, out string error)
        {
            error = "";
            this._clientWrapper = new TcpClientWrapper();
            return this._clientWrapper.Connection(serverUrl, port, out error);
        }

        // 关闭连接
        public void Close()
        {
            if (this._clientWrapper != null)
            {
                this._clientWrapper.Close();
            }
        }

        #endregion

        #region 发送，接收消息

        // 发送消息，接收消息
        public int SendAndRecvMessage(string requestText,
            out BaseMessage response,
            out string responseText,
            out string error)
        {
            error = "";
            response = null;
            responseText = "";
            int nRet = 0;

            if (this._clientWrapper == null)
            {
                error = "尚未创建TcpClient对象";
                return -1;
            }

            // 校验消息
            BaseMessage request = null;
            nRet = SIPUtility.ParseMessage(requestText, out request, out error);
            if (nRet == -1)
            {
                error = "校验发送消息异常:" + error;
                return -1;
            }

            // 发送消息
            nRet = this._clientWrapper.SendMessage(requestText, out error);
            if (nRet == -1)
            {
                error = "发送消息出错:" + error;
                return -1;
            }

            // 接收消息
            nRet = this._clientWrapper.RecvMessage(out responseText, out error);
            if (nRet == -1)
            {
                error = "接收消息出错:" + error;
                return -1;
            }

            //解析返回的消息
            nRet = SIPUtility.ParseMessage(responseText, out response, out error);
            if (nRet == -1)
            {
                error = "解析返回的消息异常:" + error + "\r\n" + responseText;
                return -1;
            }

            return 0;
        }

        #endregion


        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="error"></param>
        /// <returns>
        /// 1 登录成功
        /// 0 登录失败
        /// -1 出错
        /// </returns>
        public int Login(string username, string password,
            out LoginResponse_94 response94,
            out string responseText,
            out string error)
        {
            error = "";
            int nRet = 0;
            response94 = null;
            responseText = "";

            Login_93 request = new Login_93()
            {
                CN_LoginUserId_r = username,
                CO_LoginPassword_r = password,
            };
            request.SetDefaulValue();

            // 发送和接收消息
            string requestText = request.ToText();
            BaseMessage response = null;
            nRet = SCHelper.Instance.SendAndRecvMessage(requestText,
                out response,
                out responseText,
                out error);
            if (nRet == -1)
                return -1;

            response94 = response as LoginResponse_94;
            if (response94 == null)
            {
                error = "返回的不是94消息";
                return -1;
            }

            if (response94.Ok_1 == "0")
            {
                return 0;
            }


            return 1;
        }

        // -1出错，0不在线，1正常
        public int SCStatus(out ACSStatus_98 response98,
            out string responseText,
            out string error)
        {
            responseText = "";
            response98 = null;
            error = "";
            int nRet = 0;

            //text = "9900302.00";
            SCStatus_99 request = new SCStatus_99()
            {
                StatusCode_1 = "0",
                MaxPrintWidth_3 = "030",
                ProtocolVersion_4 = "2.00",
            };

            // 发送和接收消息
            string requestText = request.ToText();
            BaseMessage response = null;
            nRet = SCHelper.Instance.SendAndRecvMessage(requestText,
                out response,
                out responseText,
                out error);
            if (nRet == -1)
                return -1;

            response98 = response as ACSStatus_98;
            if (response98 == null)
            {
                error = "返回的不是98消息";
                return -1;
            }

            if (response98.OnlineStatus_1 != "Y")
            {
                error = "ACS当前不在线。";
                return 0;
            }
            return 1;
        }

        /// <summary>
        /// 借书
        /// </summary>
        /// <param name="patronBarcode"></param>
        /// <param name="itemBarcode"></param>
        /// <param name="error"></param>
        /// <returns>
        /// 1 借书成功
        /// 0 借书失败
        /// -1 出错
        /// -2 尚未登录,需要自动测试中断
        /// </returns>
        public int Checkout(string patronBarcode,
            string itemBarcode,
            out CheckoutResponse_12 response12,
            out string responseText,
            out string error)
        {
            error = "";
            int nRet = 0;
            responseText = "";
            response12 = null;


            Checkout_11 request = new Checkout_11()
            {
                TransactionDate_18 = SIPUtility.NowDateTime,
                AA_PatronIdentifier_r = patronBarcode,
                AB_ItemIdentifier_r = itemBarcode,
                AO_InstitutionId_r = SIPConst.AO_Value,
            };
            request.SetDefaulValue();//设置其它默认值

            // 发送和接收消息
            string requestText = request.ToText();
            BaseMessage response = null;
            nRet = SendAndRecvMessage(requestText,
                out response,
                out responseText,
                out error);
            if (nRet == -1)
                return -1;

            response12 = response as CheckoutResponse_12;
            if (response12 == null)
            {
                error = "返回的不是12消息";
                return -1;
            }

            //if (this.IsLogin == false)
            //{
            //    error = "尚未登录ASC系统";
            //    return -2;
            //}

            if (response12.Ok_1 == "0")
            {
                return 0;
            }

            return 1;


        }


        /// <summary>
        /// 还书
        /// </summary>
        /// <param name="itemBarcode"></param>
        /// <param name="error"></param>
        /// <returns>
        /// 1 还书成功
        /// 0 还书失败
        /// -1 出错
        /// -2 尚未登录,需要自动测试中断
        /// </returns>
        public int Checkin(string itemBarcode,
            out CheckinResponse_10 response10,
            out string responseText,
            out string error)
        {
            error = "";
            int nRet = 0;
            responseText = "";
            response10 = null;



            Checkin_09 request = new Checkin_09()
            {
                TransactionDate_18 = SIPUtility.NowDateTime,
                ReturnDate_18 = SIPUtility.NowDateTime,
                AB_ItemIdentifier_r = itemBarcode,
                AO_InstitutionId_r = SIPConst.AO_Value,
            };
            request.SetDefaulValue();//设置其它默认值

            // 发送和接收消息
            string requestText = request.ToText();
            BaseMessage response = null;
            nRet = SendAndRecvMessage(requestText,
                out response,
                out responseText,
                out error);
            if (nRet == -1)
                return -1;

            response10 = response as CheckinResponse_10;
            if (response10 == null)
            {
                error = "返回的不是10消息";
                return -1;
            }

            //if (this.IsLogin == false)
            //{
            //    error = "尚未登录ASC系统";
            //    return -2;
            //}

            if (response10.Ok_1 == "0")
            {
                return 0;
            }

            return 1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="patronBarcode"></param>
        /// <param name="itemBarcode"></param>
        /// <param name="error"></param>
        /// <returns>
        /// 1 续借成功
        /// 0 续借失败
        /// -1 出错
        /// -2 尚未登录,需要自动测试中断
        /// </returns>
        public int Renew(string patronBarcode,
            string itemBarcode,
            out RenewResponse_30 response30,
            out string responseText,
            out string error)
        {
            error = "";
            int nRet = 0;
            responseText = "";
            response30 = null;

            Renew_29 request = new Renew_29()
            {
                TransactionDate_18 = SIPUtility.NowDateTime,
                AO_InstitutionId_r = SIPConst.AO_Value,
                AA_PatronIdentifier_r = patronBarcode,
                AB_ItemIdentifier_o = itemBarcode,
            };
            request.SetDefaulValue();//设置其它默认值

            // 发送和接收消息
            string requestText = request.ToText();
            BaseMessage response = null;
            nRet = SendAndRecvMessage(requestText,
                out response,
                out responseText,
                out error);
            if (nRet == -1)
                return -1;

            response30 = response as RenewResponse_30;
            if (response30 == null)
            {
                error = "返回的不是30消息";
                return -1;
            }

            //if (this.IsLogin == false)
            //{
            //    error = "尚未登录ASC系统";
            //    return -2;
            //}

            if (response30.Ok_1 == "0")
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// 获取读者信息
        /// </summary>
        /// <param name="patronBarcode"></param>
        /// <param name="error"></param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        public int GetPatronInformation(string patronBarcode,
            out PatronInformationResponse_64 response64,
            out string responseText,
            out string error)
        {
            error = "";
            int nRet = 0;
            responseText = "";
            response64 = null;

            PatronInformation_63 request = new PatronInformation_63()
            {
                TransactionDate_18 = SIPUtility.NowDateTime,
                AO_InstitutionId_r = SIPConst.AO_Value,
                AA_PatronIdentifier_r = patronBarcode,
            };
            request.SetDefaulValue();//设置其它默认值

            // 发送和接收消息
            string requestText = request.ToText();
            BaseMessage response = null;
            nRet = SendAndRecvMessage(requestText,
                out response,
                out responseText,
                out error);
            if (nRet == -1)
                return -1;

            response64 = response as PatronInformationResponse_64;
            if (response64 == null)
            {
                error = "返回的不是64消息";
                return -1;
            }

            //if (this.IsLogin == false)
            //{
            //    error = "尚未登录ASC系统";
            //    return -2;
            //}

            return 0;
        }


        /// <summary>
        /// 获取册信息
        /// </summary>
        /// <param name="itemBarcode"></param>
        /// <param name="error"></param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        public int GetItemInformation(string itemBarcode,
            out ItemInformationResponse_18 response18,
            out string responseText,
            out string error)
        {
            error = "";
            int nRet = 0;
            responseText = "";
            response18 = null;


            ItemInformation_17 request = new ItemInformation_17()
            {
                TransactionDate_18 = SIPUtility.NowDateTime,
                AO_InstitutionId_r = SIPConst.AO_Value,
                AB_ItemIdentifier_r = itemBarcode,
            };
            request.SetDefaulValue();//设置其它默认值

            // 发送和接收消息
            string requestText = request.ToText();

            BaseMessage response = null;
            nRet = SendAndRecvMessage(requestText,
                out response,
                out responseText,
                out error);
            if (nRet == -1)
                return -1;

            response18 = response as ItemInformationResponse_18;
            if (response18 == null)
            {
                error = "返回的不是18消息";
                return -1;
            }

            //if (this.IsLogin == false)
            //{
            //    error = "尚未登录ASC系统";
            //    return -2;
            //}


            return 0;
        }
    }
}
