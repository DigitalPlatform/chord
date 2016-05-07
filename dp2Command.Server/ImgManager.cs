using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Command.Service
{
    public class ImgManager
    {
        XmlDocument imgDom = null;
        string imgFileName = "";

        Dictionary<string, string> imgDict = new Dictionary<string, string>();

        /// <summary>
        /// 把文件解析到内存中
        /// </summary>
        /// <param name="strImgFile"></param>
        public ImgManager(string strImgFile)        
        {
            imgFileName = strImgFile;
            imgDom = new XmlDocument();

            /*
            if (File.Exists(strImgFile) == true)
            {
                File.Delete(strImgFile);
                // 创建一个根元素
                XmlNode root = imgDom.CreateElement("root");
                imgDom.AppendChild(root);
            }
             */


            
            if (File.Exists(strImgFile) == true)
            {
                imgDom.Load(strImgFile);
                XmlNodeList nodeList = imgDom.DocumentElement.SelectNodes("img");

                bool bChange = false;
                foreach (XmlNode node in nodeList)
                {
                    string no = DomUtil.GetAttr(node, "no");
                    string url = DomUtil.GetAttr(node, "url");

                    if (String.IsNullOrEmpty(no) == false && String.IsNullOrEmpty(url) == false)
                    {
                        imgDict[no] = url;
                    }
                    else
                    {
                        imgDom.DocumentElement.RemoveChild(node);
                        bChange = true;
                    }
                }

                if (bChange == true)
                {
                    imgDom.Save(this.imgFileName);
                }
            }

            else
            {
                // 创建一个根元素
                XmlNode root = imgDom.CreateElement("root");
                imgDom.AppendChild(root);
            }
             
        }

        /// <summary>
        /// 获取当前的背景图url
        /// </summary>
        /// <returns></returns>
        public string GetTodayImgUrl()
        {
            string todayNo = DateTime.Now.Day.ToString();
            return GetImgUrl(todayNo);
        }

        /// <summary>
        /// 获取指定天的背景图url
        /// </summary>
        /// <returns></returns>
        public string GetImgUrl(string strNo)
        {


            // 先从本地记录找，有则直接返回
            if (imgDict.Keys.Contains(strNo) == true)
            {
                return imgDict[strNo];
            }

            //通过网络取url
            //我们可以通过访问：http://cn.bing.com/HPImageArchive.aspx?format=xml&idx=0&n=1获得一个XML文件，里面包含了图片的地址。
            //上面访问参数的含义分别是：
            //1、format，非必要。返回结果的格式，不存在或者等于xml时，输出为xml格式，等于js时，输出json格式。
            //2、idx，非必要。不存在或者等于0时，输出当天的图片，-1为已经预备用于明天显示的信息，1则为昨天的图片，idx最多获取到前16天的图片信息。
            //3、n，必要参数。这是输出信息的数量。比如n=1，即为1条，以此类推，至多输出8条。
            //在返回的XML文件中我们通过访问images->image->url获得图片地址，然后通过http://s.cn.bing.net/获得的图片地址进行访问
            string url = "http://cn.bing.com/HPImageArchive.aspx?format=xml&idx=0&n=1";
            WebClient client = new WebClient();
            byte[] result = client.DownloadData(url);
            string strResult = Encoding.UTF8.GetString(result);

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strResult);
            XmlNode node = dom.SelectSingleNode("images/image/url");
            string imgUrl = DomUtil.GetNodeText(node);
            imgUrl = "http://s.cn.bing.net/" + imgUrl;

            // 设到字典
            imgDict[strNo] = imgUrl;

            // 保存到dom
            XmlNode imgNode = imgDom.CreateElement("img");
            DomUtil.SetAttr(imgNode, "no", strNo);
            DomUtil.SetAttr(imgNode, "url", imgUrl);
            imgDom.DocumentElement.AppendChild(imgNode);
            imgDom.Save(this.imgFileName);
            return imgUrl;
        }


    }
}
