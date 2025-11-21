using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            JToken jToken;
            Type targetType;

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:

                    //NOTE: need to use JObject::Load to load wgole object without EndObject
                    jToken = JObject.Load(reader);

                    jToken = Upgrade(jToken, objectType);
                    
                    if (jToken is JObject)
                    {
                        if (!TryGetKindType((JObject)jToken, out targetType))
                        {
                            targetType = objectType;
                        }
                    }
                    else 
                    {
                        targetType = null;
                    }
                    break;

                default:
                    jToken = JToken.Load(reader);
                    jToken = Upgrade(jToken, objectType);
                    targetType = objectType;
                    break;
            }

            return CreateInstance(jToken, targetType, serializer);
        }

        private JToken Upgrade(JToken jToken, Type objectType)
        {
            Version version;

            if (jToken.Type == JTokenType.Object && (((JObject)jToken).TryGetValue(VersionTransformManager.VERSION_NODE_NAME, out var jVersion)
                || (((JObject)jToken).TryGetValue(VersionTransformManager.LEGACY_VERSION_NODE_NAME, out jVersion))))
            {
                version = Version.Parse(jVersion.Value<string>());
            }
            else
            {
                version = new Version();
            }

            Type kindType = null;

            if ((jToken.Type == JTokenType.Object && TryGetKindType((JObject)jToken, out kindType) && m_VersionTransformsMgr.TryGetVersionTransformInfo(kindType, out var latestVersion, out var transformer))
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
                            jToken = tr.Transform(jToken);
                        }
                    }
                }
            }

            return jToken;
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

        private object CreateInstance(JToken jToken, Type type, JsonSerializer serializer)
        {
            var inst = Activator.CreateInstance(type);
            serializer.Populate(jToken.CreateReader(), inst);
            return inst;
        }

        public override bool CanConvert(Type objectType)
        {
            return m_VersionTransformsMgr.TryGetVersionTransformInfo(objectType, out _, out _) || m_KnownKindMgr.IsOfKind(objectType);
        }
    }
}
