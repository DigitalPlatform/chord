using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Xml;

using dp2Circulation;
using DigitalPlatform.Marc;
using DigitalPlatform.Script;
using DigitalPlatform.Text;

public class CheckContainHanzi : MarcQueryHost
{
    // 将包含汉字的记录选中
    public override void OnRecord(object sender, StatisEventArgs e)
    {
        ListViewItem item = (ListViewItem)this.UiItem;

        if (StringUtil.ContainHanzi(this.MarcRecord.Text))
            item.Selected = true;
        else
            item.Selected = false;
    }
}

