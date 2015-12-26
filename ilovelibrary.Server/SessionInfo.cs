using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ilovelibrary.Server
{
    public class SessionInfo
    {
        public const string C_Session_sessioninfo = "sessioninfo";

        public string UserName = "";
        public string Password = "";
        public string Rights = "";
        public string LibraryCode = "";


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

        public int AddCmd(Command item, out string strError)
        {
            Debug.Assert(item != null, "AddCmd传进的item不能为空。");
            Debug.Assert(String.IsNullOrEmpty(item.type) == false, "命令类型不能为空。");
            strError = "";

            if (item.type == Command.C_Command_Borrow || item.type == Command.C_Command_Renew)
            {
                if (String.IsNullOrEmpty(item.readerBarcode) == true)
                {
                    strError = "读者证条码号不能为空。";
                    return -1;
                }
            }

            if (String.IsNullOrEmpty(item.itemBarcode) == true)
            {
                strError = "册条码号不能为空。";
                return -1;
            }

            // 补充命令信息
            item.id = this.cmdList.Count + 1;
            item.description = item.readerBarcode + "-" + item.type + "-" + item.itemBarcode;
            item.operTime = DateTimeUtil.DateTimeToString(DateTime.Now);
            item.typeString = Command.getTypeString(item.type);




            // 执行这个命令
            LibraryChannel channel = ilovelibraryServer.Instance.ChannelPool.GetChannel(ilovelibraryServer.Instance.dp2LibraryUrl, this.UserName);
            channel.Password = this.Password;
            try
            {
                long lRet = -1;
                // 借书或续借
                if (item.type == Command.C_Command_Borrow || item.type == Command.C_Command_Renew)
                {
                    bool bRenew = false;
                    if (item.type == Command.C_Command_Renew)
                        bRenew = true;
                    DigitalPlatform.LibraryRestClient.BorrowInfo borrowInfo = null;
                    lRet = channel.Borrow(bRenew,
                                        item.readerBarcode,
                                        item.itemBarcode,
                                        out borrowInfo,
                                        out strError);
                }
                else if (item.type == Command.C_Command_Return)
                {
                    // 还书
                    ReturnInfo returnInfo = null;
                    string strOutputReaderBarcode = "";
                    lRet = channel.Return(item.itemBarcode, 
                        out strOutputReaderBarcode,
                        out returnInfo, out strError);
                    item.readerBarcode = strOutputReaderBarcode;
                }

                if (lRet == -1)
                {
                    item.state = -1;
                    item.resultInfo = "失败：" + strError;
                    return -1;
                }                

                // 检索是否与前面同一个读者，不加要加线
                if (this.cmdList.Count > 0)
                {
                    Command firstCmd = this.cmdList[0];
                    if (firstCmd.readerBarcode != item.readerBarcode)
                        item.isAddLine = 1;
                }
                // 设链接地址
                item.itemBarcodeUrl = ilovelibraryServer.Instance.dp2OpacUrl + "/book.aspx?barcode=" + item.itemBarcode + "&borrower=" + item.readerBarcode;
                // 成功才加到集合里
                this.cmdList.Insert(0, item);
                item.state = 0;
                item.resultInfo = "成功";
                return 1;
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
