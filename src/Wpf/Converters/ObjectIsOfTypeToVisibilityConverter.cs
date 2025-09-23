//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Windows;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class ObjectIsOfTypeToVisibilityConverter : BooleanUniversalConverter
    {
        public override object FalseValue => Visibility.Collapsed;
        public override object TrueValue => Visibility.Visible;

        protected override bool? ConvertValueToBool(object value, object parameter)
        {
            if (value != null && parameter is Type)
            {
                return ((Type)parameter).IsAssignableFrom(value.GetType());
            }
            else
            {
                return null;
            }
        }
    }
}
