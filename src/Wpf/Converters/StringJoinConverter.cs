//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    /// <summary>
    /// Joins all binding values with specified separator
    /// </summary>
    public class StringJoinConverter : IMultiValueConverter, IValueConverter
    {
        /// <summary>
        /// Join separator
        /// </summary>
        public string Separator { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => string.Join(GetSeparator(parameter), values);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable)
            {
                return string.Join(GetSeparator(parameter), ((IEnumerable)value).Cast<object>());
            }
            else 
            {
                return value;
            }
        }

        private string GetSeparator(object parameter)
        {
            string separator;

            if (parameter is string)
            {
                separator = (string)parameter;
            }
            else
            {
                separator = Separator;
            }

            return separator;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
