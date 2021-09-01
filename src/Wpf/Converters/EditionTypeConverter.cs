using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.XToolkit.Wpf.Converters
{
    public class EditionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PackageEditionSpec)
            {
                var spec = (PackageEditionSpec)value;

                var exp = "";

                if (spec.ExpiryDate.HasValue)
                {
                    var expDate = spec.ExpiryDate.Value.ToString("dd MMM yyyy");

                    if (spec.ExpiryDate.Value > DateTime.Now)
                    {
                        exp = $" (expires on {expDate})";
                    }
                    else 
                    {
                        exp = $" (expired on {expDate})";
                    }
                }

                return $"{spec.EditionName}{exp}";
            }
            else 
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
