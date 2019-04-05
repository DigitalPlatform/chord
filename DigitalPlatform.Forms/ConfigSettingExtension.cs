using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

using DigitalPlatform.Xml;
using DigitalPlatform.Core;

namespace DigitalPlatform.Forms
{
    public static class ConfigSettingExtension
    {
        // 从配置文件中读取信息，设置form尺寸位置状态
        // parameters:
        //		form	Form对象
        //		strCfgTitle	配置信息路径。本函数将用此值作为GetString()或GetInt()的strPath参数使用
        public static void LoadFormStates(this ConfigSetting config, 
            Form form,
            string strCfgTitle,
            FormWindowState default_state)
        {
            // 为了优化视觉效果
            bool bVisible = form.Visible;

            if (bVisible == true)
                form.Visible = false;

            form.Width = config.GetInt(
                strCfgTitle, "width", form.Width);
            form.Height = config.GetInt(
                strCfgTitle, "height", form.Height);

            form.Location = new Point(
                config.GetInt(strCfgTitle, "x", form.Location.X),
                config.GetInt(strCfgTitle, "y", form.Location.Y));

            string strState = config.Get(
                strCfgTitle,
                "window_state",
                "");
            if (String.IsNullOrEmpty(strState) == true)
            {
                form.WindowState = default_state;
            }
            else
            {
                form.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState),
                    strState);
            }

            if (bVisible == true)
                form.Visible = true;

            /// form.Update();  // 2007/4/8
        }

        // 保存form尺寸位置状态到配置文件中
        // parameters:
        //		form	Form对象
        //		strCfgTitle	配置信息路径。本函数将用此值作为SetString()或SetInt()的strPath参数使用
        public static void SaveFormStates(this ConfigSetting config,
            Form form,
            string strCfgTitle)
        {
            // 保存窗口状态
            config.Set(
                strCfgTitle, "window_state",
                Enum.GetName(typeof(FormWindowState),
                form.WindowState));

            Size size = form.Size;
            Point location = form.Location;

            if (form.WindowState != FormWindowState.Normal)
            {
                size = form.RestoreBounds.Size;
                location = form.RestoreBounds.Location;
            }

            config.SetInt(
                strCfgTitle, "width", size.Width);  // form.Width
            config.SetInt(
                strCfgTitle, "height", size.Height);    // form.Height

            config.SetInt(strCfgTitle, "x", location.X); // form.Location.X
            config.SetInt(strCfgTitle, "y", location.Y); // form.Location.Y

            // 保存MDI窗口状态 -- 是否最大化？
            if (form.ActiveMdiChild != null)
            {
                if (form.ActiveMdiChild.WindowState == FormWindowState.Minimized)
                    config.Set(
                        strCfgTitle,
                        "mdi_child_window_state",
                        Enum.GetName(typeof(FormWindowState),
                        FormWindowState.Normal));
                else
                    config.Set(
                        strCfgTitle,
                        "mdi_child_window_state",
                        Enum.GetName(typeof(FormWindowState),
                        form.ActiveMdiChild.WindowState));
            }
            else
            {
                config.Set(
                    strCfgTitle,
                    "mdi_child_window_state",
                    "");
            }
        }

    }
}
