using System;
using System.ComponentModel;

namespace Wpf.Docs
{
    //--- enum-display-name-attribute
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : DisplayNameAttribute 
    {
        public EnumDisplayNameAttribute(string dispName) : base(dispName) 
        {
        }
    }
    //---

    //--- enum
    [Flags]
    public enum FlagEnumSample_e
    {
        None = 0,
        [EnumDisplayName("Field 1")]
        Field1 = 1,
        [Description("Second Field")]
        Field2 = 2,
        Field3 = 4,
        Field2AndField3 = Field2 | Field3,
        Field4 = 8
    }
    //---

    //--- view-model
    public class FlagEnumComboBoxControlVM
    {
        public FlagEnumSample_e EnumPrp { get; set; }
    }
    //---
}
