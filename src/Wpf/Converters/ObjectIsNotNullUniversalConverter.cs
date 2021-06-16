//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class ObjectIsNotNullUniversalConverter : BooleanUniversalConverter
    {
        protected override bool? ConvertValueToBool(object value)
            => !(value is null);
    }

    [ValueConversion(typeof(object), typeof(object))]
    public class ObjectIsNotNullVisibilityConverter : ObjectIsNotNullUniversalConverter
    {
        public override object FalseValue => Visibility.Collapsed;
        public override object TrueValue => Visibility.Visible;
    }
}
