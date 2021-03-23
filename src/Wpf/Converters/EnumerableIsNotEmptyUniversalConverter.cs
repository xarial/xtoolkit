//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    [ValueConversion(typeof(IEnumerable), typeof(object))]
    public class EnumerableIsNotEmptyUniversalConverter : BooleanUniversalConverter
    {
        protected override bool? ConvertValueToBool(object value)
            => value is IEnumerable && ((IEnumerable)value).GetEnumerator().MoveNext();
    }

    [ValueConversion(typeof(IEnumerable), typeof(object))]
    public class EnumerableIsNotEmptyVisibilityConverter : EnumerableIsNotEmptyUniversalConverter
    {
        public override object FalseValue => Visibility.Collapsed;
        public override object TrueValue => Visibility.Visible;
    }
}
