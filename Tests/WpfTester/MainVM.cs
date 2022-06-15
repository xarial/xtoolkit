using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTester
{
    public class MainVM
    {
        public FlagEnumComboBoxVM FlagEnumComboBox { get; }
        public NumberBoxVM NumberBox { get; }
        public TreeViewExVM TreeViewEx { get; }
        public EnumComboBoxVM EnumComboBox { get; }
        public XDataGridVM XDataGrid { get; }
        public XListViewVM XListView { get; }
        public ProgressPanelVM ProgressPane { get; }
        public ExpandToggleButtonVM ExpandToggleButton { get; }
        public PopupMenuVM PopupMenu { get; }
        public CheckableComboBoxVM CheckableComboBox { get; }
        public LabeledControlVM LabeledControl { get; }
        public ExpressionBoxVM ExpressionBox { get; }

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
            ExpressionBox = new ExpressionBoxVM();
        }
    }
}
