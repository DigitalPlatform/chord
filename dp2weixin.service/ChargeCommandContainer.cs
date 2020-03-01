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
        public string getItemHtml(BiblioItem record)
        {
            string itemTables = "";
            string titleClass = "title";

            //alert("disable="+record.disable);

            var addStyle = "";  /*删除线*/
            if (record.disable == true)
            {
                addStyle = "style='color:#cccccc;text-decoration:line-through;'";  /*发灰，删除线*/
            }


            var tempBarcode = record.barcode;


            itemTables += "<div class='item' id='_item_" + tempBarcode + "'>"  //mui-card 
                //+ "<div class='"+titleClass+"'>" + record.barcode + "</div>"
             + "<table>"
            + "<tr>";

            // 有图片才显示
            if (record.imgHtml != null && record.imgHtml != "")
            {
                itemTables += "<td class='label'></td>"
                + "<td class='value'>" + record.imgHtml + "</td>"
                + "</tr>";
            }

            // 册条码
            itemTables += "<tr>"
            + "<td class='label'>册条码</td>"
            + "<td class='title' " + addStyle + ">" + record.pureBarcode + "</td>"  //record.barcode
            + "</tr>";

            if (record.state != null && record.state != "")
            {
                itemTables += "<tr>"
                + "<td class='label'>状态</td>"
                + "<td class='value'  " + addStyle + ">" + record.state + "</td>"
                + "</tr>";
            }

            if (record.volume != null && record.volume != "")
            {
                itemTables += "<tr>"
                + "<td class='label'>卷册</td>"
                + "<td class='value' " + addStyle + ">" + record.volume + "</td>"
                + "</tr>";
            }

            itemTables += "<tr>"
            + "<td class='label'>馆藏地</td>"
            + "<td class='value' " + addStyle + ">" + record.location + "</td>"
            + "</tr>"
            + "<tr>"
            + "<td class='label'>索取号</td>"
            + "<td class='value' " + addStyle + ">" + record.accessNo + "</td>"
            + "</tr>"
            + "<tr>"
            + "<td class='label'>价格</td>"
            + "<td class='value' " + addStyle + ">" + record.price + "</td>"
            + "</tr>";



            // 成员册 不显示在借情况
            if (record.borrowInfo != null && record.borrowInfo != "")
            {
                itemTables += "<tr>"
                + "<td class='label'>在借情况</td>"
                + "<td class='value' " + addStyle + ">" + record.borrowInfo + "</td>"
                + "</tr>";
            }

            //// 成员册 不显示预约信息
            //if (record.reservationInfo != null && record.reservationInfo != "")
            //{
            //    itemTables += "<tr>"
            //    + "<td class='label'>预约信息</td>"
            //    + "<td class='value' " + addStyle + ">" + record.reservationInfo + "</td>"
            //    + "</tr>";
            //}

            //itemTables += "<tr>"
            //+ "<td class='label'>参考ID</td>"
            //+ "<td class='titleGray' " + addStyle + ">" + record.refID + "</td>"
            //+ "</tr>";

            //从属于
            if (record.parentInfo != null && record.parentInfo != "")
            {
                itemTables += "<tr>"
                + "<td class='label'>从属于</td>"
                + "<td class='value' " + addStyle + ">" + record.parentInfo + "</td>"
                + "</tr>";
            }

            //
            itemTables += "</table>"
            + "</div>";

            return itemTables;
        }

        public ChargeCommand AddCmd(//WxUserItem activeUser,
            string weixinId,
            string libId,
            string libraryCode,
            int needTransfrom,
            ChargeCommand cmd)
        {
            //dp2WeiXinService.Instance.WriteErrorLog1("AddCmd-1");

            Debug.Assert(cmd != null, "AddCmd传进的cmd不能为空。");
            Debug.Assert(String.IsNullOrEmpty(cmd.type) == false, "命令类型不能为空。");

            if (string.IsNullOrEmpty(libId) == true)
            {
                cmd.state = -1;
                cmd.errorInfo = "libId参数不能为空";
                return cmd;
            }

            //string libId = libId;//activeUser.libId;

            // 册list,用于ISBN借书
            cmd.itemList = new List<BiblioItem>();

            // 读者对象
            Patron patron = null;


            // 一般传进来只有3个值 type,patron,item
            cmd.patronBarcode = cmd.patronInput;
            cmd.itemBarcode = cmd.itemInput;

            // 补充命令信息
            cmd.id = this.Count + 1;
            cmd.operTime = DateTimeUtil.DateTimeToString(DateTime.Now);
            cmd.resultInfoWavText = "";

            // 其它错误信息
            string otherError = "";

            // 书目名称
            string biblioName = "";

            // 输出真正的读者条码，有可能是传来的是姓名
            string outPatronBarcode = cmd.patronBarcode;

            // 读者xml
            string patronXml = "";
            // 读者路径
            string patronRecPath = "";
            // 获取读者记录的时间戳
            string timestamp = "";
 
            // 本函数返回信息
            ReturnInfo resultInfo = null;
            int cmdRet = -1;
            string cmdError = "";

            // 登录dp2身份
            //前端传来的userName有可能为null，这里兼职一下，防止报错
            if (cmd.userName == null)
                cmd.userName = "";
            LoginInfo loginInfo = new LoginInfo(cmd.userName,
                cmd.isPatron==1?true:false );

            //dp2WeiXinService.Instance.WriteErrorLog1("AddCmd-2");

            #region 分馆的自动加前缀功能
            // 当是分馆时，检查是否需要自动加前缀，以及自动加前缀
            if (string.IsNullOrEmpty(libraryCode) == false && needTransfrom == 1) //20171029,当分馆，且明确指定的需要转换时前缀时再转换
            {
                //dp2WeiXinService.Instance.WriteErrorLog1("AddCmd-3");

                // 根据命令类型，判断转哪个条码
                string tempBarcode = "";
                if (cmd.type == ChargeCommand.C_Command_LoadPatron)
                    tempBarcode = cmd.patronBarcode;
                else
                    tempBarcode = cmd.itemBarcode;

                string resultBarcode = "";
                cmdRet =  dp2WeiXinService.Instance.GetTransformBarcode(loginInfo,
                    libId,
                    libraryCode,
                    tempBarcode,
                    out resultBarcode,
                    out cmdError);
                //dp2WeiXinService.Instance.WriteErrorLog1("AddCmd-31-[" + cmdError+"]");
                if (cmdRet == -1)
                {
                    //cmdError = cmdError.Replace("<script>", "");
                    goto END1;
                }
                //dp2WeiXinService.Instance.WriteErrorLog1("AddCmd-4");
                // 转换过的
                if (cmdRet == 1)
                {
                    if (cmd.type == ChargeCommand.C_Command_LoadPatron)
                        cmd.patronBarcode = resultBarcode;
                    else
                        cmd.itemBarcode = resultBarcode;
                }
            }
            #endregion

            #region 查询册的功能
            // 20170413 查询册
            if (cmd.type == ChargeCommand.C_Command_SearchItem)
            {
                //dp2WeiXinService.Instance.WriteErrorLog1("AddCmd-5");

                string summary = "";
                string recPath = "";
                LibEntity lib = dp2WeiXinService.Instance.GetLibById(libId);
                int nRet = dp2WeiXinService.Instance.GetBiblioSummary(lib,
                    cmd.itemBarcode,
                   "",
                   out summary,
                   out recPath,
                   out otherError);
                if (nRet == -1)
                {
                    //出错信息会加起来
                }
                //dp2WeiXinService.Instance.WriteErrorLog1("AddCmd-51");

                // 截图path
                int tempIndex = recPath.IndexOf("@");
                if (tempIndex > 0)
                    recPath = recPath.Substring(0, tempIndex);

                //if (string.IsNullOrEmpty(summary) == false)
                //{
                //    biblioName = dp2WeiXinService.Instance.GetShortSummary(summary);
                //}

                // 取item
                List<BiblioItem> itemList = null;
                nRet = (int)dp2WeiXinService.Instance.GetItemInfo(weixinId,
                    lib,
                    loginInfo,
                    "",//patronBarcode
                    recPath,
                    cmd.type,
                    out itemList,
                    out cmdError);
                if (nRet == -1) //0的情况表示没有册，不是错误
                {
                    cmdError += " 传入的册条码号为[" + cmd.itemBarcode + "]";
                    cmdRet = -1;
                }
                else if (nRet == 0)
                {
                    cmdError = "未命中";
                    cmd.resultInfo = cmdError;
                    cmd.simpleResultInfo = cmdError;
                    cmdRet = -1;
                }
                //dp2WeiXinService.Instance.WriteErrorLog1("AddCmd-6");


                BiblioItem item = null;
                foreach (BiblioItem one in itemList)
                {
                    if (one.barcode == cmd.itemBarcode)
                    {
                        item = one;
                        break;
                    }
                }

                if (item != null)
                {
                    cmd.resultInfo = summary
                        + this.getItemHtml(item);

                    string shortSummary = dp2WeiXinService.Instance.GetShortSummary(summary);

                    cmd.simpleResultInfo = shortSummary
                        + "<br>册条码：" + item.barcode + "&nbsp;&nbsp;馆藏地：" + item.location
                        + "<br>索取号：" + item.accessNo + "&nbsp;&nbsp;价格："+item.price;

                    // 成员册 不显示在借情况
                    if (String.IsNullOrEmpty(item.borrower) ==false)
                    {
                        cmd.simpleResultInfo += "<br>借阅者：" + item.borrower;
                    }
                    if (String.IsNullOrEmpty(item.state) == false)
                    {
                        if (String.IsNullOrEmpty(item.borrowInfo) == false)
                            cmd.simpleResultInfo += "&nbsp;&nbsp;";
                        else
                            cmd.simpleResultInfo += "<br/>";
                        cmd.simpleResultInfo += "状态：" + item.state;
                    }




                    // 语音返回 书名
                    cmd.resultInfoWavText =shortSummary;
                    cmdRet = 0;
                }


                // 设返回值
                cmd.state = cmdRet;
                cmd.errorInfo = cmdError;
                cmd.typeString = cmd.getTypeString(cmd.type);
                goto END2;
            }

            #endregion

            #region 加载读者
            //加载读者
            if (cmd.type == ChargeCommand.C_Command_LoadPatron) 
            {
                //dp2WeiXinService.Instance.WriteErrorLog1("走进-加载读者-1");

                cmdRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                    loginInfo,
                    cmd.patronBarcode,
                    "advancexml",
                    out patronRecPath,
                    out timestamp,
                    out patronXml,
                    out cmdError);
                if (cmdRet == -1 || cmdRet == 0)  //未找到认为出错
                {
                    //dp2WeiXinService.Instance.WriteErrorLog1("走进-加载读者-2");
                    cmdError += "，传入的条码为["+cmd.patronBarcode+"]";
                    cmdRet = -1;
                }

                //dp2WeiXinService.Instance.WriteErrorLog1("走进-加载读者-3");
                goto END1;
            }

            #endregion


            #region 检查item是否为isbn

            //检查item是否为isbn
            string strTemp = cmd.itemBarcode;
            /*
            if (IsbnSplitter.IsISBN(ref strTemp) == true)
            {
                // 根据isbn检索item
                List<BiblioItem> items=null;
                string error="";
                long lRet= dp2WeiXinService.Instance.SearchItem(activeUser,
                    loginInfo,
                    "ISBN",
                    strTemp,
                    "left",
                    cmd.type,
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
            */
            #endregion

            #region 借书、还书

#if NO
            // 借书
             if (cmd.type == ChargeCommand.C_Command_Borrow) 
            {
                cmdRet = dp2WeiXinService.Instance.Circulation(libId,
                    loginInfo,
                    "borrow",
                    cmd.patronBarcode,
                    cmd.itemBarcode,
                    out outPatronBarcode,
                    out patronXml,
                    out resultInfo,
                    out cmdError);


                // 借书失败时，也要取一下读者记录，因为读者信息还是要显示的
                if (string.IsNullOrEmpty(patronXml) == true)
                {
                    // 取读者记录
                    int nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                        loginInfo,
                        outPatronBarcode,
                        "xml",
                        out patronRecPath,
                        out timestamp,
                        out patronXml,
                        out otherError);
                    if (nRet == -1 || nRet == 0)
                    {
                        //命令成功的，但加载读者不成功，一般这种情况不可能有
                    }
                }
            }
            else if (cmd.type == ChargeCommand.C_Command_Return) // 还书
            {
                cmdRet = dp2WeiXinService.Instance.Circulation(libId,
                    loginInfo,
                    "return",
                    cmd.patronBarcode,
                    cmd.itemBarcode,
                    out outPatronBarcode,
                    out patronXml,
                    out resultInfo,
                    out cmdError);               
            }
#endif
            // 调借还书命令
            cmdRet = dp2WeiXinService.Instance.Circulation(libId,
                loginInfo,
                cmd.type,
                cmd.patronBarcode,
                cmd.itemBarcode,
                out outPatronBarcode,
                out patronXml,
                out resultInfo,
                out cmdError);           
             if (cmd.state !=-1)
             {
                 // 根据item barcode取书名
                 string summary = "";
                 string recPath = "";
                 LibEntity lib = dp2WeiXinService.Instance.GetLibById(libId);
                 int nRet = dp2WeiXinService.Instance.GetBiblioSummary(lib, 
                     cmd.itemBarcode,
                    "",
                    out summary,
                    out recPath,
                    out otherError);
                 if (nRet == -1)
                 {
                     //出错信息会加起来
                 }

                 if (string.IsNullOrEmpty(summary) == false)
                 {
                     biblioName = dp2WeiXinService.Instance.GetShortSummary(summary);
                 }
             }

            #endregion

         END1:
            // 设返回值
             cmd.state = cmdRet;
             cmd.errorInfo =  cmdError; //20171028
            cmd.typeString = cmd.getTypeString(cmd.type);

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


            // 设上实际的读者证条码，还书时用到
            cmd.patronBarcode = outPatronBarcode;

            // 解析读者信息
            if (string.IsNullOrEmpty(patronXml) == false)
            {
                int showPhoto = 0;//todo
                patron = dp2WeiXinService.Instance.ParsePatronXml(libId,
                    patronXml,
                    patronRecPath,
                    showPhoto,true);
                cmd.patronBarcode = patron.barcode;
            }

            string simpleInfo = "";
            cmd.resultInfo = cmd.GetResultInfo(out simpleInfo);
            cmd.simpleResultInfo = simpleInfo; // 简单结果信息
            if (cmd.type == ChargeCommand.C_Command_LoadPatron && patron != null)
            {
                // 语音返回  读者姓名
                cmd.resultInfoWavText = patron.name;

                cmd.resultInfo = "<span style='font-size:20pt'>" + patron.name + "</span>";
                if (String.IsNullOrEmpty(patron.department) == false)
                    cmd.resultInfo += "(" + patron.department + ")";

                cmd.simpleResultInfo = cmd.resultInfo;
            }
            else
            {
                if (cmd.state != -1)
                {
                    if (cmd.type == ChargeCommand.C_Command_Borrow)
                    {
                        // 语音返回  书名
                        cmd.resultInfoWavText = biblioName;
                        // 简单结果信息
                        cmd.simpleResultInfo = biblioName;

                        cmd.resultInfo += "<br/>"+ biblioName; //用+=是因为前面有了 借书成功
                    }
                    else if (cmd.type == ChargeCommand.C_Command_Return )
                    {
                        // 语音返回  书名
                        cmd.resultInfoWavText = biblioName;

                        // 简单结果信息
                        cmd.simpleResultInfo = biblioName;

                        cmd.resultInfo += "<br/>" + biblioName; //用+=是因为前面有了 还书成功
                        if (patron != null)
                        {
                            cmd.resultInfo += "<br/>借阅者：" + patron.name;
                            cmd.simpleResultInfo += "<br/>借阅者：" + patron.name;
                        }
                    }

                     //有提示信息
                    if (String.IsNullOrEmpty(cmd.errorInfo) == false 
                        && cmd.errorInfo !=ChargeCommand.C_ReturnSucces_FromApi)
                    {
                        cmd.resultInfo += "<br/>"+cmd.errorInfo;
                        cmd.simpleResultInfo += "<br/>" + cmd.errorInfo;

                    }
                }
            }

        END2:
     
            // 得到命令html
            string cmdHtml = this.GetCmdHtml3(libId,cmd,patron);//.GetCmdHtml(libId, cmd, patron, otherError);
            cmd.cmdHtml = cmdHtml;


            // 加到集合里
            this.Add(cmd); //this.Insert(0, cmd); //

            return cmd;
        }


        public string GetCmdHtml3(string libId, ChargeCommand cmd, Patron patron)
        {
            string html = "";
           // stepInfo = "";

            string img = "";
            string retClass = "success";
            string retInfo = cmd.typeString + "成功。";
            if (cmd.state == -1)
            {
                img = "<img src='../img/charge_error_24.png'>";
                retClass = "error";
                retInfo = cmd.typeString + "失败。";
            }
            else if (cmd.state == 1 && cmd.type != ChargeCommand.C_Command_LoadPatron) //成功但有提示
            {
                retClass = "warn";
            }
            //有提示信息
            if (String.IsNullOrEmpty(cmd.errorInfo) == false)
            {
                if (cmd.type == ChargeCommand.C_Command_Return)
                    retInfo = cmd.errorInfo;
                else
                    retInfo += cmd.errorInfo;
            }

            //开关
            html += "<div class='mui-card cmd'>";
            // 前方的提示
            html += "<div class='line'>"
                + img
                + "<span class='" + retClass + "'>" + retInfo + "</span>"
            + "</div>";

            //读者信息
            if (patron != null)
            {
                string dept = "";
                if (String.IsNullOrEmpty(patron.department) == false)
                    dept = "<span class='department'>(" + patron.department + ")</span>";

                html += "<div class='line'>"
                    + "<span class='patronName'>" + patron.name + "</span> <span class='patronBarcode'>" + patron.barcode + "</span>" + dept
                + "</div>";
            }

            // 册信息
            if (cmd.type != ChargeCommand.C_Command_LoadPatron)
            {
                string pending = "<span  class='pending summary ' style='padding-bottom:4px'>"
                           + "<label>bs-" + cmd.itemBarcode + "</label>"
                           + "<img src='../img/loading.gif' />"
                           + "<span>" + libId + "</span>"
                       + "</span>";

                html += "<div class='line'>"
                        + "<span class='itemBarcode'>" + cmd.itemBarcode + "</span>" + pending
                    + "</div>";
            }

            //操作时间
            html += "<div class='line'>"
                + "<span class='time'>" + cmd.operTime + "</span>"
            + "</div>";

            //结尾
            html += "</div>";

            // 跟上一个读者进行比较，决定是否加分隔线
            if (this.Count > 0)
            {
                ChargeCommand lastCmd = this[this.Count - 1];
                if (lastCmd.patronBarcode != cmd.patronBarcode)
                {
                    html += "<div class='split'>";
                }
            }


            return html;
        }


        public string GetCmdHtml2(string libId,ChargeCommand cmd,Patron patron)
        {
            string html = "";

            string img = "charge_success_24.png";
            string retClass = "success";
            string retInfo = cmd.typeString + "成功。";
            if (cmd.state == -1)
            {
                img = "charge_error_24.png";
                retClass = "error";
                retInfo = cmd.typeString + "失败。";
            }
            else if (cmd.state == 1 && cmd.type != ChargeCommand.C_Command_LoadPatron) //成功但有提示
            {
                retClass = "warn";
            }
            //有提示信息
            if (String.IsNullOrEmpty(cmd.errorInfo)==false)
            {
                if (cmd.type == ChargeCommand.C_Command_Return)
                    retInfo = cmd.errorInfo;
                else
                    retInfo += cmd.errorInfo;

            }
            
            //开关
            html += "<div class='mui-card cmd'>"
                    + "<table>";

            // 前方的提示
            if (cmd.state == -1)
            {
                html += "<tr>"
                                + "<td colspan='2'>"
                                    + "<table>"
                                        + "<tr>"
                                            + "<td style='width:50px;padding-left:5px;'><img src='../img/" + img + "' /></td>"
                                            + "<td style='width:100%' class='" + retClass + "'>" + retInfo + "</td>"
                                        + "</tr>"
                                    + "</table>"
                                + "</td>"
                            + "</tr>";
            }
            else
            {
                html += "<tr>"
                    + "<td colspan='2' class='" + retClass + "'>"
                     + retInfo
                    + "</td>"
                + "</tr>";
            }

            //读者信息
            if (patron != null)
            {
                string dept ="";
                if (String.IsNullOrEmpty(patron.department)==false)
                    dept="<span class='department'>("+patron.department+")</span>";

                html += "<tr>"
                            + "<td class='patronBarcode'>" + patron.barcode + "</td>"
                            + "<td class='value'>"
                                + "<span class='name'>"+patron.name+"</span>"
                                + dept
                            + "</td>"
                        + "</tr>";
            }
            // 册信息
            if (cmd.type !=ChargeCommand.C_Command_LoadPatron)
            {
                string pending="<div  class='pending' style='padding-bottom:4px'>"
                                           + "<label>bs-" + cmd.itemBarcode + "</label>"
                                           + "<img src='../img/loading.gif' />"
                                           + "<span>" + libId + "</span>"
                                       + "</div>";

                html += "<tr>"
                            + "<td class='itemBarcode'>"+cmd.itemBarcode+"</td>"
                            + "<td class='value'>"+pending+"</td>"
                        + "</tr>";
            }

            //操作时间
            html += "<tr>"
                            + "<td class='label'>操作时间</td>"
                            + "<td class='time'>"+cmd.operTime+"</td>"
                        + "</tr>";

            // 收尾
            html += "</table>"
                + "</div>";


            return html;
        }

        public string GetCmdHtml(string libId, ChargeCommand cmd, Patron patron, string otherError)
        {

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
                            + "<div class='name'>" + patron.name + "</div>"
                            + "<div class='department'>" + patron.department + "</div>";
                }
            }
            else
            {
                title = cmd.patronBarcode + "&nbsp;" + cmd.typeString + "&nbsp;" + cmd.itemBarcode;
                if (cmd.state != -1)
                {
                    string patronUrl = "../patron/PersonalInfo?loginUserName=" + HttpUtility.UrlEncode(cmd.userName) + "&patronBarcode=" + HttpUtility.UrlEncode(patron.barcode);
                    string patronLink = "<a href='" + patronUrl + "'>" + cmd.patronBarcode + "</a>";

                    string biblioPath = "@itemBarcode:" + cmd.itemBarcode;
                    string detalUrl = "../Biblio/Detail?biblioPath=" + HttpUtility.UrlEncode(biblioPath);
                    string itemLink = "<a href='" + detalUrl + "'>" + cmd.itemBarcode + "</a>";
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
            else if (cmd.errorInfo != "")
            {
                if (cmd.state == 1)
                {
                    lineClass = "warnLine";
                    cmdClass += " commandWarn ";
                }
            }

            //依据cmdError判断完了，再加了loadPatronError
            if (String.IsNullOrEmpty(otherError) == false)
            {
                if (String.IsNullOrEmpty(cmd.errorInfo) == false)
                    cmd.errorInfo += "<br/>";
                cmd.errorInfo += otherError;
            }
            if (String.IsNullOrEmpty(cmd.errorInfo) == false)
            {
                info += "<div class='error'>===<br/>"
                    + cmd.errorInfo
                    + "</div>";
            }

            cmdHtml = "<table class='" + cmdClass + "'>"
                            + "<tr>"
                                + "<td class='" + lineClass + "' ></td>"
                                + "<td class='resultIcon'><img src='../img/" + imgName + "' /> </td>"
                                + "<td class='info'><div class='title' style='word-wrap:break-word;word-break:break-all;white-space:pre-wrap'>" + title + "</div>"
                                + info
                                + "</td>"
                            + "</tr>"
                        + "</table>";
            //cmd.cmdHtml = cmdHtml;

            return cmdHtml;
        }
    }
}
