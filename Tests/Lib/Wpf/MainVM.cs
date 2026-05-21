//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Wpf
{
    public class MainVM
    {
        public FlagEnumComboBoxVM FlagEnumComboBox { get; }
        public NumberBoxVM NumberBox { get; }
        public TreeViewExVM TreeViewEx { get; }
        public EnumComboBoxVM EnumComboBox { get; }
        public DataGridExVM DataGridEx { get; }
        public ListViewExVM ListViewEx { get; }
        public ProgressPanelVM ProgressPane { get; }
        public ExpandToggleButtonVM ExpandToggleButton { get; }
        public PopupMenuVM PopupMenu { get; }
        public CheckableComboBoxVM CheckableComboBox { get; }
        public LabeledControlVM LabeledControl { get; }
        public ExpressionBoxVM ExpressionBox { get; }
        public PasswordBoxExVM PasswordBoxEx { get; }
        public ColorPickerVM ColorPicker { get; }
        public WatermarkTextBoxVM WatermarkTextBox { get; }

        public MainVM() 
        {
            FlagEnumComboBox = new FlagEnumComboBoxVM();
            NumberBox = new NumberBoxVM();
            TreeViewEx = new TreeViewExVM();
            EnumComboBox = new EnumComboBoxVM();
            DataGridEx = new DataGridExVM();
            ListViewEx = new ListViewExVM();
            ProgressPane = new ProgressPanelVM();
            ExpandToggleButton = new ExpandToggleButtonVM();
            PopupMenu = new PopupMenuVM();
            CheckableComboBox = new CheckableComboBoxVM();
            LabeledControl = new LabeledControlVM();
            ExpressionBox = new ExpressionBoxVM();
            PasswordBoxEx = new PasswordBoxExVM();
            ColorPicker = new ColorPickerVM();
            WatermarkTextBox = new WatermarkTextBoxVM();
        }
    }
}
