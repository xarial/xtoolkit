using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xarial.XToolkit;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf.Controls;
using Xarial.XToolkit.Wpf.Delegates;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Wpf.Services;

namespace Lib.Wpf
{
    public partial class WpfControls : UserControl
    {
        private MainVM m_Vm;

        public WpfControls()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_Vm = new MainVM(Window.GetWindow(this));

            this.DataContext = m_Vm;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Debugger.Break();
        }

        private void OnColumnsPreCreated(List<DataGridColumn> columns)
        {
            columns.Sort((c1, c2) =>
            {
                var h1 = c1.Header;
                var h2 = c2.Header;

                if (h1 is ColumnVM && h2 is ColumnVM)
                {
                    return 0;
                }

                if (!(h1 is ColumnVM) && !(h2 is ColumnVM))
                {
                    return 0;
                }

                if (h1 is ColumnVM && !(h2 is ColumnVM))
                {
                    return -1;
                }

                if (!(h1 is ColumnVM) && h2 is ColumnVM)
                {
                    return 1;
                }

                throw new Exception();
            });
        }
    }
}
