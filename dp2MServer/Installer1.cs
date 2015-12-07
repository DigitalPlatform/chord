using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace dp2MServer
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        private System.ServiceProcess.ServiceProcessInstaller ServiceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;

        public Installer1()
        {
            InitializeComponent();

            this.ServiceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // ServiceProcessInstaller1
            // 
            this.ServiceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;  // LocalSystem
            this.ServiceProcessInstaller1.Password = null;
            this.ServiceProcessInstaller1.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.DisplayName = "dp2 Message Service";
            this.serviceInstaller1.ServiceName = "dp2MessageService";
            this.serviceInstaller1.Description = "dp2消息服务器，数字平台北京软件有限责任公司 http://dp2003.com";
            this.serviceInstaller1.StartType = ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
				this.ServiceProcessInstaller1,
				this.serviceInstaller1});

            this.serviceInstaller1.Committed += new InstallEventHandler(serviceInstaller1_Committed);

        }

        void serviceInstaller1_Committed(object sender, InstallEventArgs e)
        {
            try
            {
                ServiceController sc = new ServiceController(this.serviceInstaller1.ServiceName);
                sc.Start();
            }
            catch (Exception ex)
            {
                // 报错，但是不停止安装
                //MessageBox.Show(ForegroundWindow.Instance,
                //    "安装已经完成，但启动 '" + this.serviceInstaller1.ServiceName + "' 失败： " + ex.Message);
                Console.WriteLine("安装已经完成，但启动 '" + this.serviceInstaller1.ServiceName + "' 失败： " + ex.Message);
            }
        }

        public override void Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);

            // 创建事件日志目录
            if (!EventLog.SourceExists("dp2MServer"))
            {
                EventLog.CreateEventSource("dp2MServer", "DigitalPlatform");
            }
            EventLog Log = new EventLog();
            Log.Source = "dp2MServer";

            Log.WriteEntry("dp2MServer 安装成功。", EventLogEntryType.Information);
        }

    }
}
