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


                        LibModel lib = new LibModel();
                        lib.libId = id;
                        lib.name = name;
                        lib.libraryCode = libraryCode;
                        lib.patronDbName = patronDbName; //2020-2-29 读者注册对应的读者库
                        lib.departments = departments;//2020-3-6
                        lib.patronBarcodeTail = patronBarcoeTail;//2020-6-5

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
            // 先将已经对应的删除
            DelLib(entity.id, entity.libName);


            // 先查一下有没有对应的地区
            Area area = this.GetArea(entity.area);
            if (area == null)
            {
                area = new Area();
                area.name = entity.area;
                this._areas.Add(area);
            }

            LibModel lib = area.GetLib(entity.id, entity.libName);
            if (lib == null)
            {
                lib = new LibModel();
                lib.libId = entity.id;
                lib.name = entity.libName;
                area.libs.Add(lib);

                this.Save2Xml();
            }
        }

        public void Save2Xml()
        {
            string xml = "";
            foreach (Area area in this._areas)
            {
                xml += "<area name='" + area.name + "'>";

                foreach (LibModel lib in area.libs)
                {
                    xml += "<lib id='" + lib.libId +"'"
                        + " name='" + lib.name + "'"
                        + " libraryCode='" + lib.libraryCode + "'"
                        + " patronDbName='" + lib.patronDbName + "'"
                        + " departments='"+lib.departments+"'"
                        + " patronBarcodeTail='"+ lib.patronBarcodeTail+"'" //2020/6/5增加证条码号尾号
                        + " />";
                }
                xml += "</area>";
            }
            xml = "<root>" + xml + "</root>";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            dom.Save(this._libcfgfile);

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

    public class Area
    {
        public string name = "";
        public List<LibModel> libs = new List<LibModel>();
        public bool visible = true;

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

    public class LibModel
    {
        // 这3个属性是配置文件定义的
        public string libId = "";
        public string name = "";
        public string libraryCode = "";

        // 2020-2-29任延华
        // 读者自助注册读者帐户时，对应的读者库
        // 这个配置还只能放在libcfg配置文件中，因为每个分馆的读者库不同，不能定义在图书馆实例的mongodb表中
        public string patronDbName = "";

        public string departments = "";

        // 2020/6/5 加证条码尾号，在馆员审核时，可以点按钮，增量
        public string patronBarcodeTail = "B000000";

        // 2020/8/24 增加第三方dll
        public string noticedll = "";


        // 对应的capo帐户
        public string capoUser = "";

        // 是否显示出来
        public bool visible = true;

        // 勾选标记
        public string Checked = "";
        public string bindFlag = "";

        // 完整名称
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
