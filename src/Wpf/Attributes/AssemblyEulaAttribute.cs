//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XToolkit.Reflection;

namespace Xarial.XToolkit.Wpf.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class AssemblyEulaAttribute : Attribute
    {
        /// <summary>
        /// RTF encoded text of the end-user license agreement
        /// </summary>
        public string Eula { get; }

        public AssemblyEulaAttribute(Type resType, string resName)
        {
            Eula = ResourceHelper.GetResource<string>(resType, resName);
        }

        public AssemblyEulaAttribute(string eula)
        {
            Eula = eula;
        }
    }
}
