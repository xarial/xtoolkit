//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
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
            
            SetupJsonSerializer(jsonSer, dataType, knownKinds, serializers);

            if (!(jsonSer.ContractResolver is NsJsonDataSerializerContractResolver))
            {
                throw new InvalidCastException($"Contract resolver must inherit {nameof(NsJsonDataSerializerContractResolver)}");
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
        protected virtual void SetupJsonSerializer(JsonSerializer jsonSer, Type settsType, IReadOnlyDictionary<Type, string> knownKinds, IValueSerializer[] serializers)
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

            var kindMgr = new KnownKindManager(knownKinds);
            var versTransformMgr = new VersionTransformManager(GetVersionTransformer);

            jsonSer.Converters.Add(new NsJsonDataSerializerJsonConverter(kindMgr, versTransformMgr));

            if (serializers != null)
            {
                foreach (var ser in serializers)
                {
                    jsonSer.Converters.Add(new CustomSerializerJsonConverter(ser));
                }
            }

            jsonSer.ContractResolver = new NsJsonDataSerializerContractResolver(kindMgr, versTransformMgr);
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
