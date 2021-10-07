//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class ProgressContentOpacityConverter : IValueConverter
    {
        private const double DEFAULT_OPACITY = 1;
        private const double WORK_IN_PROGRESS_CTRL_OPACITY = 0.25;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return WORK_IN_PROGRESS_CTRL_OPACITY;
                }
            }

            return DEFAULT_OPACITY;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
