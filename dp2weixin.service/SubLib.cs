using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2weixin.service
{
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

        public static List<SubLib> ParseSubLib(string xml)
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
            xml = "<root>" + xml + "</root>";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
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
    }
}
