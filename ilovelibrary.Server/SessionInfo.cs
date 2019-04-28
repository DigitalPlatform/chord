﻿using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ilovelibrary.Server
{
    public class SessionInfo
    {
        public const string C_Session_sessioninfo = "sessioninfo";

        public string UserName = "";
        public string Password = "";
        public string Parameters = "";
        public string Rights = "";
        public string LibraryCode = "";
        public string PersonalLibrary = "";

        // 在登录时选定的馆藏
        public string SelPerLib = "";

        public bool isReader = false;

        #region 命令相关



        //命令集合，暂放内存中
        private List<Command> cmdList = new List<Command>();

        public IEnumerable<Command> GetAllCmd()
        {
            return this.cmdList;
        }

        public Command GetCmd(int id)
        {
            return this.cmdList.Where(r => r.id == id).FirstOrDefault();
        }

        // 日期转换成yyyy-MM-dd HH:mm:ss格式字符串
        public static string DateTimeToString(DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public Command AddCmd(Command item)
        {
            Debug.Assert(item != null, "AddCmd传进的item不能为空。");
            Debug.Assert(String.IsNullOrEmpty(item.type) == false, "命令类型不能为空。");
            string strError = "";

            // 补充命令信息
            item.id = this.cmdList.Count + 1;
            item.description = item.readerBarcode + "-" + item.type + "-" + item.itemBarcode;
            item.operTime = DateTimeToString(DateTime.Now);
            item.typeString = Command.getTypeString(item.type);

            if (item.type == Command.C_Command_Borrow 
                || item.type == Command.C_Command_VerifyRenew
                || item.type == Command.C_Command_VerifyReturn)
            {
                if (String.IsNullOrEmpty(item.readerBarcode) == true)
                {
                    item.state = -1;
                    item.resultInfo = "读者证条码号不能为空。";
                }
            }

            if (String.IsNullOrEmpty(item.itemBarcode) == true)
            {
                item.state = -1;
                item.resultInfo = "册条码号不能为空。";
            }

            // 执行这个命令
            LibraryChannel channel = ilovelibraryServer.Instance.ChannelPool.GetChannel(ilovelibraryServer.Instance.dp2LibraryUrl, this.UserName);
            channel.Password = this.Password;
            channel.Parameters = this.Parameters;
            try
            {
                long lRet = -1;
                string strOutputReaderBarcode = "";
                string strReaderXml = "";
                // 借书或续借
                if (item.type == Command.C_Command_Borrow 
                    || item.type == Command.C_Command_Renew
                    || item.type == Command.C_Command_VerifyRenew)
                {
                    bool bRenew = false;
                    if (item.type == Command.C_Command_Renew 
                        || item.type==Command.C_Command_VerifyRenew)
                    {
                        bRenew = true;
                    }
                    DigitalPlatform.LibraryRestClient.BorrowInfo borrowInfo = null;
                    lRet = channel.Borrow(bRenew,
                                        item.readerBarcode,
                                        item.itemBarcode,
                                        out strOutputReaderBarcode,
                                        out strReaderXml,
                                        out borrowInfo,
                                        out strError);
                }
                else if (item.type == Command.C_Command_Return
                    || item.type == Command.C_Command_VerifyReturn
                    || item.type == Command.C_Command_Read)
                {
                    string strAction = "";
                    if (item.type == Command.C_Command_Return 
                        || item.type == Command.C_Command_VerifyReturn)
                        strAction = "return";
                    else
                        strAction = "read";

                 
                    ReturnInfo returnInfo = null;
                    lRet = channel.Return(strAction,
                        item.readerBarcode,
                        item.itemBarcode, 
                        out strOutputReaderBarcode,
                        out strReaderXml,
                        out returnInfo, 
                        out strError);
                }



                // 设上实际的读者证条码
                item.readerBarcode = strOutputReaderBarcode;

                // 读者重复
                if (lRet ==2)
                {
                    item.state = (int)lRet;
                    item.resultInfo = item.typeString + "书操作失败：" + strError;

                    //直接返回了，因为不会加到操作历史里
                    return item;
                }

                // 册重复
                if (lRet == 3)
                {
                    item.state = (int)lRet;
                    item.resultInfo = strError;//item.typeString + "书操作失败：" + strError;

                    //直接返回了，因为不会加到操作历史里
                    return item;
                }


                if (lRet == -1)
                {
                    item.state = -1;
                    item.resultInfo = item.typeString+"书操作失败：" + strError;
                }
                else if (lRet == 0)
                {
                    item.state = 0;
                    item.resultInfo = item.typeString + "书操作成功。";           
                } 
                else
                {
                    item.state = 1;
                    item.resultInfo =  strError;   
                }

                // 检索是否与前面同一个读者，不加要加线
                if (this.cmdList.Count > 0)
                {
                    Command firstCmd = this.cmdList[0];
                    if (firstCmd.readerBarcode != item.readerBarcode
                        && String.IsNullOrEmpty(item.readerBarcode) == false
                        && String.IsNullOrEmpty(firstCmd.readerBarcode) == false)
                    {
                        item.isAddLine = 1;
                    }
                }
                // 设链接地址
                if (item.itemBarcode.Contains("@biblioRecPath") == false && string.IsNullOrEmpty(ilovelibraryServer.Instance.dp2OpacUrl) == false)
                {
                    item.itemBarcodeUrl = ilovelibraryServer.Instance.dp2OpacUrl + "/book.aspx?barcode=" + HttpUtility.UrlEncode(item.itemBarcode) + "&borrower=" + item.readerBarcode;
                    
                       item.itemBarcodeUrl ="<a href='" +item.itemBarcodeUrl+ "' target='_blank'>"+item.itemBarcode+"</a>";
                }
                else
                {
                    item.itemBarcodeUrl = item.itemBarcode;
                }
                // 解析读者信息
                //PatronResult patronResult = ilovelibraryServer.Instance.GetPatronInfo(this, item.readerBarcode);
                //item.patronResult = patronResult;
                
                PatronResult patronResult = new PatronResult();
                patronResult.patron = null;
                patronResult.apiResult = new ApiResult();
                if (String.IsNullOrEmpty(strReaderXml) == true)
                {
                    patronResult.apiResult.errorCode = -1;
                    patronResult.apiResult.errorInfo = "dp2服务端操作api返回的读者xml为空。";
                }
                else
                {
                    //解析返回的读者xml
                    ilovelibraryServer.Instance.ParseReaderXml(strReaderXml, patronResult);
                }
                item.patronResult = patronResult;
                

                // 加到集合里
                this.cmdList.Insert(0, item);

                return item;
            }
            finally
            {
                ilovelibraryServer.Instance.ChannelPool.ReturnChannel(channel);
            }
        }

        public void RemoveCmd(int id)
        {
            Command item = GetCmd(id);
            if (item != null)
            {
                this.cmdList.Remove(item);
            }
        }


        #endregion

    }
}
