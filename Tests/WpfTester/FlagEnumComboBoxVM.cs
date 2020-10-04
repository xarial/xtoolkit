using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTester
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
        Value1 = 1 << 0,
        [Browsable(false)]
        Value2 = 1 << 1,
        Value1Value2 = Value1 | Value2,
        Value3 = 1 << 2,
        Value4 = 1 << 3,
        All = Value1Value2 | Value3 | Value4
    }

    public class FlagEnumComboBoxVM
    {
        public FlagEnum1 Enum1 { get; set; }
        public FlagEnum2 Enum2 { get; set; }
        public FlagEnum3 Enum3 { get; set; }

        public FlagEnumComboBoxVM() 
        {
            Enum2 = FlagEnum2.Value2;
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
