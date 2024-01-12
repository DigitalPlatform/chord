using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2weixin.service
{
    public class AreaManager
    {
        public List<Area> _areas = new List<Area>();
        string _libcfgfile = "";

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="libcfgFile"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public int init(string libcfgFile, out string error)
        {
            error = "";

            this._libcfgfile = libcfgFile;

            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(this._libcfgfile);
                XmlNode root = dom.DocumentElement;

                XmlNodeList areaNodes = root.SelectNodes("area");
                foreach (XmlNode areaNode in areaNodes)
                {
                    string areaName = DomUtil.GetAttr(areaNode, "name");
                    Area area = new Area();
                    area.name = areaName;

                    //int daoQiLibCout = 0;

                    XmlNodeList libNodes = areaNode.SelectNodes("lib");
                    foreach (XmlNode libNode in libNodes)
                    {
                        string id = DomUtil.GetAttr(libNode, "id");
                        string name = DomUtil.GetAttr(libNode, "name");
                        string libraryCode = DomUtil.GetAttr(libNode, "libraryCode");
                        string patronDbName = DomUtil.GetAttr(libNode, "patronDbName");

                        //2020-3-6 增加部门配置，方便读者注册时选择
                        string departments = DomUtil.GetAttr(libNode, "departments");

                        // 2020-6-5 增加证条码尾号
                        string patronBarcoeTail = DomUtil.GetAttr(libNode, "patronBarcodeTail");

                        // 2020-8-24 增加转发到第三方接口名称
                        string noticedll = DomUtil.GetAttr(libNode, "noticedll");

                        // 2021-7-21 增加单一绑定开关
                        string bindStyle = DomUtil.GetAttr(libNode, "bindStyle");

                        // 2021-8-3 针对收到的通知中的读者姓名和证条码号mask
                        string patronMaskValue = DomUtil.GetAttr(libNode, "patronMaskValue");

                        // 2022/10/13 简编字段规则
                        string fieldsMap= DomUtil.GetAttr(libNode, "fieldsMap");

                        // 2022/10/13 目标数据库
                        string biblioDbName = DomUtil.GetAttr(libNode, "biblioDbName");

                        // 2024/1/12 
                        string searchAllBiblio = DomUtil.GetAttr(libNode, "searchAllBiblio");

                        LibModel lib = new LibModel();
                        lib.libId = id;
                        lib.name = name;
                        lib.libraryCode = libraryCode;
                        lib.patronDbName = patronDbName; //2020-2-29 读者注册对应的读者库
                        lib.departments = departments;//2020-3-6
                        lib.patronBarcodeTail = patronBarcoeTail;//2020-6-5
                        lib.noticedll = noticedll;
                        lib.bindStyle = bindStyle;
                        lib.patronMaskValue = patronMaskValue; //2021/8/3增加
                        lib.fieldsMap = fieldsMap;//2022/10/13加
                        lib.biblioDbName = biblioDbName;//2022/10/13 加
                        lib.searchAllBiblio = searchAllBiblio;


                        area.libs.Add(lib);
                    }

                    if (area.libs.Count > 0)
                    {
                        this._areas.Add(area);
                    }
                }
            }
            catch (Exception ex)
            {
                error = "初始化图书馆配置文件出错:" + ex.Message;
                return -1;

            }

            return 0;
        }



        public void SaveLib(LibEntity entity)
        {

           //dp2WeiXinService.Instance.WriteDebug("41");

            // 2021/7/9 先找到这个老实例 ，以免丢信息
            LibModel oldLib= this.GetLibCfgByName(entity.id, entity.libName);

            //dp2WeiXinService.Instance.WriteDebug("42");
            // 先将已经对应的删除
            DelLib(entity.id, entity.libName);

            //dp2WeiXinService.Instance.WriteDebug("43");

            // 先查一下有没有对应的地区
            Area area = this.GetArea(entity.area);
            if (area == null)
            {
                area = new Area();
                area.name = entity.area;
                this._areas.Add(area);
            }
            //dp2WeiXinService.Instance.WriteDebug("44");

            LibModel lib = area.GetLib(entity.id, entity.libName);
            //dp2WeiXinService.Instance.WriteDebug("45");
            if (lib == null)
            {
                lib = new LibModel();
                lib.libId = entity.id;
                lib.name = entity.libName;
                area.libs.Add(lib);

                // todo，这里有bug，会把原来配置的信息清掉
                if (oldLib != null)
                {
                    lib.libraryCode = oldLib.libraryCode;

                    lib.departments = oldLib.departments;
                    lib.patronDbName = oldLib.patronDbName;
                    lib.patronBarcodeTail = oldLib.patronBarcodeTail;
                    lib.noticedll = oldLib.noticedll;
                    lib.bindStyle = oldLib.bindStyle;
                    lib.patronMaskValue = oldLib.patronMaskValue; //2021/8/3 增加屏蔽通知中的读者信息
                    lib.fieldsMap=oldLib.fieldsMap;//2022/10/13加
                    lib.biblioDbName = oldLib.biblioDbName;//2022/10/13加
                    lib.searchAllBiblio=oldLib.searchAllBiblio;//2024/1/12

                    //lib.capoUser = oldLib.capoUser; // 20220720-ryh 发现这个字段没有使用到
                    //lib.visible = oldLib.visible;   // 20220720-ryh 发现这个字段没有使用到
                    lib.Checked = oldLib.Checked;
                    lib.bindFlag = oldLib.bindFlag;
                }

                //dp2WeiXinService.Instance.WriteDebug("46");
                this.Save2Xml();

                //dp2WeiXinService.Instance.WriteDebug("47");
            }
        }

        public void Save2Xml()
        {
            //dp2WeiXinService.Instance.WriteDebug("461");

            string xml = "";
            foreach (Area area in this._areas)
            {
                xml += "<area name='" + area.name + "'>";

                if (area.libs != null)
                {
                    foreach (LibModel lib in area.libs)
                    {
                        xml += "<lib id='" + lib.libId + "'"
                            + " name='" + lib.name + "'"
                            + " libraryCode='" + lib.libraryCode + "'"
                            + " patronDbName='" + lib.patronDbName + "'"
                            + " departments='" + lib.departments + "'"
                            + " patronBarcodeTail='" + lib.patronBarcodeTail + "'" //2020/6/5增加证条码号尾号
                            + " noticedll='" + lib.noticedll + "'" //2020/8/24 转发通知到第三方的接口
                            + " bindStyle='" + lib.bindStyle + "'" // 2021/7/21 增加单一绑定开关
                            + " patronMaskValue='" + lib.patronMaskValue + "'" // 2021/8/3 增加通知中屏幕读者信息
                            + " fieldsMap='" + lib.fieldsMap + "'"  // 2022/10/13 增加编目配置的字段规则
                            + " biblioDbName='" + lib.biblioDbName + "'"  //2022/10/13 加
                            + " searchAllBiblio='"+lib.searchAllBiblio + "'" //2024/1/12


                            + " />";
                    }
                }
                xml += "</area>";
            }
            //dp2WeiXinService.Instance.WriteDebug("462");
            xml = "<root>" + xml + "</root>";

            //dp2WeiXinService.Instance.WriteDebug("463");

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            //dp2WeiXinService.Instance.WriteDebug("464");

            //if (this._libcfgfile == null)
            //{
            //    dp2WeiXinService.Instance.WriteDebug("_libcfgfile=null");
            //}
            //else
            //    dp2WeiXinService.Instance.WriteDebug("_libcfgfile=["+this._libcfgfile+"]");

            try
            {
                dom.Save(this._libcfgfile);
            }
            catch(Exception ex)
            {
                dp2WeiXinService.Instance.WriteDebug("异常："+ex.Message);
                throw ex;
            }

           // dp2WeiXinService.Instance.WriteDebug("465");
        }

        public Area GetArea(string name)
        {
            foreach (Area area in this._areas)
            {
                if (area.name == name)
                    return area;
            }
            return null;
        }


        public void DelLib(string id, string name)
        {
            List<Area> delAreas = new List<Area>();

            if (this._areas==null || this._areas.Count==0)
                   return;

            foreach (Area area in this._areas)
            {
                List<LibModel> delLibs = new List<LibModel>();
                foreach (LibModel lib in area.libs)
                {
                    if (lib.libId == id && lib.name == name)
                    {
                        delLibs.Add(lib);
                    }
                }

                foreach (LibModel lib in delLibs)
                {
                    area.libs.Remove(lib);
                }

                if (area.libs.Count == 0)
                    delAreas.Add(area);

            }

            foreach (Area area in delAreas)
            {
                this._areas.Remove(area);
            }
        }


        public LibModel GetLibCfg(string id, string libraryCode)
        {
            // 把null转为空字符串，这样才能查找
            if (libraryCode == null)
                libraryCode = "";

            List<Area> delAreas = new List<Area>();

            foreach (Area area in this._areas)
            {
                List<LibModel> delLibs = new List<LibModel>();
                foreach (LibModel lib in area.libs)
                {
                    if (lib.libId == id && lib.libraryCode == libraryCode)
                    {
                        return lib;
                    }
                }
            }
            return null;
        }


        public LibModel GetLibCfgByName(string id, string libName)
        {
            // 把null转为空字符串，这样才能查找
            if (libName == null)
                libName = "";

            List<Area> delAreas = new List<Area>();

            foreach (Area area in this._areas)
            {
                List<LibModel> delLibs = new List<LibModel>();
                foreach (LibModel lib in area.libs)
                {
                    if (lib.libId == id && lib.name == libName)
                    {
                        return lib;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取配置的部门 2020-3-6
        /// </summary>
        /// <param name="id"></param>
        /// <param name="libraryCode"></param>
        /// <returns></returns>
        public List<string> GetDeptartment(string id, string libraryCode)
        {
            List<string> deptList = new List<string>();
            LibModel lib = this.GetLibCfg(id, libraryCode);
            if (lib != null)
            {
                string[] depts = lib.departments.Trim().Split(new char[] {','});
                foreach (string dept in depts)
                {
                    string temp = dept.Trim();
                    if (temp == "")
                        continue;

                    deptList.Add(dept);
                }
            }
            return deptList;
        }
    }

    // 地区结构
    public class Area
    {
        // 地区名称
        public string name = "";

        // 2022/07/20-ryh，在整理接口的过程中，发现未使用这个字段。
        // 是否显示
        public bool visible = true;

        // 下级图书馆
        public List<LibModel> libs = new List<LibModel>();

        // 根据图书馆id和名称获取下级图书馆
        public LibModel GetLib(string id, string name)
        {
            foreach (LibModel lib in this.libs)
            {
                if (lib.libId == id && lib.name == name)
                    return lib;
            }
            return null;
        }
    }

    // 图书馆配置信息结构
    // 由于总分馆是一套系统，在我爱图书馆后台配置图书馆时，图书馆的基本信息是存储在mongodb中，一个图书馆实例一条记录
    // 同时还将一些信息存储在libcfg文件，在libcfg才能配置分馆信息，因为多个分馆对应的是一个图书馆实例，通过libid与mongodb对应。
    // 这个libMode的数据是从libcfg配置文件来的。
    public class LibModel
    {
        // 图书馆id
        public string libId = "";

        // 图书馆名称
        public string name = "";

        // 图书馆 馆代码，针对分馆有意义
        public string libraryCode = "";

        // 2020-2-29 renyh
        // 读者自助注册读者帐户时，对应的读者库
        // 这个配置还只能放在libcfg配置文件中，因为每个分馆的读者库不同，不能定义在图书馆实例的mongodb表中
        public string patronDbName = "";
        public string departments = "";
        // 2020/6/5 加证条码尾号，在馆员审核时，可以点按钮，增量
        public string patronBarcodeTail = "B000000";

        // 2020/8/24 增加第三方dll
        public string noticedll = "";

        // 2021/7/21 指定是否只能绑定单一的手机
        public string bindStyle = "";

        // 2021/8/3 屏幕通知中的读者信息
        public string patronMaskValue = "";

        // 2022/10/13 增加简编的字段规则
        public string fieldsMap = "";

        // 2022/10/13 增加目标数据库，用于新增书目
        public string biblioDbName = "";

        // 2024/1/12 是否检索全部书目
        public string searchAllBiblio = "0";

        // 2022/07/20 在整理接口过程中发现capoUser这个字段没有使用到，所以注释掉
        // 对应的capo帐户
        //public string capoUser = "";

        // 是否显示出来，20220720整理接口时，发现这个字段未使用。
        public bool visible = true;

        // 下面这两个字段是根据当前用户的绑定信息来的，不是存储在配置文件和图书馆mongodb库
        // 勾选标记
        public string Checked = "";
        // 绑定标记
        public string bindFlag = "";

        // 图书馆完整名称，图书馆id~馆代码
        public string FullLibId
        {
            get
            {
                if (string.IsNullOrEmpty(libraryCode) == true)
                    return libId;
                else
                    return libId + "~" + libraryCode;
            }
        }
    }



}
