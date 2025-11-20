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

            var dupKnownKinds = knownKinds.Values.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();

            if (dupKnownKinds.Any()) 
            {
                throw new Exception($"Duplicate know kinds {string.Join(", ", dupKnownKinds)}");
            }

            m_KindToKnownTypeMap = knownKinds?.ToDictionary(x => x.Value, x => x.Key);
        }

        public override bool CanWrite => false;

        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotImplementedException();

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
                            return CreateInstance(jObj, type, serializer);
                        }
                        else
                        {
                            throw new Exception($"Unknown kind '{kind}' for {objectType?.FullName}");
                        }
                    }
                    else
                    {
                        return CreateInstance(jObj, objectType, serializer);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private object CreateInstance(JObject jObj, Type type, JsonSerializer serializer)
        {
            var inst = Activator.CreateInstance(type);
            serializer.Populate(jObj.CreateReader(), inst);
            return inst;
        }

        public override bool CanConvert(Type objectType) => m_KnownTypeToKindMap.Keys.Any(t => objectType.IsAssignableFrom(t));
    }
}
