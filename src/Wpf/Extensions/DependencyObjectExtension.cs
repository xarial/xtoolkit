//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Xarial.XToolkit.Wpf.Extensions
{
    /// <summary>
    /// Extension methods for dependency object
    /// </summary>
    public static class DependencyObjectExtension
    {
        /// <summary>
        /// Finds first visual parent of this object of a specified type
        /// </summary>
        /// <typeparam name="T">Type of the parent</typeparam>
        /// <param name="child">Object to find the parent for</param>
        /// <returns>Parent or null</returns>
        public static T TryFindParentOfType<T>(this DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentDepObj = child;

            do
            {
                parentDepObj = VisualTreeHelper.GetParent(parentDepObj);

                if (parentDepObj is T)
                {
                    return parentDepObj as T;
                }
            }
            while (parentDepObj != null);

            return null;
        }
    }
}
