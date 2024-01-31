using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTester
{
    public enum Enum1 
    {
        Value1,
        Value2,
        Value3,
        Value4
    }

    public enum Enum2
    {
        Value1,
        [EnumDescription("Second Value")]
        Value2,
        [EnumDisplayName("Value 3")]
        Value3,
        Value4
    }

    public enum Enum3
    {
        Value1,
        [Browsable(false)]
        Value2,
        Value3,
        Value4,
    }

    public class EnumComboBoxVM
    {
        public Enum1 Enum1 { get; set; }
        public Enum2 Enum2 { get; set; }
        public Enum3 Enum3 { get; set; }

        public EnumComboBoxVM() 
        {
            Enum2 = Enum2.Value2;
        }
    }
}
