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
using static Xarial.XToolkit.Services.Data.NsJsonDataSerializerContractResolver;

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

        private readonly NsJsonDataSerializerContractResolver m_ContractResolver;

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

            if (jsonSer.ContractResolver is NsJsonDataSerializerContractResolver cr)
            {
                //cr.Load(dataType, knownKinds);
                //m_ContractResolver = cr;
            }
            else 
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
                //var jToken = JToken.Load(jsonReader);
                
                //var contractResolver = (NsJsonDataSerializerContractResolver)m_JsonSer.ContractResolver;

                //var transforms = contractResolver.VersionTransformers;

                //Upgrade(jToken, DataType);

                //return jToken.ToObject(DataType, m_JsonSer);
            }
        }

        //private void Upgrade(JToken jToken, Type type)
        //{
        //    if (jToken.Type != JTokenType.Null)
        //    {
        //        if (m_ContractResolver.VersionTransformers.TryGetValue(type, out var typeVersTransf))
        //        {
        //            Version objVers;

        //            var versToken = jToken.SelectToken("$version");

        //            if (versToken == null)
        //            {
        //                versToken = jToken.SelectToken("__version");
        //            }

        //            if (versToken == null)
        //            {
        //                objVers = new Version();
        //            }
        //            else
        //            {
        //                objVers = Version.Parse(versToken.Value<string>());
        //            }

        //            if (typeVersTransf.LatestVersion > objVers)
        //            {
        //                if (typeVersTransf.Transformer?.Transforms != null)
        //                {
        //                    foreach (var tr in typeVersTransf.Transformer?.Transforms
        //                        .Where(t => t.From >= objVers && t.To <= typeVersTransf.LatestVersion)
        //                        .OrderBy(t => t.From))
        //                    {
        //                        jToken = tr.Transform(jToken);
        //                    }
        //                }

        //                if (versToken == null)
        //                {
        //                    if (jToken.Type == JTokenType.Object)
        //                    {
        //                        ((JObject)jToken).Add("$version", typeVersTransf.LatestVersion.ToString());
        //                    }
        //                }
        //                else
        //                {
        //                    ((JValue)versToken).Value = typeVersTransf.LatestVersion.ToString();
        //                }
        //            }
        //        }

        //        if (m_ContractResolver.Properties.TryGetValue(type, out var prps))
        //        {
        //            switch (jToken.Type)
        //            {
        //                case JTokenType.Object:
        //                    foreach (var jPrp in ((JObject)jToken).Properties())
        //                    {
        //                        var prp = prps.FirstOrDefault(p => p.PropertyName == jPrp.Name);

        //                        if (prp != null)
        //                        {
        //                            Upgrade(jPrp.Value, prp.PropertyType);
        //                        }
        //                    }
        //                    break;

        //                case JTokenType.Array:

        //                    var itemType = m_ContractResolver.CollectionItemTypes[type];

        //                    foreach (var item in (JArray)jToken)
        //                    {
        //                        Upgrade(item, itemType);
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //}

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

            //jsonSer.Converters.Add(new VersionJsonConverter(GetVersionTransformer));

            if (serializers != null)
            {
                foreach (var ser in serializers)
                {
                    jsonSer.Converters.Add(new CustomSerializerJsonConverter(ser));
                }
            }

            //if (knownKinds?.Any() == true)
            //{
            //    jsonSer.Converters.Add(new KnownKindJsonConverter(knownKinds));
            //}

            jsonSer.Converters.Add(new NsJsonDataSerializerJsonConverter(knownKinds, GetVersionTransformer));

            jsonSer.ContractResolver = new NsJsonDataSerializerContractResolver(knownKinds);
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
