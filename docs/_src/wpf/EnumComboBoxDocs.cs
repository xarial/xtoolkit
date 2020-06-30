using System;

namespace Wpf.Docs
{
    //--- enum
    [Flags]
    public enum EnumSample_e
    {
        None = 0,
        Field1 = 1,
        Field2 = 2,
        Field3 = 4,
        Field2AndField3 = Field2 | Field3,
        Field4 = 8
    }
    //---

    //--- view-model
    public class EnumComboBoxControlVM
    {
        public EnumSample_e EnumPrp { get; set; }
    }
    //---
}
