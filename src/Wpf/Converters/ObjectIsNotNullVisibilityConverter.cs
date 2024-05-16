//*********************************************************************
//xToolkit
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System.Windows;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class ObjectIsNotNullVisibilityConverter : ObjectIsNotNullUniversalConverter
    {
        public override object FalseValue => Visibility.Collapsed;
        public override object TrueValue => Visibility.Visible;
    }
}
