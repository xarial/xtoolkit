using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTester
{
    public class MainVM
    {
        public FlagEnumComboBoxVM FlagEnumComboBox { get; set; }
        public NumberBoxVM NumberBox { get; set; }
        public TreeViewExVM TreeViewEx { get; set; }
        public EnumComboBoxVM EnumComboBox { get; set; }

        public MainVM() 
        {
            FlagEnumComboBox = new FlagEnumComboBoxVM();
            NumberBox = new NumberBoxVM();
            TreeViewEx = new TreeViewExVM();
            EnumComboBox = new EnumComboBoxVM();
        }
    }
}
