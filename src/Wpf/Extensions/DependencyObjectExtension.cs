//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
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

        /// <summary>
        /// Finds first visual child of this object of a specified type
        /// </summary>
        /// <typeparam name="T">Type of the child</typeparam>
        /// <param name="parent">Object to find the child for</param>
        /// <returns>Child or null</returns>
        public static T TryFindChildOfType<T>(this DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T res)
                {
                    return res;
                }
                else
                {
                    res = TryFindChildOfType<T>(child);

                    if (res != null)
                    {
                        return res;
                    }
                }
            }

            return null;
        }
    }
}
