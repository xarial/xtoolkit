//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace Xarial.XToolkit.Wpf.Dialogs
{
    public partial class EulaDialog : Window
    {
        public EulaDialog(string rtf)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(rtf))
            {
                var range = new TextRange(EulaRichTextBox_PART.Document.ContentStart, EulaRichTextBox_PART.Document.ContentEnd);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(rtf)))
                {
                    range.Load(stream, DataFormats.Rtf);
                }
            }
        }

        private void OnOk(object sender, RoutedEventArgs e)
            => this.Close();
    }
}
