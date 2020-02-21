using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing.Printing;

using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.Forms;
using DigitalPlatform.LibraryRestClient;
using System.Collections.Generic;
using DigitalPlatform.IO;
using DigitalPlatform;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

namespace dp2Mini
{
    public partial class PrepForm : Form
    {
        // mid父窗口
        MainForm _mainForm = null;

        // 名字以用途命名即可。TokenSource 这种类型名称可以不出现在名字中
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public const string C_State_outof = "outof";
        public const string C_State_arrived = "arrived";

        /// <summary>
        ///  构造函数
        /// </summary>
        public PrepForm()
        {
            InitializeComponent();

            // 接管增加Note事件
            DbManager.Instance.RemoveNoteHandler -= new RemoveNoteDelegate(this.RemoveNote);
            DbManager.Instance.RemoveNoteHandler += new RemoveNoteDelegate(this.RemoveNote);

        }

        private void RemoveNote(string noteId,string itemPaths)
        {
            string[] paths = itemPaths.Split(new char[] {','});
            if (paths.Length > 0)
            {
                this.listView_results.BeginUpdate();
                try
                {
                    // 加到预约查询界面，省得用户再次检索
                    foreach (string path in paths)
                    {
                        ReservationItem item = DbManager.Instance.GetItem(path);
                        AppendNewLine(this.listView_results, item);
                    }

                    //// 按证条码号排序
                    //this.SortCol(1);
                    //this.SortCol(1);  // 点两次才是原来的序
                }
                finally
                {
                    this.listView_results.EndUpdate();
                }
            }
        }

        /// <summary>
        /// 窗体装载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrepForm_Load(object sender, EventArgs e)
        {
            this._mainForm = this.MdiParent as MainForm;
        }

        /// <summary>
        /// 检索预约到书记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_search_Click(object sender, EventArgs e)
        {
            //预约到书库和检索词
            string strQueryWord = this.textBox_queryWord.Text;
            string arrivedDbName = this._mainForm.ArrivedDbName;

            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();

            // 开一个新线程
            Task.Run(() =>
            {
                doSearch(this._cancel.Token,
                    arrivedDbName,
                    strQueryWord);
            });
        }

        /// <summary>
        /// 检索做事的函数
        /// </summary>
        /// <param name="token"></param>
        private void doSearch(CancellationToken token,
            string arrivedDbName,
            string strQueryWord)
        {
            string strError = "";

            // 记下原来的光标
            Cursor oldCursor = Cursor.Current;

            // 用Invoke线程安全的方式来调
            this.Invoke((Action)(() =>
            {
                // 设置按钮状态
                EnableControls(false);

                //清空界面数据
                this.ClearInfo();

                // 鼠标设为等待状态
                oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
            }
            ));

            RestChannel channel = this._mainForm.GetChannel();
            try
            {
                string strFrom = "读者证条码号"; // 检索途径
                string strMatchStyle = "exact";  //匹配方式
                // 如果检索词为空，则按__id检索出全部记录
                if (string.IsNullOrEmpty(strQueryWord))
                {
                    strFrom = "__id";
                    strMatchStyle = "left";
                }

                // 拼装检索语句
                string strQueryXml = "<target list='" + arrivedDbName + ":" + strFrom + "'>" +
                    "<item>" +
                    "<word>" + strQueryWord + "</word>" +
                    "<match>" + strMatchStyle + "</match>" +
                    "<relation>=</relation>" +
                    "<dataType>string</dataType>" +
                    "</item>" +
                    "<lang>zh</lang>" +
                    "</target>";

                string strOutputStyle = "";
                SearchResponse searchResponse = channel.Search(strQueryXml,
                    "arrived",
                    strOutputStyle);
                long lRet = searchResponse.SearchResult.Value;
                if (lRet == -1)
                {
                    strError = "检索发生错误：" + strError;
                    goto ERROR1;
                }
                if (lRet == 0)
                {
                    strError = "未命中";
                    goto ERROR1;
                }

                long todoCount = 0;

                // 获取结果集记录
                long lTotalCount = lRet;
                long lStart = 0;
                long lOnceCount = lTotalCount;
                Record[] searchresults = null;
                while (token.IsCancellationRequested == false)
                {
                    lRet = channel.GetSearchResult("arrived",
                        lStart,
                        lOnceCount,
                        "id,xml",// cols,
                        "zh",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获得检索结果发生错误：" + strError;
                        goto ERROR1;
                    }
                    else if (lRet == 0)
                    {
                        strError = "获得0 条检索结果";
                        goto ERROR1;
                    }

                    // 处理每一条记录
                    int i = 0;
                    foreach (Record record in searchresults)
                    {
                        if (token.IsCancellationRequested == true)
                            break;

                        string strPath = record.Path;

                        // 先检查一下本地库有没有记录，如果有从本地拉，如果没有服务器拉
                        ReservationItem item = DbManager.Instance.GetItem(strPath);
                        if (item == null)
                        {
                            // 把一条记录，解析出各列
                            //string[] cols = null;
                            int nRet = GetDetailInfo(channel,
                               strPath,
                               record.RecordBody.Xml,
                               out item,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            // 先存到本地库，省得下次再获取详细信息
                            DbManager.Instance.AddItem(item);
                        }

                        // 如果这笔记录还没有做备书单，则在这里显示
                        // 否则不在结果列表中显示，在备书单那儿显示
                        if (String.IsNullOrEmpty(item.NoteId) == true)
                        {
                            this.Invoke((Action)(() =>
                            {
                                // 状态为outof且未创建地预约单的记录，加到保留期列表中
                                if (item.State == C_State_outof 
                                && string.IsNullOrEmpty(item.NoteId)==true)
                                {
                                    // 增加一行到超过保留期的
                                    AppendNewLine(this.listView_outof, item);
                                }
                                else
                                {
                                    // 增加一行到预约到书
                                    AppendNewLine(this.listView_results, item);
                                    todoCount++;
                                }

                                // 设置状态栏信息 todo需要弄清楚这里怎么理解合理 2020/2/21
                                //this._mainForm.SetStatusMessage((lStart + i + 1).ToString() + " / " + lTotalCount);

                                // 数量加1
                                i++;
                            }
                        ));
                        }

                    }

                    lStart += searchresults.Length;
                    if (lStart >= lTotalCount)
                        break;
                }
                this.Invoke((Action)(() =>
                {
                    if (todoCount > 0)
                    {
                        // 按证条码号排序
                        this.SortCol(1);
                    }
                }
                ));

                return;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                // 设置按置状态
                this.Invoke((Action)(() =>
                {
                    EnableControls(true);
                    this.Cursor = oldCursor;

                    this._mainForm.ReturnChannel(channel);
                }
                ));
            }

        ERROR1:
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(strError);
            }
            ));
        }

        /// <summary>
        /// 清空界面数据
        /// </summary>
        public void ClearInfo()
        {
            // 清空listview
            this.listView_results.Items.Clear();
            this.listView_outof.Items.Clear();

            //设置父窗口状态栏参数
            this._mainForm.SetStatusMessage("");
        }

        /// <summary>
        /// 获取记录详细信息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="strRecord"></param>
        /// <param name="cols"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        int GetDetailInfo(RestChannel channel,
            string recPath,
            string strRecordXml,
            out ReservationItem reserItem,
           out string strError)
        {
            strError = "";

            reserItem = new ReservationItem();
            reserItem.RecPath = recPath;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strRecordXml);
            XmlNode nodeRoot = dom.DocumentElement;

            reserItem.RecPath = recPath;
            reserItem.State = DomUtil.GetElementText(nodeRoot, "state");
            reserItem.ItemBarcode = DomUtil.GetElementText(nodeRoot, "itemBarcode");
            reserItem.ItemRefID = DomUtil.GetElementText(nodeRoot, "itemRefID");
            reserItem.PatronBarcode = DomUtil.GetElementText(nodeRoot, "readerBarcode");
            reserItem.LibraryCode = DomUtil.GetElementText(nodeRoot, "libraryCode");
            reserItem.OnShelf = DomUtil.GetElementText(nodeRoot, "onShelf");
            reserItem.NotifyTime = DateTimeUtil.ToLocalTime(DomUtil.GetElementText(nodeRoot, "notifyDate"), "yyyy-MM-dd HH:mm:ss");
            reserItem.Location = DomUtil.GetElementText(nodeRoot, "location");
            reserItem.AccessNo = DomUtil.GetElementText(nodeRoot, "accessNo");

            // 以下字段为图书信息
            reserItem.ISBN = "";
            reserItem.Title = "";
            reserItem.Author = "";

            // 以下字段是读者信息
            reserItem.PatronName = "";
            reserItem.Department = "";
            reserItem.PatronTel = "";
            reserItem.RequestTime = "";
            reserItem.ArrivedTime = "";

            // 备书产生的字段
            reserItem.PrintState = DomUtil.GetElementText(nodeRoot, "printState");
            reserItem.CheckResult = "";// 是否找到图书，值为：找到/未找到

            // 过了保留期的数据，不再获取详细数据
            if (reserItem.State == C_State_outof)
            {
                strError = "因为过了保留期，不必再获取详细数据了。";
                return 0;
            }

            // 获取册信息以及书目信息
            if (!string.IsNullOrEmpty(reserItem.ItemBarcode))
            {
                GetItemInfoResponse response = channel.GetItemInfo(reserItem.ItemBarcode,
                    "xml",
                    "xml");
                if (response.GetItemInfoResult.Value == -1)
                {
                    strError = "获取册记录'"+reserItem.ItemBarcode+"'出错:" + response.GetItemInfoResult.ErrorInfo;
                    return -1;
                }
                if (response.GetItemInfoResult.Value == 0)
                {
                    strError = "获取册记录'" + reserItem.ItemBarcode + "未命中";
                    return -1;
                }

                string strOutMarcSyntax = "";
                string strMARC = "";
                string strMarcXml = response.strBiblio;
                int nRet = MarcUtil.Xml2Marc(strMarcXml,
                    false,
                    "", // 自动识别 MARC 格式
                    out strOutMarcSyntax,
                    out strMARC,
                    out strError);
                if (nRet == -1)
                    return -1;

                MarcRecord marcRecord = new MarcRecord(strMARC);
                reserItem.ISBN = marcRecord.select("field[@name='010']/subfield[@name='a']").FirstContent;
                reserItem.Title = marcRecord.select("field[@name='200']/subfield[@name='a']").FirstContent;
                reserItem.Author = marcRecord.select("field[@name='200']/subfield[@name='f']").FirstContent;
            }

            // 获取读者信息
            if (string.IsNullOrEmpty(reserItem.PatronBarcode) == false)
            {
                GetReaderInfoResponse readerRet = channel.GetReaderInfo(reserItem.PatronBarcode,
                    "xml:noborrowhistory");
                if (readerRet.GetReaderInfoResult.Value == -1)
                {
                    strError = "获取册记录'" + reserItem.ItemBarcode + "'出错:" + readerRet.GetReaderInfoResult.ErrorInfo;
                    return -1;
                }
                if (readerRet.GetReaderInfoResult.Value == 0)
                {
                    strError = "获取册记录'" + reserItem.ItemBarcode + "'未命中。";// + readerRet.GetReaderInfoResult.ErrorInfo;
                    return -1;
                }

                string strPatronXml = readerRet.results[0];
                dom.LoadXml(strPatronXml);
                XmlNode rootNode = dom.DocumentElement;
                reserItem.PatronName = DomUtil.GetElementText(rootNode, "name");
                reserItem.Department = DomUtil.GetElementText(rootNode, "department");
                reserItem.PatronTel = DomUtil.GetElementText(rootNode, "tel");

                /*
                - <root expireDate="">
                 <barcode>XZP10199</barcode> 
                 <readerType>学生</readerType> 
                 <name>李明</name> 
                 <overdues /> 
               - <reservations>
                 <request items="XZ000101" requestDate="Tue, 11 Feb 2020 00:30:27 +0800" 
                    operator="XZP10199" state="arrived" arrivedDate="Tue, 11 Feb 2020 00:31:45 +0800" 
                    arrivedItemBarcode="XZ000101" notifyID="59abfc23-f44f-4b34-a22c-f8a8aa5e289e" 
                    accessNo="K825.6=76/Z780" location="星洲学校/图书馆,#reservation" /> 
                 </reservations>
                 </root>
                 */
                XmlNodeList nodeList = rootNode.SelectNodes("reservations/request");
                foreach (XmlNode node in nodeList)
                {
                    string arrivedItemBarcode = DomUtil.GetAttr(node, "arrivedItemBarcode");
                    if (arrivedItemBarcode == reserItem.ItemBarcode)
                    {
                        reserItem.RequestTime = DateTimeUtil.ToLocalTime(DomUtil.GetAttr(node, "requestDate"), "yyyy-MM-dd HH:mm:ss");
                        reserItem.ArrivedTime = DateTimeUtil.ToLocalTime(DomUtil.GetAttr(node, "arrivedDate"), "yyyy-MM-dd HH:mm:ss");
                        break;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// 在 ListView 最后追加一行
        /// </summary>
        /// <param name="list">ListView 对象</param>
        /// <param name="strID">左边第一列内容</param>
        /// <param name="others">其余列内容</param>
        /// <returns>新创建的 ListViewItem 对象</returns>
        public static ListViewItem AppendNewLine(ListView list,
            ReservationItem resItem)
        {
            ListViewItem viewItem = new ListViewItem(resItem.RecPath, 0);
            list.Items.Add(viewItem);

            /*
            路径
            读者证条码
            读者姓名
            */
            viewItem.SubItems.Add(resItem.PatronBarcode);
            viewItem.SubItems.Add(resItem.PatronName);
            /*
            册条码
            书名
            索取号
            馆藏地点
            ISBN
            作者
            */
            viewItem.SubItems.Add(resItem.ItemBarcode);
            viewItem.SubItems.Add(resItem.Title);
            viewItem.SubItems.Add(resItem.AccessNo);
            viewItem.SubItems.Add(resItem.Location);
            viewItem.SubItems.Add(resItem.ISBN);
            viewItem.SubItems.Add(resItem.Author);
            /*
            读者电话
            读者部门
            预约时间
            到书时间
            预约状态
             */
            viewItem.SubItems.Add(resItem.PatronTel);
            viewItem.SubItems.Add(resItem.Department);
            viewItem.SubItems.Add(resItem.RequestTime);
            viewItem.SubItems.Add(resItem.NotifyTime);
            viewItem.SubItems.Add(resItem.State);

            if (resItem.State == C_State_outof)
                viewItem.BackColor = Color.LightGray;

            return viewItem;
        }

        /// <summary>
        /// 设置控件是否可用
        /// </summary>
        /// <param name="bEnable"></param>
        void EnableControls(bool bEnable)
        {
            this.button_search.Enabled = bEnable;
            this.button_stop.Enabled = !(bEnable);
        }

        /// <summary>
        /// 停止检索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            // 停止
            this._cancel.Cancel();
        }

        /// <summary>
        /// 右键命令是否可用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (this.listView_results.SelectedItems.Count <= 0)
            {
                this.toolStripMenuItem_createNote.Enabled = false;
            }
            else
            {
                this.toolStripMenuItem_createNote.Enabled = true;
            }
        }
        
        #region 列表排序

        SortColumns SortColumns1 = new SortColumns();

        // 参与排序的列号数组
        SortColumns SortColumns2 = new SortColumns();

        private void listView_outof_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;

            this.SortColumns2.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_outof.Columns,
                true);

            // 排序
            this.listView_outof.ListViewItemSorter = new SortColumnsComparer(this.SortColumns2);

            this.listView_outof.ListViewItemSorter = null;
        }

        private void listView_results_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            this.SortCol(nClickColumn);
        }

        public void SortCol(int nClickColumn)
        {
            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;

            this.SortColumns1.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_results.Columns,
                true);

            // 排序
            this.listView_results.ListViewItemSorter = new SortColumnsComparer(this.SortColumns1);

            this.listView_results.ListViewItemSorter = null;
        }

        #endregion


        /// <summary>
        /// 制作备书单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_bs_Click(object sender, EventArgs e)
        {
            if (this.listView_results.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择预约到书记录。");
                return;
            }


            // 按读者拆分，每个读者一个备书单
            Hashtable patronTable = new Hashtable();
            foreach (ListViewItem viewItem in this.listView_results.SelectedItems)
            {
                string path = viewItem.SubItems[0].Text;
                string patronBarcode = viewItem.SubItems[1].Text;
                string patronName = viewItem.SubItems[2].Text;

                string fullName = patronName + "("+patronBarcode+")";
                // 检查是否已在hashtable 
                if (patronTable.ContainsKey(fullName) == true)
                {
                    List<ListViewItem> items = (List<ListViewItem>)patronTable[fullName];
                    items.Add(viewItem);
                }
                else
                {
                    List<ListViewItem> items = new List<ListViewItem>();
                    items.Add(viewItem);
                    patronTable[fullName] = items;
                }
            }

            // 处理每一个备书单
            //遍历方法一：遍历哈希表中的键
            foreach (string key in patronTable.Keys)
            {
                string patronTel = "";
                List<string> pathList = new List<string>();
                List<ListViewItem> items = (List<ListViewItem>)patronTable[key];
                foreach (ListViewItem item in items)
                {
                    string path = item.SubItems[0].Text;
                    pathList.Add(path);

                    if (patronTel == "")
                    {
                        patronTel = item.SubItems[9].Text;
                    }

                    // 从预约到书列表中删除
                    this.listView_results.Items.Remove(item);
                }

                // 存储到本地备书单库
                DbManager.Instance.AddNote(pathList,key, patronTel);
            }

            MessageBox.Show(this, "创建备书单完成，请到'备书单管理'界面查看。");
            return;
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 全选AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView_results.Items)
            {
                item.Selected = true;
            }
        }
    }



}
