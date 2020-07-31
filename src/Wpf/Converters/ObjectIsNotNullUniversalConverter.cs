using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class ObjectIsNotNullUniversalConverter : IValueConverter
    {
        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is null))
            {
                return TrueValue;
            }
            else
            {
                return FalseValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
