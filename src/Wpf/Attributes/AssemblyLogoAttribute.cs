//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xarial.XToolkit.Reflection;

namespace Xarial.XToolkit.Wpf.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class AssemblyLogoAttribute : Attribute
    {
        public Image Logo { get; }

        public AssemblyLogoAttribute(Type resType, string resName) 
        {
            Logo = ResourceHelper.GetResource<Image>(resType, resName);
        }
    }
}
