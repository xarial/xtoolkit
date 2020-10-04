using System;
using System.Windows.Controls;

namespace Wpf.Docs
{
    public partial class EnumComboBoxControl : UserControl
    {
        public EnumComboBoxControl()
        {
            InitializeComponent();
            this.DataContext = new FlagEnumComboBoxControlVM();
        }
    }
}
