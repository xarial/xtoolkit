//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;

namespace Xarial.XToolkit.Services.UserSettings.Attributes
{
    public class UserSettingVersionAttribute : Attribute
    {
        internal Version Version { get; }
        internal IVersionsTransformer VersionTransformer { get; }

        /// <summary>
        /// Initiates the version support for this user setting
        /// </summary>
        /// <param name="version">Current (latest) version of settings</param>
        /// <param name="versionTransformerType">Collection of version transformers of <see cref="IVersionsTransformer"/></param>
        public UserSettingVersionAttribute(string version, Type versionTransformerType)
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
