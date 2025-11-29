using System;
using System.Collections.Generic;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.Data.Attributes;

namespace Xarial.XToolkit.Services.Data
{
    internal class VersionTransformManager 
    {
        internal const string VERSION_NODE_NAME = "$version";
        internal const string LEGACY_VERSION_NODE_NAME = "__version";

        private class VersionTransformInfo
        {
            
            internal Version LatestVersion { get; }
            internal IVersionsTransformer Transformer { get; }

            internal VersionTransformInfo(Version latestVersion, IVersionsTransformer transformer)
            {
                LatestVersion = latestVersion;
                Transformer = transformer;
            }
        }

        private readonly Dictionary<Type, VersionTransformInfo> m_VersionTransforms;

        private readonly Func<IVersionsTransformer, IVersionsTransformer> m_TransformerAdapter;

        internal VersionTransformManager(Func<IVersionsTransformer, IVersionsTransformer> transformerAdapter) 
        {
            m_TransformerAdapter = transformerAdapter;

            m_VersionTransforms = new Dictionary<Type, VersionTransformInfo>();
        }

        internal bool TryGetVersionTransformInfo(Type objectType, out Version latestVersion, out IVersionsTransformer transformer)
        {
            if (!m_VersionTransforms.TryGetValue(objectType, out var versTransInfo))
            {
                if (objectType.TryGetAttribute(out DataVersionAttribute att, true))
                {
                    transformer = m_TransformerAdapter.Invoke(att.VersionTransformer);
                    versTransInfo = new VersionTransformInfo(att.Version, transformer);
                }
                else
                {
                    versTransInfo = null;
                }

                m_VersionTransforms.Add(objectType, versTransInfo);
            }

            if (versTransInfo != null)
            {
                latestVersion = versTransInfo.LatestVersion;
                transformer = versTransInfo.Transformer;
                return true;
            }
            else
            {
                latestVersion = null;
                transformer = null;
                return false;
            }

        }
    }
}
