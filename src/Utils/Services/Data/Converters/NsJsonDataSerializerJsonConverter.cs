//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

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
        internal delegate bool TryGetVersionTransformInfoDelegate(JToken jToken, Type objectType, object existingValue, out Version latestVersion, out IVersionsTransformer transformer);

        private readonly KnownKindManager m_KnownKindMgr;

        private readonly TryGetVersionTransformInfoDelegate m_TryGetVersionTransformInfoFunc;

        private readonly Func<Type, bool> m_SupportsVersioningFunc;

        private readonly Func<JToken, Type, object, JsonSerializer, object> m_CreateInstanceFunc;

        internal NsJsonDataSerializerJsonConverter(KnownKindManager knownKindMgr,
            TryGetVersionTransformInfoDelegate tryGetVersionTransformInfoFunc, Func<Type, bool> supportsVersioningFunc,
            Func<JToken, Type, object, JsonSerializer, object> createInstanceFunc)
        {
            m_KnownKindMgr = knownKindMgr;

            m_TryGetVersionTransformInfoFunc = tryGetVersionTransformInfoFunc;
            m_SupportsVersioningFunc = supportsVersioningFunc;
            m_CreateInstanceFunc = createInstanceFunc;
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

                    //NOTE: need to use JObject::Load to load whole object without EndObject
                    jToken = JObject.Load(reader);

                    jToken = Upgrade(jToken, objectType, existingValue, serializer);
                    
                    if (jToken is JObject)
                    {
                        if (!m_KnownKindMgr.TryGetKindType((JObject)jToken, out targetType))
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
                    jToken = Upgrade(jToken, objectType, existingValue, serializer);
                    targetType = objectType;
                    break;
            }

            foreach (var conv in serializer.Converters) 
            {
                if (conv != this) 
                {
                    if (conv.CanConvert(objectType)) 
                    {
                        return conv.ReadJson(jToken.CreateReader(), objectType, existingValue, serializer);
                    }
                }
            }

            return m_CreateInstanceFunc.Invoke(jToken, targetType, existingValue, serializer);
        }

        private JToken Upgrade(JToken jToken, Type objectType, object existingValue, JsonSerializer serializer)
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

            if (m_TryGetVersionTransformInfoFunc.Invoke(jToken, objectType, existingValue, out var latestVersion, out var transformer))
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

        public override bool CanConvert(Type objectType)
            => m_SupportsVersioningFunc.Invoke(objectType) || m_KnownKindMgr.IsOfKind(objectType);
    }
}
