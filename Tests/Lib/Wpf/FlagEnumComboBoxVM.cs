//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Controls;

namespace Lib.Wpf
{
    [Flags]
    public enum FlagEnum1 
    {
        Value1 = 1 << 0,

        [EnumDisplayName("Value 2")]
        [EnumDescription("Second Value")]
        Value2 = 1 << 1,

        [EnumDisplayName("Value 3")]
        Value3 = 1 << 2,

        [EnumDescription("Forth Value")]
        Value4 = 1 << 3
    }

    [Flags]
    public enum FlagEnum2
    {
        None = 0,
        Value1 = 1 << 0,
        Value2 = 1 << 1,
        Value3 = 1 << 2,
        Value4 = 1 << 3
    }

    [Flags]
    public enum FlagEnum3
    {
        Value4 = 1 << 3,

        Value1 = 1 << 0,
        
        [Browsable(false)]
        Value2 = 1 << 1,
        
        Value1Value2 = Value1 | Value2,
        
        Value3 = 1 << 2,
        
        All = Value1Value2 | Value3 | Value4
    }

    public class FlagEnumComboBoxVM
    {
        public FlagEnum1 Enum1 { get; set; }
        public FlagEnum2 Enum2 { get; set; }
        public FlagEnum3 Enum3 { get; set; }

        public ICommand ItemCreateCommand { get; }

        public FlagEnumComboBoxVM() 
        {
            Enum2 = FlagEnum2.Value2;

            ItemCreateCommand = new RelayCommand<EnumComboBoxItem>(OnItemCreate);
        }

        private void OnItemCreate(EnumComboBoxItem item)
        {
            if (Enum.Equals(item.Value, FlagEnum1.Value2))
            {
                item.DisplayName = "#VALUE2";
                item.Tooltip = "#CUSTOM VALUE 2";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : DisplayNameAttribute 
    {
        public EnumDisplayNameAttribute(string dispName) : base(dispName) 
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDescriptionAttribute : DescriptionAttribute
    {
        public EnumDescriptionAttribute(string desc) : base(desc)
        {
        }
    }
}
