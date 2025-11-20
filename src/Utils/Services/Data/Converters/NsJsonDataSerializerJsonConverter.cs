using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.Data.Attributes;

namespace Xarial.XToolkit.Services.Data.Converters
{
    internal class NsJsonDataSerializerJsonConverter : JsonConverter
    {
        private readonly KnownKindManager m_KnownKindMgr;
        private readonly VersionTransformManager m_VersionTransformsMgr;

        internal NsJsonDataSerializerJsonConverter(KnownKindManager knownKindMgr, VersionTransformManager versTransMgr)
        {
            m_KnownKindMgr = knownKindMgr;

            m_VersionTransformsMgr = versTransMgr;
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

                    jObj = Upgrade(jObj, objectType);

                    if (!TryGetKindType(jObj, out var type))
                    {
                        type = objectType;
                    }

                    return CreateInstance(jObj, type, serializer);

                default:
                    throw new NotSupportedException();
            }
        }

        private JObject Upgrade(JObject jObj, Type objectType)
        {
            Version version;

            if (jObj.TryGetValue(VersionTransformManager.VERSION_NODE_NAME, out var jVersion)
                || jObj.TryGetValue(VersionTransformManager.LEGACY_VERSION_NODE_NAME, out jVersion))
            {
                version = Version.Parse(jVersion.Value<string>());
            }
            else
            {
                version = new Version();
            }

            if ((TryGetKindType(jObj, out var kindType) && m_VersionTransformsMgr.TryGetVersionTransformInfo(kindType, out var latestVersion, out var transformer)) 
                || (kindType != objectType && m_VersionTransformsMgr.TryGetVersionTransformInfo(objectType, out latestVersion, out transformer)))
            {
                if (latestVersion > version)
                {
                    if (transformer?.Transforms != null)
                    {
                        foreach (var tr in transformer?.Transforms
                            .Where(t => t.From >= version && t.To <= latestVersion)
                            .OrderBy(t => t.From))
                        {
                            jObj = (JObject)tr.Transform(jObj);
                        }
                    }
                }
            }

            return jObj;
        }

        private bool TryGetKindType(JObject jObj, out Type type) 
        {
            if (jObj.TryGetValue(KnownKindManager.KIND_NODE_NAME, out var jKind) && (jKind.Type != JTokenType.Null))
            {
                var kind = jKind.Value<string>();

                if (!string.IsNullOrEmpty(kind))
                {
                    if (m_KnownKindMgr.TryGetType(kind, out type))
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception($"Unknown kind '{kind}'");
                    }
                }
            }

            type = null;
            return false;
        }

        private object CreateInstance(JObject jObj, Type type, JsonSerializer serializer)
        {
            var inst = Activator.CreateInstance(type);
            serializer.Populate(jObj.CreateReader(), inst);
            return inst;
        }

        public override bool CanConvert(Type objectType)
        {
            return m_VersionTransformsMgr.TryGetVersionTransformInfo(objectType, out _, out _) || m_KnownKindMgr.IsOfKind(objectType);
        }
    }
}
