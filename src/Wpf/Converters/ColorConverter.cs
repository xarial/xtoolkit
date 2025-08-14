//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Globalization;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Converters
{
    /// <summary>
    /// Converter mode of <see cref="ColorConverter"/>
    /// </summary>
    public enum ColorConvertMode_e 
    {
        /// <summary>
        /// Converts <see cref="System.Drawing.Color"/> to <see cref="System.Windows.Media.Color"/>
        /// </summary>
        DrawingColorToMediaColor,

        /// <summary>
        /// Converts <see cref="System.Windows.Media.Color"/> to <see cref="System.Drawing.Color"/>
        /// </summary>
        MediaColorToDrawingColor
    }

    /// <summary>
    /// <see cref="System.Drawing.Color"/> and <see cref="System.Windows.Media.Color"/> converter
    /// </summary>
    public class ColorConverter : IValueConverter
    {
        /// <summary>
        /// Conversion mode
        /// </summary>
        public ColorConvertMode_e Mode { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (Mode) 
            {
                case ColorConvertMode_e.DrawingColorToMediaColor:
                    return DrawingColorToMediaColor(value);

                case ColorConvertMode_e.MediaColorToDrawingColor:
                    return MediaColorToDrawingColor(value);

                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (Mode)
            {
                case ColorConvertMode_e.DrawingColorToMediaColor:
                    return MediaColorToDrawingColor(value);

                case ColorConvertMode_e.MediaColorToDrawingColor:
                    return DrawingColorToMediaColor(value);
                    
                default:
                    return null;
            }
        }

        private object MediaColorToDrawingColor(object value)
        {
            if (value is System.Windows.Media.Color)
            {
                var color = (System.Windows.Media.Color)value;

                return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            }
            else
            {
                return value;
            }
        }

        private object DrawingColorToMediaColor(object value)
        {
            if (value is System.Drawing.Color)
            {
                var color = (System.Drawing.Color)value;

                return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            }
            else
            {
                return value;
            }
        }
    }
}
