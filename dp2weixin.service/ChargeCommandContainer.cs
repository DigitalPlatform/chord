using DigitalPlatform.IO;
//using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace dp2weixin.service
{
    public class ChargeCommandContainer:List<ChargeCommand>
    {
        public ChargeCommand AddCmd(string weixinId,
            string libId,
            ChargeCommand cmd)
        {
            Debug.Assert(cmd != null, "AddCmd传进的cmd不能为空。");
            Debug.Assert(String.IsNullOrEmpty(cmd.type) == false, "命令类型不能为空。");

            cmd.itemList = new List<BiblioItem>();

            Patron patron = null;

            if (cmd.userName == null)
                cmd.userName = "";

            // 一般传进来只有3个值 type,patron,item
            cmd.patronBarcode = cmd.patron;
            cmd.itemBarcode = cmd.item;

            // 补充命令信息
            cmd.id = this.Count + 1;
            cmd.operTime = DateTimeUtil.DateTimeToString(DateTime.Now);
            cmd.typeString = ChargeCommand.getTypeString(cmd.type);

            // 其它错误信息
            string otherError = "";


            string outPatronBarcode = cmd.patron;
            string patronXml = "";
            string patronRecPath = "";
            ReturnInfo resultInfo = null;
            int cmdRet = -1;
            string cmdError = "";

            //加载读者
            if (cmd.type == ChargeCommand.C_Command_LoadPatron) 
            {
                cmdRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                    cmd.userName,
                    false,
                    cmd.patronBarcode,
                    "advancexml",
                    out patronRecPath,
                    out patronXml,
                    out cmdError);
                if (cmdRet == -1 || cmdRet == 0)  //未找到认为出错
                {
                    cmdRet = -1;
                }
                goto END1;
            }

             //检查item是否为isbn
            string strTemp = cmd.itemBarcode;
            if (IsbnSplitter.IsISBN(ref strTemp) == true)
            {
                // 根据isbn检索item
                List<BiblioItem> items=null;
                string error="";
                long lRet= dp2WeiXinService.Instance.SearchItem(weixinId,
                    libId,
                    cmd.userName,
                    false,
                    "ISBN",
                    strTemp,
                    "left",
                    out items,
                    out  error);
                if (lRet == -1 || lRet == 0)
                {
                    cmdError = "根据ISBN检索书目出错:" + error;
                    cmd.state = -1;
                }
                else
                {
                    cmd.itemList = items;
                    cmdRet = -3;
                }

                goto END1;
            }

            // 流通命令
             if (cmd.type == ChargeCommand.C_Command_Borrow) //借书
            {
                cmdRet = dp2WeiXinService.Instance.Circulation(libId,
                    cmd.userName,
                    false,
                    "borrow",
                    cmd.patron,
                    cmd.item,
                    out outPatronBarcode,
                    out resultInfo,
                    out cmdError);   
            }
            else if (cmd.type == ChargeCommand.C_Command_Return) // 还书
            {
                cmdRet = dp2WeiXinService.Instance.Circulation(libId,
                    cmd.userName,
                    false,
                    "return",
                    cmd.patron,
                    cmd.item,
                    out outPatronBarcode,
                    out resultInfo,
                    out cmdError);               
            }

             if (cmdRet == 1 || cmdRet==0)
             {
                 // 取一下读者记录
                 int nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                     cmd.userName,
                     false,
                     outPatronBarcode,
                     "advancexml",
                     out patronRecPath,
                     out patronXml,
                     out otherError);
                 if (nRet == -1 || nRet == 0) 
                 {
                     //命令成功的，但加载读者不成功，一般这种情况不可能有
                 }
             }

END1:
            // 设返回值
             cmd.state = cmdRet;


            //========以下两种情况直接返回，不加到操作历史中===

             // 读者姓名重复的情况
             if (cmdRet == -2)
             {
                 return cmd;
             }

            // isbn的情况
             if (cmdRet == -3)
             {
                 return cmd;
             }

            //=================

            // 设上实际的读者证条码
            cmd.patronBarcode = outPatronBarcode;

            // 解析读者信息
            if (string.IsNullOrEmpty(patronXml) == false)
            {
                int showPhoto = 0;//todo
                patron = dp2WeiXinService.Instance.ParsePatronXml(libId,
                    patronXml,
                    patronRecPath,
                    showPhoto);
                cmd.patronHtml = dp2WeiXinService.Instance.GetPatronSummary(patron,cmd.userName);//GetPatronHtml(patron,false);
                cmd.patronBarcode = patron.barcode;
            }

            //cmdError=""
            

            string cmdHtml = "";
            string title = "";
            string info = "";
            if (cmd.type == ChargeCommand.C_Command_LoadPatron)
            {
                title = "装载读者信息" + "&nbsp;" + cmd.patronBarcode;
                if (cmd.state != -1 && patron != null)
                {
                    string url = "../patron/PersonalInfo?loginUserName=" + HttpUtility.UrlEncode(cmd.userName) + "&patronBarcode=" + HttpUtility.UrlEncode(patron.barcode);
                    title = "装载读者信息" + "&nbsp;<a href='" + url + "'>" + cmd.patronBarcode + "</a>";

                    info = "<div class='patronBarcode'>" + patron.barcode + "</div>"
                            + "<div class='name'>"+patron.name+"</div>"
                            + "<div class='department'>"+patron.department+"</div>";
                }
            }
            else
            {
                title = cmd.patronBarcode + "&nbsp;" + cmd.typeString + "&nbsp;" + cmd.itemBarcode;
                if (cmd.state != -1)
                {
                    string patronUrl = "../patron/PersonalInfo?loginUserName=" + HttpUtility.UrlEncode(cmd.userName) + "&patronBarcode=" + HttpUtility.UrlEncode(patron.barcode);
                    string patronLink = "<a href='" + patronUrl + "'>" + cmd.patronBarcode + "</a>";

                    string biblioPath = "@itemBarcode:"+cmd.itemBarcode;
                    string detalUrl = "../Biblio/Detail?biblioPath=" + HttpUtility.UrlEncode(biblioPath);                  
                    string itemLink = "<a href='"+detalUrl+"'>" + cmd.itemBarcode + "</a>";
                    title = patronLink + "&nbsp;" + cmd.typeString + "&nbsp;" + itemLink;

                    info = "<div  class='pending' style='padding-bottom:4px'>"
                                           + "<label>bs-" + cmd.itemBarcode + "</label>"
                                           + "<img src='../img/loading.gif' />"
                                           + "<span>" + libId + "</span>"
                                       + "</div>";
                }
            }

            if (string.IsNullOrEmpty(info) == false)
            {
                info = "---" + info;
            }


            string cmdClass = "command";
            string lineClass = "rightLine";
            string imgName = "charge_success_24.png";
            if (cmd.state == -1)
            {
                imgName = "charge_error_24.png";
                lineClass = "errorLine";
            }
            else if (cmdError != "")
            {
                if (cmdRet == 1)
                {
                    lineClass = "warnLine";
                    cmdClass += " commandWarn ";
                }
            }

            //依据cmdError判断完了，再加了loadPatronError
            if (String.IsNullOrEmpty(otherError) == false)
            {
                if (String.IsNullOrEmpty(cmdError) == false)
                    cmdError += "<br/>";
                cmdError += otherError;
            }
            if (String.IsNullOrEmpty(cmdError) == false)
            {
                info += "<div class='error'>===<br/>"
                    + cmdError
                    + "</div>";
            }

            cmdHtml = "<table class='"+cmdClass+"'>"
                            + "<tr>"
                                + "<td class='"+lineClass+"' ></td>"
                                + "<td class='resultIcon'><img src='../img/" + imgName + "' /> </td>"
                                + "<td class='info'><div class='title' style='word-wrap:break-word;word-break:break-all;white-space:pre-wrap'>" + title + "</div>"
                                + info
                                + "</td>"
                            + "</tr>"
                        + "</table>";
            cmd.cmdHtml = cmdHtml;

            // 加到集合里
            this.Add(cmd); //this.Insert(0, cmd); //

            return cmd;

        }


    }
}
