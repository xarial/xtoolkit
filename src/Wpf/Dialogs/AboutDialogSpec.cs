//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Xarial.XToolkit.Wpf.Dialogs
{
    public class AboutDialogSpec
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public Version Version { get; set; }
        public Image Logo { get; set; }
    }
}
