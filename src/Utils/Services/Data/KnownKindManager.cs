using System;
using System.Collections.Generic;
using System.Linq;

namespace Xarial.XToolkit.Services.Data
{
    internal class KnownKindManager
    {
        internal const string KIND_NODE_NAME = "$kind";

        private readonly IReadOnlyDictionary<Type, string> m_KnownTypeToKindMap;
        private readonly IReadOnlyDictionary<string, Type> m_KindToKnownTypeMap;

        internal KnownKindManager(IReadOnlyDictionary<Type, string> knownKinds)
        {
            if (knownKinds == null)
            {
                knownKinds = new Dictionary<Type, string>();
            }

            var dupKnownKinds = knownKinds.Values.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();

            if (dupKnownKinds.Any())
            {
                throw new Exception($"Duplicate know kinds {string.Join(", ", dupKnownKinds)}");
            }

            m_KnownTypeToKindMap = knownKinds;
            m_KindToKnownTypeMap = knownKinds?.ToDictionary(x => x.Value, x => x.Key);
        }

        internal bool IsOfKind(Type type) => m_KnownTypeToKindMap.Keys.Any(t => type.IsAssignableFrom(t));

        internal bool TryGetType(string kind, out Type type) => m_KindToKnownTypeMap.TryGetValue(kind, out type);

        internal bool TryGetKind(Type type, out string kind) => m_KnownTypeToKindMap.TryGetValue(type, out kind);
    }
}
