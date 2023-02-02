//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class MatchValueUniversalConverter : BooleanUniversalConverter
    {
        public object TargetValue { get; set; }

        protected override bool? ConvertValueToBool(object value)
            => object.Equals(value, TargetValue);
    }
}
