using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using St.Common;
using St.Mail;
using St.Web;
using St.IO;
using St;
using AxAPlayer3Lib;

namespace WinForm_Test
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }


        string BaiduFanyiAutoCheckText = "自动检测|";
        private void Main_Load(object sender, EventArgs e)
        {

            this.label17.BackColor = Color.FromArgb(0, Color.Transparent);

            this.label2.Text = "";
            this.label4.Text = "";

            //0  bd
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(timer_Tick);
            ts.Add(0, timer);
            res.Add(0, new ReHander(Re));
            thrs.Add(0, new System.Threading.Thread(SendMail1));

            //1  生日祝福
            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(timerBirthday_Tick);
            ts.Add(1, timer);
            res.Add(1, new ReHander(ReBirthday));
            thrs.Add(1, new System.Threading.Thread(SendMail2));

            //2  度小译
            #region 度小译
            this.comboBox1.DataSource = new BindingSource() { DataSource = BaidufanyiYuzhong };
            this.comboBox1.DisplayMember = "value";
            this.comboBox1.ValueMember = "key";
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.Items.Clear();
            this.comboBox2.Items.Add(BaiduFanyiAutoCheckText.Split('|')[0]);
            foreach (string item in BaidufanyiYuzhong.Values)
            {
                this.comboBox2.Items.Add(item);
            }
            this.comboBox2.SelectedIndex = 0;
            this.comboBox2.Tag = BaiduFanyiAutoCheckText.Split('|')[1];


            StAPIHelper.ExHander_BaiduFanyiDebuger += new StAPIHelper.StAPIHander(StAPIHelper_ExHander_BaiduFanyiDebuger);
            winSize.Add(2, new Size(831, 524));
            #endregion


            //3
            winSize.Add(3, new Size(682, 538));

            //4 文件监视
            winSize.Add(4, new Size(1166, 691));

            //5  站点查询
            #region 站点查询
            this.comboBox3.DataSource = new BindingSource() { DataSource = HuiLvZhuanHuan };
            this.comboBox3.DisplayMember = "value";
            this.comboBox3.ValueMember = "key";
            this.comboBox3.SelectedIndex = 0;
            this.comboBox4.DataSource = new BindingSource() { DataSource = HuiLvZhuanHuan };
            this.comboBox4.DisplayMember = "value";
            this.comboBox4.ValueMember = "key";
            this.comboBox4.SelectedIndex = 0;
            winSize.Add(5, new Size(547, 244));
            #endregion

            //6 酷我歌曲
            #region 酷我歌曲
            winSize.Add(6, new Size(1465, 691));
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
            axPlayer1.SetCustomLogo(new  Bitmap(@"config\Img\St.png").GetHbitmap().ToInt32());  //自定义logo
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

            //7  快查
            #region 快查

            winSize.Add(7, new Size(670, 697));
            #endregion




            tabControl1_SelectedIndexChanged(sender, e);
            this.textBox5.KeyUp += new KeyEventHandler(textBox_KeyUp_SelectAll);
            this.textBox6.KeyUp += new KeyEventHandler(textBox_KeyUp_SelectAll);
        }

        /// <summary>
        /// 需要联网功能调用的方法
        /// </summary>
        /// <returns></returns>
        public bool Pub_NetWork()
        {
            bool rel = false;
            if (!(rel = StEnvironment.IsInternetAlive()))
                if (!(rel = StEnvironment.IsInternetAlive()))
                    MessageBox.Show("请检查您的网络连接", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            return rel;
        }

        public delegate void ReHander();

        Dictionary<int, Timer> ts = new Dictionary<int, Timer>();//计时器  用于及时刷新控件的值
        Dictionary<int, ReHander> res = new Dictionary<int, ReHander>();//委托池 用于发送邮件的方法
        Dictionary<int, System.Threading.Thread> thrs = new Dictionary<int, System.Threading.Thread>();//线程池  用于刷新控件的值[在发送邮件时]

        Dictionary<int, Size> winSize = new Dictionary<int, Size>();//窗体的大小

        public int TabColtrolIndex
        {
            set
            {
                this.tabControl1.SelectedIndex = value;
                foreach (var item in ts.Values)
                {
                    if (item == null) { continue; }
                    item.Stop();
                }

                if (value >= this.tabControl1.TabCount) { return; }

                if (ts.ContainsKey(value) && ts[value] != null)
                {
                    ts[value].Start();
                }
                if (res.ContainsKey(value) && res[value] != null)
                {
                    res[value]();
                }
                if (winSize.ContainsKey(value) && winSize[value] != null)
                {
                    this.Size = winSize[value];
                }
                else
                {
                    this.Size = new Size(702, 336);
                }
            }
        }






        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabColtrolIndex = this.tabControl1.SelectedIndex;
        }

        public void SetControlValue(Control tbox, string value)
        {
            this.SuspendLayout();
            tbox.Text = value;
            System.Threading.Thread.Sleep(200);
            this.ResumeLayout(false);
        }

        #region bd

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { return; }
            thrs[0].Start();
        }

        public void SendMail1()
        {
            StDateConvert2 dat = new StDateConvert2(DateTime.Now);
            string nowDay = dat.GanZhiYearString + dat.ChineseMonthString + dat.ChineseDayString + "日";
            DateTime dt1 = new DateTime(2014, 11, 21);
            DateTime dt2 = DateTime.Now;
            //邮件内容
            string ya = string.IsNullOrWhiteSpace(dat.DateHoliday) ? "" : "(" + dat.DateHoliday + ")";
            string yi = string.IsNullOrWhiteSpace(dat.ChineseCalendarHoliday) ? "" : " " + dat.ChineseCalendarHoliday + "";
            string w = string.IsNullOrWhiteSpace(dat.WeekDayHoliday) ? "" : "(" + dat.WeekDayHoliday + ")";
            string SendContent = "亲爱的bd,今天是" + DateTime.Now.ToString("yyyy年MM月dd日") + ya + "(农历:" + nowDay + yi + ")" + w
                + ",庆祝我俩相爱 <span style='color:red;'>" + StDate.GetDateTimeSpanString(dt1, dt2) + "</span>";


            StMail mail = Pub.GetMailAcc("你家的sg");

            Account to = new Account() { Address = this.textBox1.Text, Name = "亲爱的bd", IsSSL = true, IsBodyHtml = true };
            //Account to = new Account() { Address = "173230097@qq.com", IsSSL = true, IsBodyHtml = true };
            SetControlValue(this.label2, "准备发送...");
            bool rel = mail.Send(to, "sg的致电", SendContent);
            SetControlValue(this.label2, string.IsNullOrWhiteSpace(mail.ErroMsg) ? "发送成功" : mail.ErroMsg);

        }

        void timer_Tick(object sender, EventArgs e)
        {
            res[0]();
        }

        private void Re()
        {
            this.label1.Text = "";
            DateTime dt1 = new DateTime(2014, 11, 21);
            DateTime dt2 = DateTime.Now;
            this.label1.Text += "今天是：" + dt2.ToString("yyyy-MM-dd HH:mm:ss");
            this.label1.Text += "\r\n" + dt1.ToString("yyyy-MM-dd HH:mm:ss") + "距离现在";
            this.label1.Text += "\r\n" + StDate.GetDateTimeSpanString(dt1, dt2) + "或者";
            this.label1.Text += "\r\n" + (int)((dt2 - dt1).TotalSeconds) + "秒";
        }
        #endregion


        #region 生日祝福

        void timerBirthday_Tick(object sender, EventArgs e)
        {
            res[1]();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { return; }
            thrs[1].Start();
        }
        public void SendMail2()
        {
            string nicheng = this.textBox4.Text;
            string mailTo = this.textBox2.Text;
            string content = this.textBox3.Text.Replace("\r\n", "<br />");

            StMail mail = Pub.GetMailAcc("来自远方的祝福");

            Account to = new Account() { Address = mailTo, Name = nicheng, IsSSL = true, IsBodyHtml = true };
            //Account to = new Account() { Address = "173230097@qq.com", IsSSL = true, IsBodyHtml = true };
            SetControlValue(this.label4, "准备发送...");
            bool rel = mail.Send(to, "生日快乐", content);
            SetControlValue(this.label4, string.IsNullOrWhiteSpace(mail.ErroMsg) ? "发送成功" : mail.ErroMsg);

        }

        private void ReBirthday()
        {
            this.textBox3.Text = "";

            StDateConvert dat = new StDateConvert(DateTime.Now);
            string nowDay = dat.LunarYearSexagenary + "年" + dat.LunarMonthText + "月" + dat.LunarDayText + "日";

            DateTime dt1 = new DateTime(2014, 11, 21);
            DateTime dt2 = DateTime.Now;
            this.textBox3.Text += this.textBox4.Text + (string.IsNullOrWhiteSpace(this.textBox4.Text) ? "" : "，") + "今天是：" + dt2.ToString("yyyy-MM-dd") + "(农历:" + nowDay + ")";
            this.textBox3.Text += "\r\n" + "我知道，在这个特殊的日子里是您的生日，我在这里祝您 生日快乐、家庭和睦、事业有成！";
        }
        #endregion


        #region 百度翻译API


        void StAPIHelper_ExHander_BaiduFanyiDebuger(object msg)
        {
            this.textBox5.Text += "\r\n" + msg;
        }

        Dictionary<string, string> _baidufanyiYuzhong = null;
        /// <summary>
        /// 获取百度翻译语种 key：code   value：名称
        /// </summary>
        Dictionary<string, string> BaidufanyiYuzhong
        {
            get
            {
                if (_baidufanyiYuzhong == null)
                {
                    _baidufanyiYuzhong = StIniHelper.GetGroup(Environment.CurrentDirectory + @"\config\BaiduFanyi_ini.ini", "BaiduFanyi");
                }
                return _baidufanyiYuzhong;
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            BaiDuFanYi();
        }


        string BaiDuFanYiPreStr = "";



        public void BaiDuFanYi()
        {

            if (!Pub_NetWork()) { return; }
            this.textBox5.Text = "\r\n\r\n";

            string query = this.textBox6.Text;
            if (string.IsNullOrWhiteSpace(query)) { return; }
            if (query == "sg give to bd")
            {
                StAPIHelper.Ex_BaiduFanYiDebuger = true;
            }
            else if (query == "sg give to bd.")
            {
                StAPIHelper.Ex_BaiduFanYiDebuger = false;
            }

            

            //语种
            int baidufanyi_len = 20;//这个是百度翻译获取语种的长度限制,在后面可能会考虑配置到文件

            string en_form = this.comboBox2.Tag.ToString();
            int cb2Index = this.comboBox2.SelectedIndex;
            this.comboBox2.Items.RemoveAt(0);
            if (en_form == BaiduFanyiAutoCheckText.Split('|')[1])
            {
                en_form = StAPIHelper.Ex_BaiduFanyiLangDetect(query.Length > baidufanyi_len ? query.Substring(0, baidufanyi_len) : query);
                string en_formText = BaidufanyiYuzhong[en_form];
                this.comboBox2.Items.Insert(0, "自动检测到 " + en_formText);
            }
            else
            {
                this.comboBox2.Items.Insert(0, "自动检测");
            }
            this.comboBox2.SelectedIndex = cb2Index;



            string en_to = this.comboBox1.SelectedValue.ToString();
            if (en_form == en_to)
            {
                if (this.comboBox1.SelectedIndex == 0)
                {
                    this.comboBox1.SelectedIndex = 1;
                }
                else
                {
                    this.comboBox1.SelectedIndex = 0;
                }
            }
            en_to = this.comboBox1.SelectedValue.ToString();
            this.comboBox1.Tag = en_to;

            string[] rel;

            rel = StAPIHelper.Ex_BaiduFanYi(query, en_form, en_to);





            if (rel == null)
            {
                this.textBox5.Text += "\r\n翻译失败";
                return;
            }


            this.textBox5.Text += string.Format("\r\n\r\n[{1} => {2}]\r\n\r\n{0}", query, en_form, en_to);
            this.textBox5.Text += "\r\n";


            rel = rel.Distinct().ToArray();
            if (query.IndexOf("\r\n") > -1)
            {
                this.textBox5.Text += string.Join("\r\n", rel);
            }
            else
            {
                this.textBox5.Text += this.checkBox2.Checked ? string.Format("[\"{0}\"]", string.Join("\",\"", rel)) : rel[0];
            }
            BaiDuFanYiPreStr = query;



            this.textBox5.Update();
            this.textBox5.Select(this.textBox5.Text.Length, 0);
            this.textBox5.ScrollToCaret();

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (BaidufanyiYuzhong.ContainsValue(this.comboBox2.Text))
            {
                this.comboBox2.Tag = this.BaidufanyiYuzhong.Where(i => i.Value == this.comboBox2.Text).FirstOrDefault().Key;
            }
            else
            {
                this.comboBox2.Tag = BaiduFanyiAutoCheckText.Split('|')[1];
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.comboBox1.Tag = this.comboBox1.SelectedValue;
        }


        #endregion


        /// <summary>
        /// 实现TextBox的Ctrl+A全选功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_KeyUp_SelectAll(object sender, KeyEventArgs e)
        {
            if (sender is TextBox)
            {
                if (e.Modifiers.CompareTo(Keys.Control) == 0 && e.KeyCode == Keys.A)
                {
                    ((TextBox)sender).SelectAll();
                }
            }
        }


        #region 站点查询

        //IP地址查询
        private void button4_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { return; }
            this.label10.Text = string.Join(",", StAPIHelper.Ex_GetIPAddress(this.textBox7.Text));
        }

        //站点Title查询
        private void button5_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { return; }
            this.label11.ForeColor = Color.Black;
            SetControlValue(this.label11, "正在查询...");
            this.label11.Text = StAPIHelper.Ex_GetWebSiteTitle(this.textBox8.Text).Trim();
            if (label11.Text.IndexOf("div>") == label11.Text.Length - 4)
            {
                this.label11.Text = "请输入正确的网址或查询失败";
                this.label11.ForeColor = Color.Red;
            }
        }

        //同IP域名查询
        private void button6_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { return; }
            this.textBox9.Text = "";
            int totleCount;
            string msg;

            Dictionary<string, string> sss = StAPIHelper.Ex_GetUrlsByIP(this.textBox10.Text, out totleCount, out msg, 0, -1);

            string rel = string.Join("", sss.Select(i => "\r\n" + i.Key + "\t\t" + i.Value));

            this.textBox9.Text += "序号\t\t网址" + rel;
        }


        private void button7_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { return; }
            this.textBox12.Text = StNetHelper.GetIpByHostName(this.textBox11.Text);
        }
        #endregion


        #region 汇率转换

        private Dictionary<string, string> _huiLvZhuanHuan;
        public Dictionary<string, string> HuiLvZhuanHuan
        {
            get
            {
                if (_huiLvZhuanHuan == null)
                {
                    _huiLvZhuanHuan = StIniHelper.GetGroup(Environment.CurrentDirectory + @"\config\HuiLvZhuanHuan_ini.ini", "ExchangeRate");
                }
                return _huiLvZhuanHuan;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { return; }
            this.textBox14.Text = "";
            string mStr = this.textBox13.Text;

            decimal mDe = 0;
            if (!decimal.TryParse(mStr, out mDe))
            {
                MessageBox.Show("请输入数字");
                this.textBox13.Focus();
                return;
            }

            string form = this.comboBox3.SelectedValue + "";
            string to = this.comboBox4.SelectedValue + "";
            if (form.Contains("otherCode"))
            {
                MessageBox.Show("请选择币种(form)");
                return;
            }
            if (to.Contains("otherCode"))
            {
                MessageBox.Show("请选择币种(to)");
                return;
            }
            if (form == to)
            {
                if (this.comboBox3.SelectedIndex == 0)
                {
                    this.comboBox4.SelectedIndex = 1;
                }
                else
                {
                    this.comboBox4.SelectedIndex = 0;
                }
            }

            to = this.comboBox4.SelectedValue + "";
            string erroMsg;
            decimal? r = StAPIHelper.Ex_GetExchangeRate(form, to, out erroMsg);
            if (r.HasValue)
            {
                this.textBox14.Text = (r * mDe) + "";
            }
            else
            {
                this.textBox14.Text = erroMsg;
            }
        }
        #endregion


        #region 文件监视器

        private StFileWatcher sw = new StFileWatcher();
        private void button9_Click(object sender, EventArgs e)
        {

            if (this.button9.Tag.ToString() == "0")
            {
                string iniPath = this.textBox15.Text;
                if (!StDirectory.ExistsDirectory(iniPath))
                {
                    MessageBox.Show("您监视的文件夹不存在");
                    return;
                }
                sw = new StFileWatcher();
                string wPath = this.textBox17.Text;
                wPath = wPath.Replace("\r\n", ";");


                sw.AddFolderWhite(wPath.Split(';'));
                sw.FullPath = iniPath;
                sw.Changed += new StFileWatcher.StFileHander(sw_Changed);
                sw.Created += new StFileWatcher.StFileHander(sw_Changed);
                sw.Deleted += new StFileWatcher.StFileHander(sw_Changed);
                sw.Renamed += new StFileWatcher.StFileHander(sw_Changed);
                sw.Erro += new StFileWatcher.StFileErroHander(sw_Erro);
                sw.Open();
                this.button9.Tag = "1";
                this.button9.Text = "暂 停";
            }
            else
            {
                sw.Close();
                this.button9.Tag = "0";
                this.button9.Text = "开 始";
            }

            this.textBox15.Enabled = this.button9.Tag + "" == "0";
            this.textBox17.Enabled = this.button9.Tag + "" == "0";
            this.checkBox1.Enabled = this.button9.Tag + "" == "0";
        }

        bool sw_Erro(StFileEventArgs e, Exception erro)
        {
            return false;
        }

        void sw_Changed(StFileEventArgs e)
        {
            string text = e.Time.ToString("HH:mm:ss.fff") + "[" + e.ChangeType + "]\t";

            if (e is StFileRenamedEventArgs)
            {
                StFileRenamedEventArgs e1 = e as StFileRenamedEventArgs;
                text += e1.OldFullPath + " 重命名为 " + e1.FullPath;
            }
            else
            {
                text += e.FullPath;
            }
            this.SuspendLayout();

            this.textBox16.Update();
            //this.textBox16.Text += "\r\n" + text;
            this.textBox16.AppendText("\r\n" + text);
            this.textBox16.SelectionStart = this.textBox16.Text.Length;
            this.textBox16.ScrollToCaret();
            this.ResumeLayout(false);

            if (this.textBox18.Enabled)
            {
                string outPath = StDirectory.CurrentAppDirectory + this.textBox18.Text;
                outPath = outPath.Replace("{YYYY}", (DateTime.Now.Year + "").PadLeft(4, '0'));
                outPath = outPath.Replace("{MM}", (DateTime.Now.Month + "").PadLeft(2, '0'));
                outPath = outPath.Replace("{DD}", (DateTime.Now.Day + "").PadLeft(2, '0'));
                StFile.AppendAllText(outPath, "\r\n" + text);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            this.textBox16.Text = "";
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox18.Enabled = this.checkBox1.Checked;
        }
        #endregion


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
            button3.Enabled = true;
            button4.Enabled = true;
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

            if (!Pub_NetWork()) { this.trackBar1.Enabled = false; this.richTextBox1.Text = "网络连接中断，请检查您的网络...";return; }
            else { this.trackBar1.Enabled = true; }
            System.Threading.Thread.Sleep(20);

            int rowIndex = dataGridView1.SelectedRows[0].Index;
            string ID = dataGridView1.SelectedRows[0].Cells["ID"].Value + "";
            string SongName = dataGridView1.SelectedRows[0].Cells["SongName"].Value + "";
            string Singer = dataGridView1.SelectedRows[0].Cells["Singer"].Value + "";

            var u = StAPIHelper.Ex_Kuwo_SearchLyrics(ID);

            this.richTextBox1.Text = string.Join("\r\n", u.Values.Select(i =>i.StartTime+","+ i.Text));
            if (string.IsNullOrWhiteSpace(this.richTextBox1.Text)) { this.richTextBox1.Text = "很抱歉，未能找到您要的歌词"; }

        }


        public void CheckedMvOrLrc(int num,bool checkedR=false)
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
            this.label18.Text = "正在下载" + type + "( " + fileName + " ) " ;
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
            if (this.textBox6.Text != " \r\n")
            {
                MessageBox.Show("对不起，您未授权此操作");
                return false;
            }
            return true;
        }
        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            播放ToolStripMenuItem_Click(sender, e);
        }
        private void 查看真实地址ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Ve())
            {
                this.textBox24.Visible = false;
                return;
            }

            this.textBox24.Visible = true;
            string ID = dataGridView1.SelectedRows[0].Cells["ID"].Value + "";
            string u = StAPIHelper.Ex_Kuwo_GetSongDownUrl(ID);
            this.textBox24.Text = u;
        }
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
            SetSongName( SongName, Singer);
            axPlayer1.Open(u);
            CheckedMvOrLrc(1,true);



            kuwo_lrcs  = StAPIHelper.Ex_Kuwo_SearchLyrics(ID);
            
            this.richTextBox1.Tag = "0";
            this.richTextBox1.Text = string.Join("\r\n", kuwo_lrcs.Values.Select(i => i.TimeStr + "," + i.Text));
            if (!string.IsNullOrWhiteSpace(this.richTextBox1.Text))
            {
                this.richTextBox1.Text += "\r\n";
                this.richTextBox1.Tag = "1";
                kuwo_lrcs_index = 0;


                Timer t = new Timer();
                int tttI= Kuwo_GetWainTime(kuwo_lrcs_index, out temp_i);
                kuwo_lrcs_index += temp_i - 1;

                int i = richTextBox1.GetFirstCharIndexFromLine(kuwo_lrcs_index);
                int j = richTextBox1.GetFirstCharIndexFromLine(kuwo_lrcs_index + temp_i );//注意如果要选取最后一行的时候，这里可能会出错，应当直接把j设置成为最后一个字符的位置+1
                richTextBox1.Select(i, j - i);

                this.richTextBox1.SelectionColor = Color.Red;
                this.richTextBox1.ScrollToCaret();
                t.Tick += new EventHandler(t_Tick);
                t.Interval = tttI;
                t.Start();
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
        int temp_i=0;
        void t_Tick(object sender, EventArgs e)
        {
            if (this.richTextBox1.Tag+"" == "1")
            {
                if (kuwo_lrcs.Count > 0)
                {
                    this.richTextBox1.SelectionColor = richTextBox1.ForeColor;
                    this.richTextBox1.SelectionFont = richTextBox1.Font;
                    //歌词状态正常
                    kuwo_lrcs_index++;
                    //this.Text = kuwo_lrcs.Count + "," + kuwo_lrcs_index;
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
            if (inx == 0) {
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


        private void textBox19_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                button11_Click(sender, e);
                e.Handled = true;
            }
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


        #region 关闭窗体事件
        
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //因为如果播放器开始播放 则关闭窗体的过程有点慢  所以这里假装关闭窗体
            this.Hide();
            if (KuaiDi_Th != null)
            {
                KuaiDi_Th.Abort();
            }
            Application.Exit();
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (KuaiDi_Th != null)
            {
                KuaiDi_Th.Abort();
            }
            Application.Exit();
        }



        #endregion
        #region 快查
        private System.Threading.Thread KuaiDi_Th = null;
        private void button20_Click(object sender, EventArgs e)
        {
            if (!Pub_NetWork()) { return; }
            this.webBrowser1.DocumentText = @"正在查询，请稍后...";
            KuaiDi_Th = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(KuaiDi_SearchProg));
            string[] a = new string[] { this.textBox23.Text };
            KuaiDi_Th.Start(a);
            
        }

        private void KuaiDi_SearchProg(object obj)
        {
            string[] pars = obj as string[];
            string num = pars[0];

            string cssCss = StFile.ReadAllText(StDirectory.CurrentAppDirectory + @"\config\KuaiCha\CSS\css.css");
            string indexCss = StFile.ReadAllText(StDirectory.CurrentAppDirectory + @"\config\KuaiCha\CSS\index.css");
            string queryCss = StFile.ReadAllText(StDirectory.CurrentAppDirectory + @"\config\KuaiCha\CSS\query.css");

            string link = @"
<style rel='Stylesheet'>" + cssCss + indexCss + queryCss + "</style>";

            string type = StAPIHelper.Ex_GetKuaiDiFirm(num);
            if (string.IsNullOrWhiteSpace(type)) { MessageBox.Show("找不到您要查询的单号"); return; }
            string state = "";
            List<string[]> rel = StAPIHelper.Ex_GetKuaiDiProgress(num, type,out state);
            if (rel.Count == 0) { MessageBox.Show("网络错误"); return; }
            string con = "";
            int inx = 0;
            foreach (var item in rel)
            {
                /*<tr>
                 * <td class='row1'>2016-10-05 17:31:58</td>
                 * <td class='status status-first'>&nbsp;</td>
                 * <td>南充(0817-8188382，0817-3639249，0817-3336947)的顺庆7部<a target='_blank' href='/courier/detail_BFCC6B5FA91E869CE5B34F5A670A6ED0.html'>13540940004</a>已收件，扫描员是顺庆7部<a target='_blank' href='/courier/detail_BFCC6B5FA91E869CE5B34F5A670A6ED0.html'>13540940004</a></td>
                 * </tr><tr>
                 * <td class='row1'>2016-10-05 18:03:13</td><td class='status'>&nbsp;</td><td>快件由南充(0817-8188382，0817-3639249，0817-3336947)发往成都分拨中心</td></tr><tr><td class='row1'>2016-10-05 20:36:51</td><td class='status'>&nbsp;</td><td>成都分拨中心已进行装袋扫描</td></tr><tr><td class='row1'>2016-10-05 20:36:52</td><td class='status'>&nbsp;</td><td>快件由成都分拨中心发往重庆分拨中心</td></tr><tr><td class='row1'>2016-10-05 20:56:13</td><td class='status'>&nbsp;</td><td>快件到达成都分拨中心，上一站是无，扫描员是汪春霞</td></tr><tr><td class='row1'>2016-10-06 04:00:54</td><td class='status'>&nbsp;</td><td>快件由成都分拨中心发往重庆分拨中心</td></tr><tr><td class='row1'>2016-10-07 08:25:05</td><td class='status'>&nbsp;</td><td>快件到达重庆分拨中心，上一站是成都分拨中心，扫描员是进港到件组</td></tr><tr><td class='row1'>2016-10-07 09:35:04</td><td class='status'>&nbsp;</td><td>快件由重庆分拨中心发往重庆茶园(023-62605557)</td></tr><tr><td class='row1'>2016-10-07 10:56:45</td><td class='status'>&nbsp;</td><td>快件到达重庆茶园(023-62605557)，上一站是重庆分拨中心，扫描员是重庆茶园分部</td></tr><tr><td class='row1'>2016-10-07 11:54:05</td><td class='status'>&nbsp;</td><td>重庆茶园(023-62605557)的冉启华<a target='_blank' href='/courier/detail_B763F9AF7F828291171AC64A01381E85.html'>15213331496</a>正在派件</td></tr><tr class='last'><td class='row1'>2016-10-07 15:35:09</td><td class='status status-check'>&nbsp;</td><td>快件已签收,签收人是丰巢，签收网点是重庆茶园(023-62605557)</td></tr>*/
                string css = "";

                if (inx == 0) { css = " status-first"; }
                else if (inx == rel.Count - 1)
                {
                    if (state == "3") css = " status-check";
                    else css = " status-wait";
                }
                con += string.Format("<tr><td class='row1'>{0}1</td><td class='status {2}'>&nbsp;</td><td>{1}</td></tr>", item[0], item[1], css);
                inx++;
            }

            this.webBrowser1.DocumentText = @"
<html>
<head>
    " + link + @"
    <style type='text/css'>
    html .hide{display: none;}
    </style>
</head>

<body id='body' class='hidden'  style='display: block;'>
<div class='container mt10px' style='display: block;clear: both; margin: auto; width: 600px;'>
<div class='section' style='display: block;padding:0 0 19px;'>
<div id='queryContext' class='mt10px hidden relative' style='z-index: 4; display: block;'> <span class='qr-sf hidden' id='sfQr' style='display: none;'></span>
      <div class='result-top'><span class='col1'>时间</span><span class='col2'>地点和跟踪进度</span><a id='rssBtn' class='a-rss hide'>订阅</a><a id='shareBtn' class='a-share hide'>分享</a></div>
      <table id='queryResult2' class='result-info2' cellspacing='0'><tbody>" + con + @"</tbody></table>
    </div>
</div>
</div>
<body>
</html>";
            KuaiDi_Th = null;
        }

        #endregion


        #region 菜单

        private void 酷音乐ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KMusic kmusic = KMusic.GetInstance();
            kmusic.Activate();
            kmusic.Show();
        }

        private void 实时网速ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        #endregion





        //END 以下具体设置 如APlayer.SetConfig()方法的用法请参见  开发文档chm












    }
}
