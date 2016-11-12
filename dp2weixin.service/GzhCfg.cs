using DigitalPlatform.Xml;
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
            foreach (GzhCfg gzh in this)
            {
                if (gzh.applName == appName)
                    return gzh;
            }
            return null;
        }
    }

    //公众号配置信息
    public class GzhCfg
    {
        public string applName = "";
        public string appId { get; set; }
        public string secret { get; set; }
        public string token { get; set; }
        public string encodingAESKey { get; set; }

        public bool isDefault = false;

        // 模板消息id
        public string Template_Bind_Id = "";//微信绑定通知        
        public string Template_UnBind_Id = "";// 微信解绑通知        
        public string Template_Borrow_Id = "";//借书成功        
        public string Template_Return_Id = "";//归书成功       
        public string Template_Pay_Id = ""; //交费成功        
        public string Template_CancelPay_Id = "";//撤消交费成功        
        public string Template_Message_Id = "";//个人消息通知         
        public string Template_Arrived_Id = "";//预约到书通知         
        public string Template_CaoQi_Id = "";//超期通知 

        /*
          <gzhs>
            <gzh appName="ilovelibrary" appId="wx57aa3682c59d16c2" 
                 secret="5d1a0507f05be41a56e27c632c0a808d" token="dp3weixin" 
                 encodingAESKey="ReQ72EHh7KkROvs1AEE5IK76py9oHmhRtVs30ur2DlD" isDefault="true" >
              <templates>
                <template name='Bind' id="hFmNH7on2FqSOAiYPZVJN-FcXBv4xpVLBvHsfpLLQKU"/>
                <template name="UnBind" id="1riAKkt2W0AOtkx5rx-Lwa0RKRydDTHaMjSoUBGuHog"/>
                <template name="Borrow" id="2AVbpcn0y1NtqIQ7X7KY1Ebcyyhx6mUXTpAxuOcxSE0"/>
                <template name="Return" id="zzlLzStt_qZlzMFhcDgRm8Zoi-tsxjWdsI2b3FeoRMs"/>
                <template name="Pay" id="xFg1P44Hbk_Lpjc7Ds4gU8aZUqAlzoKpoeixtK1ykBI"/>
                <template name="CancelPay" id="-XsD34ux9R2EgAdMhH0lpOSjcozf4Jli_eC86AXwM3Q"/>
                <template name="Message" id="rtAx0BoUAwZ3npbNIO8Y9eIbdWO-weLGE2iOacGqN_s"/>
                <template name="Arrived" id="U79IrJOgNJWZnqeKy2467ZoN-aM9vrEGQf2JJtvdBPM"/>
                <template name="CaoQi" id="2sOCuATcFdSNbJM24zrHnFv89R3D-cZFIpk4ec_Irn4"/>
              </templates>
            </gzh>
         </gzhs>
         */
        public GzhCfg(XmlNode node)
        {
            this.applName = DomUtil.GetAttr(node, "AppName");
            this.appId = DomUtil.GetAttr(node, "AppId"); 
            this.secret = DomUtil.GetAttr(node, "Secret"); 
            this.token = DomUtil.GetAttr(node, "Token");
            this.encodingAESKey = DomUtil.GetAttr(node, "EncodingAESKey");
            if (this.applName == "")
                throw new Exception("尚未定义公众号名称");

            // 模板id,todo 模板名称有空变成常量，与配置文件对应
            this.Template_Bind_Id = this.GetTemplateId(node, "Bind");
            this.Template_UnBind_Id = this.GetTemplateId(node, "UnBind");
            this.Template_Borrow_Id = this.GetTemplateId(node, "Borrow");
            this.Template_Return_Id = this.GetTemplateId(node, "Return");
            this.Template_Pay_Id = this.GetTemplateId(node, "Pay");
            this.Template_CancelPay_Id = this.GetTemplateId(node, "CancelPay");
            this.Template_Message_Id = this.GetTemplateId(node, "Message");
            this.Template_Arrived_Id = this.GetTemplateId(node, "Arrived");
            this.Template_CaoQi_Id = this.GetTemplateId(node, "CaoQi");
        }


        // 得到模板id
        private string GetTemplateId(XmlNode root, string type)
        {
            string id = "";
            XmlNode templateNode = root.SelectSingleNode("templates/template[@name='" + type + "']");
            if (templateNode != null)
            { 
                id=DomUtil.GetAttr(templateNode, "id");
            }
            return id;
        }
    }
}
