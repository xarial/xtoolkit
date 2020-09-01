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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xarial.XToolkit.Wpf.Dialogs;

namespace WpfTester
{
    public partial class MainWindow : Window
    {
        private readonly MainVM m_Vm;

        public MainWindow()
        {
            InitializeComponent();

            m_Vm = new MainVM()
            {
                EnumComboBox = new EnumComboBoxVM() 
                {
                    Enum2 = FlagEnum2.Value2
                },
                NumberBox = new NumberBoxVM() 
                {
                    DoubleVal = 10.0,
                    IntVal = 2
                }
            };

            this.DataContext = m_Vm;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Debugger.Break();
        }

        private void OnShowAboutClick(object sender, RoutedEventArgs e)
        {
            AboutDialog.Show(typeof(AboutDialog).Assembly, 
                Properties.Resources.icon, IntPtr.Zero);
        }
    }
}
