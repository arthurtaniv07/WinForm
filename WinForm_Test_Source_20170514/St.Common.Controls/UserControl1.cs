using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace St.Common.Controls
{
    public partial class UserControl1 : UserControl
    {


        public UserControl1()
        {

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            this.InitializeComponent();
            base.Size = new Size(100, 100);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        
        
        private string _musicText="wwwwww";   
        public string MusicText   
        {   
            get  
            {   
                return this._musicText;   
            }   
            set { this._musicText = value; }   
        }   
       /// <summary>   
       /// 播放前的颜色   
       /// </summary>   
       public Color PrevPlayColor = Color.Red;   
       /// <summary>   
       /// 播放后的速度   
       /// </summary>   
       public Color PlayedColor = Color.Blue;   
 
       public Font TextFont = new Font("宋体", 20);   
       /// <summary>   
       /// 播放速度   
       /// </summary>   
       private int _speed;   
       /// <summary>   
       /// 颜色分割线   
       /// </summary>   
       private int _colorLine = 50;   
       /// <summary>   
       /// 文字开始位置。   
       /// </summary>   
       private Point _startTextPos = new Point(0, 0);   

        protected override void OnPaint(PaintEventArgs e)
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            Graphics g = e.Graphics;
            Image img = new Bitmap(this.Width, this.Height);
            g.DrawImage(img, new Point(0, 0));


           Graphics gc = e.Graphics;   
           /// 定义好颜色格式   
           System.Drawing.Drawing2D.LinearGradientBrush preyPlayBrush = new System.Drawing.Drawing2D.LinearGradientBrush(   
               this._startTextPos, new Point(1500, 0), this.PlayedColor, this.PrevPlayColor);   
 
           System.Drawing.Drawing2D.ColorBlend colorBlend = new System.Drawing.Drawing2D.ColorBlend(4);   
           colorBlend.Colors = new Color[4] { this.PlayedColor, this.PlayedColor, PrevPlayColor, PrevPlayColor};   
           float f = this._colorLine;   
           f /= (1500 - this._startTextPos.Y);   
           /// 设置颜色显示范围，三个区域： Blue区，Blue向Red过度区（过度区很短），Red区。   
           colorBlend.Positions = new float[4]{ 0, f, f + 0.0001f, 1.0f};   
 
           preyPlayBrush.InterpolationColors = colorBlend;   
           gc.DrawString(this.MusicText, this.TextFont, preyPlayBrush, new PointF(0, 0));


        }


    }
}
