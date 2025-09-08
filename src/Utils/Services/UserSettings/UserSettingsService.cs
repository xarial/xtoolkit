//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// Settings type
        /// </summary>
        Type SettingsType { get; }

        /// <summary>
        /// Reads user settings
        /// </summary>
        /// <param name="settsReader">Reader</param>
        /// <returns>Settings</returns>
        object ReadSettings(TextReader settsReader);
        
        /// <summary>
        /// Stores user settings
        /// </summary>
        /// <param name="setts">Settings to store</param>
        /// <param name="settsWriter">Writer</param>
        void StoreSettings(object setts, TextWriter settsWriter);
    }

    /// <inheritdoc/>
    public interface IUserSettingsService<T> : IUserSettingsService 
    {
        /// <inheritdoc/>
        new T ReadSettings(TextReader settsReader);

        /// <inheritdoc/>
        void StoreSettings(T setts, TextWriter settsWriter);
    }

    /// <summary>
    /// Newtonsoft.Json based <see cref="IUserSettingsService"/>
    /// </summary>
    public class UserSettingsService : IUserSettingsService
    {
        /// <summary>
        /// Finds all known types
        /// </summary>
        /// <param name="srcType">Source type</param>
        /// <returns>Known types and their short name</returns>
        /// <remarks>Known types are found based on <see cref="KnownKindAttribute"/> attribute</remarks>
        public static IReadOnlyDictionary<Type, string> GetKnownTypes(Type srcType) 
        {
            var knownTypes = new Dictionary<Type, string>();

            FindKnownTypes(srcType, knownTypes, new List<Type>());

            return knownTypes;
        }

        private static void FindKnownTypes(Type type, Dictionary<Type, string> knownTypes, List<Type> processedTypes) 
        {
            foreach (var att in type.GetCustomAttributes<KnownKindAttribute>(true))
            {
                if (!knownTypes.ContainsKey(type))
                {
                    knownTypes.Add(att.Type, att.Kind);
                }
            }

            if (!processedTypes.Contains(type))
            {
                processedTypes.Add(type);

                if (!type.IsPrimitive && !type.IsArray && !type.IsEnum)
                {
                    foreach (var prp in type.GetProperties())
                    {
                        var prpType = prp.PropertyType;

                        FindKnownTypes(prpType, knownTypes, processedTypes);

                        if (prpType.IsArray) 
                        {
                            if (prpType.HasElementType)
                            {
                                FindKnownTypes(prpType.GetElementType(), knownTypes, processedTypes);
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public Type SettingsType { get; }

        private readonly JsonSerializer m_JsonSer;

        /// <summary>Constuctor</summary>
        /// <param name="settsType">Type of settings</param>
        /// <param name="serializers">Custom serializers</param>
        public UserSettingsService(Type settsType, params IValueSerializer[] serializers) : this(settsType, null, serializers)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settsType">Type of settings</param>
        /// <param name="knownTypes">Known types for serialization</param>
        /// <param name="serializers">Custom serializers</param>
        public UserSettingsService(Type settsType, IReadOnlyDictionary<Type, string> knownTypes, params IValueSerializer[] serializers)
        {
            SettingsType = settsType;

            m_JsonSer = CreateJsonSerializer();

            if (TryGetVersionInfo(SettingsType, out Version vers, out IVersionsTransformer transform))
            {
                transform = GetVersionTransformer(transform);

                m_JsonSer.Converters.Add(new SettingsJsonConverter(SettingsType, transform?.Transforms ?? Array.Empty<VersionTransform>(), vers));
            }

            if (serializers != null)
            {
                foreach (var ser in serializers)
                {
                    m_JsonSer.Converters.Add(new CustomSerializerJsonConverter(ser));
                }
            }

            if (knownTypes?.Any() == true)
            {
                m_JsonSer.Converters.Add(new KnownKindJsonConverter(knownTypes));
            }
        }

        /// <summary>
        /// Provides version transformer
        /// </summary>
        /// <param name="src">Source version transformer</param>
        /// <returns>Bersion transformer</returns>
        protected virtual IVersionsTransformer GetVersionTransformer(IVersionsTransformer src) => src;

        /// <inheritdoc/>
        public object ReadSettings(TextReader settsReader)
            => m_JsonSer.Deserialize(settsReader, SettingsType);

        /// <inheritdoc/>
        public void StoreSettings(object setts, TextWriter settsWriter)
            => m_JsonSer.Serialize(settsWriter, setts, SettingsType);

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

    /// <inheritdoc/>
    public class UserSettingsService<T> : UserSettingsService, IUserSettingsService<T>
    {
        /// <inheritdoc/>
        public new T ReadSettings(TextReader settsReader) => (T)base.ReadSettings(settsReader);

        /// <inheritdoc/>
        public UserSettingsService(params IValueSerializer[] serializers) : base(typeof(T), serializers)
        {
        }

        /// <inheritdoc/>
        public UserSettingsService(IReadOnlyDictionary<Type, string> knownTypes, params IValueSerializer[] serializers) : base(typeof(T), knownTypes, serializers)
        {
        }

        /// <inheritdoc/>
        public void StoreSettings(T setts, TextWriter settsWriter) => base.StoreSettings(setts, settsWriter);
    }

    /// <summary>
    /// Additional methods for the <see cref="IUserSettingsService"/>
    /// </summary>
    public static class UserSettingsServiceExtension
    {
        /// <summary>
        /// Reads settings of the specified type from a file
        /// </summary>
        /// <typeparam name="T">Settings type</typeparam>
        /// <param name="settsSvc">Settings service</param>
        /// <param name="settsFile">Settings file path</param>
        public static T ReadSettings<T>(this IUserSettingsService<T> settsSvc, string settsFile)
        {
            using (var textReader = File.OpenText(settsFile))
            {
                return settsSvc.ReadSettings(textReader);
            }
        }

        /// <summary>
        /// Reads settings from the string builder
        /// </summary>
        /// <typeparam name="T">Settings type</typeparam>
        /// <param name="settsSvc">Settings service</param>
        /// <param name="settsStr">Settings data</param>
        public static T ReadSettings<T>(this IUserSettingsService<T> settsSvc, StringBuilder settsStr)
        {
            using (var stringReader = new StringReader(settsStr.ToString()))
            {
                return settsSvc.ReadSettings(stringReader);
            }
        }

        /// <summary>
        /// Stores settings into the string builder
        /// </summary>
        /// <param name="settsSvc">Settings service</param>
        /// <param name="setts">Settings</param>
        /// <param name="settsStr">String builder buffer</param>
        public static void StoreSettings<T>(this IUserSettingsService<T> settsSvc, T setts, StringBuilder settsStr)
        {
            using (var stringWriter = new StringWriter(settsStr))
            {
                settsSvc.StoreSettings(setts, stringWriter);
            }
        }

        /// <summary>
        /// Stores settings into a specified file
        /// </summary>
        /// <typeparam name="T">Settings type</typeparam>
        /// <param name="settsSvc">Settings service</param>
        /// <param name="setts">Settings</param>
        /// <param name="settsFile">Settings file path</param>
        public static void StoreSettings<T>(this IUserSettingsService<T> settsSvc, T setts, string settsFile)
        {
            var settsDir = Path.GetDirectoryName(settsFile);

            if (!Directory.Exists(settsDir))
            {
                Directory.CreateDirectory(settsDir);
            }

            using (var textWriter = File.CreateText(settsFile))
            {
                settsSvc.StoreSettings(setts, textWriter);
            }
        }
    }
}
