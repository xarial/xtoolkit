//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
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
using Xarial.XToolkit.Services.Data.Attributes;

namespace Xarial.XToolkit.Services.Data
{
    /// <summary>
    /// Services allows to manage data serializing with the backwards compatibility
    /// </summary>
    public interface IDataSerializer 
    {
        /// <summary>
        /// Settings type
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Reads data
        /// </summary>
        /// <param name="settsReader">Reader</param>
        /// <returns>Settings</returns>
        object Read(TextReader settsReader);
        
        /// <summary>
        /// Saves data
        /// </summary>
        /// <param name="setts">Data to save</param>
        /// <param name="settsWriter">Writer</param>
        void Save(object setts, TextWriter settsWriter);
    }

    /// <inheritdoc/>
    public interface IDataSerializer<T> : IDataSerializer 
    {
        /// <inheritdoc/>
        new T Read(TextReader settsReader);

        /// <inheritdoc/>
        void Save(T setts, TextWriter settsWriter);
    }

    /// <summary>
    /// Additional methods for the <see cref="IDataSerializer"/>
    /// </summary>
    public static class DataSerializerExtension
    {
        /// <summary>
        /// Finds all known kinds
        /// </summary>
        /// <param name="type">Source type</param>
        /// <returns>Known types and their kind name</returns>
        /// <remarks>Known kinds are found based on <see cref="KnownKindAttribute"/> attribute</remarks>
        public static IReadOnlyDictionary<Type, string> GetKnownKinds(Type type)
        {
            var knownKinds = new Dictionary<Type, string>();

            FindKnownKinds(type, knownKinds, new List<Type>());

            return knownKinds;
        }

        /// <inheritdoc/>
        /// <typeparam name="T">Type</typeparam>
        public static IReadOnlyDictionary<Type, string> GetKnownKinds<T>() 
            => GetKnownKinds(typeof(T));

        private static void FindKnownKinds(Type type, Dictionary<Type, string> knownKinds, List<Type> processedTypes)
        {
            foreach (var att in type.GetCustomAttributes<KnownKindAttribute>(true))
            {
                if (!knownKinds.ContainsKey(att.Type))
                {
                    knownKinds.Add(att.Type, att.Kind);
                }
            }

            if (!processedTypes.Contains(type))
            {
                processedTypes.Add(type);

                if (!type.IsPrimitive && !type.IsArray && !type.IsEnum)
                {
                    foreach (var prp in type.GetProperties())
                    {
                        foreach (KnownKindAttribute att in prp.GetCustomAttributes(typeof(KnownKindAttribute))) 
                        {
                            if (!knownKinds.ContainsKey(att.Type))
                            {
                                knownKinds.Add(att.Type, att.Kind);
                            }
                        }

                        var prpType = prp.PropertyType;

                        FindKnownKinds(prpType, knownKinds, processedTypes);

                        if (prpType.IsArray)
                        {
                            if (prpType.HasElementType)
                            {
                                FindKnownKinds(prpType.GetElementType(), knownKinds, processedTypes);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads settings of the specified type from a file
        /// </summary>
        /// <param name="settsSvc">Settings service</param>
        /// <param name="settsFile">Settings file path</param>
        public static object Read(this IDataSerializer settsSvc, string settsFile)
        {
            using (var textReader = File.OpenText(settsFile))
            {
                return settsSvc.Read(textReader);
            }
        }

        ///<inheritdoc cref="Read(IDataSerializer, string)"/>
        /// <typeparam name="T">Settings type</typeparam>
        public static T Read<T>(this IDataSerializer<T> settsSvc, string settsFile)
            => (T)Read((IDataSerializer)settsSvc, settsFile);

        /// <summary>
        /// Reads settings from the string builder
        /// </summary>
        /// <param name="settsSvc">Settings service</param>
        /// <param name="settsStr">Settings data</param>
        public static object Read(this IDataSerializer settsSvc, StringBuilder settsStr)
        {
            using (var stringReader = new StringReader(settsStr.ToString()))
            {
                return settsSvc.Read(stringReader);
            }
        }

        /// <inheritdoc cref="Read(IDataSerializer, StringBuilder)"/>
        /// <typeparam name="T">Settings type</typeparam>
        public static T Read<T>(this IDataSerializer<T> settsSvc, StringBuilder settsStr)
            => (T)Read((IDataSerializer)settsSvc, settsStr);

        /// <summary>
        /// Stores settings into the string builder
        /// </summary>
        /// <param name="settsSvc">Settings service</param>
        /// <param name="setts">Settings</param>
        /// <param name="settsStr">String builder buffer</param>
        public static void Save(this IDataSerializer settsSvc, object setts, StringBuilder settsStr)
        {
            using (var stringWriter = new StringWriter(settsStr))
            {
                settsSvc.Save(setts, stringWriter);
            }
        }

        /// <inheritdoc cref="Save(IDataSerializer, object, StringBuilder)"/>
        /// <typeparam name="T">Settings type</typeparam>
        public static void Save<T>(this IDataSerializer<T> settsSvc, T setts, StringBuilder settsStr)
            => Save((IDataSerializer)settsSvc, setts, settsStr);

        /// <summary>
        /// Stores settings into a specified file
        /// </summary>
        /// <param name="settsSvc">Settings service</param>
        /// <param name="setts">Settings</param>
        /// <param name="settsFile">Settings file path</param>
        public static void Save(this IDataSerializer settsSvc, object setts, string settsFile)
        {
            var settsDir = Path.GetDirectoryName(settsFile);

            if (!Directory.Exists(settsDir))
            {
                Directory.CreateDirectory(settsDir);
            }

            using (var textWriter = File.CreateText(settsFile))
            {
                settsSvc.Save(setts, textWriter);
            }
        }

        /// <inheritdoc cref="Save(IDataSerializer, object, string)"/>
        /// <typeparam name="T">Settings type</typeparam>
        public static void Save<T>(this IDataSerializer<T> settsSvc, T setts, string settsFile)
            => Save((IDataSerializer)settsSvc, setts, settsFile);
    }
}
