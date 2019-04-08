using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace St.Common.Controls
{
    public partial class LrcShow : Component
    {
        public LrcShow()
        {
            InitializeComponent();
        }

        public LrcShow(IContainer container)
        {
            container.Add(this);



            InitializeComponent();
        }




    }
}
