//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xarial.XToolkit.Wpf.Attributes;

namespace Xarial.XToolkit.Wpf.Dialogs
{
    public class LicenseItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate LinkTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var lic = item as LicenseInfo;

            if (string.IsNullOrEmpty(lic.Url))
            {
                return TextTemplate;
            }
            else 
            {
                return LinkTemplate;
            }
        }
    }

    public partial class LicensesListDialog : Window
    {
        public LicensesListDialog()
        {
            InitializeComponent();
        }

        private void OnLicenseHyperlinkClick(object sender, RoutedEventArgs e)
        {
            var licInfo = (sender as Hyperlink).DataContext as LicenseInfo;

            try
            {
                System.Diagnostics.Process.Start(licInfo.Url);
            }
            catch 
            {
            }
        }
    }
}
