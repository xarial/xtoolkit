//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
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
        internal IEnumerable<VersionTransform> VersionTransformers { get; }

        /// <summary>
        /// Initiates the version support for this user setting
        /// </summary>
        /// <param name="version">Current (latest) version of settings</param>
        /// <param name="versionTransformerType">Collection of version transformers of <see cref="IEnumerable<VersionTransform>"/>. Use <see cref="BaseUserSettingsVersionsTransformer"/></param>
        public UserSettingVersionAttribute(string version, Type versionTransformerType)
        {
            Version = new Version(version);

            if (typeof(IEnumerable<VersionTransform>).IsAssignableFrom(versionTransformerType))
            {
                VersionTransformers = (IEnumerable<VersionTransform>)Activator.CreateInstance(versionTransformerType);
            }
            else 
            {
                throw new InvalidCastException($"'{versionTransformerType.FullName}' must implement '{nameof(IEnumerable<VersionTransform>)}' interface. Use '{typeof(BaseUserSettingsVersionsTransformer)}'");
            }

        }
    }
}
