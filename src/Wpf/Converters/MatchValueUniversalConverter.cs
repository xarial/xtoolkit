//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    /// <summary>
    /// Converter compares binding value to the nominated value
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class MatchValueUniversalConverter : BooleanUniversalConverter
    {
        /// <summary>
        /// Equality comparer to compare values
        /// </summary>
        public IEqualityComparer EqualityComparer { get; set; }

        /// <summary>
        /// Value to compare to
        /// </summary>
        public object TargetValue { get; set; }

        /// <inheritdoc/>
        protected override bool? ConvertValueToBool(object value, object parameter)
        {
            object targVal;

            if (parameter != null)
            {
                targVal = parameter;
            }
            else 
            {
                targVal = TargetValue;
            }

            if (EqualityComparer != null)
            {
                return EqualityComparer.Equals(value, targVal);
            }
            else
            {
                return object.Equals(value, targVal);
            }
        }
    }
}
