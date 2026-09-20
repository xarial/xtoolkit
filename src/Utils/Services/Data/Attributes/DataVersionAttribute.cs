//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;

namespace Xarial.XToolkit.Services.Data.Attributes
{
    /// <summary>
    /// Defines the data version
    /// </summary>
    /// <remarks>Used in <see cref="IDataSerializer"/></remarks>
    public class DataVersionAttribute : Attribute
    {
        internal Version Version { get; }
        internal Type VersionTransformerType { get; }

        /// <summary>
        /// Initiates the version support for this data
        /// </summary>
        /// <param name="version">Current (latest) version of the data</param>
        public DataVersionAttribute(string version)
        {
            Version = new Version(version);
        }

        /// <inheritdoc cref="DataVersionAttribute"/>
        /// <param name="versionTransformerType">Collection of version transformers of <see cref="IVersionsTransformer"/></param>
        public DataVersionAttribute(string version, Type versionTransformerType) : this(version)
        {
            VersionTransformerType = versionTransformerType;
        }
    }
}
