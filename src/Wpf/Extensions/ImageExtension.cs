//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace Xarial.XToolkit.Wpf.Extensions
{
    /// <summary>
    /// Additional methods for the <see cref="Image"/>
    /// </summary>
    public static class ImageExtension
    {
        /// <summary>
        /// Converts <see cref="Image"/> to <see cref="BitmapImage"/>
        /// </summary>
        /// <param name="img">Image to convert</param>
        /// <param name="freeze">True to freeze the image</param>
        /// <returns>Converted image</returns>
        public static BitmapImage ToBitmapImage(this Image img, bool freeze = true)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                var bmpImg = new BitmapImage();
                bmpImg.BeginInit();
                bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                bmpImg.StreamSource = stream;
                bmpImg.EndInit();

                if (freeze) 
                {
                    if (bmpImg.CanFreeze && !bmpImg.IsFrozen) 
                    {
                        bmpImg.Freeze();
                    }
                }

                return bmpImg;
            }
        }
    }
}
