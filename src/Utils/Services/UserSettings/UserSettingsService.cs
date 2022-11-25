//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.UserSettings.Attributes;
using Xarial.XToolkit.Services.UserSettings.Converters;

namespace Xarial.XToolkit.Services.UserSettings
{
    /// <summary>
    /// Services allows to manage user settings with the backwards compatibility
    /// </summary>
    public interface IUserSettingsService 
    {
        /// <summary>
        /// Reads user settings
        /// </summary>
        /// <param name="settsReader">Reader</param>
        /// <param name="settsType">Settings type</param>
        /// <param name="versTransformerHandler">Version transformer</param>
        /// <param name="serializers">Custom serializers</param>
        /// <returns>Settings</returns>
        object ReadSettings(TextReader settsReader, Type settsType, Func<IVersionsTransformer, IVersionsTransformer> versTransformerHandler, params IValueSerializer[] serializers);
        
        /// <summary>
        /// Stores user settings
        /// </summary>
        /// <param name="setts">Settings to store</param>
        /// <param name="settsType">Settings type</param>
        /// <param name="settsWriter">Writer</param>
        /// <param name="serializers">Custom serializers</param>
        void StoreSettings(object setts, Type settsType, TextWriter settsWriter, params IValueSerializer[] serializers);
    }

    /// <inheritdoc/>
    public class UserSettingsService : IUserSettingsService
    {
        /// <inheritdoc/>
        public object ReadSettings(TextReader settsReader, Type settsType, Func<IVersionsTransformer, IVersionsTransformer> versTransformerHandler, params IValueSerializer[] serializers)
        {
            var jsonSer = CreateJsonSerializer();

            if (TryGetVersionInfo(settsType, out Version vers, out IVersionsTransformer transform))
            {
                if (versTransformerHandler != null)
                {
                    transform = versTransformerHandler.Invoke(transform);
                }

                jsonSer.Converters.Add(new ReadSettingsJsonConverter(settsType, transform?.Transforms ?? Enumerable.Empty<VersionTransform>(), vers));
            }

            foreach (var ser in serializers)
            {
                jsonSer.Converters.Add(new CustomSerializerJsonConverter(ser));
            }

            return jsonSer.Deserialize(settsReader, settsType);
        }

        /// <inheritdoc/>
        public void StoreSettings(object setts, Type settsType, TextWriter settsWriter, params IValueSerializer[] serializers)
        {
            var jsonSer = CreateJsonSerializer();

            if (TryGetVersionInfo(settsType, out Version vers, out _))
            {
                jsonSer.Converters.Add(new WriteSettingsJsonConverter(settsType, vers));
            }

            foreach (var ser in serializers)
            {
                jsonSer.Converters.Add(new CustomSerializerJsonConverter(ser));
            }

            jsonSer.Serialize(settsWriter, setts, settsType);
        }

        /// <summary>
        /// Override to provide custom JSON serializer
        /// </summary>
        protected virtual JsonSerializer CreateJsonSerializer() => new JsonSerializer();

        private bool TryGetVersionInfo(Type settsType, out Version vers, out IVersionsTransformer transforms)
        {
            if (settsType.TryGetAttribute(out UserSettingVersionAttribute att, true))
            {
                vers = att.Version;
                transforms = att.VersionTransformer;
                return true;
            }
            else
            {
                vers = null;
                transforms = null;
                return false;
            }
        }
    }

    public static class UserSettingsServiceExtension
    {
        public static T ReadSettings<T>(this UserSettingsService settsSvc, TextReader settsReader, params IValueSerializer[] serializers)
            => ReadSettings<T>(settsSvc, settsReader, default(Func<IVersionsTransformer, IVersionsTransformer>), serializers);

        public static T ReadSettings<T>(this UserSettingsService settsSvc, TextReader settsReader, Func<IVersionsTransformer, IVersionsTransformer> versTransformerHandler, params IValueSerializer[] serializers)
            => (T)settsSvc.ReadSettings(settsReader, typeof(T), versTransformerHandler, serializers);

        public static void StoreSettings<T>(this UserSettingsService settsSvc, T setts, TextWriter settsWriter, params IValueSerializer[] serializers)
            => settsSvc.StoreSettings(setts, typeof(T), settsWriter, serializers);

        public static T ReadSettings<T>(this UserSettingsService settsSvc, string settsFile, params IValueSerializer[] serializers)
        {
            using (var textReader = File.OpenText(settsFile))
            {
                return settsSvc.ReadSettings<T>(textReader, serializers);
            }
        }

        public static T ReadSettings<T>(this UserSettingsService settsSvc, StringBuilder settsStr, params IValueSerializer[] serializers)
        {
            using (var stringReader = new StringReader(settsStr.ToString()))
            {
                return settsSvc.ReadSettings<T>(stringReader, serializers);
            }
        }

        public static void StoreSettings<T>(this UserSettingsService settsSvc, T setts, StringBuilder settsStr, params IValueSerializer[] serializers)
        {
            using (var stringWriter = new StringWriter(settsStr))
            {
                settsSvc.StoreSettings(setts, stringWriter, serializers);
            }
        }

        public static void StoreSettings<T>(this UserSettingsService settsSvc, T setts, string settsFile, params IValueSerializer[] serializers)
        {
            var settsDir = Path.GetDirectoryName(settsFile);

            if (!Directory.Exists(settsDir))
            {
                Directory.CreateDirectory(settsDir);
            }

            using (var textWriter = File.CreateText(settsFile))
            {
                settsSvc.StoreSettings<T>(setts, textWriter, serializers);
            }
        }
    }
}
