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
    [ValueConversion(typeof(bool), typeof(object))]
    public class BooleanUniversalConverter : IValueConverter
    {
        public virtual object TrueValue { get; set; } = true;
        public virtual object FalseValue { get; set; } = false;

        public bool Reverse { get; set; } = false;

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolVal = ConvertValueToBool(value);

            if (boolVal.HasValue)
            {
                if (boolVal.Value)
                {
                    return Reverse ? FalseValue : TrueValue;
                }
                else
                {
                    return Reverse ? TrueValue : FalseValue;
                }
            }
            else
            {
                return null;
            }
        }

        protected virtual bool? ConvertValueToBool(object value) 
        {
            if (value is bool)
            {
                return (bool)value;
            }
            else 
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == TrueValue)
            {
                return !Reverse;
            }
            else if (value == FalseValue)
            {
                return Reverse;
            }
            else 
            {
                return null;
            }
        }
    }

    [ValueConversion(typeof(bool), typeof(object))]
    public class BooleanVisibilityConverter : BooleanUniversalConverter
    {
        public override object FalseValue => Visibility.Collapsed;
        public override object TrueValue => Visibility.Visible;
    }
}
