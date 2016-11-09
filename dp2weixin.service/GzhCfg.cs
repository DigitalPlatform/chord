using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2weixin.service
{
    public class GzhContainer
    {
        private List<GzhCfg> _gzhList = new List<GzhCfg>();

        /*
         <gzhs>
            <gzh AppId="wxd24b193130bbaa7c" Secret="137afb49af90a07c2c72b11f520bb4b3" EncodingAESKey="m9WlCouFChERHjpyVciBSza0I1XV51Zs4GVgkhefxDE" Token="dp3weixin" AppName="wxtest">
<templates>
<template name='Bind' id="ciMkFC2uP2-i6gLrsuIvgs_4_5RrAxEh0qhJ2XkWWXc"/>
<template name="UnBind" id="dNnntAgPvPs3YUwsEKREt8TrBJnqYX73J2d7ftPXPL4"/>

<template name="Borrow" id="uEdYFQrAAIeZxjevVxEkbEV6JsqL2iLQ_c-mme42ZWM"/>
<template name="Return" id="xxbppjxQIUIuAiS66bA4NuBTjvczNvW8Hb_ot5tEY34"/>

<template name="Pay" id="AOKbOX6OhOfyFLbJig2LpmDaPD0QwHYLfxMP3sPxq4M"/>
<template name="CancelPay" id="538QqZd7iAB2I_Ob5fCKHn7Ti06UXpflW5dFUCbtv0c"/>

<template name="Message" id="qaPAe8R9UdHBCHPap0DLuFaHNGSBVG1fzkFkQhTFevM"/>
<template name="Arrived" id="I_SdDXYJRg0wfy2iW7d0pmwnlf0PdmE2ddRqQp2CbOs"/>
<template name="CaoQi" id="2XcSZ1G7nJRi3X2_oPdYw003x0VzM0UMtLkGD45Dlyc"/>

</templates>         
            </gzh>
         </gzhs>
         */
        public void Init(XmlDocument dom)
        {
            XmlNode gzhsNode = dom.DocumentElement.SelectSingleNode("gzhs");
            if (gzhsNode == null)
                return;

            XmlNodeList gzhList = gzhsNode.SelectNodes("gzh");
            foreach (XmlNode node in gzhList)
            {
 
            }
 
        }
    }

    //公众号配置信息
    public class GzhCfg
    {
        // 微信信息
        public string weiXinAppId { get; set; }
        public string weiXinSecret { get; set; }
        public string weixin_Token { get; set; }
        public string weixin_EncodingAESKey { get; set; }

        public bool bTrace = false;

        public string ApplName = "";

        #region 模板消息id

        //微信绑定通知
        public string Template_Bind = "";//"hFmNH7on2FqSOAiYPZVJN-FcXBv4xpVLBvHsfpLLQKU";
        // 微信解绑通知
        public string Template_UnBind = "";//"1riAKkt2W0AOtkx5rx-Lwa0RKRydDTHaMjSoUBGuHog";
        //借书成功
        public string Template_Borrow = "";//"2AVbpcn0y1NtqIQ7X7KY1Ebcyyhx6mUXTpAxuOcxSE0";
        //归书成功
        public string Template_Return = "";// "zzlLzStt_qZlzMFhcDgRm8Zoi-tsxjWdsI2b3FeoRMs";
        //交费成功
        public string Template_Pay = "";//"xFg1P44Hbk_Lpjc7Ds4gU8aZUqAlzoKpoeixtK1ykBI";
        //撤消交费成功
        public string Template_CancelPay = "";//"-XsD34ux9R2EgAdMhH0lpOSjcozf4Jli_eC86AXwM3Q";
        //个人消息通知 
        public string Template_Message = "";//"rtAx0BoUAwZ3npbNIO8Y9eIbdWO-weLGE2iOacGqN_s";
        //预约到书通知 
        public string Template_Arrived = "";//"U79IrJOgNJWZnqeKy2467ZoN-aM9vrEGQf2JJtvdBPM";
        //超期通知 
        public string Template_CaoQi = "";//2sOCuATcFdSNbJM24zrHnFv89R3D-cZFIpk4ec_Irn4";

        #endregion

        public void Init(XmlNode node)
        {
            this.weiXinAppId = DomUtil.GetAttr(node, "AppId"); //WebConfigurationManager.AppSettings["weiXinAppId"];
            this.weiXinSecret = DomUtil.GetAttr(node, "Secret"); //WebConfigurationManager.AppSettings["weiXinSecret"];
            this.weixin_Token = DomUtil.GetAttr(node, "Token");
            this.weixin_EncodingAESKey = DomUtil.GetAttr(node, "EncodingAESKey");
            this.ApplName = DomUtil.GetAttr(node, "AppName");
            if (this.ApplName == "")
                throw new Exception("尚未定义后台应用名称");

            // 模板id
            this.Template_Bind = this.GetTemplateId(node, "Bind");
            this.Template_UnBind = this.GetTemplateId(node, "UnBind");
            this.Template_Borrow = this.GetTemplateId(node, "Borrow");
            this.Template_Return = this.GetTemplateId(node, "Return");
            this.Template_Pay = this.GetTemplateId(node, "Pay");
            this.Template_CancelPay = this.GetTemplateId(node, "CancelPay");
            this.Template_Message = this.GetTemplateId(node, "Message");
            this.Template_Arrived = this.GetTemplateId(node, "Arrived");
            this.Template_CaoQi = this.GetTemplateId(node, "CaoQi");
        }


        // 得到模板id
        private string GetTemplateId(XmlNode root, string type)
        {
            XmlNode templateNode = root.SelectSingleNode("templates/template[@name='" + type + "']");
            if (templateNode == null)
                throw new Exception("尚未配置" + type + "模板");
            return DomUtil.GetAttr(templateNode, "id");
        }
    }
}
