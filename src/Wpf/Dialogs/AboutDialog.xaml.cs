//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Xarial.XToolkit.Wpf.Dialogs
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        public static void Show(Assembly assm, Image logo, IntPtr parent) 
        {
            var spec = new AboutDialogSpec();
            spec.Title = assm.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            spec.Description = assm.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
            spec.Copyright = assm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            spec.Version = assm.GetName().Version;
            spec.Logo = logo;

            Show(spec, parent);
        }

        public static void Show(AboutDialogSpec spec, IntPtr parent) 
        {
            var dlg = new AboutDialog();
            dlg.DataContext = spec;

            var interopHelper = new WindowInteropHelper(dlg);
            interopHelper.Owner = parent;
            dlg.ShowDialog();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
