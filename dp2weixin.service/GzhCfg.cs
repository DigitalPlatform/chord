using DigitalPlatform.Xml;
using Senparc.Weixin.MP.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2weixin.service
{
    public class GzhContainer:List<GzhCfg>
    {
        // 构造函数
        public  GzhContainer(XmlDocument dom)
        {
            XmlNode gzhsNode = dom.DocumentElement.SelectSingleNode("gzhs");
            if (gzhsNode == null)
                return;

            XmlNodeList gzhList = gzhsNode.SelectNodes("gzh");
            foreach (XmlNode node in gzhList)
            {
                GzhCfg gzh = new GzhCfg(node);
                this.Add(gzh);
            } 
        }

        /// <summary>
        /// 通过appid得到一个公众号对象
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public GzhCfg GetByAppId(string appId)
        {
            foreach (GzhCfg gzh in this)
            {
                if (gzh.appId == appId)
                    return gzh;
            }
            return null;
        }

        /// <summary>
        /// 通过公众号名称得到公众号
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public GzhCfg GetByAppName(string appName)
        {
            if (string.IsNullOrEmpty(appName) == true)
                return null;

            foreach (GzhCfg gzh in this)
            {
                if (gzh.appName == appName)
                    return gzh;
            }
            return null;
        }

        public GzhCfg GetDefault()
        {

            foreach (GzhCfg gzh in this)
            {
                if (gzh.isDefault == true)
                    return gzh;
            }

            // 未设默认的，返回第一项
            if (this.Count > 0)
                return this[0];

            return null;
        }
    }

    //公众号配置信息
    public class GzhCfg
    {


        public string appName = "";
        public string appNameCN = "";
        public string appId { get; set; }
        public string secret { get; set; }
        public string token { get; set; }
        public string encodingAESKey { get; set; }

        public bool isDefault = false;

        private XmlNode _node = null;

        // 微信通知类型，到时根据此类型转为对应的模板消息id
        public const string C_Template_Bind = "Bind";
        public const string C_Template_UnBind = "UnBind";
        public const string C_Template_Borrow = "Borrow";
        public const string C_Template_Return = "Return";

        public const string C_Template_Pay = "Pay";
        public const string C_Template_CancelPay = "CancelPay";
        public const string C_Template_Message = "Message";
        public const string C_Template_Arrived = "Arrived";
        public const string C_Template_CaoQi = "CaoQi";
        public const string C_Template_KuaiCaoQi = "KuaiCaoQi";
        public const string C_Template_CancelReserve = "CancelReserve";

        // 2020-3-1增加 审核读者
        public const string C_Template_ReviewPatron = "ReviewPatron";
        // 2020-3-7加 审核结果
        public const string C_Template_ReviewResult = "ReviewResult";
        // 2020-3-17 加 读者信息变更
        public const string C_Template_PatronInfoChanged = "PatronInfoChanged";

        public GzhCfg(XmlNode node)
        {
            this._node = node;

            this.appName = DomUtil.GetAttr(node, "appName");
            this.appNameCN = DomUtil.GetAttr(node, "appNameCN");

            this.appId = DomUtil.GetAttr(node, "appId"); 
            this.secret = DomUtil.GetAttr(node, "secret"); 
            this.token = DomUtil.GetAttr(node, "token");
            this.encodingAESKey = DomUtil.GetAttr(node, "encodingAESKey");
            if (this.appName == "")
                throw new Exception("尚未定义公众号名称");

            string isDefaultText = DomUtil.GetAttr(node, "isDefault").ToLower();
            if (isDefaultText == "true")
                isDefault = true;

            //// 模板id,todo 模板名称有空变成常量，与配置文件对应
            //this.Template_Bind_Id = this.GetTemplateId(node, "Bind");
            //this.Template_UnBind_Id = this.GetTemplateId(node, "UnBind");
            //this.Template_Borrow_Id = this.GetTemplateId(node, "Borrow");
            //this.Template_Return_Id = this.GetTemplateId(node, "Return");
            //this.Template_Pay_Id = this.GetTemplateId(node, "Pay");
            //this.Template_CancelPay_Id = this.GetTemplateId(node, "CancelPay");
            //this.Template_Message_Id = this.GetTemplateId(node, "Message");
            //this.Template_Arrived_Id = this.GetTemplateId(node, "Arrived");
            //this.Template_CaoQi_Id = this.GetTemplateId(node, "CaoQi");

            //全局只需注册一次
            AccessTokenContainer.RegisterAsync(this.appId, this.secret);
            dp2WeiXinService.Instance.WriteDebug("注册"+this.appName+"-"+this.appId+"-"+this.secret);
        }


        // 得到模板id
        public string GetTemplateId(string name)
        {
            string id = "";
            XmlNode templateNode = this._node.SelectSingleNode("templates/template[@name='" + name + "']");
            if (templateNode != null)
            { 
                id=DomUtil.GetAttr(templateNode, "id");
            }
            return id;
        }
    }
}
