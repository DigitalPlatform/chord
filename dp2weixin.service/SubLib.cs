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
        public List<Area> areas = new List<Area>();
        string file = "";

        public int init(string libcfgFile, out string error)
        {
            error = "";

            this.file = libcfgFile;

            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(this.file);
                XmlNode root = dom.DocumentElement;
                XmlNodeList areaNodes = root.SelectNodes("area");
                foreach (XmlNode areaNode in areaNodes)
                {
                    string areaName = DomUtil.GetAttr(areaNode, "name");
                    Area area = new Area();
                    area.name = areaName;
                    this.areas.Add(area);

                    XmlNodeList libNodes = areaNode.SelectNodes("lib");
                    foreach (XmlNode libNode in libNodes)
                    {
                        string id = DomUtil.GetAttr(libNode, "id");
                        string name = DomUtil.GetAttr(libNode, "name");
                        string libraryCode = DomUtil.GetAttr(libNode, "libraryCode");
                        libModel lib = new libModel();
                        lib.libId = id;
                        lib.name = name;
                        lib.libraryCode = libraryCode;
                        area.libs.Add(lib);
                    }

                }
            }
            catch(Exception ex)
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
                this.areas.Add(area);
            }

            libModel lib = area.GetLib(entity.id,entity.libName);
            if (lib == null)
            {
                lib = new libModel();
                lib.libId = entity.id;
                lib.name = entity.libName;
                area.libs.Add(lib);

                this.Save2Xml();
            }
        }

        public void Save2Xml()
        {
            string xml = "";
            foreach (Area area in this.areas)
            {
                xml += "<area name='"+area.name+"'>";

                foreach (libModel lib in area.libs)
                {
                    xml += "<lib id='"+lib.libId+"' name='"+lib.name+"' libraryCode='"+lib.libraryCode+"'/>";
                }
                xml += "</area>";
            }
            xml = "<root>" + xml + "</root>";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            dom.Save(this.file);

        }

        public Area GetArea(string name)
        {
            foreach (Area area in this.areas)
            {
                if (area.name == name)
                    return area;
            }
            return null;
        }


        public void DelLib(string id, string name)
        {
            List<Area> delAreas = new List<Area>();

            foreach (Area area in this.areas)
            {
                List<libModel> delLibs = new List<libModel>();
                foreach (libModel lib in area.libs)
                {
                    if (lib.libId == id && lib.name == name)
                    {
                        delLibs.Add(lib);
                    }
                }

                foreach (libModel lib in delLibs)
                {
                    area.libs.Remove(lib);
                }

                if (area.libs.Count == 0)
                    delAreas.Add(area);

            }

            foreach (Area area in delAreas)
            {
                this.areas.Remove(area);
            }
        }
    }

    public class Area
    {
        public string name = "";
        public List<libModel> libs = new List<libModel>();

        public libModel GetLib(string id,string name)
        {
            foreach (libModel lib in this.libs)
            {
                if (lib.libId == id && lib.name==name)
                    return lib;
            }
            return null;
        }


    }

    public class libModel
    {
        public string name = "";

        public string Checked = "";
        public string bindFlag = "";

        public string capoUser = "";
        public string libId = "";
        public string libraryCode = "";

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


    public class Location
    {
        public string Name = "";
        public string Checked = "";
        public string Path = "";

        public Location(string name,string libCode)
        {
            this.Name = name;
            this.Path = libCode + "/" + name;
        }
 
    }
    public class SubLib
    {
        public string libCode = "";
        public List<Location> Locations = new List<Location>();
        public string Checked = "";

        public static List<SubLib> ParseSubLib(string xml,bool bAddRoot)
        {
            List<SubLib> subLibs = new List<SubLib>();
            /*
<item canborrow="no" itemBarcodeNullable="yes">保存本库</item>
<item canborrow="no" itemBarcodeNullable="yes">阅览室</item>
<item canborrow="yes" itemBarcodeNullable="yes">流通库</item>
<library code="方洲小学">
  <item canborrow="yes" itemBarcodeNullable="yes">图书总库</item>
</library>
<library code="星洲小学">
  <item canborrow="yes" itemBarcodeNullable="yes">阅览室</item>
</library>
             * 
==样例2==
  <library code="方洲小学">
    <item canborrow="yes" itemBarcodeNullable="yes">图书总库</item>
  </library>
  <library code="星洲小学">
    <item canborrow="yes" itemBarcodeNullable="yes">阅览室</item>
  </library>
            */

            // 将xml用<root>包起来
            if (bAddRoot==true)
                xml = "<root>" + xml + "</root>";
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch
            {
                return subLibs;
            }
            XmlNode root = dom.DocumentElement;

            // 第一层的item，没有分馆代码，一般表示总馆
            XmlNodeList level1ItemList = root.SelectNodes("item");
            if (level1ItemList.Count > 0)
            {
                SubLib subLib = new SubLib();
                subLib.libCode = "";
                foreach (XmlNode item in level1ItemList)
                {
                    // todo 不参与流通的库，是否需要过滤???
                    string location = DomUtil.GetNodeText(item);
                    Location loc = new Location(location, subLib.libCode);
                    subLib.Locations.Add(loc);
                }
                subLibs.Add(subLib);
            }

            // 分馆
            XmlNodeList libList = root.SelectNodes("library");
            foreach (XmlNode lib in libList)
            {
                SubLib subLib = new SubLib();
                subLib.libCode = DomUtil.GetAttr(lib, "code");

                XmlNodeList itemList = lib.SelectNodes("item");
                foreach (XmlNode item in itemList)
                {
                    // todo 不参与流通的库，是否需要过滤???
                    string location = DomUtil.GetNodeText(item);
                    Location loc = new Location(location, subLib.libCode);
                    subLib.Locations.Add(loc);
                }
                subLibs.Add(subLib);
            }



            return subLibs;
        }

        public static string ParseToSplitByComma(string xml)
        {
            string result = "";
            List<SubLib> libs = ParseSubLib(xml, false);
            foreach (SubLib lib in libs)
            {
                foreach (Location loc in lib.Locations)
                {
                    if (result != "")
                        result += ",";

                    result += lib.libCode + "/" + loc.Name;
                }
            }
            return result;
        }

        public static string ParseToView(string xml)
        {
            string result = "";
            List<SubLib> libs = ParseSubLib(xml, false);
            foreach (SubLib lib in libs)
            {
                if (result != "")
                    result += ",";

                string one = lib.libCode;

                string tempLocs = "";
                foreach (Location loc in lib.Locations)
                {
                    if (tempLocs != "")
                        tempLocs += ",";

                    tempLocs +=loc.Name;
                }

                if (tempLocs != "")
                    one = lib.libCode + "(" + tempLocs + ")";

                result += one;
            }
            return result;
        }
    }
}
