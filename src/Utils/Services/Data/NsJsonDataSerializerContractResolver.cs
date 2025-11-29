//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XToolkit.Services.Data.Converters;

namespace Xarial.XToolkit.Services.Data
{
    /// <summary>
    /// Contract resolver of <see cref="NsJsonDataSerializer"/>
    /// </summary>
    public class NsJsonDataSerializerContractResolver : DefaultContractResolver
    {
        private class VersionValueProvider : IValueProvider
        {
            private Type m_PrpType;
            private VersionTransformManager m_VersTransformsMgr;

            public VersionValueProvider(Type prpType, VersionTransformManager versTransformsMgr)
            {
                m_PrpType = prpType;
                m_VersTransformsMgr = versTransformsMgr;
            }

            public object GetValue(object target)
            {
                if (target != null)
                {
                    var type = target.GetType();

                    if (!m_VersTransformsMgr.TryGetVersionTransformInfo(type, out var latestVersion, out _))
                    {
                        if (type != m_PrpType)
                        {
                            m_VersTransformsMgr.TryGetVersionTransformInfo(m_PrpType, out latestVersion, out _);
                        }
                    }

                    if (latestVersion != null)
                    {
                        return new NsJsonDataSerializerSpecialValue(latestVersion.ToString());
                    }
                    else
                    {
                        return null;
                    }
                }
                else 
                {
                    return null;
                }
            }

            public void SetValue(object target, object value) => throw new NotImplementedException();
        }

        private class KindValueProvider : IValueProvider
        {
            private readonly KnownKindManager m_KnownKindMgr;

            internal KindValueProvider(KnownKindManager knownKindMgr)
            {
                m_KnownKindMgr = knownKindMgr;
            }

            public object GetValue(object target)
            {
                if (target != null)
                {
                    if (m_KnownKindMgr.TryGetKind(target.GetType(), out var kind))
                    {
                        return new NsJsonDataSerializerSpecialValue(kind);
                    }
                }

                return null;
            }

            public void SetValue(object target, object value) => throw new NotImplementedException();
        }

        /// <summary>
        /// Special value
        /// </summary>
        /// <remarks>This is used to avoid conflicts with other custom <see cref="IValueSerializer"/></remarks>
        private class NsJsonDataSerializerSpecialValue
        {
            internal string Value { get; }

            internal NsJsonDataSerializerSpecialValue(string val) 
            {
                Value = val;
            }
        }

        private class NsJsonDataSerializerSpecialValueConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(NsJsonDataSerializerSpecialValue);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var val = reader.Value;

                if (val != null)
                {
                    return new NsJsonDataSerializerSpecialValue(val.ToString());
                }
                else 
                {
                    return val;
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value != null)
                {
                    var specVal = (NsJsonDataSerializerSpecialValue)value;

                    writer.WriteValue(specVal.Value);
                }
            }
        }

        private readonly KnownKindManager m_KnownKindMgr;
        private readonly VersionTransformManager m_VersionTransformsMgr;

        internal NsJsonDataSerializerContractResolver(KnownKindManager knownKindMgr, VersionTransformManager versTransMgr)
        {
            m_KnownKindMgr = knownKindMgr;
            m_VersionTransformsMgr = versTransMgr;
        }

        /// <inheritdoc/>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            var versPrp = new JsonProperty
            {
                PropertyName = VersionTransformManager.VERSION_NODE_NAME,
                PropertyType = typeof(NsJsonDataSerializerSpecialValue),
                ValueProvider = new VersionValueProvider(type, m_VersionTransformsMgr),
                NullValueHandling = NullValueHandling.Ignore,
                Converter = new NsJsonDataSerializerSpecialValueConverter(),
                Readable = true,
                Writable = false,
            };

            props.Add(versPrp);

            if (m_KnownKindMgr.IsOfKind(type))
            {
                var kindPrp = new JsonProperty
                {
                    PropertyName = KnownKindManager.KIND_NODE_NAME,
                    PropertyType = typeof(NsJsonDataSerializerSpecialValue),
                    ValueProvider = new KindValueProvider(m_KnownKindMgr),
                    NullValueHandling = NullValueHandling.Ignore,
                    Converter = new NsJsonDataSerializerSpecialValueConverter(),
                    Readable = true,
                    Writable = false
                };

                props.Add(kindPrp);
            }

            return props;
        }
    }
}
