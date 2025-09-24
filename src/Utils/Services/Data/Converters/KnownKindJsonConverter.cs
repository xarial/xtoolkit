//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xarial.XToolkit.Services.Data.Converters
{
    internal class KnownKindJsonConverter : JsonConverter
    {
        private const string KIND_NODE_NAME = "$kind";

        private readonly IReadOnlyDictionary<Type, string> m_KnownTypeToKindMap;
        private readonly IReadOnlyDictionary<string, Type> m_KindToKnownTypeMap;

        internal KnownKindJsonConverter(IReadOnlyDictionary<Type, string> knownKinds) 
        {
            m_KnownTypeToKindMap = knownKinds;
            m_KindToKnownTypeMap = knownKinds?.ToDictionary(x => x.Value, x => x.Key);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var type = value?.GetType();

            if (m_KnownTypeToKindMap.TryGetValue(type, out var kind))
            {
                using (var jsonSerProv = new JsonSerializerExcludeConverterProvider(serializer, this))
                {
                    var jObject = JObject.FromObject(value, jsonSerProv.Serializer);

                    jObject.Add(KIND_NODE_NAME, kind);

                    jObject.WriteTo(writer);
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType) 
            {
                case JsonToken.Null:
                    return null;

                case JsonToken.StartObject:
                    var jObj = JObject.Load(reader);

                    if (jObj.TryGetValue(KIND_NODE_NAME, out var jKind))
                    {
                        var kind = jKind.Value<string>();

                        if (m_KindToKnownTypeMap.TryGetValue(kind, out var type))
                        {
                            return jObj.ToObject(type);
                        }
                        else
                        {
                            throw new Exception($"Unknown kind '{kind}' for {objectType?.FullName}");
                        }
                    }
                    else
                    {
                        throw new Exception($"Kind is not found for {objectType?.FullName}");
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        public override bool CanConvert(Type objectType) => m_KnownTypeToKindMap.Keys.Any(t => objectType.IsAssignableFrom(t));
    }
}
