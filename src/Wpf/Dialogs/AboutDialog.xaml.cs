//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
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
    /// <summary>
    /// About dialog
    /// </summary>
    public partial class AboutDialog : Window
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AboutDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with specification
        /// </summary>
        /// <param name="spec"></param>
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

        private void OnShowEula(object sender, RoutedEventArgs e)
        {
            var spec = (AboutDialogSpec)this.DataContext;

            var eulaDlg = new EulaDialog(spec.Eula);
            eulaDlg.Owner = this;
            eulaDlg.ShowDialog();
        }
    }
}
