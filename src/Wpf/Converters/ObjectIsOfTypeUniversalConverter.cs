//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class ObjectIsOfTypeUniversalConverter : BooleanUniversalConverter
    {
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
