//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XToolkit.Services.Data.Attributes
{
    /// <summary>
    /// Specifies the known kind for the serialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = true)]
    public class KnownKindAttribute : Attribute
    {
        internal Type Type { get; }

        internal string Kind { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="kind">Kind of known type</param>
        public KnownKindAttribute(Type type, string kind)
        {
            Type = type;
            Kind = kind;
        }
    }
}
