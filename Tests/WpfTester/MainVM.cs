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
        public XDataGridVM XDataGrid { get; set; }
        public XListViewVM XListView { get; set; }
        public ProgressPanelVM ProgressPane { get; set; }
        public ExpandToggleButtonVM ExpandToggleButton { get; set; }
        public PopupMenuVM PopupMenu { get; set; }
        public CheckableComboBoxVM CheckableComboBox { get; set; }
        public LabeledControlVM LabeledControl { get; set; }

        public MainVM() 
        {
            FlagEnumComboBox = new FlagEnumComboBoxVM();
            NumberBox = new NumberBoxVM();
            TreeViewEx = new TreeViewExVM();
            EnumComboBox = new EnumComboBoxVM();
            XDataGrid = new XDataGridVM();
            XListView = new XListViewVM();
            ProgressPane = new ProgressPanelVM();
            ExpandToggleButton = new ExpandToggleButtonVM();
            PopupMenu = new PopupMenuVM();
            CheckableComboBox = new CheckableComboBoxVM();
            LabeledControl = new LabeledControlVM();
        }
    }
}
