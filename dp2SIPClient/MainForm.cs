using DigitalPlatform;
using SIP2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using DigitalPlatform.Text;
using System.Web;
using DigitalPlatform.SIP2;
using DigitalPlatform.SIP2.Request;

namespace dp2SIPClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        #region 连接断开服务器

        // 窗体加载
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.SIPServerUrl))
            {
                Form_Setting dlg = new Form_Setting();
                dlg.ShowDialog(this);
            }

            this.EnableControlsForConnection(false);
            // 当配置了SIP2服务器地址时，自动连接服务器
            if (string.IsNullOrEmpty(this.SIPServerUrl) == false)
            {
                // string info = "";
                // this.ConnectionServer(out info);
            }

            //SIPUtility.Logger.Info("test");
            //SIPUtility.Logger.Error("error1");
            //SIPUtility.Logger.Warn("警告");

            this.tabControl_main.SelectedTab = this.tabPage_function;
            ClearHtml();
        }

        public string SIPServerUrl
        {
            get
            {
                return Properties.Settings.Default.SIPServerUrl;
            }
        }

        public int SIPServerPort
        {
            get
            {
                return Properties.Settings.Default.SIPServerPort;
            }
        }



        public void ConnectionServer(out string info)
        {
            info = "";
            bool bRet = SCHelper.Instance.Connection(this.SIPServerUrl, this.SIPServerPort, out info);
            if (bRet == false) // 出错
            {
                this.toolStripStatusLabel_info.Text = info;
                this.EnableControlsForConnection(false);
                return;
            }

            // 连接成功
            string text = this.SIPServerUrl + ":" + this.SIPServerPort.ToString();
            info = "连接SIP2服务器[" + text + "]成功.";
            this.toolStripStatusLabel_info.Text = info;
            this.EnableControlsForConnection(true);
        }

        // 再次连接
        private void toolStripLabel_ConnectSIP2Server_Click(object sender, EventArgs e)
        {
            this.toolStripLabel_ConnectSIP2Server.Enabled = false;
            Application.DoEvents();
            string info = "";
            this.ConnectionServer(out info);
            MessageBox.Show(this, info);
        }

        // 断开连接
        private void toolStripLabel_DisconnectSIP2Server_Click(object sender, EventArgs e)
        {
            this.toolStripLabel_DisconnectSIP2Server.Enabled = false;
            Application.DoEvents();
            SCHelper.Instance.Close();
            this.toolStripStatusLabel_info.Text = "断开SIP2服务器连接.";
            this.EnableControlsForConnection(false);
            MessageBox.Show(this, "成功断开SIP2服务器连接.");
        }

        // 设置按钮状态
        void EnableControlsForConnection(bool isConnect)
        {
            if (isConnect == true)
            {
                this.toolStripLabel_send.Enabled = true;
                this.toolStripLabel_ConnectSIP2Server.Enabled = false;
                this.toolStripLabel_DisconnectSIP2Server.Enabled = true;
            }
            else
            {
                this.toolStripLabel_send.Enabled = false;
                this.toolStripLabel_ConnectSIP2Server.Enabled = true;
                this.toolStripLabel_DisconnectSIP2Server.Enabled = false;
            }
        }

        private void 参数配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Setting dlg = new Form_Setting();

            //当小窗口ok时，自动连接服务器
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                string info = "";
                this.ConnectionServer(out info);
            }
        }

        #endregion

        #region 发送接收消息

        //发送消息
        private void toolStripLabel_send_Click(object sender, EventArgs e)
        {
            string cmdText = "";
            try
            {
                if (this.tabControl_main.SelectedTab == this.tabPage_Login93)
                {
                    Login_93 request93 = new Login_93()
                    {
                        UIDAlgorithm_1 = this.textBox_Login93_UIDAlgorithm_1.Text,
                        PWDAlgorithm_1 = this.textBox_Login93_PWDAlgorithm_1.Text,

                        CN_LoginUserId_r = getText(this.textBox_Login93_loginUserId_CN_r),//.Text == "null" ? null : this.textBox_Login93_loginUserId_CN_r.Text
                        CO_LoginPassword_r = getText(this.textBox_Login93_loginPassword_CO_r),//.Text == "null" ? null : this.textBox_Login93_loginPassword_CO_r.Text,
                        CP_LocationCode_o = getText(this.textBox_Login93_locationCode_CP_o),//.Text == "null" ? null : this.textBox_Login93_locationCode_CP_o.Text
                    };
                    cmdText = request93.ToText();
                }
                else if (this.tabControl_main.SelectedTab == this.tabPage_SCStatus99)
                {
                    SCStatus_99 request = new SCStatus_99()
                    {
                        StatusCode_1 = this.textBox_SCStatus99_statusCode_1.Text,
                        MaxPrintWidth_3 = this.textBox_SCStatus99_maxPrintWidth_3.Text,
                        ProtocolVersion_4 = this.textBox_SCStatus99_protocolVersion_4.Text
                    };
                    cmdText = request.ToText();
                }
                else if (this.tabControl_main.SelectedTab == this.tabPage_Checkout11)
                {
                    Checkout_11 request = new Checkout_11()
                    {
                        SCRenewalPolicy_1 = this.textBox_Checkout11_SCRenewalPolicy_1.Text,
                        NoBlock_1 = this.textBox_Checkout11_noBlock_1.Text,
                        TransactionDate_18 = this.textBox_Checkout11_transactionDate_18.Text,
                        NbDueDate_18 = this.textBox_Checkout11_nbDueDate_18.Text,

                        AO_InstitutionId_r = getText(this.textBox_Checkout11_institutionId_AO_r),//.Text == "null" ? null : this.textBox_Checkout11_institutionId_AO_r.Text,
                        AA_PatronIdentifier_r = getText(this.textBox_Checkout11_patronIdentifier_AA_r),//.Text == "null" ? null : this.textBox_Checkout11_patronIdentifier_AA_r.Text,
                        AB_ItemIdentifier_r = getText(this.textBox_Checkout11_itemIdentifier_AB_r),//.Text == "null" ? null : this.textBox_Checkout11_itemIdentifier_AB_r.Text,

                        AC_TerminalPassword_r = getText(this.textBox_Checkout11_terminalPassword_AC_r),//.Text == "null" ? null : this.textBox_Checkout11_terminalPassword_AC_r.Text;
                        CH_ItemProperties_o = getText(this.textBox_Checkout11_itemProperties_CH_o),
                        AD_PatronPassword_o = getText(this.textBox_Checkout11_patronPassword_AD_o),
                        BO_FeeAcknowledged_1_o = getText(this.textBox_Checkout11_feeAcknowledged_BO_1_o),
                        BI_Cancel_1_o = getText(this.textBox_Checkout11_cancel_BI_1_o),
                    };
                    cmdText = request.ToText();
                }
                else if (this.tabControl_main.SelectedTab == this.tabPage_Checkin09)
                {
                    Checkin_09 request = new Checkin_09()
                    {
                        NoBlock_1 = this.textBox_Checkin09_noBlock_1.Text,
                        TransactionDate_18 = this.textBox_Checkin09_transactionDate_18.Text,
                        ReturnDate_18 = this.textBox_Checkin09_returnDate_18.Text,

                        AP_CurrentLocation_r = getText(this.textBox_Checkin09_currentLocation_AP_r),
                        AO_InstitutionId_r = getText(this.textBox_Checkin09_institutionId_AO_r),
                        AB_ItemIdentifier_r = getText(this.textBox_Checkin09_itemIdentifier_AB_r),

                        AC_TerminalPassword_r = getText(this.textBox_Checkin09_terminalPassword_AC_r),
                        CH_ItemProperties_o = getText(this.textBox_Checkin09_itemProperties_CH_o),
                        BI_Cancel_1_o = getText(this.textBox_Checkin09_cancel_BI_1_o),
                    };
                    cmdText = request.ToText();
                }
                else if (this.tabControl_main.SelectedTab == this.tabPage_PatronInformation63)
                {
                    PatronInformation_63 request = new PatronInformation_63()
                    {
                        Language_3 = this.textBox_PatronInformation63_language_3.Text,
                        TransactionDate_18 = this.textBox_PatronInformation63_transactionDate_18.Text,
                        Summary_10 = this.textBox_PatronInformation63_summary_10.Text,

                        AO_InstitutionId_r = getText(this.textBox_PatronInformation63_institutionId_AO_r),
                        AA_PatronIdentifier_r = getText(this.textBox_PatronInformation63_patronIdentifier_AA_r),
                        AC_TerminalPassword_o = getText(this.textBox_PatronInformation63_terminalPassword_AC_o),

                        AD_PatronPassword_o = getText(this.textBox_PatronInformation63_patronPassword_AD_o),
                        BP_StartItem_o = getText(this.textBox_PatronInformation63_startItem_BP_o),
                        BQ_EndItem_o = getText(this.textBox_PatronInformation63_endItem_BQ_o),
                    };
                    cmdText = request.ToText();
                }
                else if (this.tabControl_main.SelectedTab == this.tabPage_ItemInformation17)
                {

                    ItemInformation_17 request = new ItemInformation_17()
                    {
                        TransactionDate_18 = this.textBox_ItemInformation17_transactionDate_18.Text,

                        AO_InstitutionId_r = getText(this.textBox_ItemInformation17_institutionId_AO_r),
                        AB_ItemIdentifier_r = getText(this.textBox_ItemInformation17_itemIdentifier_AB_r),
                        AC_TerminalPassword_o = getText(this.textBox_ItemInformation17_terminalPassword_AC_o),
                    };
                    cmdText = request.ToText();
                }
                else if (this.tabControl_main.SelectedTab == this.tabPage_Renew29)
                {
                    Renew_29 request = new Renew_29()
                    {
                        ThirdPartyAllowed_1 = this.textBox_Renew29_thirdPartyAllowed_1.Text,
                        NoBlock_1 = this.textBox_Renew29_noBlock_1.Text,
                        TransactionDate_18 = this.textBox_Renew29_transactionDate_18.Text,
                        NbDueDate_18 = this.textBox_Renew29_nbDueDate_18.Text,

                        AO_InstitutionId_r = getText(this.textBox_Renew29_institutionId_AO_r),
                        AA_PatronIdentifier_r = getText(this.textBox_Renew29_patronIdentifier_AA_r),

                        AD_PatronPassword_o = getText(this.textBox_Renew29_patronPassword_AD_o),
                        AB_ItemIdentifier_o = getText(this.textBox_Renew29_itemIdentifier_AB_o),
                        AJ_TitleIdentifier_o = getText(this.textBox_Renew29_titleIdentifier_AJ_o),

                        AC_TerminalPassword_o = getText(this.textBox_Renew29_terminalPassword_AC_o),
                        CH_ItemProperties_o = getText(this.textBox_Renew29_itemProperties_CH_o),
                        BO_FeeAcknowledged_1_o = getText(this.textBox_Renew29_feeAcknowledged_BO_1_o),
                    };

                    cmdText = request.ToText();

                }
                else if (this.tabControl_main.SelectedTab == this.tabPage_FeePaid37)
                {
                    FeePaid_37 request = new FeePaid_37()
                    {
                        TransactionDate_18 = getText(this.textBox_FeePaid37_transactionDate_18),
                        FeeType_2 = getText(this.textBox_FeePaid37_feeType),
                        PaymentType_2 = getText(this.textBox_FeePaid37_paymentType),
                        CurrencyType_3 = getText(this.textBox_FeePaid37_currencyType),

                        BV_FeeAmount_r = getText(this.textBox_FeePaid37_feeAmount),
                        AO_InstitutionId_r = getText(this.textBox_FeePaid37_institutionId_AO_r),
                        AA_PatronIdentifier_r = getText(this.textBox_FeePaid37_patronIdentifier_AA_r),
                        AC_TerminalPassword_o = getText(this.textBox_FeePaid37_terminalPassword_AC_o),

                        AD_PatronPassword_o = getText(this.textBox_FeePaid37_patronPassword_AD_o),
                        CG_FeeIdentifier_o = getText(this.textBox_FeePaid37_feeIdentifier_CG_o),
                        BK_TransactionId_o = getText(this.textBox_FeePaid37_transactionId_BK_o),
                    };
                    cmdText = request.ToText();
                }



                //发送命令
                this.txtMsg.Text = cmdText;
                this.sendCmd();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }


        }

        private string SamplePatron
        {
            get
            {
                return Properties.Settings.Default.Patron;
            }
        }

        private string SampleItem
        {
            get
            {
                return Properties.Settings.Default.Item;
            }
        }

        private void toolStripLabel_sample_Click(object sender, EventArgs e)
        {
            this.txtMsg.Text = "";

            string error = "";
            int nRet = 0;
            //BaseRequest request = null;
            string text = "";
            if (this.tabControl_main.SelectedTab == this.tabPage_Login93)
            {
                text = "93  CNsupervisor|CO1|CPC00|AY0AZFB58";
                Login_93 request93 = new Login_93();
                nRet = request93.parse(text, out error);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_Login93_UIDAlgorithm_1.Text = request93.UIDAlgorithm_1;//.GetFixedFieldValue(SIPConst.F_UIDAlgorithm);
                this.textBox_Login93_PWDAlgorithm_1.Text = request93.PWDAlgorithm_1;//.GetFixedFieldValue(SIPConst.F_PWDAlgorithm);
                this.textBox_Login93_loginUserId_CN_r.Text = request93.CN_LoginUserId_r;//.GetVariableFieldValue(SIPConst.F_CN_LoginUserId);
                this.textBox_Login93_loginPassword_CO_r.Text = request93.CO_LoginPassword_r;//.GetVariableFieldValue(SIPConst.F_CO_LoginPassword);
                this.textBox_Login93_locationCode_CP_o.Text = request93.CP_LocationCode_o;//.GetVariableFieldValue(SIPConst.F_CP_LocationCode);
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_SCStatus99)
            {
                text = "9900302.00";
                SCStatus_99 request99 = new SCStatus_99();
                nRet = request99.parse(text, out error);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_SCStatus99_statusCode_1.Text = request99.StatusCode_1;//.GetFixedFieldValue(SIPConst.F_StatusCode);
                this.textBox_SCStatus99_maxPrintWidth_3.Text = request99.MaxPrintWidth_3;//.GetFixedFieldValue(SIPConst.F_MaxPrintWidth);
                this.textBox_SCStatus99_protocolVersion_4.Text = request99.ProtocolVersion_4;//.GetFixedFieldValue(SIPConst.F_ProtocolVersion);
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_Checkout11)
            {
                //20170630    141135
                text = "11YN" + SIPUtility.NowDateTime + "                  AOdp2Library|AA" + SamplePatron + "|AB" + SampleItem + "|AC|BON|BIN|";
                Checkout_11 request11 = new Checkout_11();
                nRet = request11.parse(text, out error);
                if (nRet == -1)
                    goto ERROR1;


                this.textBox_Checkout11_SCRenewalPolicy_1.Text = request11.SCRenewalPolicy_1;//.GetFixedFieldValue(SIPConst.F_SCRenewalPolicy);//.SCRenewalPolicy_1;
                this.textBox_Checkout11_noBlock_1.Text = request11.NoBlock_1;//.GetFixedFieldValue(SIPConst.F_NoBlock);//.NoBlock_1;
                this.textBox_Checkout11_transactionDate_18.Text = request11.TransactionDate_18;//.GetFixedFieldValue(SIPConst.F_TransactionDate);//.TransactionDate_18;
                this.textBox_Checkout11_nbDueDate_18.Text = request11.NbDueDate_18;//.GetFixedFieldValue(SIPConst.F_NbDueDate);//.NbDueDate_18;

                this.textBox_Checkout11_institutionId_AO_r.Text = request11.AO_InstitutionId_r;//.GetVariableFieldValue(SIPConst.F_AO_InstitutionId);//.InstitutionId_AO_r;
                this.textBox_Checkout11_patronIdentifier_AA_r.Text = request11.AA_PatronIdentifier_r;//.GetVariableFieldValue(SIPConst.F_AA_PatronIdentifier);//.PatronIdentifier_AA_r;

                this.textBox_Checkout11_itemIdentifier_AB_r.Text = request11.AB_ItemIdentifier_r;//.GetVariableFieldValue(SIPConst.F_AB_ItemIdentifier);//.ItemIdentifier_AB_r;
                this.textBox_Checkout11_terminalPassword_AC_r.Text = request11.AC_TerminalPassword_r;//.GetVariableFieldValue(SIPConst.F_AC_TerminalPassword);//.TerminalPassword_AC_r;
                this.textBox_Checkout11_itemProperties_CH_o.Text = request11.CH_ItemProperties_o;//.GetVariableFieldValue(SIPConst.F_CH_ItemProperties);//.ItemProperties_CH_o;

                this.textBox_Checkout11_patronPassword_AD_o.Text = request11.AD_PatronPassword_o;//.GetVariableFieldValue(SIPConst.F_AD_PatronPassword);//.PatronPassword_AD_o;
                this.textBox_Checkout11_feeAcknowledged_BO_1_o.Text = request11.BO_FeeAcknowledged_1_o;//.GetVariableFieldValue(SIPConst.F_BO_FeeAcknowledged);//.FeeAcknowledged_BO_1_o;
                this.textBox_Checkout11_cancel_BI_1_o.Text = request11.BI_Cancel_1_o;//.GetVariableFieldValue(SIPConst.F_BI_Cancel);//Cancel_BI_1_o;


            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_Checkin09)
            {
                //20170630    141630
                string transactionDate = SIPUtility.NowDateTime;
                string returnDate = SIPUtility.NowDateTime;
                text = "09N" + transactionDate + returnDate + "AP|AOdp2Library|AB" + SampleItem + "|AC|BIN|";

                //text = "09N20170906    170441|AB378344|AOhuanshuji|AC|BIN|AP|AY1AZEFA4";
                Checkin_09 request09 = new Checkin_09();
                nRet = request09.parse(text, out error);
                if (nRet == -1)
                    goto ERROR1;


                this.textBox_Checkin09_noBlock_1.Text = request09.NoBlock_1;//.GetFixedFieldValue(SIPConst.F_NoBlock);//.F_no.NoBlock_1;
                this.textBox_Checkin09_transactionDate_18.Text = request09.TransactionDate_18;//.GetFixedFieldValue(SIPConst.F_TransactionDate);//_18;
                this.textBox_Checkin09_returnDate_18.Text = request09.ReturnDate_18;//.GetFixedFieldValue(SIPConst.F_ReturnDate);

                this.textBox_Checkin09_currentLocation_AP_r.Text = request09.AP_CurrentLocation_r;//.GetVariableFieldValue(SIPConst.F_AP_CurrentLocation);// _r;
                this.textBox_Checkin09_institutionId_AO_r.Text = request09.AO_InstitutionId_r;//.GetVariableFieldValue(SIPConst.F_AO_InstitutionId);
                this.textBox_Checkin09_itemIdentifier_AB_r.Text = request09.AB_ItemIdentifier_r;//.GetVariableFieldValue(SIPConst.F_AB_ItemIdentifier);

                this.textBox_Checkin09_terminalPassword_AC_r.Text = request09.AC_TerminalPassword_r;//.GetVariableFieldValue(SIPConst.F_AC_TerminalPassword);
                this.textBox_Checkin09_itemProperties_CH_o.Text = request09.CH_ItemProperties_o;//.GetVariableFieldValue(SIPConst.F_CH_ItemProperties);
                this.textBox_Checkin09_cancel_BI_1_o.Text = request09.BI_Cancel_1_o;//.GetVariableFieldValue(SIPConst.F_BI_Cancel);


            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_PatronInformation63)
            {
                //6301920170630    090808  Y       AOdp2Library|AAA005312|
                string transactionDate = SIPUtility.NowDateTime;
                text = "63019" + transactionDate + "  Y       AOdp2Library|AA" + SamplePatron + "|";
                PatronInformation_63 request63 = new PatronInformation_63();
                nRet = request63.parse(text, out error);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_PatronInformation63_language_3.Text = request63.Language_3;//.GetFixedFieldValue(SIPConst.F_Language);
                this.textBox_PatronInformation63_transactionDate_18.Text = request63.TransactionDate_18;//.GetFixedFieldValue(SIPConst.F_TransactionDate);//.TransactionDate_18;
                this.textBox_PatronInformation63_summary_10.Text = request63.Summary_10;//.GetFixedFieldValue(SIPConst.F_Summary);//.Summary_10;

                this.textBox_PatronInformation63_institutionId_AO_r.Text = request63.AO_InstitutionId_r;//.GetVariableFieldValue(SIPConst.F_AO_InstitutionId);//.InstitutionId_AO_r;
                this.textBox_PatronInformation63_patronIdentifier_AA_r.Text = request63.AA_PatronIdentifier_r;//.GetVariableFieldValue(SIPConst.F_AA_PatronIdentifier);//.PatronIdentifier_AA_r;
                this.textBox_PatronInformation63_terminalPassword_AC_o.Text = request63.AC_TerminalPassword_o;//.GetVariableFieldValue(SIPConst.F_AC_TerminalPassword);//.TerminalPassword_AC_o;

                this.textBox_PatronInformation63_patronPassword_AD_o.Text = request63.AD_PatronPassword_o;//.GetVariableFieldValue(SIPConst.F_AD_PatronPassword);//.PatronPassword_AD_o;
                this.textBox_PatronInformation63_startItem_BP_o.Text = request63.BP_StartItem_o;//.GetVariableFieldValue(SIPConst.F_BP_StartItem);//.StartItem_BP_o;
                this.textBox_PatronInformation63_endItem_BQ_o.Text = request63.BQ_EndItem_o;//.GetVariableFieldValue(SIPConst.F_BQ_EndItem);//.EndItem_BQ_o;                 
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_ItemInformation17)
            {
                //1720170623    151645AOdp2Library|AB700635|
                string transactionDate = SIPUtility.NowDateTime;
                text = "17" + transactionDate + "AOdp2Library|AB" + SampleItem + "|";
                ItemInformation_17 request17 = new ItemInformation_17();
                nRet = request17.parse(text, out error);
                if (nRet == -1)
                    goto ERROR1;


                this.textBox_ItemInformation17_transactionDate_18.Text = request17.TransactionDate_18;//.GetFixedFieldValue(SIPConst.F_TransactionDate);//.TransactionDate_18;

                this.textBox_ItemInformation17_institutionId_AO_r.Text = request17.AO_InstitutionId_r;//.GetVariableFieldValue(SIPConst.F_AO_InstitutionId);//.InstitutionId_AO_r;
                this.textBox_ItemInformation17_itemIdentifier_AB_r.Text = request17.AB_ItemIdentifier_r;//.GetVariableFieldValue(SIPConst.F_AB_ItemIdentifier);//.ItemIdentifier_AB_r;
                this.textBox_ItemInformation17_terminalPassword_AC_o.Text = request17.AC_TerminalPassword_o;//.GetVariableFieldValue(SIPConst.F_AC_TerminalPassword);//.TerminalPassword_AC_o;


            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_Renew29)
            {
                //29NN20170630    144419                  AOdp2Library|AAL905071|AB510105|BON|
                string transactionDate = SIPUtility.NowDateTime;
                text = "29NN" + transactionDate + "                  AOdp2Library|AA" + SamplePatron + "|AB" + SampleItem + "|BON|";
                Renew_29 request29 = new Renew_29();
                nRet = request29.parse(text, out error);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_Renew29_thirdPartyAllowed_1.Text = request29.ThirdPartyAllowed_1;//.GetFixedFieldValue(SIPConst.F_ThirdPartyAllowed);//.ThirdPartyAllowed_1;
                this.textBox_Renew29_noBlock_1.Text = request29.NoBlock_1;//.GetFixedFieldValue(SIPConst.F_NoBlock);//.NoBlock_1;
                this.textBox_Renew29_transactionDate_18.Text = request29.TransactionDate_18;//.GetFixedFieldValue(SIPConst.F_TransactionDate);//.TransactionDate_18;
                this.textBox_Renew29_nbDueDate_18.Text = request29.NbDueDate_18;//.GetFixedFieldValue(SIPConst.F_NbDueDate);//.NbDueDate_18;

                this.textBox_Renew29_institutionId_AO_r.Text = request29.AO_InstitutionId_r;//.GetVariableFieldValue(SIPConst.F_AO_InstitutionId);//.InstitutionId_AO_r;
                this.textBox_Renew29_patronIdentifier_AA_r.Text = request29.AA_PatronIdentifier_r;//.GetVariableFieldValue(SIPConst.F_AA_PatronIdentifier);//.PatronIdentifier_AA_r;

                //AD	AB AJ
                this.textBox_Renew29_patronPassword_AD_o.Text = request29.AD_PatronPassword_o;//.GetVariableFieldValue(SIPConst.F_AD_PatronPassword);//.PatronPassword_AD_o;
                this.textBox_Renew29_itemIdentifier_AB_o.Text = request29.AB_ItemIdentifier_o;//.GetVariableFieldValue(SIPConst.F_AB_ItemIdentifier);//.ItemIdentifier_AB_o;
                this.textBox_Renew29_titleIdentifier_AJ_o.Text = request29.AJ_TitleIdentifier_o;//.GetVariableFieldValue(SIPConst.F_AJ_TitleIdentifier);//.TitleIdentifier_AJ_o;

                //AC	CH	BO
                this.textBox_Renew29_terminalPassword_AC_o.Text = request29.AC_TerminalPassword_o;//.GetVariableFieldValue(SIPConst.F_AC_TerminalPassword);//.TerminalPassword_AC_o;
                this.textBox_Renew29_itemProperties_CH_o.Text = request29.CH_ItemProperties_o;//.GetVariableFieldValue(SIPConst.F_CH_ItemProperties);//.ItemProperties_CH_o;
                this.textBox_Renew29_feeAcknowledged_BO_1_o.Text = request29.BO_FeeAcknowledged_1_o;//.GetVariableFieldValue(SIPConst.F_BO_FeeAcknowledged);//.FeeAcknowledged_BO_1_o;


            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_FeePaid37)
            {
                //3720180118    1309170100USDBV0.1|AOj163-z1|AAL120100000000000002|AY3AZEFFC
                string transactionDate = SIPUtility.NowDateTime;
                text = "37" + transactionDate + "0100USDBV0.1|AOj163-z1|AA" + SamplePatron + "|AY3AZEFFC";
                FeePaid_37 request37 = new FeePaid_37();
                nRet = request37.parse(text, out error);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_FeePaid37_transactionDate_18.Text = request37.TransactionDate_18;
                this.textBox_FeePaid37_feeType.Text = request37.FeeType_2;
                this.textBox_FeePaid37_paymentType.Text = request37.PaymentType_2;
                this.textBox_FeePaid37_currencyType.Text = request37.CurrencyType_3;

                this.textBox_FeePaid37_feeAmount.Text = request37.BV_FeeAmount_r;
                this.textBox_FeePaid37_institutionId_AO_r.Text = request37.AO_InstitutionId_r;
                this.textBox_FeePaid37_patronIdentifier_AA_r.Text = request37.AA_PatronIdentifier_r;
                this.textBox_FeePaid37_terminalPassword_AC_o.Text = request37.AC_TerminalPassword_o;

                this.textBox_FeePaid37_patronPassword_AD_o.Text = request37.AD_PatronPassword_o;
                this.textBox_FeePaid37_feeIdentifier_CG_o.Text = request37.CG_FeeIdentifier_o;
                this.textBox_FeePaid37_transactionId_BK_o.Text = request37.BK_TransactionId_o;
            }
            return;

            ERROR1:
            this.Print("error:" + error);


        }

        // 回车 触发 发送
        private void txtMsg_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.sendCmd();
                return;
            }
        }

        // 发送消息
        private void sendCmd()
        {
            // 发送信息不允许为空
            if (txtMsg.Text == "")
            {
                MessageBox.Show("发送的信息不能为空!");
                txtMsg.Focus();
                return;
            }

            string requestText = this.txtMsg.Text.Trim() + new string((char)13, 1);   // 2018/7/28
            string responseText = "";
            string error = "";

            this.Print("send:" + requestText);
            LogManager.Logger.Info("send:" + requestText);
            BaseMessage response = null;
            int nRet = SCHelper.Instance.SendAndRecvMessage(requestText,
                out response,
                out responseText,
                out error);
            if (nRet == -1)
            {
                MessageBox.Show(error);
                this.Print("error:" + error);
                LogManager.Logger.Error("error:" + error);
                return;
            }

            this.Print("recv:" + responseText);
            LogManager.Logger.Info("recv:" + responseText);

        }


        private string getText(TextBox txtBox)
        {
            return txtBox.Text == "null" ? null : txtBox.Text;
        }


        #endregion

        #region 通用函数

        private void Print(string strHtml)
        {
            strHtml = String.Format("{0}  {1}<br />", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), strHtml);
            WriteHtml(this.webBrowser1,
                strHtml);
        }

        // 不支持异步调用
        public static void WriteHtml(WebBrowser webBrowser,
            string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
                doc.Write("<pre>");
            }

            doc.Write(strHtml);

            // 保持末行可见
            ScrollToEnd(webBrowser);
        }

        public void ClearHtml()
        {
            HtmlDocument doc = webBrowser1.Document;

            if (doc == null)
            {
                webBrowser1.Navigate("about:blank");
                doc = webBrowser1.Document;
            }
            doc = doc.OpenNew(true);
            doc.Write("<pre>");
        }


        public static void ScrollToEnd(WebBrowser webBrowser1)
        {
            if (webBrowser1.Document != null
                && webBrowser1.Document.Window != null
                && webBrowser1.Document.Body != null)
                webBrowser1.Document.Window.ScrollTo(
                    0,
                    webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        #endregion

        #region 菜单功能

        private void 实用工具ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form_Checksum dlg = new Form_Checksum();
            dlg.ShowDialog(this);
        }

        private void 清空信息区ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearHtml();
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.txtMsg.Text = "";
        }


        private void 自动测试ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form_Test dlg = new Form_Test(this.toolStripStatusLabel_info.Text);
            dlg.Show();
        }

        private void sample参数设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_SampleParam dlg = new Form_SampleParam();
            dlg.ShowDialog(this);
        }


        #endregion


        #region 功能测试

        string TransactionDate
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMdd    HHmmss");
            }
        }


        Encoding _encoding
        {
            get
            {
                string strEncoding = this.comboBox_encoding.Text;
                if (string.IsNullOrEmpty(strEncoding))
                    return Encoding.UTF8;
                else
                    return Encoding.GetEncoding(strEncoding);
            }
        }

        private void button_connection_Click(object sender, EventArgs e)
        {
            string info = "";

            for (int i = 0; i < int.Parse(this.textBox_copies.Text); i++)
            {
                this.Update();
                Application.DoEvents();

                bool bRet = SCHelper.Instance.Connection(this.textBox_addr.Text, int.Parse(this.textBox_port.Text), _encoding, out info);
                if (bRet == false) // 出错
                {
                    this.toolStripStatusLabel_info.Text = info;
                    return;
                }

                // 连接成功
                string text = this.textBox_addr.Text + ":" + this.textBox_port.Text;
                info = "连接SIP2服务器[" + text + "]成功(" + (i + 1).ToString() + ")编码方式：" + this._encoding?.EncodingName;
                this.toolStripStatusLabel_info.Text = info;

                this.button_connection.Text = "连接(" + (i + 1).ToString() + ")";
                this.button_connection.Update();
            }
        }

        private void button_login_Click(object sender, EventArgs e)
        {
            string responseText = "";
            string error = "";

            for (int i = 0; i < int.Parse(this.textBox_login_copies.Text); i++)
            {
                Login_93 request = new Login_93()
                {
                    UIDAlgorithm_1 = " ",
                    PWDAlgorithm_1 = " ",

                    CN_LoginUserId_r = this.textBox_username.Text,
                    CO_LoginPassword_r = this.textBox_password.Text,
                    CP_LocationCode_o = this.textBox_locationCode.Text
                };
                string cmdText = request.ToText();

                this.Print("send:" + cmdText);
                BaseMessage response = null;
                int nRet = SCHelper.Instance.SendAndRecvMessage(cmdText,
                    out response,
                    out responseText,
                    out error);
                if (nRet == -1)
                {
                    MessageBox.Show(error);
                    this.Print("error:" + error);
                    return;
                }

                this.Print("recv:" + responseText);

                this.button_login.Text = "登录(" + (i + 1).ToString() + ")";
                this.button_login.Update();
            }
        }

        // 2021/3/3 AO依据从界面输入的值
        public string AO
        {
            get
            {
                return this.textBox_AO.Text.Trim();
            }
        }

        private void button_getItemInfo_Click(object sender, EventArgs e)
        {
            ItemInformation_17 request = new ItemInformation_17()
            {
                TransactionDate_18 = this.TransactionDate,

                AO_InstitutionId_r = this.AO, //"",// dp2Library",
                AC_TerminalPassword_o = "",
            };

            string responseText = "";
            string error = "";
            string[] barcodes = this.textBox_barcodes.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (string barcode in barcodes)
            {
                this.Update();
                Application.DoEvents();
                Thread.Sleep(100);

                request.AB_ItemIdentifier_r = barcode;

                string cmdText = request.ToText();

                this.Print("send:" + cmdText);
                BaseMessage response = null;
                int nRet = SCHelper.Instance.SendAndRecvMessage(cmdText,
                    out response,
                    out responseText,
                    out error);
                if (nRet == -1)
                {
                    MessageBox.Show(error);
                    this.Print("error:" + error);
                    //return;
                }

                this.Print("recv:" + responseText);

                this.button_getItemInfo.Text = "获取(" + (i + 1).ToString() + ")";
                i++;
            }
        }

        #endregion

        private void button_checkOut_Click(object sender, EventArgs e)
        {
            Checkout_11 request = new Checkout_11()
            {
                SCRenewalPolicy_1 = "Y",
                NoBlock_1 = "N",
                TransactionDate_18 = this.TransactionDate,
                AO_InstitutionId_r = this.AO,// "",// dp2Library",
                AA_PatronIdentifier_r = this.textBox_readerBarcode.Text,
                AD_PatronPassword_o = this.textBox_readerPassword.Text,
                BO_FeeAcknowledged_1_o = "N",
                BI_Cancel_1_o = "N",
                NbDueDate_18 = "                  ",
                AC_TerminalPassword_r = "",
            };

            Button button = sender as Button;
            string responseText = "";
            string error = "";
            string[] barcodes = this.textBox_barcodes.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (string barcode in barcodes)
            {
                this.Update();
                Application.DoEvents();
                Thread.Sleep(100);

                request.AB_ItemIdentifier_r = barcode;

                string cmdText = request.ToText();

                this.Print("send:" + cmdText);
                BaseMessage response = null;
                int nRet = SCHelper.Instance.SendAndRecvMessage(cmdText,
                    out response,
                    out responseText,
                    out error);
                if (nRet == -1)
                {
                    MessageBox.Show(error);
                    this.Print("error:" + error);
                    return;
                }

                this.Print("recv:" + responseText);

                button.Text = "借书(" + (i + 1).ToString() + ")";
                i++;
            }
        }

        private void button_getPatronInfo_Click(object sender, EventArgs e)
        {
            PatronInformation_63 request = new PatronInformation_63()
            {
                Language_3 = "019",
                TransactionDate_18 = this.TransactionDate,
                Summary_10 = "  Y       ",
                AO_InstitutionId_r = this.AO,//"",// dp2Library",
                BP_StartItem_o=this.textBox_BP.Text.Trim(),//"1",
                BQ_EndItem_o=this.textBox_BQ.Text.Trim(),//"5",
            };
            Button button = sender as Button;
            string responseText = "";
            string error = "";
            string[] barcodes = this.textBox_readerBarcode.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach(string barcode in barcodes)
            {
                this.Update();
                Application.DoEvents();
                Thread.Sleep(100);

                request.AA_PatronIdentifier_r = barcode;
                string cmdText = request.ToText();

                this.Print("send:" + cmdText);
                BaseMessage response = null;
                int nRet = SCHelper.Instance.SendAndRecvMessage(cmdText,
                    out response,
                    out responseText,
                    out error);
                if (nRet == -1)
                {
                    MessageBox.Show(error);
                    this.Print("error:" + error);
                    return;
                }

                this.Print("recv:" + responseText);

                button.Text = "获取(" + (i + 1).ToString() + ")";
                i++;
            }
        }

        private void button_checkIn_Click(object sender, EventArgs e)
        {
            Checkin_09 request = new Checkin_09()
            {
                NoBlock_1 = "N",
                TransactionDate_18 = this.TransactionDate,
                ReturnDate_18 = this.TransactionDate,
                AP_CurrentLocation_r = this.textBox_returnCurLocation.Text.Trim(),//2022/2/25 当前位置，可以只传馆藏地
                AC_TerminalPassword_r = "",
                AO_InstitutionId_r = this.AO,//"",// dp2Library",
                BI_Cancel_1_o = "N"
            };

            Button button = sender as Button;
            string responseText = "";
            string error = "";
            string[] barcodes = this.textBox_barcodes.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (string barcode in barcodes)
            {
                this.Update();
                Application.DoEvents();
                Thread.Sleep(100);

                request.AB_ItemIdentifier_r = barcode;

                string cmdText = request.ToText();

                this.Print("send:" + cmdText);
                BaseMessage response = null;
                int nRet = SCHelper.Instance.SendAndRecvMessage(cmdText,
                    out response,
                    out responseText,
                    out error);
                if (nRet == -1)
                {
                    MessageBox.Show(error);
                    this.Print("error:" + error);
                    return;
                }

                if (string.IsNullOrEmpty(responseText) == false
                    && responseText.Length >= 6
                    && responseText.Substring(5, 1) == "Y")
                    {
                    this.Print("<span style='background-color:yellow'>recv:" + responseText +"</span>");

                }
                else
                {
                    this.Print("recv:" + responseText);
                }

                button.Text = "还书(" + (i + 1).ToString() + ")";
                i++;
            }
        }

        private void checkBox_UrlEncode_CheckedChanged(object sender, EventArgs e)
        {
            if (ContainHanzi(this.textBox_username.Text) && this.checkBox_UrlEncode.Checked)
            {
                this.textBox_username.Text = HttpUtility.UrlEncode(this.textBox_username.Text);
            }
            else if(this.checkBox_UrlEncode.Checked == false)
            {
                this.textBox_username.Text = HttpUtility.UrlDecode(this.textBox_username.Text);
            }
        }


        static int[] iSign =   
        {
            65306,
            8220,
            65307,
            8216,
            65292,
            65281,
            12289,
            65311,
            8212,
            12290,
            12298,
            12297,
            8230,
            65509,
            65288,
            65289,
            8217,
            8221
        };

        public static bool IsHanzi(char ch)
        {
            int n = (int)ch;
            if (n <= 0X1ef3) // < 1024
                return false;
            foreach (int v in iSign)
            {
                if (ch == v)
                    return false;
            }

            return true;
        }

        // 是否包含一个以上的汉字
        public static bool ContainHanzi(string strText)
        {
            foreach (char ch in strText)
            {
                if (IsHanzi(ch) == true)
                    return true;
            }

            return false;
        }

        // 更新册
        private void button_19_submit_Click(object sender, EventArgs e)
        {
            ItemStatusUpdate_19 request = new ItemStatusUpdate_19()
            {
                TransactionDate_18 = this.TransactionDate,

                AO_InstitutionId_r = this.AO,//"",// dp2Library",
                AC_TerminalPassword_o = "",
                CH_ItemProperties_r = ""
            };

            string AQ = this.textBox_19_AQ.Text.Trim();  // 永久馆藏地
            string AP = this.textBox_19_AP.Text.Trim();  // 当前馆藏地
            string KQ=this.textBox_19_KQ.Text.Trim();  // 永久架位
            string KP = this.textBox_19_KP.Text.Trim();// 当前架位
            string HS = this.textBox_19_HS.Text.Trim();// 状态


            string responseText = "";
            string error = "";
            string[] barcodes = this.textBox_barcodes.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (string barcode in barcodes)
            {
                this.Update();
                Application.DoEvents();
                Thread.Sleep(100);

                // 册条码
                request.AB_ItemIdentifier_r = barcode;

                // 永久馆藏地 2020/12/9
                if (string.IsNullOrEmpty(AQ) == false)
                    request.AQ_PermanentLocation_o = AQ;

                // 当前馆藏地 2020/12/9
                if (string.IsNullOrEmpty(AP) == false)
                    request.AP_CurrentLocation_o = AP;

                // 永久架位 2020/12/9
                if (string.IsNullOrEmpty(KQ) == false)
                    request.KQ_PermanentShelfNo_o = KQ;

                // 当前架位 2020/12/9
                if (string.IsNullOrEmpty(KP) == false)
                    request.KP_CurrentShelfNo_o = KP;

                // 状态 2020/12/9
                if (string.IsNullOrEmpty(HS) == false)
                    request.HS_HoldingState_o = HS;


                string cmdText = request.ToText();

                this.Print("send:" + cmdText);
                BaseMessage response = null;
                int nRet = SCHelper.Instance.SendAndRecvMessage(cmdText,
                    out response,
                    out responseText,
                    out error);
                if (nRet == -1)
                {
                    MessageBox.Show(error);
                    this.Print("error:" + error);
                    return;
                }

                this.Print("recv:" + responseText);

                this.button_getItemInfo.Text = "获取(" + (i + 1).ToString() + ")";
                i++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.SendMessage();
        }

        public void SendMessage()
        {
            string cmdText = this.textBox_message.Text.Trim();//this.textBox_barcodes.Text.Trim();
            this.Print("send:" + cmdText);
            BaseMessage response = null;
            int nRet = SCHelper.Instance.SendAndRecvMessage(cmdText,
                out response,
                out string responseText,
                out string error);
            if (nRet == -1)
            {
                MessageBox.Show(error);
                this.Print("error:" + error);
            }

            this.Print("recv:" + responseText);
        }




        private void button_renew_Click(object sender, EventArgs e)
        {
            Renew_29 request = new Renew_29()
            {
                ThirdPartyAllowed_1 = "N",
                NoBlock_1 = "N",
                TransactionDate_18 = this.TransactionDate,
                NbDueDate_18 = "                  ",
                AO_InstitutionId_r = this.AO,// "",// dp2Library",
                BO_FeeAcknowledged_1_o = "N",
            };

            Button button = sender as Button;

            string responseText = "";
            string error = "";

            request.AA_PatronIdentifier_r = this.textBox_readerBarcode.Text.Trim();//认作一个册条码号
            request.AB_ItemIdentifier_o = this.textBox_barcodes.Text.Trim();//认作一个册条码号
            request.AD_PatronPassword_o = this.textBox_readerPassword.Text.Trim();


            string cmdText = request.ToText();

            this.Print("send:" + cmdText);
            BaseMessage response = null;
            int nRet = SCHelper.Instance.SendAndRecvMessage(cmdText,
                out response,
                out responseText,
                out error);
            if (nRet == -1)
            {
                MessageBox.Show(error);
                this.Print("error:" + error);
            }

            this.Print("recv:" + responseText);

        }

        private void button_send98_Click(object sender, EventArgs e)
        {
            this.textBox_message.Text = "9900302.00";
            this.SendMessage();
        }

        /// <summary>
        /// 交费
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_fee_Click(object sender, EventArgs e)
        {
            FeePaid_37 request = new FeePaid_37()
            {
                TransactionDate_18 = this.TransactionDate,
                FeeType_2 = "01",
                PaymentType_2= "00",
            };

            Button button = sender as Button;

            string responseText = "";
            string error = "";

            request.AA_PatronIdentifier_r = this.textBox_readerBarcode.Text.Trim();//认作一个册条码号
            request.AO_InstitutionId_r = this.AO;
            request.AD_PatronPassword_o = this.textBox_readerPassword.Text.Trim();
            request.CurrencyType_3 = this.textBox_currencyType.Text.Trim();
            request.BV_FeeAmount_r = this.textBox_feeAmount.Text.Trim();


            string cmdText = request.ToText();

            this.Print("send:" + cmdText);
            BaseMessage response = null;
            int nRet = SCHelper.Instance.SendAndRecvMessage(cmdText,
                out response,
                out responseText,
                out error);
            if (nRet == -1)
            {
                MessageBox.Show(error);
                this.Print("error:" + error);
            }

            this.Print("recv:" + responseText);
        }
    }
}
