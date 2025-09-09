//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
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
        internal IVersionsTransformer VersionTransformer { get; }

        /// <summary>
        /// Initiates the version support for this data
        /// </summary>
        /// <param name="version">Current (latest) version of the data</param>
        /// <param name="versionTransformerType">Collection of version transformers of <see cref="IVersionsTransformer"/></param>
        public DataVersionAttribute(string version, Type versionTransformerType)
        {
            Version = new Version(version);

            if (typeof(IVersionsTransformer).IsAssignableFrom(versionTransformerType))
            {
                VersionTransformer = (IVersionsTransformer)Activator.CreateInstance(versionTransformerType);
            }
            else 
            {
                throw new InvalidCastException($"'{versionTransformerType.FullName}' must implement '{nameof(IVersionsTransformer)}' interface'");
            }
        }
    }
}
