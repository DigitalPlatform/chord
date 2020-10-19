using DigitalPlatform.IO;
using DigitalPlatform.Message;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using dp2Command.Service;
using dp2weixin.service;
using dp2weixinWeb.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace dp2weixinWeb.Controllers
{
    public class PatronController : BaseController
    {

        #region 选择图书馆

        /// <summary>
        /// 选择图书馆
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult SelectOwnerLib(string code, string state, string returnUrl)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                false, //是否校验图书馆状态
                false,  //是否重新获取activeuser
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 获取该微信帐户绑定了哪些图书馆帐户
            List<WxUserItem> list = WxUserDatabase.Current.Get(sessionInfo.WeixinId, null, -1);

            // 如果未选择图书馆，则直接跳转到全部图书馆界面
            if (list.Count == 0)
            {
                return Redirect("~/Patron/selectlib?returnUrl=" + HttpUtility.UrlEncode(returnUrl));
            }

            // 选择完返回的页面
            ViewBag.returnUrl = returnUrl;

            // 获取可访问的图书馆
            List<Library> avaiblelibList = dp2WeiXinService.Instance.LibManager.GetLibraryByIds(sessionInfo.libIds);



            // 可显示的区域
            List<Area> areaList = new List<Area>();
            // 从所有区域中查找
            foreach (Area area in dp2WeiXinService.Instance._areaMgr._areas)
            {
                List<LibModel> libList = new List<LibModel>();
                foreach (LibModel lib in area.libs)
                {
                    lib.Checked = "";
                    lib.bindFlag = "";

                    // 检查微信用户是否绑定了这个图书馆
                    WxUserItem tempUser = null;
                    if (this.CheckIsBind(list, lib, out tempUser) == true)  //libs.Contains(lib.libId)
                    {
                        // 加到显示列表
                        libList.Add(lib);

                        if (tempUser.userName != WxUserDatabase.C_Public)
                        {
                            lib.bindFlag = " * ";
                        }


                        // 当前绑定帐户的图书馆显示为勾中状态
                        if (sessionInfo.ActiveUser != null)
                        {
                            if (lib.libId == sessionInfo.ActiveUser.libId
                                && lib.libraryCode == sessionInfo.ActiveUser.bindLibraryCode)
                            {
                                lib.Checked = " checked ";
                            }
                        }
                    }
                }

                // 只有当有下级图书馆时，才显示地区
                if (libList.Count > 0)
                {
                    Area newArea = new Area();
                    newArea.name = area.name;
                    newArea.libs = libList;
                    areaList.Add(newArea);
                }
            }

            // 放到界面上
            ViewBag.areaList = areaList;

            return View();
        }


        /// <summary>
        /// 选择图书馆
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult SelectLib(string code, string state, string returnUrl)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                false, //是否校验图书馆状态
                false,  //是否重新获取activeuser
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 选择完返回的页面
            ViewBag.returnUrl = returnUrl;

            // 获取可访问的图书馆
            List<Library> avaiblelibList = dp2WeiXinService.Instance.LibManager.GetLibraryByIds(sessionInfo.libIds);


            // 获取该微信帐户绑定了哪些图书馆帐户
            List<WxUserItem> list = WxUserDatabase.Current.Get(sessionInfo.WeixinId, null, -1);

            // 可显示的区域
            List<Area> areaList = new List<Area>();
            // 从所有区域中查找
            foreach (Area area in dp2WeiXinService.Instance._areaMgr._areas)
            {
                List<LibModel> libList = new List<LibModel>();
                foreach (LibModel lib in area.libs)
                {
                    lib.Checked = "";
                    lib.bindFlag = "";

                    // 如果是到期的图书馆，不显示出来
                    Library thisLib = dp2WeiXinService.Instance.LibManager.GetLibrary(lib.libId);//.GetLibById(lib.libId);
                    if (thisLib != null && thisLib.Entity.state == "到期")
                    {
                        continue;
                    }

                    //如果不在可访问范围，不显示
                    if (thisLib != null && avaiblelibList.IndexOf(thisLib) == -1)
                    {
                        continue;
                    }

                    // 如果从mongodb库没有找到图书馆，不显示
                    // 有可能是mongodb库删除，但配置文件还没有删除
                    if (thisLib == null)
                    {
                        dp2WeiXinService.Instance.WriteDebug("选择图书馆时，根据[" + lib.libId + "]未找到对应的图书馆");
                        continue;
                    }

                    // 加到显示列表
                    libList.Add(lib);

                    // 检查微信用户是否绑定了这个图书馆
                    WxUserItem tempUser = null;
                    if (this.CheckIsBind(list, lib, out tempUser) == true)  //libs.Contains(lib.libId)
                    {
                        if (tempUser.userName != WxUserDatabase.C_Public)
                            lib.bindFlag = " * ";
                    }

                    // 当前绑定帐户的图书馆显示为勾中状态
                    if (sessionInfo.ActiveUser != null)
                    {
                        if (lib.libId == sessionInfo.ActiveUser.libId
                            && lib.libraryCode == sessionInfo.ActiveUser.bindLibraryCode)
                        {
                            lib.Checked = " checked ";
                        }
                    }
                }

                // 只有当有下级图书馆时，才显示地区
                if (libList.Count > 0)
                {
                    Area newArea = new Area();
                    newArea.name = area.name;
                    newArea.libs = libList;
                    areaList.Add(newArea);
                }

            }

            // 放到界面上
            ViewBag.areaList = areaList;

            return View();
        }



        /// <summary>
        /// 检查一个图书馆是否在绑定列表中
        /// </summary>
        /// <param name="list"></param>
        /// <param name="lib"></param>
        /// <returns></returns>
        public bool CheckIsBind(List<WxUserItem> list, LibModel lib, out WxUserItem outUser)
        {
            outUser = null;
            foreach (WxUserItem user in list)
            {
                //if (user.libId == lib.libId)
                //{
                //    if (user.bindLibraryCode == lib.libraryCode)  //这里按bind帐户来
                //        return true;
                //    else
                //        return false;
                //}

                if (user.libId == lib.libId && user.bindLibraryCode == lib.libraryCode)
                {
                    outUser = user;
                    return true;
                }
            }
            return false;
        }

        #endregion


        /// <summary>
        /// 修改手机号界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult ChangePhone(string code, string state)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/ChangePhone");
                return View();
            }

            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("修改手机号", "/Patron/ChangePhone");
                return View();
            }

            ViewBag.PatronName = sessionInfo.ActiveUser.readerName;
            ViewBag.PatronRecPath = sessionInfo.ActiveUser.recPath;

            dp2WeiXinService.SplitTel(sessionInfo.ActiveUser.phone,
                out string pureTel,
                out string purePhone); 
            ViewBag.PureTel = pureTel;
            ViewBag.PurePhone = purePhone;

            return View();

        }


        /// <summary>
        /// 检查读者界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="patronName"></param>
        /// <returns></returns>
        public ActionResult PatronSearch(string code, string state, string patronName)
        {
            string strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(patronName) == true)
            {
                ViewBag.Error = "未传入读者姓名参数";
                return View();
            }
            ViewBag.patronName = patronName; //用于提示显示出检索用的姓名来

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PatronList");
                return View();
            }

            // 必须是工作人员，且不能为public
            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Worker
                || sessionInfo.ActiveUser.userName == WxUserDatabase.C_Public)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("检索读者", "/Patron/PatronSearch", true);
                return View();
            }



            List<PatronInfo> patronList = new List<PatronInfo>();
            nRet = dp2WeiXinService.Instance.GetPatronsByName(sessionInfo.ActiveUser.libId,
                sessionInfo.ActiveUser.bindLibraryCode,
                patronName,
               out patronList,
               out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }



            return View(patronList);
        }


        /// <summary>
        /// 审核列表界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult PatronList(string code, string state)
        {
            string strError = "";
            int nRet = 0;


            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PatronList");
                return View();
            }

            // 必须是工作人员，且不能为public
            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Worker
                || sessionInfo.ActiveUser.userName == WxUserDatabase.C_Public)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("读者审核", "/Patron/PatronList", true);
                return View();
            }


            List<Patron> patronList = new List<Patron>();
            nRet = dp2WeiXinService.Instance.GetTempPatrons(sessionInfo.ActiveUser.libId,
                sessionInfo.ActiveUser.bindLibraryCode,
               out patronList,
               out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }



            return View(patronList);
        }


        /// <summary>
        /// 审核单个读者界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult PatronReview(string code, string state,
            string libId, string patronLibCode,
            string patronPath,
            string barcode,
            string f)
        {
            string strError = "";
            int nRet = 0;
            if (patronLibCode == null)
                patronLibCode = "";

            if (barcode == null)
                barcode = "";

            if (string.IsNullOrEmpty(libId) == true)
            {
                ViewBag.Error = "libId参数不能为空。";
                return View();
            }

            //dp2WeiXinService.Instance.WriteDebug("!!!PatronReview页面 Request.Path=[" + this.Request.Path + "]"
            //    + "\r\n RawUrl=" + this.Request.RawUrl
            //    + "\r\n Url=" + this.Request.Url);
            if (string.IsNullOrEmpty(patronPath) == true)
            {
                ViewBag.Error = "patronPath参数不能为空。";
                return View();
            }

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PatronReview");
                return View();
            }

            // 必须是工作人员，且不能为public
            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Worker
                || sessionInfo.ActiveUser.userName == WxUserDatabase.C_Public)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("读者审核", "/Patron/PatronReview", true);
                return View();
            }

            // 2020-3-1
            // 如果传入了libId和patronBarcode表示是从读者注册通知过来的审核
            // 需要当前绑定帐户的图书馆与传过来的图书馆一致
            if (sessionInfo.ActiveUser.libId != libId)
            {
                Library lib = dp2WeiXinService.Instance.LibManager.GetLibrary(libId);
                ViewBag.Error = "读者注册的图书馆是[" + lib.Entity.libName + "]，请先绑定该图书馆的工作人员帐户，才能审核。";
                return View();
            }

            if (sessionInfo.ActiveUser.bindLibraryCode != patronLibCode)
            {
                string patronLib = patronLibCode;
                if (patronLib == "")
                {
                    Library lib1 = dp2WeiXinService.Instance.LibManager.GetLibrary(libId);
                    patronLib = lib1.Entity.libName;
                }

                string workerLib = sessionInfo.ActiveUser.bindLibraryCode;
                if (string.IsNullOrEmpty(workerLib) == true)
                {
                    Library lib2 = dp2WeiXinService.Instance.LibManager.GetLibrary(sessionInfo.ActiveUser.libId);
                    workerLib = lib2.Entity.libName;
                }
                ViewBag.Error = "读者注册的图书馆是[" + patronLib + "]，与当前工作人员帐户绑定的[" + workerLib + "]不一致，请先绑定[" + patronLib + "]的工作人员帐户，才能审核。";
                return View();
            }

            // 装载读者帐号信息
            string patronXml = "";
            string recPath = "";
            LoginInfo loginInfo = new LoginInfo("", false);  // todo，这里是用工作人员帐户还是用代理帐户

            // 获取读者记录
            string timestamp = "";
            nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                loginInfo,
                "@path:" + patronPath,
                "advancexml,timestamp",  // 格式
                out recPath,
                out timestamp,
                out patronXml,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }
            if (nRet == 0)
            {
                ViewBag.Error = "证条码为'" + barcode + "'的读者在图书馆系统不存在，可能是由于读者删除了注册信息。";
                return View();
            }

            ViewBag.recPath = recPath;
            ViewBag.timestamp = timestamp;

            // 把读者xml解析成对象
            Patron patron = null;
            patron = dp2WeiXinService.Instance.ParsePatronXml(libId,
                    patronXml,
                    recPath,
                    sessionInfo.ActiveUser.showPhoto,//ViewBag.showPhoto,
                    false);

            // 2020-3-12 已审核过的读者不需要再次审核
            if (patron.state != WxUserDatabase.C_PatronState_TodoReview)
            {
                ViewBag.Error = "姓名为[" + patron.name + "]，手机号为[" + patron.phone + "]的读者已审核完成。";
                return View();
            }

            // 设置一些信息

            // 当前读者性别
            if (patron.gender == "男")
            {
                ViewBag.manSel = " selected ";
            }
            else if (patron.gender == "女")
            {
                ViewBag.womanSel = " selected ";
            }

            // 读者类别 2020/4/7更新
            string readerType = sessionInfo.GetReaderType(sessionInfo.ActiveUser,
                out strError);
            if (string.IsNullOrEmpty(strError) == false)
            {
                ViewBag.Error = strError;
                return View(patron);
            }
            ViewBag.readerTypeHtml = this.GetReaderTypeHtml(readerType,
                patron.readerType);// typesHtml;

            // 来源
            ViewBag.From = f;


            /*
                   <option value=''>请选择 部门</option>"
                    <option value="部门1">部门1</option>
                    <option value="部门2">部门2</option>
             */
            string deptHtml = "<option value=''>请选择 部门</option>";
            List<string> deptList = dp2WeiXinService.Instance._areaMgr.GetDeptartment(
                sessionInfo.ActiveUser.libId,
                sessionInfo.ActiveUser.bindLibraryCode);
            if (deptList.Count == 0)
            {
                ViewBag.Error = "尚未配置部门信息，请联系管理员。";
                return View();
            }

            bool bFind = false;
            string sel = "";

            foreach (string dept in deptList)
            {
                sel = "";
                if (dept == patron.department)
                {
                    sel = " selected ";
                    bFind = true;
                }

                deptHtml += "<option value='" + dept + "' " + sel + ">" + dept + "</option>";
            }

            string displayText = "  style='display:none'  ";
            sel = "";
            if (deptHtml != "")
            {
                if (bFind == false)
                {
                    sel = " selected ";
                    displayText = "  style='display: block' ";
                }

                deptHtml += "<option value='其它' " + sel + ">其它</option>";
            }
            ViewBag.deptHtml = deptHtml;
            ViewBag.displayText = displayText;

            dp2WeiXinService.SplitTel(patron.phone,
            out string pureTel,
            out string purePhone);
                    
            ViewBag.PureTel = pureTel;
            ViewBag.PurePhone = purePhone;

            return View(patron);
        }


        // 得到读者类型字符串
        public string GetReaderTypeHtml(string readerTypes, string currentPatronType)
        {
            // 读者类别
            string types = readerTypes;
            string typesHtml = "";
            if (String.IsNullOrEmpty(types) == false)
            {
                string[] typeList = types.Split(new char[] { ',' });
                foreach (string type in typeList)
                {
                    string temp = type;
                    int nIndex = type.IndexOf("}");
                    if (nIndex != -1)
                    {
                        temp = type.Substring(nIndex + 1).Trim();
                    }

                    string sel = "";
                    if (currentPatronType == temp)
                        sel = " selected ";
                    typesHtml += "<option value='" + type + "' " + sel + ">" + type + "</option>";
                }
            }
            typesHtml = "<select id='selReaderType' name='selReaderType' class='selArrowRight'>"
                    + "<option value=''>请选择</option>"
                    + typesHtml
                    + "</select>";

            return typesHtml;
        }


        /// <summary>
        /// 读者自助注册功能
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult PatronRegister(string code, string state, string userId)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code,
                state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            //// 这里还是需要提前选择一下图书馆，要不后面没法获取配置的单位列表
            //// 如果尚未选择图书馆，不存在当前帐号，出现绑定帐号链接
            //if (sessionInfo.ActiveUser == null)
            //{
            //    ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PatronRegister");
            //    return View();
            //}
            // 保存到的读者数据库
            if (string.IsNullOrEmpty(ViewBag.PatronDbName) == false)
            {
                ViewBag.PatronRecPath = ViewBag.PatronDbName + "/?";
            }

            // 如果传入了本地帐户id，则直接把本地信息显示出来
            WxUserItem userItem = null;
            if (string.IsNullOrEmpty(userId) == false)
            {
                userItem = WxUserDatabase.Current.GetById(userId);
            }
            // 如果不是编辑，new一个对象，防止前端使用的时候报null错误
            if (userItem == null)
                userItem = new WxUserItem();
            else
                ViewBag.PatronRecPath = userItem.recPath;

            // 部门信息
            string deptHtml = "<option value=''>请选择 部门</option>";
            List<string> deptList = new List<string>();
            // 注册的界面，没让用户先必须选择图书馆，所以要判断为null的情况
            if (sessionInfo.ActiveUser != null)
            {
                deptList = dp2WeiXinService.Instance._areaMgr.GetDeptartment(
                    sessionInfo.ActiveUser.libId,
                    sessionInfo.ActiveUser.bindLibraryCode);
                if (deptList.Count == 0)
                {
                    ViewBag.Error = "尚未配置部门信息，请联系管理员。";
                    return View();
                }
            }
            // 组织部分下拉列表
            string sel = "";
            bool bFind = false;
            foreach (string dept in deptList)
            {
                if (dept == userItem.department)
                {
                    sel = " selected ";
                    bFind = true;
                }
                deptHtml += "<option value='" + dept + "' " + sel + ">" + dept + "</option>";
            }
            //if (deptHtml != "")
            //{
            //    deptHtml += "<option value='其它'>其它</option>";
            //}

            string displayText = "  style='display:none'  ";
            sel = "";
            if (deptHtml != "")
            {
                if (bFind == false && string.IsNullOrEmpty(userItem.department)==false)
                {
                    sel = " selected ";
                    displayText = "  style='display: block' ";
                }

                deptHtml += "<option value='其它' " + sel + ">其它</option>";
            }
            ViewBag.deptHtml = deptHtml;
            ViewBag.displayText = displayText;



            // 当前读者性别
            if (userItem.gender == "男")
            {
                ViewBag.manSel = " selected ";
            }
            else if (userItem.gender == "女")
            {
                ViewBag.womanSel = " selected ";
            }

            string pureTel = "";
            string purePhone = "";
            if (userItem != null && string.IsNullOrEmpty(userItem.phone) == false)
            {
                dp2WeiXinService.SplitTel(userItem.phone,
        out pureTel,
        out purePhone);
            }
            ViewBag.PureTel = pureTel;
            ViewBag.PurePhone = purePhone;
            return View(userItem);
        }

        /// <summary>
        /// 工作人员登记读者 或者 编辑读者信息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="libId"></param>
        /// <param name="patronBarcode"></param>
        /// <returns></returns>
        public ActionResult PatronEdit(string code, string state)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PatronEdit");
                return View();
            }

            // 必须是工作人员，且不能为public
            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Worker
                || sessionInfo.ActiveUser.userName == WxUserDatabase.C_Public)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("读者登记", "/Patron/PatronEdit", true);
                return View();
            }

            // 当前工作人员帐号
            ViewBag.userName = sessionInfo.ActiveUser.userName;

            // 管理的分馆
            string[] libraryList = sessionInfo.ActiveUser.libraryCode.Split(new[] { ',' });

            // 获取读者类别 2020/4/7更新
            string readerType = sessionInfo.GetReaderType(sessionInfo.ActiveUser,
                out strError);
            if (string.IsNullOrEmpty(strError) == false)
            {
                ViewBag.Error = strError;
                return View();
            }

            ViewBag.readerTypeHtml = this.GetReaderTypeHtml(readerType, "");//typesHtml;


            // 新增读者时，统一使用配置的数据库
            if (ViewBag.PatronDbName == "")
            {
                ViewBag.Error = "尚未配置读者库，请联系管理员。";
                return View();
            }
            ViewBag.PatronRecPath = ViewBag.PatronDbName + "/?";

            /*
            // 目标数据库
            string dbs = sessionInfo.ReaderDbnames;
            string dbsHtml = "";
            if (String.IsNullOrEmpty(dbs) == false)
            {
                string[] dbList = dbs.Split(new char[] { ',' });
                foreach (string db in dbList)
                {
                    dbsHtml += "<option value='" + db + "'>" + db + "</option>";
                }
            }
            if (dbsHtml != "")
            {
                dbsHtml = "<select id='selDbName' name='selDbName' class='selArrowRight'>"
                    + "<option value=''>请选择</option>"
                    + dbsHtml
                    + "</select>";
            }
            ViewBag.readerDbnamesHtml = dbsHtml;
            */


            return View();
        }


        /// <summary>
        /// 我的信息主界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult PersonalInfo(string code, string state)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                true,
                true,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PersonalInfo");
                return View();
            }

            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("我的信息", "/Patron/PersonalInfo");
                return View();
            }


            string patronXml = "";
            string recPath = "";

            string timestamp = "";
            nRet = this.GetReaderXml(sessionInfo.ActiveUser,
               out patronXml,
               out recPath,
               out timestamp,
               out strError);
            if (nRet == -1 || nRet == 0)
            {
                ViewBag.Error = strError;
                return View();
            }

            ViewBag.userItemId = sessionInfo.ActiveUser.id;
            ViewBag.timestamp = timestamp;


            ViewBag.overdueUrl = "../Patron/OverdueInfo";
            ViewBag.borrowUrl = "../Patron/BorrowInfo";
            ViewBag.reservationUrl = "../Patron/Reservation";

            string libId = ViewBag.LibId;
            Patron patron = null;
            patron = dp2WeiXinService.Instance.ParsePatronXml(libId,
                    patronXml,
                    recPath,
                    sessionInfo.ActiveUser.showPhoto,//ViewBag.showPhoto,
                    true);

            string comment = patron.comment;
            comment = comment.Replace("\r\n", "\n");
            comment = comment.Replace("\n", "<br/>");
            patron.comment = comment;


            return View(patron);
        }

        //违约交费信息
        public ActionResult OverdueInfo(string code, string state)
        {
            string strError = "";
            int nRet = 0;

            string patronXml = "";
            string recPath = "";


            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }
            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PersonalInfo");
                return View();
            }

            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                strError = "当前帐户不是读者帐户";
                goto ERROR1;
            }

            ViewBag.patronBarcode = sessionInfo.ActiveUser.readerBarcode;

            nRet = this.GetReaderXml(sessionInfo.ActiveUser,
               out patronXml,
               out recPath,
               out string timestamp,
               out strError);
            if (nRet == -1 || nRet == 0 || nRet == -2)
                goto ERROR1;

            string strWarningText = "";
            List<OverdueInfo> overdueList = dp2WeiXinService.Instance.GetOverdueInfo(patronXml,
                out strWarningText);

            return View(overdueList);

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        /// <summary>
        /// 预约请求界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult Reservation(string code, string state)
        {
            string strError = "";
            int nRet = 0;


            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }
            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PersonalInfo");
                return View();
            }

            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                strError = "当前帐户不是读者帐户";
                goto ERROR1;
            }
            // 放到界面的变量
            ViewBag.patronBarcode = sessionInfo.ActiveUser.readerBarcode;

            string patronXml = "";
            string recPath = "";
            nRet = this.GetReaderXml(sessionInfo.ActiveUser,
               out patronXml,
               out recPath,
               out string timestamp,
               out strError);
            if (nRet == -1 || nRet == 0 || nRet == -2)
                goto ERROR1;


            // 预约请求
            string strReservationWarningText = "";
            List<ReservationInfo> reservations = dp2WeiXinService.Instance.GetReservations(patronXml,
                out strReservationWarningText);


            return View(reservations);

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        /// <summary>
        /// 在借续借界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult BorrowInfo(string code, string state)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }
            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PersonalInfo");
                return View();
            }

            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                strError = "当前帐户不是读者帐户";
                ViewBag.Error = strError;
                return View();
            }
            ViewBag.patronBarcode = sessionInfo.ActiveUser.readerBarcode;

            string patronXml = "";
            string recPath = "";

            nRet = this.GetReaderXml(sessionInfo.ActiveUser,
                out patronXml,
                out recPath,
                out string timestamp,
                out strError);
            if (nRet == -1 || nRet == 0 || nRet == -2)
            {
                ViewBag.Error = strError;
                return View();
            }



            string strWarningText = "";
            string maxBorrowCountString = "";
            string curBorrowCountString = "";
            List<BorrowInfo2> overdueList = dp2WeiXinService.Instance.GetBorrowInfo(patronXml,
                out strWarningText,
                out maxBorrowCountString,
                out curBorrowCountString);
            ViewBag.maxBorrowCount = maxBorrowCountString;
            ViewBag.curBorrowCount = curBorrowCountString;


            return View(overdueList);


        }

        #region 内部函数



        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns>
        /// -1 出错，或者非正常途径登录
        /// -2 未绑定 
        /// -3 未设置默认账户
        /// 0 未找到读者记录
        /// 1 成功
        /// </returns>
        private int GetReaderXml(WxUserItem activeUser,
            out string patronXml,
            out string recPath,
            out string timestamp,
            out string strError)
        {
            patronXml = "";
            strError = "";
            recPath = "";
            int nRet = 0;
            timestamp = "";
            if (activeUser == null)
            {
                strError = "activeUser参数不能为空";
                return -1;
            }

            if (activeUser.type != WxUserDatabase.C_Type_Patron)
            {
                strError = "当前页面需为读者帐号";
                return -1;
            }


            string libId = activeUser.libId;
            string patronBarcode = activeUser.readerBarcode;

            //// 登录人是读者自己
            //string loginUserName = activeUser.readerBarcode;
            //bool isPatron = true;
            //LoginInfo loginInfo = new LoginInfo(loginUserName, isPatron);

            // 2020-3-17 读者修改手机号完成后，也是进入我的信息界面，但此时通道已失效，所以改为代理帐号
            LoginInfo loginInfo = new LoginInfo("", false);


            string searchWord = patronBarcode;  //检索不支持@refid，只支持@path:格式
            // 2020-3-5 读者注册时改为产生临时guid证条码号,所以这是还是用当前帐户身份
            //if (patronBarcode.Length > 7 && patronBarcode.Substring(0, 7) == "@refid:")
            //{
            //    searchWord = "@path:" + activeUser.recPath;
            //    // 采用代理帐户
            //    loginInfo = new LoginInfo("", false);
            //}

            // 获取读者记录
            //string timestamp = "";
            nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                loginInfo,
                searchWord,
                "advancexml,timestamp",  // 格式
                out recPath,
                out timestamp,
                out patronXml,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;

            activeUser.recPath = recPath; // todo 应该在绑定的时候赋值，但绑定时没有返回路径


            return 1;
        }


        #endregion


        #region 设置

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult Setting(string code, string state, string returnUrl)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/Setting");
                return View();
            }

            // 返回url
            ViewBag.returnUrl = returnUrl;

            // 是否显示头像
            string photoChecked = "";
            if (sessionInfo.ActiveUser.showPhoto == 1)  //ViewBag.showPhoto == 1)
                photoChecked = " checked='checked' ";
            ViewBag.photoChecked = photoChecked;

            // 是否显示图书封面
            string coverChecked = "";
            if (sessionInfo.ActiveUser.showCover == 1) //ViewBag.showCover == 1)
                coverChecked = " checked='checked' ";
            ViewBag.coverChecked = coverChecked;

            // 检查是否绑定工作人员，决定界面上是否出现 监控消息
            ViewBag.info = "监控本馆消息";
            string tracingChecked = "";
            string maskChecked = "";
            // 工作人员且不是public时，出现监控消息选项
            if (sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker
                && sessionInfo.ActiveUser.userName != WxUserDatabase.C_Public)
            {
                ViewBag.workerId = sessionInfo.ActiveUser.id;
                if (sessionInfo.ActiveUser.tracing == "on" || sessionInfo.ActiveUser.tracing == "on -mask")
                {
                    tracingChecked = " checked='checked' ";
                    maskChecked = " checked='checked' ";
                    if (sessionInfo.ActiveUser.tracing == "on -mask")  // 是否马赛克敏感消息
                        maskChecked = " ";
                }
            }
            ViewBag.tracingChecked = tracingChecked;
            ViewBag.maskChecked = maskChecked;

            // 如果当前馆是数字平台，数字平台维护人员可以监控所有图书馆消息
            if (ViewBag.LibName == "[" + WeiXinConst.C_Dp2003LibName + "]")
            {
                ViewBag.info = "监控所有图书馆的消息";
            }

            ViewBag.subLibGray = "";

            // 未绑定帐户 ，todo 普通读者一样可选择关注馆藏地
            if (sessionInfo.ActiveUser == null
                || sessionInfo.ActiveUser.userName == WxUserDatabase.C_Public)
            {
                ViewBag.subLibGray = "color:#cccccc";
                return View();
            }

            // 当前帐户信息显示在界面上
            string accountInfo = "";
            if (sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker)
            {
                accountInfo = "帐号:" + sessionInfo.ActiveUser.userName;
            }
            else
            {
                accountInfo = "读者:" + sessionInfo.ActiveUser.readerBarcode;
            }
            if (accountInfo != "")
            {
                accountInfo = "(" + accountInfo + ")";
            }
            ViewBag.accountInfo = accountInfo;
            ViewBag.userId = sessionInfo.ActiveUser.id;

            // 如果location为空，从服务器获取馆藏地信息
            string locationXml = sessionInfo.ActiveUser.location;
            if (String.IsNullOrEmpty(sessionInfo.ActiveUser.location) == true
                && sessionInfo.ActiveUser.userName != WxUserDatabase.C_Public)
            {
                // 从dp2服务器获取
                nRet = dp2WeiXinService.Instance.GetLocation(ViewBag.LibId,
                    sessionInfo.ActiveUser,
                   out locationXml,
                   out strError);
                if (nRet == -1)
                {
                    ViewBag.Error = strError;
                    return View();
                }


                //保存到微信用户库
                sessionInfo.ActiveUser.location = locationXml;
                WxUserDatabase.Current.Update(sessionInfo.ActiveUser);
            }


            // 解析本帐户拥有的全部馆藏地
            List<SubLib> subLibs = SubLib.ParseSubLib(locationXml, true);

            //上次选中的打上勾
            if (String.IsNullOrEmpty(sessionInfo.ActiveUser.selLocation) == false)
            {
                string selLocation = SubLib.ParseToSplitByComma(sessionInfo.ActiveUser.selLocation);
                if (selLocation != "")
                {
                    string[] selLocList = selLocation.Split(new char[] { ',' });
                    foreach (SubLib subLib in subLibs)
                    {
                        if (subLib.libCode == sessionInfo.ActiveUser.bindLibraryCode)
                        {
                            foreach (Location loc in subLib.Locations)
                            {
                                string locPath = subLib.libCode + "/" + loc.Name;
                                if (selLocList.Contains(locPath) == true)
                                {
                                    subLib.Checked = "checked";
                                    loc.Checked = "checked";
                                }
                            }
                        }
                    }
                }
                // end
            }

            // todo 其实，可以用一个字段来表示馆藏地和选中的项，就量在xml的字段中加checked属性，
            // 但如果服务器更新了，刷的时候就全部覆盖了。
            // 现在还没做到服务器更新后，自动刷过来


            ViewBag.libList = subLibs;
            ViewBag.verifyBarcode = "";
            if (sessionInfo.ActiveUser != null && sessionInfo.ActiveUser.verifyBarcode == 1)
            {
                ViewBag.verifyBarcode = "checked";
            }

            ViewBag.audioType = 1;
            if (sessionInfo.ActiveUser != null && sessionInfo.ActiveUser.audioType > 0)
            {
                ViewBag.audioType = sessionInfo.ActiveUser.audioType;
            }

            ViewBag.allowPatronBorrow = false;

            return View();

        }

        #endregion

        #region 二维码 图片

        // 二维码
        public ActionResult QRcode(string code, string state)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/QRcode");
                return View();
            }

            if (sessionInfo.ActiveUser != null
                && sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("二维码", "/Patron/QRcode");
                return View();
            }


            string strXml = "";
            string recPath = "";
            nRet = this.GetReaderXml(sessionInfo.ActiveUser,
               out strXml,
               out recPath,
               out string timestamp,
               out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            string warn = "";
            string qrcodeUrl = "";
            if (String.IsNullOrEmpty(warn) == true)
            {
                qrcodeUrl = "./getphoto?libId=" + HttpUtility.UrlEncode(sessionInfo.ActiveUser.libId)
                     + "&type=pqri"
                     + "&barcode=" + HttpUtility.UrlEncode(sessionInfo.ActiveUser.readerBarcode);
                //+ "&width=400&height=400";
            }
            ViewBag.qrcodeUrl = qrcodeUrl;


            return View(sessionInfo.ActiveUser);
        }

        // 图片
        public ActionResult GetPhoto(string code, string state, string libId, string type, string barcode, string objectPath)
        {
            MemoryStream ms = new MemoryStream(); ;
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)//???为什么要做这一句 2020-2-8 && ViewBag.LibState != LibraryManager.C_State_Hangup1)
            {
                goto ERROR1;
            }


            // 读者二维码
            if (type == "pqri")
            {
                // 设置媒体类型
                Response.ContentType = "image/jpeg";

                // 获得读者证号二维码字符串
                string strCode = "";
                nRet = dp2WeiXinService.Instance.GetQRcode(libId,
                    barcode,
                    out strCode,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                // 获得二维码图片
                string strWidth = Request.QueryString["width"];
                string strHeight = Request.QueryString["height"];
                int nWidth = 0;
                if (string.IsNullOrEmpty(strWidth) == false)
                {
                    if (Int32.TryParse(strWidth, out nWidth) == false)
                    {
                        strError = "width 参数 '" + strWidth + "' 格式不合法";
                        goto ERROR1;
                    }
                }
                int nHeight = 0;
                if (string.IsNullOrEmpty(strHeight) == false)
                {
                    if (Int32.TryParse(strHeight, out nHeight) == false)
                    {
                        strError = "height 参数 '" + strHeight + "' 格式不合法";
                        goto ERROR1;
                    }
                }
                dp2WeiXinService.Instance.GetQrImage(strCode,
                   nWidth,
                   nHeight,
                   Response.OutputStream,
                   out strError);
                if (strError != "")
                    goto ERROR1;
                return null;
            }

            // 取头像 或 封面
            string weixinId = ViewBag.weixinId;
            nRet = dp2WeiXinService.GetObject0(this, libId, weixinId, objectPath, out strError);
            if (nRet == -1)
                goto ERROR1;

            return null;


        ERROR1:

            ms = dp2WeiXinService.Instance.GetErrorImg(strError);
            return File(ms.ToArray(), "image/jpeg");
        }

        // 资源
        public ActionResult GetObject(string code, string state, string libId, string uri)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1) //??? 为什么加后面部分 2020-2-8 && ViewBag.LibState != LibraryManager.C_State_Hangup1)
            {
                goto ERROR1;
            }

            //处理 dp2 系统外部的 URL
            Uri tempUri = dp2WeiXinService.GetUri(uri);
            if (tempUri != null
                && (tempUri.Scheme == "http" || tempUri.Scheme == "https"))
            {
                return Redirect(uri);
            }

            string weixinId = ViewBag.weixinId;
            nRet = dp2WeiXinService.GetObject0(this, libId, weixinId, uri, out strError);
            if (nRet == -1)
                goto ERROR1;

            return null;


        ERROR1:
            MemoryStream ms = dp2WeiXinService.Instance.GetErrorImg(strError);
            return File(ms.ToArray(), "image/jpeg");
        }


        #endregion
    }
}