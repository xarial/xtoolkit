//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.Data.Attributes;
using Xarial.XToolkit.Services.Data.Converters;

namespace Xarial.XToolkit.Services.Data
{
    /// <summary>
    /// Contract resolver of <see cref="NsJsonDataSerializer"/>
    /// </summary>
    public class NsJsonDataSerializerContractResolver : DefaultContractResolver
    {
        internal static bool TryGetDataCollection(Type objectType, out ExpandCollectionAttribute att, out Type itemsType)
        {
            itemsType = null;

            if (objectType.TryGetAttribute(out att, true))
            {
                var dictType = objectType.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                if (dictType != null)
                {
                    var genArgs = dictType.GetGenericArguments();

                    itemsType = typeof(Dictionary<,>).MakeGenericType(genArgs[0], genArgs[1]);

                    return true;
                }
                else
                {
                    var listType = objectType.GetInterfaces()
                        .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));

                    if (listType != null)
                    {
                        itemsType = typeof(List<>).MakeGenericType(listType.GetGenericArguments()[0]);

                        return true;
                    }
                    else
                    {
                        throw new JsonSerializationException(
                            $"'{objectType.FullName}' is marked with '{nameof(ExpandCollectionAttribute)}' but implements neither '{typeof(IDictionary<,>).Name}' nor '{typeof(IList<>).Name}'");
                    }
                }
            }
            else
            {
                return false;
            }
        }

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

        private class CollectionItemsValueProvider : IValueProvider
        {
            private readonly Type m_ItemsType;

            internal CollectionItemsValueProvider(Type itemsType)
            {
                m_ItemsType = itemsType;
            }

            public object GetValue(object target)
            {
                if (target != null)
                {
                    var items = Activator.CreateInstance(m_ItemsType);

                    Copy(target, items);

                    return items;
                }
                else
                {
                    return null;
                }
            }

            public void SetValue(object target, object value)
            {
                if (target != null && value != null && !ReferenceEquals(target, value))
                {
                    Copy(value, target);
                }
            }

            private void Copy(object source, object dest)
            {
                if (dest is IDictionary)
                {
                    foreach (DictionaryEntry entry in (IDictionary)source)
                    {
                        ((IDictionary)dest)[entry.Key] = entry.Value;
                    }
                }
                else
                {
                    foreach (var item in (IEnumerable)source)
                    {
                        ((IList)dest).Add(item);
                    }
                }
            }
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

        private readonly Action<Type, IList<JsonProperty>> m_ResolvePrpsFunc;

        internal NsJsonDataSerializerContractResolver(KnownKindManager knownKindMgr, VersionTransformManager versTransMgr, Action<Type, IList<JsonProperty>> resolvePrpsFunc)
        {
            m_KnownKindMgr = knownKindMgr;
            m_VersionTransformsMgr = versTransMgr;

            m_ResolvePrpsFunc = resolvePrpsFunc;
        }

        /// <inheritdoc/>
        protected override JsonContract CreateContract(Type objectType)
        {
            if (TryGetDataCollection(objectType, out _, out _))
            {
                return CreateObjectContract(objectType);
            }
            else
            {
                return base.CreateContract(objectType);
            }
        }

        /// <inheritdoc/>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            m_ResolvePrpsFunc?.Invoke(type, props);

            if (TryGetDataCollection(type, out var dataCollAtt, out var itemsType))
            {
                var collBaseType = GetCollectionBaseType(type);

                foreach (var collPrp in props.Where(p => IsCollectionMember(p, collBaseType)).ToArray())
                {
                    props.Remove(collPrp);
                }

                props.Add(new JsonProperty()
                {
                    PropertyName = dataCollAtt.ItemsNodeName,
                    PropertyType = itemsType,
                    DeclaringType = type,
                    ValueProvider = new CollectionItemsValueProvider(itemsType),
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    Readable = true,
                    Writable = true
                });
            }

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

        private Type GetCollectionBaseType(Type objectType)
        {
            Type collBaseType = null;

            for (var baseType = objectType; baseType != null && baseType != typeof(object); baseType = baseType.BaseType)
            {
                if (IsCollectionType(baseType))
                {
                    collBaseType = baseType;
                }
            }

            return collBaseType;
        }

        private bool IsCollectionType(Type objectType)
            => objectType.GetInterfaces().Any(t => t.IsGenericType
                && (t.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    || t.GetGenericTypeDefinition() == typeof(IList<>)));

        private bool IsCollectionMember(JsonProperty prp, Type collBaseType)
            => prp.DeclaringType == null || prp.DeclaringType.IsAssignableFrom(collBaseType);
    }
}
