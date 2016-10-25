using DigitalPlatform.IO;
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
        public ChargeCommand AddCmd(string libId,
            ChargeCommand cmd)
        {
            Debug.Assert(cmd != null, "AddCmd传进的cmd不能为空。");
            Debug.Assert(String.IsNullOrEmpty(cmd.type) == false, "命令类型不能为空。");

            if (cmd.userName == null)
                cmd.userName = "";

            string strError = "";
            // 一般传进来只有3个值 type,patron,item
            cmd.patronBarcode = cmd.patron;
            cmd.itemBarcode = cmd.item;

            // 补充命令信息
            cmd.id = this.Count + 1;
            cmd.operTime = DateTimeUtil.DateTimeToString(DateTime.Now);
            cmd.typeString = ChargeCommand.getTypeString(cmd.type);

            if (cmd.type == ChargeCommand.C_Command_Borrow
                || cmd.type == ChargeCommand.C_Command_LoadPatron
                || cmd.type == ChargeCommand.C_Command_VerifyRenew
                || cmd.type == ChargeCommand.C_Command_VerifyReturn)
            {
                if (String.IsNullOrEmpty(cmd.patron) == true)
                {
                    cmd.state = -1;
                    cmd.resultInfo = "读者证条码号不能为空。";
                }
            }

            // 执行这个命令
            int nRet = -1;
            string outputReaderBarcode = cmd.patron;
            string patronXml = "";
            string patronRecPath = "";


            if (cmd.type == ChargeCommand.C_Command_LoadPatron) //加载读者
            {
                nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                    cmd.patronBarcode,
                    "advancexml",
                    out patronRecPath,
                    out patronXml,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    nRet = -1;
                }
            }
            else if (cmd.type == ChargeCommand.C_Command_Borrow) //借书
            {
                nRet = dp2WeiXinService.Instance.CirculationByWorker(libId,
                    cmd.userName,
                    "borrow",
                    cmd.patron,
                    cmd.item,
                    out outputReaderBarcode,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }
            }
            else if (cmd.type == ChargeCommand.C_Command_Return) // 还书
            {
                nRet = dp2WeiXinService.Instance.CirculationByWorker(libId,
                    cmd.userName,
                    "return",
                    cmd.patron,
                    cmd.item,
                    out outputReaderBarcode,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }

                // 取一下读者记录
                nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                    outputReaderBarcode,
                    "advancexml",
                    out patronRecPath,
                    out patronXml,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    nRet = -1;
                }                
            }



            // 设上实际的读者证条码
            cmd.patronBarcode = outputReaderBarcode;

            // 解析读者信息
            if (string.IsNullOrEmpty(patronXml) == false)
            {

                int showPhoto = 0;//todo
                Patron patron = dp2WeiXinService.Instance.ParsePatronXml(libId,
                    patronXml,
                    patronRecPath,
                    showPhoto);
                cmd.patronHtml = dp2WeiXinService.Instance.GetPatronHtml(patron);
                cmd.patronBarcode = patron.barcode;

            }

ERROR1:

            if (nRet == -1)
            {
                cmd.state = -1;
                cmd.resultInfo = cmd.typeString + " 操作失败：" + strError;
            }
            else if (nRet == 0)
            {
                cmd.state = 0;
                cmd.resultInfo = cmd.typeString + " 操作成功。";
            }
            else
            {
                cmd.state = 1;
                cmd.resultInfo = strError;
            }

            //// 检索是否与前面同一个读者，不加要加线
            //if (this.Count > 0)
            //{
            //    ChargeCommand firstCmd = this[0];
            //    if (firstCmd.patronBarcode != cmd.patronBarcode
            //        && String.IsNullOrEmpty(cmd.patronBarcode) == false
            //        && String.IsNullOrEmpty(firstCmd.patronBarcode) == false)
            //    {
            //        cmd.isAddLine = 1;
            //    }
            //}
            //// 设链接地址 链到书的详细信息
            //cmd.itemBarcodeUrl = cmd.itemBarcode;




            string cmdHtml = "";
            string title = "";
            string info = "";
            if (cmd.type == ChargeCommand.C_Command_LoadPatron)
            {
                title = "装载读者信息" + "&nbsp;" + cmd.patronBarcode;
                if (cmd.state != -1)
                {
                    info = "<div class='patronBarcode'>R00001</div>"
                            + "<div class='name'>任1</div>"
                            + "<div class='department'>苏州学校</div>";
                }
            }
            else
            {
                title = cmd.patronBarcode + "&nbsp;" + cmd.typeString + "&nbsp;" + cmd.item;
                if (cmd.state != -1)
                {
                    //info = "<div class='summary'>书目摘要</div>";
                    string biblioPath = "";
                    string detalUrl = "/Biblio/Detail?biblioPath=" + HttpUtility.UrlEncode(biblioPath);
                    string itemLink = "<a href='javascript:void(0)' onclick='gotoBiblioDetail(\"" + detalUrl + "\")'>" + cmd.itemBarcode + "</a>";
                    title = cmd.patronBarcode + "&nbsp;" + cmd.typeString + "&nbsp;" + itemLink;


                    info = "<div  class='pending' style='padding-bottom:4px'>"
                                           + "<label>bs-" + cmd.itemBarcode + "</label>"
                                           + "<img src='../img/loading.gif' />"
                                           + "<span>" + libId + "</span>"
                                       + "</div>";
                }
            }

            string lineClass = "rightLine";
            string imgName = "right.png";
            if (cmd.state == -1)
            {
                imgName = "error.png";
                info = "<div class='error'>===<br/>"
                    + strError
                    + "</div>";
                lineClass = "errorLine";
            }


            cmdHtml = "<table class='command'>"
                            + "<tr>"
                                + "<td class='"+lineClass+"' ></td>"
                                + "<td class='resultIcon'><img src='../img/" + imgName + "' /> </td>"
                                + "<td class='info'><div class='title'>" + title + "</div>"
                                + info
                                + "</td>"
                            + "</tr>"
                        + "</table>";
            cmd.cmdHtml = cmdHtml;

            // 加到集合里
            this.Add(cmd); //this.Insert(0, cmd);

            return cmd;

        }


    }
}
