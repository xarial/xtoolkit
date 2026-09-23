//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Concurrent;
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

        private readonly ConcurrentDictionary<Type, VersionTransformInfo> m_VersionTransforms;

        private readonly Func<Type, DataVersionAttribute, IVersionsTransformer> m_TransformerFact;

        internal VersionTransformManager(Func<Type, DataVersionAttribute, IVersionsTransformer> transformerFact)
        {
            m_TransformerFact = transformerFact;

            m_VersionTransforms = new ConcurrentDictionary<Type, VersionTransformInfo>();
        }

        internal bool TryGetVersionTransformInfo(Type objectType, out Version latestVersion, out IVersionsTransformer transformer)
        {
            var versTransInfo = m_VersionTransforms.GetOrAdd(objectType, t =>
            {
                if (t.TryGetAttribute(out DataVersionAttribute att, true))
                {
                    return new VersionTransformInfo(att.Version, m_TransformerFact.Invoke(t, att));
                }
                else
                {
                    return null;
                }
            });

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
