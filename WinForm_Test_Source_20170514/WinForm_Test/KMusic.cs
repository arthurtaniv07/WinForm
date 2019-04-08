using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using St.IO;
using St.Web;
using St.Common;
using AxAPlayer3Lib;
using St;

namespace WinForm_Test
{
    public partial class KMusic : Form
    {
        private volatile static KMusic _this = null;
        private static readonly object lockHelper = new object();

        private KMusic()
        {
            InitializeComponent();
        }
        public static  KMusic GetInstance()
        {
            if (_this == null)
            {
                lock (lockHelper)
                {
                    if (_this == null)
                    {
                        _this = new KMusic();
                    }
                }
            }
            return _this;
        }

        private void KMusic_Load(object sender, EventArgs e)
        {

            //6 酷我歌曲
            #region 酷我歌曲
            //winSize.Add(6, new Size(1465, 691));
            this.richTextBox1.Tag = "0";
            this.label16.Text = "";
            this.label18.Text = "";
            Kuwo_Btn_Enabled(false);
            //文本居中对齐
            this.richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            //播放器
            axPlayer1.OnBuffer += new _IPlayerEvents_OnBufferEventHandler(axPlayer1_OnBuffer);
            axPlayer1.OnStateChanged += new _IPlayerEvents_OnStateChangedEventHandler(axPlayer1_OnStateChanged);
            axPlayer1.OnSeekCompleted += new _IPlayerEvents_OnSeekCompletedEventHandler(axPlayer1_OnSeekCompleted);
            axPlayer1.OnOpenSucceeded += new EventHandler(axPlayer1_OnOpenSucceeded);
            axPlayer1.OnDownloadCodec += new _IPlayerEvents_OnDownloadCodecEventHandler(axPlayer1_OnDownloadCodec);
            axPlayer1.OnMessage += new _IPlayerEvents_OnMessageEventHandler(axPlayer1_OnMessage);
            axPlayer1.SetCustomLogo(new Bitmap(@"config\Img\St.png").GetHbitmap().ToInt32());  //自定义logo
            axPlayer1.SetVolume(50);
            trackBar2.Value = 50;
            this.label23.Text = "酷音乐  因悦你的生活";
            button16.Enabled = false;
            button17.Enabled = false;
            trackBar1.Enabled = false;
            musicPlay.Interval = 200;
            musicPlay.Tick += new EventHandler(delegate(object obj, EventArgs e_mucis)
            {
                label22.Text = TimeToString(TimeSpan.FromMilliseconds(axPlayer1.GetPosition()));
                trackBar1.Value = axPlayer1.GetPosition() <= 0 ? 0 : axPlayer1.GetPosition();
            });
            #endregion
        }


        private void KMusic_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (lrcPlay_Time != null)
            {
                lrcPlay_Time.Enabled = false;
                lrcPlay_Time.Stop();
            }
            if (axPlayer1 != null)
            {
                axPlayer1.Dispose();
            }
            if (_this != null)
            {
                _this = null;
            }
        }

        /// <summary>
        /// 需要联网功能调用的方法
        /// </summary>
        /// <returns></returns>
        public bool Pub_NetWork()
        {
            bool rel = StEnvironment.IsInternetAlive();
            if (!rel) MessageBox.Show("请检查您的网络连接", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            return rel;
        }



        #region 酷我音乐

        //以下具体设置 如APlayer.SetConfig()方法的用法请参见  开发文档chm
        private void 截图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //截图功能正常
            axPlayer1.SetConfig(702, string.Format("{0}\\截图{1}.bmp", Application.StartupPath, DateTime.Now.ToString("yyyyMMddHHmmss")));

        }
        /// <summary>
        /// 时间格式转换
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        string TimeToString(TimeSpan span)
        {
            return span.Hours.ToString("00") + ":" +
            span.Minutes.ToString("00") + ":" +
            span.Seconds.ToString("00");
        }

        #region  axPlayer事件处理程序
        Timer musicPlay = new Timer();

        /// <summary>
        /// 定时更新进度条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            label22.Text = TimeToString(TimeSpan.FromMilliseconds(axPlayer1.GetPosition()));
            trackBar1.Value = axPlayer1.GetPosition() <= 0 ? 0 : axPlayer1.GetPosition();
        }

        /// <summary>
        /// axPlayer的鼠标、键盘事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void axPlayer1_OnMessage(object sender, _IPlayerEvents_OnMessageEvent e)
        {

            switch (e.nMessage)
            {
                case 513:
                    APlayPlayOrPause();
                    break;
                case 517:
                    //contextMenuStrip2.Show(axPlayer1, axPlayer1.PointToClient(Cursor.Position));
                    break;
                default:
                    break;
            }
            //....
        }
        /// <summary>
        /// 格式不支持，需要下载解码器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void axPlayer1_OnDownloadCodec(object sender, _IPlayerEvents_OnDownloadCodecEvent e)
        {
            MessageBox.Show(e.strCodecPath);
        }
        /// <summary>
        /// 文件打开完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void axPlayer1_OnOpenSucceeded(object sender, EventArgs e)
        {
            label22.Text = "00:00:00";
            label21.Text = TimeToString(TimeSpan.FromMilliseconds(axPlayer1.GetDuration()));
            trackBar1.Enabled = true;
            trackBar1.Maximum = axPlayer1.GetDuration();
            button17.Enabled = true;
            button16.Enabled = true;
            musicPlay.Start();
        }
        /// <summary>
        /// 跳转指定位置完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void axPlayer1_OnSeekCompleted(object sender, _IPlayerEvents_OnSeekCompletedEvent e)
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 播放器状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void axPlayer1_OnStateChanged(object sender, _IPlayerEvents_OnStateChangedEvent e)
        {
            if (e.nNewState == 0)  //就绪
            {
                //初始化
                button17.Enabled = false;
                button16.Enabled = false;

                button16.Enabled = true;
                button17.Enabled = true;
                trackBar1.Maximum = 100;
                trackBar1.Value = 0;
                trackBar1.Enabled = false;
                label22.Text = "00:00:00";
                label21.Text = "00:00:00";
                musicPlay.Stop();
            }
            if (e.nNewState == 1)  //正在打开
            {
                button17.Enabled = false;
                button16.Enabled = false;
            }
            if (e.nNewState == 5 || e.nNewState == 4)  //播放
            {
                button17.Text = "暂停";
                button17.Enabled = true;
                button16.Enabled = true;
            }
            if (e.nNewState == 3 || e.nNewState == 2)  //暂停
            {
                button17.Text = "播放";
                button17.Enabled = true;
                button16.Enabled = true;
            }
            if (e.nNewState == 6)  //停止
            {

            }
        }
        /// <summary>
        /// 缓冲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void axPlayer1_OnBuffer(object sender, _IPlayerEvents_OnBufferEvent e)
        {
            if (e.nPercent != 100)
            {
                label20.Visible = true;
                label20.Text = "正在缓冲...(" + e.nPercent + "%)";
            }
            else
            {
                label20.Visible = false;
            }
        }
        #endregion


        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

            CheckedMvOrLrc(1);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            CheckedMvOrLrc(2);
        }


        #region 右键菜单


        private void 搜索歌手ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { this.trackBar1.Enabled = false; return; }
            else { this.trackBar1.Enabled = true; }
            string Singer = dataGridView1.SelectedRows[0].Cells["Singer"].Value + "";
            if (string.IsNullOrWhiteSpace(Singer)) { return; }
            BingKuwoMusicList(Singer, 1, 0);
        }


        private void 搜索专辑ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { this.trackBar1.Enabled = false; return; }
            else { this.trackBar1.Enabled = true; }
            string AlbumName = dataGridView1.SelectedRows[0].Cells["AlbumName"].Value + "";
            if (string.IsNullOrWhiteSpace(AlbumName)) { return; }
            BingKuwoMusicList(AlbumName, 2, 0);
        }

        private void 查看歌词ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Text = "正在查询...";

            if (!Pub_NetWork()) { this.trackBar1.Enabled = false; this.richTextBox1.Text = "网络连接中断，请检查您的网络..."; return; }
            else { this.trackBar1.Enabled = true; }
            System.Threading.Thread.Sleep(20);

            int rowIndex = dataGridView1.SelectedRows[0].Index;
            string ID = dataGridView1.SelectedRows[0].Cells["ID"].Value + "";
            string SongName = dataGridView1.SelectedRows[0].Cells["SongName"].Value + "";
            string Singer = dataGridView1.SelectedRows[0].Cells["Singer"].Value + "";

            var u = StAPIHelper.Ex_Kuwo_SearchLyrics(ID);

            this.richTextBox1.Text = string.Join("\r\n", u.Values.Select(i => i.StartTime + "," + i.Text));
            if (string.IsNullOrWhiteSpace(this.richTextBox1.Text)) { this.richTextBox1.Text = "很抱歉，未能找到您要的歌词"; }

        }


        public void CheckedMvOrLrc(int num, bool checkedR = false)
        {
            this.richTextBox1.Visible = false;
            this.axPlayer1.Visible = false;
            switch (num)
            {
                case 1:
                    this.richTextBox1.Visible = true;
                    if (checkedR)
                        this.radioButton3.Checked = true;
                    break;
                case 2:
                    this.axPlayer1.Visible = true;
                    if (checkedR)
                        this.radioButton4.Checked = true;
                    break;
            }
        }

        private void 播放MVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.label20.Visible = false;
            if (dataGridView1.SelectedRows.Count <= 0)
            {
                MessageBox.Show("请先选择需要播放的MV");
                return;
            }

            if (!Pub_NetWork()) { this.trackBar1.Enabled = false; return; }
            else { this.trackBar1.Enabled = true; }
            //if (!Ve()) return;
            string MV = dataGridView1.SelectedRows[0].Cells["ExistMV"].Value + "";
            if (string.IsNullOrWhiteSpace(MV))
            {
                MessageBox.Show("对不起您要播放的MV不存在");
                return;
            }
            string ID = dataGridView1.SelectedRows[0].Cells["ID"].Value + "";
            string SongName = dataGridView1.SelectedRows[0].Cells["SongName"].Value + "";
            string Singer = dataGridView1.SelectedRows[0].Cells["Singer"].Value + "";

            //StAPIHelper.Kuwo_Song song = dataGridView1.SelectedRows[0] as StAPIHelper.Kuwo_Song;

            DateTime start = DateTime.Now;
            string u = StAPIHelper.Ex_Kuwo_GetMVDownUrl(ID);
            if (string.IsNullOrEmpty(u)) { MessageBox.Show("资源不存在"); return; }
            CheckedMvOrLrc(2, true);
            SetSongName(SongName, Singer);
            axPlayer1.Open(u);
        }


        private void 下载MVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count <= 0)
            {
                MessageBox.Show("请先选择需要下载的MV");
                return;
            }

            if (!Pub_NetWork()) { this.trackBar1.Enabled = false; return; }
            else { this.trackBar1.Enabled = true; }
            if (!Ve()) return;


            string MV = dataGridView1.SelectedRows[0].Cells["ExistMV"].Value + "";
            if (string.IsNullOrWhiteSpace(MV))
            {
                MessageBox.Show("对不起您要下载的MV不存在");
                return;
            }
            string ID = dataGridView1.SelectedRows[0].Cells["ID"].Value + "";
            string SongName = dataGridView1.SelectedRows[0].Cells["SongName"].Value + "";
            string Singer = dataGridView1.SelectedRows[0].Cells["Singer"].Value + "";

            string u = StAPIHelper.Ex_Kuwo_GetMVDownUrl(ID);

            string filepath = StDirectory.CurrentAppDirectory + "\\" + this.textBox22.Text.Replace("/", "\\") + "\\" + Singer + " - " + SongName + u.Substring(u.LastIndexOf('.'));
            if (StFile.Exists(filepath, false, false))
            {
                MessageBox.Show("该MV已经被下载过啦");
                return;
            }

            Down(u, StDirectory.CurrentAppDirectory + "\\" + this.textBox22.Text.Replace("/", "\\"), Singer + " - " + SongName + u.Substring(u.LastIndexOf('.')));
            //MessageBox.Show("下载" + (re ? "成功" : "失败"));
        }
        private void 下载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count <= 0)
            {
                MessageBox.Show("请先选择需要下载的歌曲");
                return;
            }
            if (!Pub_NetWork()) { this.trackBar1.Enabled = false; return; }
            else { this.trackBar1.Enabled = true; }

            if (!Ve()) return;

            DateTime start = DateTime.Now;
            int rowIndex = dataGridView1.SelectedRows[0].Index;
            string ID = dataGridView1.SelectedRows[0].Cells["ID"].Value + "";
            string SongName = dataGridView1.SelectedRows[0].Cells["SongName"].Value + "";
            string Singer = dataGridView1.SelectedRows[0].Cells["Singer"].Value + "";

            string u = StAPIHelper.Ex_Kuwo_GetSongDownUrl(ID);
            string filepath = StDirectory.CurrentAppDirectory + "\\" + this.textBox22.Text.Replace("/", "\\") + "\\" + Singer + " - " + SongName + u.Substring(u.LastIndexOf('.'));
            if (StFile.Exists(filepath, false, false))
            {
                MessageBox.Show("该歌曲已经被下载过啦");
                return;
            }
            Down(u, StDirectory.CurrentAppDirectory + "\\" + this.textBox22.Text.Replace("/", "\\"), Singer + " - " + SongName + u.Substring(u.LastIndexOf('.')), "歌曲");
            //MessageBox.Show("下载" + (re ? "成功" : "失败"));
        }


        public void Down(string url, string savePath, string fileName, string type = "MV")
        {
            this.label18.Text = "正在下载" + type + "( " + fileName + " ) ";
            System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Down_ThreadFun));
            th.Start(new string[] { url, savePath, fileName, type });
        }

        public void Down_ThreadFun(object par)
        {
            string[] pars = par as string[];
            string url = pars[0];
            string savePath = pars[1];
            string fileName = pars[2];
            string type = pars[3];

            DateTime start = DateTime.Now;

            long v = -1;
            long.TryParse(this.textBox21.Text.Trim(), out v);
            this.textBox21.Text = v.ToString();

            bool re = new DownLoadHelper().DownLoadFile(url, savePath, fileName, v);
            string filepath = savePath + "\\" + fileName;

            double size = StMath.ToPrecision(StFile.GetFileSize(filepath) * 1.0 / 1000000, 2);
            double t = StMath.ToPrecision((DateTime.Now - start).TotalSeconds, 2);
            this.label18.Text = "上次下载" + type + "( " + fileName + " ) " + size + "M 共耗时 " + t + " 秒，平均 " + StMath.ToPrecision(size / t, 2) + "M/s ";
        }

        public void SetSongName(string songName, string Singer)
        {
            songName = songName.Split('-')[0].Trim();
            this.label23.Text = string.Format("{0} - {1}", songName, Singer);
        }
        /// <summary>
        /// 验证是否有下载权限 
        /// </summary>
        /// <returns></returns>
        private bool Ve()
        {
            if (this.textBox19.Text != "dbbd")
            {
                MessageBox.Show("对不起，您未授权下载");
                return false;
            }
            return true;
        }
        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            播放ToolStripMenuItem_Click(sender, e);
        }
        Timer lrcPlay_Time = null;

        private void 播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.label20.Visible = false;
            if (dataGridView1.SelectedRows.Count <= 0)
            {
                MessageBox.Show("请先选择需要播放的歌曲");
                return;
            }
            if (!Pub_NetWork()) { this.trackBar1.Enabled = false; return; }
            else { this.trackBar1.Enabled = true; }

            //if (!Ve()) return;

            DateTime start = DateTime.Now;
            int rowIndex = dataGridView1.SelectedRows[0].Index;
            string ID = dataGridView1.SelectedRows[0].Cells["ID"].Value + "";
            string SongName = dataGridView1.SelectedRows[0].Cells["SongName"].Value + "";
            string Singer = dataGridView1.SelectedRows[0].Cells["Singer"].Value + "";

            string u = StAPIHelper.Ex_Kuwo_GetSongDownUrl(ID);
            if (string.IsNullOrEmpty(u)) { MessageBox.Show("资源不存在"); return; }
            SetSongName(SongName, Singer);
            axPlayer1.Open(u);
            CheckedMvOrLrc(1, true);



            kuwo_lrcs = StAPIHelper.Ex_Kuwo_SearchLyrics(ID);

            this.richTextBox1.Tag = "0";
            this.richTextBox1.Text = string.Join("\r\n", kuwo_lrcs.Values.Select(i => i.TimeStr + "," + i.Text));
            if (!string.IsNullOrWhiteSpace(this.richTextBox1.Text))
            {
                this.richTextBox1.Text += "\r\n";
                this.richTextBox1.Tag = "1";
                kuwo_lrcs_index = 0;


                lrcPlay_Time = new Timer();
                int tttI = Kuwo_GetWainTime(kuwo_lrcs_index, out temp_i);
                kuwo_lrcs_index += temp_i - 1;

                int i = richTextBox1.GetFirstCharIndexFromLine(kuwo_lrcs_index);
                int j = richTextBox1.GetFirstCharIndexFromLine(kuwo_lrcs_index + temp_i);//注意如果要选取最后一行的时候，这里可能会出错，应当直接把j设置成为最后一个字符的位置+1
                richTextBox1.Select(i, j - i);

                this.richTextBox1.SelectionColor = Color.Red;
                this.richTextBox1.ScrollToCaret();
                lrcPlay_Time.Tick += new EventHandler(t_Tick);
                lrcPlay_Time.Interval = tttI;
                lrcPlay_Time.Start();
            }

            if (string.IsNullOrWhiteSpace(this.richTextBox1.Text))
            {
                this.richTextBox1.Text = "很抱歉，未能找到您要的歌词";
            }
            //string filepath = StDirectory.CurrentAppDirectory + "\\" + this.textBox22.Text.Replace("/", "\\") + "\\" + Singer + " - " + SongName + u.Substring(u.LastIndexOf('.'));
            //if (StFile.Exists(filepath, false, false))
            //{
            //    MessageBox.Show("该歌曲已经被下载过啦");
            //    return;
            //}
            //bool re = StAPIHelper.DownLoad(u, StDirectory.CurrentAppDirectory + "\\" + this.textBox22.Text.Replace("/", "\\"), Singer + " - " + SongName);

            //this.label18.Text = "上次下载歌曲(" + SongName + ")共耗时 " + StPub.ToPrecision((DateTime.Now - start).TotalSeconds, 2) + " 秒";
            //MessageBox.Show("下载" + (re ? "成功" : "失败"));
        }
        Dictionary<int, StAPIHelper.Kuwo_Lyric> _kuwo_lrcs = null;
        int kuwo_lrcs_index = 0;
        Dictionary<int, StAPIHelper.Kuwo_Lyric> kuwo_lrcs
        {
            get
            {
                if (_kuwo_lrcs == null)
                    _kuwo_lrcs = new Dictionary<int, StAPIHelper.Kuwo_Lyric>();
                return _kuwo_lrcs;
            }
            set
            {
                _kuwo_lrcs = value;
            }
        }
        int temp_i = 0;
        void t_Tick(object sender, EventArgs e)
        {
            if (this.richTextBox1.Tag + "" == "1")
            {
                if (kuwo_lrcs.Count > 0)
                {
                    this.richTextBox1.SelectionColor = richTextBox1.ForeColor;
                    this.richTextBox1.SelectionFont = richTextBox1.Font;
                    //歌词状态正常
                    kuwo_lrcs_index++;
                    this.Text = kuwo_lrcs.Count + "," + kuwo_lrcs_index;
                    if (kuwo_lrcs_index < kuwo_lrcs.Count)
                    {
                        (sender as Timer).Interval = Kuwo_GetWainTime(kuwo_lrcs_index, out temp_i);
                        kuwo_lrcs_index += temp_i - 1;
                        int i = richTextBox1.GetFirstCharIndexFromLine(kuwo_lrcs_index);

                        int j = richTextBox1.GetFirstCharIndexFromLine(kuwo_lrcs_index + temp_i);//注意如果要选取最后一行的时候，这里可能会出错，应当直接把j设置成为最后一个字符的位置+1
                        richTextBox1.Select(i, j - i);

                        this.richTextBox1.SelectionColor = Color.Red;
                        this.richTextBox1.ScrollToCaret();
                    }
                    else
                    {
                        (sender as Timer).Stop();
                    }
                }
            }
        }

        public int Kuwo_GetWainTime(int inx, out int c)
        {
            c = 1;
            if (inx >= kuwo_lrcs.Count - 1)
            {
                return int.MaxValue;
            }

            long t = kuwo_lrcs[inx].StartTime;
            if (inx == 0)
            {
                return (int)t;
            }
            long temp = 0;
            for (int i = inx + 1; i < kuwo_lrcs.Count; i++)
            {
                temp = kuwo_lrcs[i].StartTime;
                if (temp != t) { break; }
                c++;
            }
            return (int)(temp - t);

        }
        #endregion



        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            //当前操作的DataGridView
            DataGridView dgv = sender as DataGridView;

            //当前鼠标位置的行列信息
            int col = dgv.HitTest(e.X, e.Y).ColumnIndex;
            int row = dgv.HitTest(e.X, e.Y).RowIndex;


            //取消选中当前所有选中的行和单元格
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                dgv.Rows[i].Selected = false;
                for (int j = 0; j < dgv.Columns.Count; j++)
                {
                    dgv.Rows[i].Cells[j].Selected = false;
                }
            }
            if (row > -1)
            {
                //选中当前鼠标所在的行
                dgv.Rows[row].Selected = true;
            }

            ////右键弹出菜单
            //if (row>-1 && e.Button == MouseButtons.Right)
            //{



            //    //建立快捷菜单
            //    ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

            //    ////功能
            //    //contextMenuStrip.Items.Add("功能1");
            //    //contextMenuStrip.Items.Add("功能2");
            //    //contextMenuStrip.Items.Add("功能3");

            //    ////分隔线
            //    //contextMenuStrip.Items.Add(new ToolStripSeparator());

            //    //删除当前行
            //    ToolStripMenuItem tsmiRemoveCurrentRow = new ToolStripMenuItem("删除本行");
            //    tsmiRemoveCurrentRow.Click += (obj, arg) =>
            //    {
            //        dgv.Rows.RemoveAt(row);
            //    };
            //    contextMenuStrip.Items.Add(tsmiRemoveCurrentRow);

            //    //清空所有数据
            //    ToolStripMenuItem tsmiRemoveAll = new ToolStripMenuItem("清空数据");
            //    tsmiRemoveAll.Click += (obj, arg) =>
            //    {
            //        dgv.Rows.Clear();
            //    };
            //    contextMenuStrip.Items.Add(tsmiRemoveAll);

            //    contextMenuStrip.Show(dgv, new Point(e.X, e.Y));

            //}

        }

        /// <summary>
        /// 总页数
        /// </summary>
        static int kuwolist_totalPageCount = 0;
        /// <summary>
        /// 总条数
        /// </summary>
        static int kuwolist_totalCount = 0;
        static int kuwolist_Pageindex = 0;
        static int kuwolist_current = 20;

        public void Kuwo_Btn_Enabled(bool Enabled)
        {
            this.textBox20.Enabled = Enabled;
            this.button12.Enabled = Enabled;
            this.button13.Enabled = Enabled;
            this.button14.Enabled = Enabled;
            this.richTextBox1.Text = "";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { this.trackBar1.Enabled = false; return; }
            else { this.trackBar1.Enabled = true; }
            BingKuwoMusicList(this.textBox19.Text, -1, 0);
        }

        private void textBox19_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void textBox19_KeyDown(object sender, KeyEventArgs e)
        {
            button11_Click(sender, e);
        }

        public void BingKuwoMusicList(string query, int type, int pageindex)
        {
            if (!this.button11.Enabled) return;

            this.button11.Enabled = false;
            if (string.IsNullOrWhiteSpace(query))
            {
                this.button11.Enabled = true;
                MessageBox.Show("请输入关键字");
                return;
            }
            switch (type)
            {
                case 1:
                    this.radioButton1.Checked = true;
                    break;
                case 2:
                    this.radioButton2.Checked = true;
                    break;
                default:
                    break;
            }

            this.textBox19.Text = query;
            kuwolist_totalPageCount = 0;
            kuwolist_Pageindex = 0;
            Kuwo_Btn_Enabled(false);
            kuwolist_Pageindex = pageindex;
            bool mvShow = false;
            bool isErro = false;
            try
            {
                //不允许自动创建列
                dataGridView1.AutoGenerateColumns = false;

                List<StAPIHelper.Kuwo_Song> rel = null;
                if (this.radioButton1.Checked)
                {
                    kuwolist_current = 20;
                    rel = StAPIHelper.Ex_Kuwo_SearchSongBySinger(this.textBox19.Text, pageindex, kuwolist_current, out kuwolist_totalPageCount);
                }
                else
                {
                    rel = StAPIHelper.Ex_Kuwo_SearchSong(this.textBox19.Text, pageindex, out kuwolist_totalCount, out kuwolist_current, out kuwolist_totalPageCount);
                }
                dataGridView1.DataSource = rel;
                mvShow = rel.Where(i => !string.IsNullOrWhiteSpace(i.ExistMV)).Count() > 0;

            }
            catch (Exception)
            {
                MessageBox.Show("很抱歉 出错啦！"); isErro = true;
            }
            finally
            {
                Kuwo_Btn_Enabled(true);
            }
            this.button12.Enabled = pageindex > 0;
            this.button13.Enabled = pageindex < kuwolist_totalPageCount - 1;
            this.label16.Text = "第 " + (pageindex + 1) + " 页，共 " + kuwolist_totalPageCount + " 页，每页 " + kuwolist_current + " 条";
            if (!isErro && dataGridView1.SelectedRows.Count < 1) { MessageBox.Show("很抱歉，没能找到您要的数据"); }


            dataGridView1.Columns["ExistMV"].Visible = mvShow;
            dataGridView1.Columns["ID"].Width = 77;
            this.button11.Enabled = true;

        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (kuwolist_Pageindex > kuwolist_totalPageCount) { MessageBox.Show("已经是最后一页啦"); return; }
            BingKuwoMusicList(this.textBox19.Text, -1, kuwolist_Pageindex + 1);

        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (kuwolist_Pageindex == 0) { MessageBox.Show("已经是第一页啦"); return; }
            BingKuwoMusicList(this.textBox19.Text, -1, kuwolist_Pageindex - 1);
        }
        private void button14_Click(object sender, EventArgs e)
        {
            int toindex = 0;
            if (int.TryParse(this.textBox20.Text, out toindex))
            {
                if (toindex <= 0 || toindex > kuwolist_totalPageCount)
                {
                    MessageBox.Show("只能输入 1 到 " + kuwolist_totalPageCount + " 的数字");
                }
                else
                {
                    if (!Pub_NetWork()) return;
                    BingKuwoMusicList(this.textBox19.Text, -1, toindex - 1);
                }
            }
            else
            {
                MessageBox.Show("只能输入数字");

            }
        }
        private void button15_Click(object sender, EventArgs e)
        {
            string path = StDirectory.CurrentAppDirectory + "\\" + this.textBox22.Text;
            StDirectory.ExistsDirectory(path, true);
            if (StDirectory.ExistsDirectory(path))
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
        }


        private void button19_Click(object sender, EventArgs e)
        {
            //打开本地音乐
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = StDirectory.CurrentAppDirectory;
                ofd.Filter = "mp3|*.mp3|mp4|*.mp4|avi|*.avi|rm|*.rm|rmvb|*.rmvb|flv|*.flv|xr|*.xr|所有文件|*.*";
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    axPlayer1.Open(ofd.FileName);
                }
            }
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            //拖拽进度
            axPlayer1.SetPosition(trackBar1.Value);
            label22.Text = TimeToString(TimeSpan.FromMilliseconds(trackBar1.Value));
        }

        public void APlayPlayOrPause()
        {

            //播放 | 暂停

            if (axPlayer1.GetState() == 5)  //播放-暂停
            {
                axPlayer1.Pause();
            }
            if (axPlayer1.GetState() == 3)  //暂停-播放
            {
                axPlayer1.Play();
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            APlayPlayOrPause();
        }

        private void button16_Click(object sender, EventArgs e)
        {

            axPlayer1.Close();//停止
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            //音量
            axPlayer1.SetVolume(trackBar2.Value);  //10倍
            this.label25.Text = trackBar2.Value + "%";
        }
        #endregion

    }
}
