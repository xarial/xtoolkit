//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xarial.XToolkit.Services.Data.Converters
{
    internal class ValueSerializerDictionaryJsonConverter : JsonConverter
    {
        private readonly Func<Type, JsonConverter> m_KeyConverterProvider;

        internal ValueSerializerDictionaryJsonConverter(Func<Type, JsonConverter> keyConverterProvider)
        {
            m_KeyConverterProvider = keyConverterProvider;
        }

        public override bool CanConvert(Type objectType)
            => !NsJsonDataSerializerContractResolver.TryGetDataCollection(objectType, out _, out _)
                && TryGetDictionaryTypes(objectType, out var keyType, out _)
                && m_KeyConverterProvider.Invoke(keyType) != null;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                TryGetDictionaryTypes(value.GetType(), out var keyType, out _);

                var keyConv = m_KeyConverterProvider.Invoke(keyType);

                writer.WriteStartObject();

                foreach (var entry in EnumerateEntries(value))
                {
                    writer.WritePropertyName(WriteKey(entry.Key, keyType, keyConv, serializer));

                    serializer.Serialize(writer, entry.Value);
                }

                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Null)
            {
                TryGetDictionaryTypes(objectType, out var keyType, out var valueType);

                var keyConv = m_KeyConverterProvider.Invoke(keyType);

                var dict = (IDictionary)(existingValue ?? CreateDictionary(objectType, keyType, valueType));

                var jObject = JObject.Load(reader);

                foreach (var jPrp in jObject.Properties())
                {
                    var key = ReadKey(jPrp.Name, keyType, keyConv, serializer);

                    using (var valueReader = jPrp.Value.CreateReader())
                    {
                        dict[key] = serializer.Deserialize(valueReader, valueType);
                    }
                }

                return dict;
            }
            else
            {
                return null;
            }
        }

        private string WriteKey(object key, Type keyType, JsonConverter keyConv, JsonSerializer serializer)
        {
            if (key != null)
            {
                var jTokenWriter = new JTokenWriter();

                keyConv.WriteJson(jTokenWriter, key, serializer);

                var jToken = jTokenWriter.Token;

                if (jToken is JValue)
                {
                    var keyValue = ((JValue)jToken).Value;

                    if (keyValue != null)
                    {
                        return keyValue.ToString();
                    }
                    else
                    {
                        throw new JsonSerializationException(
                            $"Dictionary key of type '{keyType.FullName}' is serialized to a null value");
                    }
                }
                else
                {
                    throw new JsonSerializationException(
                        $"Dictionary key of type '{keyType.FullName}' must be serialized to a primitive value");
                }
            }
            else
            {
                throw new JsonSerializationException($"Dictionary key of type '{keyType.FullName}' is null");
            }
        }

        private object ReadKey(string name, Type keyType, JsonConverter keyConv, JsonSerializer serializer)
        {
            using (var keyReader = new JValue(name).CreateReader())
            {
                keyReader.Read();

                var key = keyConv.ReadJson(keyReader, keyType, null, serializer);

                if (key != null)
                {
                    return key;
                }
                else
                {
                    throw new JsonSerializationException(
                        $"'{name}' is not resolved to the dictionary key of type '{keyType.FullName}'");
                }
            }
        }

        private static IEnumerable<DictionaryEntry> EnumerateEntries(object dict)
        {
            if (dict is IDictionary)
            {
                foreach (DictionaryEntry entry in (IDictionary)dict)
                {
                    yield return entry;
                }
            }
            else
            {
                PropertyInfo keyPrp = null;
                PropertyInfo valuePrp = null;

                foreach (var item in (IEnumerable)dict)
                {
                    if (keyPrp == null)
                    {
                        keyPrp = item.GetType().GetProperty(nameof(DictionaryEntry.Key));
                        valuePrp = item.GetType().GetProperty(nameof(DictionaryEntry.Value));
                    }

                    yield return new DictionaryEntry(keyPrp.GetValue(item), valuePrp.GetValue(item));
                }
            }
        }

        private object CreateDictionary(Type objectType, Type keyType, Type valueType)
        {
            if (!objectType.IsInterface && !objectType.IsAbstract)
            {
                return Activator.CreateInstance(objectType);
            }
            else
            {
                return Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
            }
        }

        private static bool TryGetDictionaryTypes(Type objectType, out Type keyType, out Type valueType)
        {
            keyType = null;
            valueType = null;

            if (typeof(IEnumerable).IsAssignableFrom(objectType) && objectType != typeof(string))
            {
                var dictType = objectType.GetInterfaces().Concat(new Type[] { objectType })
                    .FirstOrDefault(t => t.IsGenericType
                        && (t.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                            || t.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)));

                if (dictType != null)
                {
                    var genArgs = dictType.GetGenericArguments();

                    keyType = genArgs[0];
                    valueType = genArgs[1];

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
