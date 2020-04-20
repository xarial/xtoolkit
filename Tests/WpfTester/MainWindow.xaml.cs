using System;
using System.Collections.Generic;
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

namespace WpfTester
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainVM()
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
        }
    }
}
