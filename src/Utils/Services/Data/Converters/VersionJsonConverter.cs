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
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.Data.Attributes;

namespace Xarial.XToolkit.Services.Data.Converters
{
    internal class VersionJsonConverter : JsonConverter
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

        //protected Type m_SettsType;
        //protected Version m_LatestVersion;

        public override bool CanRead => true;
        public override bool CanWrite => false;

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

            return versTransInfo != null;
        }

        //private IReadOnlyList<VersionTransform> m_Transformers;

        private readonly Dictionary<Type, VersionTransformInfo1> m_VersionTransforms;

        private readonly Func<IVersionsTransformer, IVersionsTransformer> m_TransformerAdapter;

        internal VersionJsonConverter(Func<IVersionsTransformer, IVersionsTransformer> transformerAdapter)
        {
            m_TransformerAdapter = transformerAdapter;
            m_VersionTransforms = new Dictionary<Type, VersionTransformInfo1>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotSupportedException();

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.ReadFrom(reader);

            Version settsVers;

            var versToken = jToken.SelectToken(VERSION_NODE_NAME);

            if (versToken == null)
            {
                versToken = jToken.SelectToken(LEGACY_VERSION_NODE_NAME);
            }

            if (versToken == null)
            {
                settsVers = new Version();
            }
            else
            {
                settsVers = Version.Parse(versToken.Value<string>());
            }

            var versTransInfo = m_VersionTransforms[objectType];

            if (versTransInfo.LatestVersion > settsVers)
            {
                if (versTransInfo.Transformer?.Transforms != null)
                {
                    foreach (var tr in versTransInfo.Transformer?.Transforms
                        .Where(t => t.From >= settsVers && t.To <= versTransInfo.LatestVersion)
                        .OrderBy(t => t.From))
                    {
                        jToken = tr.Transform(jToken);
                    }
                }
            }

            throw new NotImplementedException();
            //return jToken.ToObject(objectType, jsonSerProv.Serializer);
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
