//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.Data.Attributes;

namespace Xarial.XToolkit.Services.Data
{
    public class NsJsonDataSerializerContractResolver : DefaultContractResolver
    {
        //internal class VersionTransformInfo 
        //{
        //    internal Version LatestVersion { get; }
        //    internal IVersionsTransformer Transformer { get; }

        //    internal VersionTransformInfo(Version latestVersion, IVersionsTransformer transformer)
        //    {
        //        LatestVersion = latestVersion;
        //        Transformer = transformer;
        //    }
        //}

        private class VersionValueProvider : IValueProvider
        {
            private readonly Version m_Version;

            internal VersionValueProvider(Version version) 
            {
                m_Version = version;
            }

            public object GetValue(object target) => m_Version.ToString();

            public void SetValue(object target, object value) => throw new NotImplementedException();
        }

        private class KindValueProvider : IValueProvider
        {
            private IReadOnlyDictionary<Type, string> m_KnownKinds;

            internal KindValueProvider(IReadOnlyDictionary<Type, string> knownKinds)
            {
                m_KnownKinds = knownKinds;
            }

            public object GetValue(object target)
            {
                if (target != null) 
                {
                    if (m_KnownKinds.TryGetValue(target.GetType(), out var kind)) 
                    {
                        return kind;
                    }
                }

                return null;
            }

            public void SetValue(object target, object value) => throw new NotImplementedException();
        }

        //internal IReadOnlyDictionary<Type, VersionTransformInfo> VersionTransformers => m_VersionTransformers;
        //internal IReadOnlyDictionary<Type, IList<JsonProperty>> Properties => m_PrpsCache;
        //internal IReadOnlyDictionary<Type, Type> CollectionItemTypes => m_CollectionItemTypes;

        //private Dictionary<Type, VersionTransformInfo> m_VersionTransformers;

        //private readonly Dictionary<Type, IList<JsonProperty>> m_PrpsCache;

        private IReadOnlyDictionary<Type, string> m_KnownKinds;
        //private readonly Dictionary<Type, Type> m_CollectionItemTypes;

        internal NsJsonDataSerializerContractResolver(IReadOnlyDictionary<Type, string> knownKinds)
        {
            m_KnownKinds = knownKinds;

            //m_VersionTransformers = new Dictionary<Type, VersionTransformInfo>();
            //m_PrpsCache = new Dictionary<Type, IList<JsonProperty>>();
            //m_CollectionItemTypes = new Dictionary<Type, Type>();
        }

        //internal void Load(Type dataType, IReadOnlyDictionary<Type, string> knownKinds) 
        //{
        //    m_KnownKinds = knownKinds;

        //    var visited = new HashSet<Type>();

        //    ResolveAllContracts(dataType, visited);

        //    if (knownKinds != null) 
        //    {
        //        foreach (var knownType in knownKinds.Keys) 
        //        {
        //            ResolveAllContracts(knownType, visited);
        //        }
        //    }
        //}

        //private void ResolveAllContracts(Type type, HashSet<Type> visited)
        //{
        //    var contract = ResolveContract(type);

        //    if (contract is JsonObjectContract oc)
        //    {
        //        foreach (var prp in oc.Properties)
        //        {
        //            var subType = prp.PropertyType;

        //            if (!visited.Contains(subType))
        //            {
        //                visited.Add(subType);

        //                ResolveAllContracts(subType, visited);
        //            }
        //        }
        //    }
        //    else if (contract is JsonArrayContract ac) 
        //    {
        //        m_CollectionItemTypes.Add(type, ac.CollectionItemType);

        //        ResolveAllContracts(ac.CollectionItemType, visited);
        //    }
        //}

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            //if (!m_PrpsCache.TryGetValue(type, out var props))
            //{
                var props = base.CreateProperties(type, memberSerialization);
                
                if (TryGetVersionInfo(type, out var vers, out var transforms))
                {
                    var versPrp = new JsonProperty
                    {
                        PropertyName = "$version",
                        PropertyType = typeof(string),
                        ValueProvider = new VersionValueProvider(vers),
                        Readable = true,
                        Writable = false
                    };

                    props.Add(versPrp);

                    //m_VersionTransformers.Add(type, new VersionTransformInfo(vers, transforms));
                }

                if (m_KnownKinds.Keys.Any(t => type.IsAssignableFrom(t)))
                {
                    var kindPrp = new JsonProperty
                    {
                        PropertyName = "$kind",
                        PropertyType = typeof(string),
                        ValueProvider = new KindValueProvider(m_KnownKinds),
                        Readable = true,
                        Writable = false
                    };

                    props.Add(kindPrp);
                }

            //    m_PrpsCache.Add(type, props);
            //}

            return props;
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
