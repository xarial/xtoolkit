//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using System;
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

    /// <summary>
    /// Additional methods for the <see cref="IUserSettingsService"/>
    /// </summary>
    public static class UserSettingsServiceExtension
    {
        /// <summary>
        /// Reads settings of aa specified type
        /// </summary>
        /// <typeparam name="T">Settings type</typeparam>
        public static T ReadSettings<T>(this IUserSettingsService settsSvc, TextReader settsReader, params IValueSerializer[] serializers)
            => ReadSettings<T>(settsSvc, settsReader, default, serializers);

        /// <summary>
        /// Reads settings of the specified type with custom version transformers
        /// </summary>
        /// <typeparam name="T">Settings type</typeparam>
        public static T ReadSettings<T>(this IUserSettingsService settsSvc, TextReader settsReader, Func<IVersionsTransformer, IVersionsTransformer> versTransformerHandler, params IValueSerializer[] serializers)
            => (T)settsSvc.ReadSettings(settsReader, typeof(T), versTransformerHandler, serializers);

        /// <summary>
        /// Stores settings of the specified type
        /// </summary>
        /// <typeparam name="T">Settings type</typeparam>
        public static void StoreSettings<T>(this IUserSettingsService settsSvc, T setts, TextWriter settsWriter, params IValueSerializer[] serializers)
            => settsSvc.StoreSettings(setts, typeof(T), settsWriter, serializers);

        /// <summary>
        /// Reads settings of the specified type from a file
        /// </summary>
        /// <typeparam name="T">Settings type</typeparam>
        /// <param name="settsFile">Settings file path</param>
        public static T ReadSettings<T>(this IUserSettingsService settsSvc, string settsFile, params IValueSerializer[] serializers)
        {
            using (var textReader = File.OpenText(settsFile))
            {
                return settsSvc.ReadSettings<T>(textReader, serializers);
            }
        }

        /// <summary>
        /// Reads settings from the string builder
        /// </summary>
        /// <typeparam name="T">Settings type</typeparam>
        /// <param name="settsStr">Settings data</param>
        public static T ReadSettings<T>(this IUserSettingsService settsSvc, StringBuilder settsStr, params IValueSerializer[] serializers)
        {
            using (var stringReader = new StringReader(settsStr.ToString()))
            {
                return settsSvc.ReadSettings<T>(stringReader, serializers);
            }
        }

        /// <summary>
        /// Stores settings into the string builder
        /// </summary>
        /// <param name="settsStr">String builder buffer</param>
        public static void StoreSettings<T>(this IUserSettingsService settsSvc, T setts, StringBuilder settsStr, params IValueSerializer[] serializers)
        {
            using (var stringWriter = new StringWriter(settsStr))
            {
                settsSvc.StoreSettings(setts, stringWriter, serializers);
            }
        }

        /// <summary>
        /// Stores settings into a specified file
        /// </summary>
        /// <typeparam name="T">Settings type</typeparam>
        /// <param name="settsFile">Settings file path</param>
        public static void StoreSettings<T>(this IUserSettingsService settsSvc, T setts, string settsFile, params IValueSerializer[] serializers)
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
