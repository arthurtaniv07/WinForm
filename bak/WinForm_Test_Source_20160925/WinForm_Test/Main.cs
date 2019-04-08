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

namespace WinForm_Test
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
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

        string BaiduFanyiAutoCheckText = "自动检测|";
        private void Main_Load(object sender, EventArgs e)
        {

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

            //3
            winSize.Add(3, new Size(682, 538));

            //4 文件监视
            winSize.Add(4, new Size(1166, 691));

            //5  站点查询
            this.comboBox3.DataSource = new BindingSource() { DataSource = HuiLvZhuanHuan };
            this.comboBox3.DisplayMember = "value";
            this.comboBox3.ValueMember = "key";
            this.comboBox3.SelectedIndex = 0;
            this.comboBox4.DataSource = new BindingSource() { DataSource = HuiLvZhuanHuan };
            this.comboBox4.DisplayMember = "value";
            this.comboBox4.ValueMember = "key";
            this.comboBox4.SelectedIndex = 0;
            winSize.Add(5, new Size(547, 244));




            tabControl1_SelectedIndexChanged(sender, e);


            this.textBox5.KeyUp += new KeyEventHandler(textBox_KeyUp_SelectAll);
            this.textBox6.KeyUp += new KeyEventHandler(textBox_KeyUp_SelectAll);
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
            thrs[0].Start();
        }

        public void SendMail1()
        {
            StChineseCalendar dat = new StChineseCalendar(DateTime.Now);
            string nowDay = dat.GanZhiYearString + dat.ChineseMonthString + dat.ChineseDayString + "日";
            DateTime dt1 = new DateTime(2014, 11, 21);
            DateTime dt2 = DateTime.Now;
            //dt2 = new DateTime(2016, 11, 21);
            //邮件内容
            string ya = string.IsNullOrWhiteSpace(dat.DateHoliday) ? "" : "(" + dat.DateHoliday + ")";
            string yi = string.IsNullOrWhiteSpace(dat.ChineseCalendarHoliday) ? "" : " " + dat.ChineseCalendarHoliday + "";
            string w = string.IsNullOrWhiteSpace(dat.WeekDayHoliday) ? "" : "(" + dat.WeekDayHoliday + ")";
            string SendContent = "亲爱的bd,今天是" + DateTime.Now.ToString("yyyy年MM月dd日") + ya + "(农历:" + nowDay + yi + ")" + w
                + ",庆祝我俩相爱 <span style='color:red;'>" + StDate.GetDateTimeSpanString(dt1, dt2) + "</span>";


            StMail mail = Pub.GetMailAcc("你家的sg");

            Account to = new Account() { Address = this.textBox1.Text, IsSSL = true, IsBodyHtml = true };
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
            this.label10.Text = string.Join(",", StAPIHelper.Ex_GetIPAddress(this.textBox7.Text));
        }

        //站点Title查询
        private void button5_Click(object sender, EventArgs e)
        {

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
            this.textBox9.Text = "";
            int totleCount;
            string msg;

            Dictionary<string, string> sss = StAPIHelper.Ex_GetUrlsByIP(this.textBox10.Text, out totleCount, out msg, 0, -1);

            string rel = string.Join("", sss.Select(i => "\r\n" + i.Key + "\t\t" + i.Value));

            this.textBox9.Text += "序号\t\t网址" + rel;
        }


        private void button7_Click(object sender, EventArgs e)
        {
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




    }
}
