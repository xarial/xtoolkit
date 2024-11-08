//*********************************************************************
//xToolkit
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xarial.XToolkit.Reflection
{
    /// <summary>
    /// Match filter for <see cref="LocalFolderReferencesResolver"/>
    /// </summary>
    [Flags]
    public enum AssemblyMatchFilter_e 
    {
        /// <summary>
        /// Match by public key token
        /// </summary>
        PublicKeyToken = 1,

        /// <summary>
        /// Match by culture
        /// </summary>
        Culture = 2,

        /// <summary>
        /// Match by version
        /// </summary>
        Version = 4
    }

    /// <summary>
    /// Resolver to load referenced from the local folder
    /// </summary>
    public class LocalFolderReferencesResolver : AssemblyNameReferenceResolver
    {
        private readonly string m_SearchDir;

        private readonly AssemblyMatchFilter_e m_MatchFilter;

        private readonly string[] m_AssemblyNameFilters;

        public LocalFolderReferencesResolver(string searchDir,
            AssemblyMatchFilter_e matchFilter = AssemblyMatchFilter_e.PublicKeyToken | AssemblyMatchFilter_e.Culture,
            string name = "", string[] assemblyNameFilters = null, string[] filterDirs = null) : base(name, filterDirs)
        {
            m_MatchFilter = matchFilter;

            m_SearchDir = searchDir;

            m_AssemblyNameFilters = assemblyNameFilters;
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
            if (m_AssemblyNameFilters?.Any() != true
                || m_AssemblyNameFilters.Contains(searchAssmName.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                return (probeAssmName.Name == searchAssmName.Name)
                    && (!m_MatchFilter.HasFlag(AssemblyMatchFilter_e.PublicKeyToken) || GetPublicKeyToken(probeAssmName) == GetPublicKeyToken(searchAssmName))
                    && (!m_MatchFilter.HasFlag(AssemblyMatchFilter_e.Culture) || probeAssmName.CultureName == probeAssmName.CultureName)
                    && (!m_MatchFilter.HasFlag(AssemblyMatchFilter_e.Version) || probeAssmName.Version == searchAssmName.Version);
            }
            else 
            {
                return false;
            }
        }
    }
}
