//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    /// <summary>
    /// Formats all binidng values into the string with specified format
    /// </summary>
    public class StringFormatConverter : IMultiValueConverter
    {
        /// <summary>
        /// String format
        /// </summary>
        public string Format { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string format;

            if (parameter is string)
            {
                format = (string)parameter;
            }
            else
            {
                format = Format;
            }

            return string.Format(format, values);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
