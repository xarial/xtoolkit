//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
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

        private readonly VersionTransformManager m_VersionTransformsMgr;
        private readonly KnownKindManager m_KnownKindMgr;

        /// <summary>Constuctor</summary>
        /// <param name="dataType">Type of data</param>
        /// <param name="serializers">Custom serializers</param>
        public NsJsonDataSerializer(Type dataType, params IValueSerializer[] serializers) : this(dataType, null, serializers)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataType">Type of data</param>
        /// <param name="knownKinds">Known kinds for serialization</param>
        /// <param name="serializers">Custom serializers</param>
        public NsJsonDataSerializer(Type dataType, IReadOnlyDictionary<Type, string> knownKinds, params IValueSerializer[] serializers) : this(dataType, knownKinds, new JsonSerializer(), serializers)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataType">Type of data</param>
        /// <param name="knownKinds">Known kinds for serialization</param>
        /// <param name="jsonSer">Json serializer</param>
        /// <param name="serializers">Custom serializers</param>
        public NsJsonDataSerializer(Type dataType, IReadOnlyDictionary<Type, string> knownKinds, JsonSerializer jsonSer, params IValueSerializer[] serializers)
        {
            DataType = dataType;

            m_JsonSer = jsonSer;

            m_VersionTransformsMgr = new VersionTransformManager(GetVersionTransformer);

            m_KnownKindMgr = new KnownKindManager(knownKinds);

            SetupJsonSerializer(jsonSer, dataType, serializers);

            if (!(jsonSer.ContractResolver is NsJsonDataSerializerContractResolver))
            {
                throw new InvalidCastException($"Contract resolver must inherit {nameof(NsJsonDataSerializerContractResolver)}");
            }

            var convIndex = -1;

            for (int i = 0; i < jsonSer.Converters.Count; i++) 
            {
                if (jsonSer.Converters[i] is NsJsonDataSerializerJsonConverter) 
                {
                    convIndex = i;
                    break;
                }
            }

            if (convIndex != -1)
            {
                if (convIndex != 0) 
                {
                    var conv = jsonSer.Converters[convIndex];
                    jsonSer.Converters.RemoveAt(convIndex);
                    jsonSer.Converters.Insert(0, conv);
                }
            }
            else 
            {
                throw new Exception($"Missing the {nameof(NsJsonDataSerializerJsonConverter)} converter");
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
        {
            using (var jsonReader = new JsonTextReader(settsReader))
            {
                return m_JsonSer.Deserialize(jsonReader, DataType);
            }
        }

        /// <inheritdoc/>
        public void Save(object setts, TextWriter settsWriter)
            => m_JsonSer.Serialize(settsWriter, setts, DataType);

        /// <summary>
        /// Sets up json serializer
        /// </summary>
        protected virtual void SetupJsonSerializer(JsonSerializer jsonSer, Type settsType, IValueSerializer[] serializers)
        {
            if (!settsType.TryGetAttribute(out DataSerializerOptionsAttribute att, true)) 
            {
                this.GetType().TryGetAttribute(out att, true);
            }

            if (att != null)
            {
                if (att.Formatting == DataFormatting_e.Indented)
                {
                    jsonSer.Formatting = Formatting.Indented;
                }

                if (att.EnumSerialization == EnumSerializationType_e.Text)
                {
                    jsonSer.Converters.Add(new StringEnumConverter());
                }

                switch (att.NullValueHandling)
                {
                    case NullValueHandling_e.Include:
                        jsonSer.NullValueHandling = NullValueHandling.Include;
                        break;

                    case NullValueHandling_e.Ignore:
                        jsonSer.NullValueHandling = NullValueHandling.Ignore;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            
            jsonSer.Converters.Add(new NsJsonDataSerializerJsonConverter(m_KnownKindMgr, TryGetVersionTransformInfo, SupportsVersioning, CreateInstance));

            if (serializers != null)
            {
                foreach (var ser in serializers)
                {
                    jsonSer.Converters.Add(new CustomSerializerJsonConverter(ser));
                }
            }

            jsonSer.ContractResolver = new NsJsonDataSerializerContractResolver(m_KnownKindMgr, m_VersionTransformsMgr);
        }

        /// <summary>
        /// Override to ptovide version information for the type while deserializing
        /// </summary>
        /// <param name="jToken">Current JSON token</param>
        /// <param name="objectType">Property type to serialize to</param>
        /// <param name="existingValue">Current value (initiated via the parent class in the constructor)</param>
        /// <param name="latestVersion">Latest version for this data type</param>
        /// <param name="transformer">Version transfomer</param>
        /// <returns>False if there is no version information for this type</returns>
        protected virtual bool TryGetVersionTransformInfo(JToken jToken, Type objectType, object existingValue, out Version latestVersion, out IVersionsTransformer transformer)
        {
            Type kindType = null;

            if (jToken.Type == JTokenType.Object && m_KnownKindMgr.TryGetKindType((JObject)jToken, out kindType) && m_VersionTransformsMgr.TryGetVersionTransformInfo(kindType, out latestVersion, out transformer))
            {
                return true;
            }
            else if (kindType != objectType && m_VersionTransformsMgr.TryGetVersionTransformInfo(objectType, out latestVersion, out transformer))
            {
                return true;
            }
            else
            {
                latestVersion = null;
                transformer = null;
                return false;
            }
        }

        /// <summary>
        /// Override to indicate if the version property should be added to json serialization
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        /// <returns>True if versioning is supported/returns>
        protected virtual bool SupportsVersioning(Type objectType) => m_VersionTransformsMgr.TryGetVersionTransformInfo(objectType, out _, out _);

        /// <summary>
        /// Creates new instance of the JSON token
        /// </summary>
        /// <param name="jToken">JSON token</param>
        /// <param name="type">Target type</param>
        /// <param name="existingValue">Current value</param>
        /// <param name="serializer">Serializer</param>
        /// <returns>new instance</returns>
        /// <remarks>This method is only called for types which support versioning or known kinds</remarks>
        protected virtual object CreateInstance(JToken jToken, Type type, object existingValue, JsonSerializer serializer)
        {
            if (jToken.Type != JTokenType.Null)
            {
                var inst = Activator.CreateInstance(type);
                serializer.Populate(jToken.CreateReader(), inst);
                return inst;
            }
            else 
            {
                return null;
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
        public NsJsonDataSerializer(IReadOnlyDictionary<Type, string> knownKinds, JsonSerializer jsonSer, params IValueSerializer[] serializers) : base(typeof(T), knownKinds, jsonSer, serializers)
        {
        }

        /// <inheritdoc/>
        public void Save(T setts, TextWriter settsWriter) => base.Save(setts, settsWriter);
    }
}
