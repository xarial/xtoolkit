using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Xarial.XToolkit.Reflection
{
    [Flags]
    public enum AssemblyMatchFilter_e 
    {
        PublicKeyToken = 1,
        Culture = 2,
        Version = 4
    }

    public class LocalFolderReferencesResolver : AssemblyNameReferenceResolver
    {
        private readonly string m_SearchDir;

        private readonly AssemblyMatchFilter_e m_MatchFilter;
        
        public LocalFolderReferencesResolver(string searchDir,
            AssemblyMatchFilter_e matchFilter = AssemblyMatchFilter_e.PublicKeyToken | AssemblyMatchFilter_e.Culture)
        {
            m_MatchFilter = matchFilter;

            m_SearchDir = searchDir;
        }

        protected override AssemblyName GetReplacementAssemblyName(AssemblyName assmName, Assembly requestingAssembly,
            out string searchDir, out bool recursiveSearch)
        {
            searchDir = m_SearchDir;
            recursiveSearch = true;
            return assmName;
        }

        protected override bool Match(AssemblyName probeAssmName, AssemblyName searchAssmName)
        {
            return (probeAssmName.Name == searchAssmName.Name)
                && (!m_MatchFilter.HasFlag(AssemblyMatchFilter_e.PublicKeyToken) || GetPublicKeyToken(probeAssmName) == GetPublicKeyToken(searchAssmName))
                && (!m_MatchFilter.HasFlag(AssemblyMatchFilter_e.Culture) || probeAssmName.CultureName == probeAssmName.CultureName)
                && (!m_MatchFilter.HasFlag(AssemblyMatchFilter_e.Version) || probeAssmName.Version == searchAssmName.Version);
        }
    }
}
