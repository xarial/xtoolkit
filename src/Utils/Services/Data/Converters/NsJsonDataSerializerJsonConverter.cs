using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.Data.Attributes;

namespace Xarial.XToolkit.Services.Data.Converters
{
    internal class NsJsonDataSerializerJsonConverter : JsonConverter
    {
        private class VersionTransformInfo1
        {
            internal Version LatestVersion { get; }
            internal IVersionsTransformer Transformer { get; }

            internal VersionTransformInfo1(Version latestVersion, IVersionsTransformer transformer)
            {
                LatestVersion = latestVersion;
                Transformer = transformer;
            }
        }

        protected string VERSION_NODE_NAME = "$version";
        protected string LEGACY_VERSION_NODE_NAME = "__version";
        private const string KIND_NODE_NAME = "$kind";

        private readonly IReadOnlyDictionary<Type, string> m_KnownTypeToKindMap;
        private readonly IReadOnlyDictionary<string, Type> m_KindToKnownTypeMap;

        private readonly Dictionary<Type, VersionTransformInfo1> m_VersionTransforms;

        private readonly Func<IVersionsTransformer, IVersionsTransformer> m_TransformerAdapter;

        internal NsJsonDataSerializerJsonConverter(IReadOnlyDictionary<Type, string> knownKinds, Func<IVersionsTransformer, IVersionsTransformer> transformerAdapter)
        {
            if (knownKinds == null) 
            {
                knownKinds = new Dictionary<Type, string>();
            }

            m_KnownTypeToKindMap = knownKinds;

            var dupKnownKinds = knownKinds.Values.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();

            if (dupKnownKinds.Any())
            {
                throw new Exception($"Duplicate know kinds {string.Join(", ", dupKnownKinds)}");
            }

            m_KindToKnownTypeMap = knownKinds?.ToDictionary(x => x.Value, x => x.Key);

            m_TransformerAdapter = transformerAdapter;
            m_VersionTransforms = new Dictionary<Type, VersionTransformInfo1>();
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

                    Version version;

                    if (jObj.TryGetValue(VERSION_NODE_NAME, out var jVersion)
                        || jObj.TryGetValue(LEGACY_VERSION_NODE_NAME, out jVersion))
                    {
                        version = Version.Parse(jVersion.Value<string>());
                    }
                    else
                    {
                        version = new Version();
                    }

                    if (!m_VersionTransforms.TryGetValue(objectType, out var versTransInfo))
                    {
                        if (TryGetVersionInfo(objectType, out var vers, out var transformer))
                        {
                            transformer = m_TransformerAdapter.Invoke(transformer);
                            versTransInfo = new VersionTransformInfo1(vers, transformer);
                        }
                        else
                        {
                            versTransInfo = null;
                        }

                        m_VersionTransforms.Add(objectType, versTransInfo);
                    }

                    if (versTransInfo.LatestVersion > version)
                    {
                        if (versTransInfo.Transformer?.Transforms != null)
                        {
                            foreach (var tr in versTransInfo.Transformer?.Transforms
                                .Where(t => t.From >= version && t.To <= versTransInfo.LatestVersion)
                                .OrderBy(t => t.From))
                            {
                                jObj = (JObject)tr.Transform(jObj);
                            }
                        }
                    }

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

        public override bool CanConvert(Type objectType)
        {
            if (!m_VersionTransforms.TryGetValue(objectType, out var versTransInfo))
            {
                if (TryGetVersionInfo(objectType, out var vers, out var transformer))
                {
                    transformer = m_TransformerAdapter.Invoke(transformer);
                    versTransInfo = new VersionTransformInfo1(vers, transformer);
                }
                else
                {
                    versTransInfo = null;
                }

                m_VersionTransforms.Add(objectType, versTransInfo);
            }

            return versTransInfo != null || m_KnownTypeToKindMap.Keys.Any(t => objectType.IsAssignableFrom(t));
        }

        private bool TryGetVersionInfo(Type type, out Version vers, out IVersionsTransformer transforms)
        {
            if (type.TryGetAttribute(out DataVersionAttribute att, true))
            {
                vers = att.Version;
                transforms = att.VersionTransformer;
                return true;
            }
            else
            {
                vers = null;
                transforms = null;
                return false;
            }
        }
    }
}
