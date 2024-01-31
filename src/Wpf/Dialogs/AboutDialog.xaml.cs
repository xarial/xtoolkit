﻿//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
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
    public static class About 
    {
        public static void Show(Assembly assm, Image logo, IntPtr parent)
        {
            var spec = new AboutDialogSpec(assm, logo);

            Show(spec, parent);
        }

        public static void Show(Assembly assm, Image logo, Window parent)
        {
            var spec = new AboutDialogSpec(assm, logo);

            Show(spec, parent);
        }

        public static void Show(Assembly assm, Window parent)
        {
            var spec = new AboutDialogSpec(assm);

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

        public static void Show(AboutDialogSpec spec, Window parent)
        {
            var dlg = new AboutDialog();
            dlg.DataContext = spec;
            dlg.Owner = parent;
            dlg.ShowDialog();
        }
    }

    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        public AboutDialog(AboutDialogSpec spec) : this()
        {
            this.DataContext = spec;
        }

        private void OnOk(object sender, RoutedEventArgs e)
            => this.Close();

        private void OnShowLicenses(object sender, RoutedEventArgs e)
        {
            var licDlg = new LicensesListDialog();
            licDlg.DataContext = this.DataContext;
            licDlg.Owner = this;
            licDlg.ShowDialog();
        }
    }
}
