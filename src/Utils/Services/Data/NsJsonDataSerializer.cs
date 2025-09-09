//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.Data.Attributes;
using Xarial.XToolkit.Services.Data.Converters;

namespace Xarial.XToolkit.Services.Data
{
    /// <summary>
    /// Newtonsoft.Json based <see cref="IDataSerializer"/>
    /// </summary>
    public class NsJsonDataSerializer : IDataSerializer
    {
        /// <inheritdoc/>
        public Type DataType { get; }

        private readonly JsonSerializer m_JsonSer;

        /// <summary>Constuctor</summary>
        /// <param name="settsType">Type of settings</param>
        /// <param name="serializers">Custom serializers</param>
        public NsJsonDataSerializer(Type settsType, params IValueSerializer[] serializers) : this(settsType, null, serializers)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settsType">Type of settings</param>
        /// <param name="knownKinds">Known kinds for serialization</param>
        /// <param name="serializers">Custom serializers</param>
        public NsJsonDataSerializer(Type settsType, IReadOnlyDictionary<Type, string> knownKinds, params IValueSerializer[] serializers)
        {
            DataType = settsType;

            m_JsonSer = CreateJsonSerializer();

            if (TryGetVersionInfo(DataType, out Version vers, out IVersionsTransformer transform))
            {
                transform = GetVersionTransformer(transform);

                m_JsonSer.Converters.Add(new SettingsJsonConverter(DataType, transform?.Transforms ?? Array.Empty<VersionTransform>(), vers));
            }

            if (serializers != null)
            {
                foreach (var ser in serializers)
                {
                    m_JsonSer.Converters.Add(new CustomSerializerJsonConverter(ser));
                }
            }

            if (knownKinds?.Any() == true)
            {
                m_JsonSer.Converters.Add(new KnownKindJsonConverter(knownKinds));
            }
        }

        /// <summary>
        /// Provides version transformer
        /// </summary>
        /// <param name="src">Source version transformer</param>
        /// <returns>Bersion transformer</returns>
        protected virtual IVersionsTransformer GetVersionTransformer(IVersionsTransformer src) => src;

        /// <inheritdoc/>
        public object Read(TextReader settsReader)
            => m_JsonSer.Deserialize(settsReader, DataType);

        /// <inheritdoc/>
        public void Save(object setts, TextWriter settsWriter)
            => m_JsonSer.Serialize(settsWriter, setts, DataType);

        /// <summary>
        /// Override to provide custom JSON serializer
        /// </summary>
        protected virtual JsonSerializer CreateJsonSerializer()
        {
            var ser = new JsonSerializer();

            if (DataType.TryGetAttribute(out DataSerializerOptionsAttribute att, true))
            {
                if (att.Formatting == DataFormatting_e.Indented) 
                {
                    ser.Formatting = Formatting.Indented;
                }

                if (att.EnumSerialization == EnumSerializationType_e.Text) 
                {
                    ser.Converters.Add(new StringEnumConverter());
                }
            }

            return ser;
        }

        private bool TryGetVersionInfo(Type settsType, out Version vers, out IVersionsTransformer transforms)
        {
            if (settsType.TryGetAttribute(out DataVersionAttribute att, true))
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
    public class NsJsonDataSerializer<T> : NsJsonDataSerializer, IDataSerializer<T>
    {
        /// <inheritdoc/>
        public new T Read(TextReader settsReader) => (T)base.Read(settsReader);

        /// <inheritdoc/>
        public NsJsonDataSerializer(params IValueSerializer[] serializers) : base(typeof(T), serializers)
        {
        }

        /// <inheritdoc/>
        public NsJsonDataSerializer(IReadOnlyDictionary<Type, string> knownTypes, params IValueSerializer[] serializers) : base(typeof(T), knownTypes, serializers)
        {
        }

        /// <inheritdoc/>
        public void Save(T setts, TextWriter settsWriter) => base.Save(setts, settsWriter);
    }
}
