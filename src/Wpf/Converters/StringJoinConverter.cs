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
    /// Joins all binding values with specified separator
    /// </summary>
    public class StringJoinConverter : IMultiValueConverter
    {
        /// <summary>
        /// Join separator
        /// </summary>
        public string Separator { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
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

            return string.Join(separator, values);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
