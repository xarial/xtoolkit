﻿//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
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
        public static DataTemplate LoadFromResources(this Assembly assm, string path, string name)
        {
            var dllName = assm.GetName();

            var resDict = new ResourceDictionary()
            {
                Source = new Uri($"/{dllName};component/{path}",
                        UriKind.RelativeOrAbsolute)
            };

            return (DataTemplate)resDict[name];
        }
    }
}