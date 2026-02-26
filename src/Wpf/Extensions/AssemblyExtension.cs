//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Xarial.XToolkit.Wpf.Extensions
{
    public static class AssemblyExtension
    {
        /// <summary>
        /// Loads specific resource from dictionary
        /// </summary>
        /// <typeparam name="T">Type of resource to load</typeparam>
        /// <param name="assm">Assembly</param>
        /// <param name="path">Path to resource file</param>
        /// <param name="name">Name of the resource</param>
        /// <returns>Instance of the resource</returns>
        public static T LoadFromResources<T>(this Assembly assm, string path, string name)
            => (T)LoadFromResources(assm, path, name);

        internal static object LoadFromResources(this Assembly assm, string path, object key)
        {
            var assmName = assm.GetName();

            var dictionary = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/{assmName.Name};v{assmName.Version};component/{path}", UriKind.Absolute)
            };

            return dictionary[key];
        }
    }
}
