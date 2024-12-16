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
    /// Resolver to load referenced from the local folder
    /// </summary>
    public class LocalFolderReferencesResolver : AssemblyNameReferenceResolver
    {
        private readonly string m_SearchDir;

        private readonly AssemblyNamePart_e m_MatchFilter;

        private readonly string[] m_AssemblyNameFilters;

        public LocalFolderReferencesResolver(string searchDir)
            : this(searchDir, AssemblyNamePart_e.PublicKeyToken | AssemblyNamePart_e.Culture | AssemblyNamePart_e.Version,
                  "", null, new string[] { searchDir })
        {
        }

        public LocalFolderReferencesResolver(string searchDir,
            AssemblyNamePart_e matchFilter) 
            : this(searchDir, matchFilter, "", null, new string[] { searchDir })
        {
        }

        public LocalFolderReferencesResolver(string searchDir,
            AssemblyNamePart_e matchFilter, string name) 
            : this(searchDir, matchFilter, name, null, new string[] { searchDir })
        {
        }

        public LocalFolderReferencesResolver(string searchDir,
            AssemblyNamePart_e matchFilter, string name, string[] assemblyNameFilters) 
            : this(searchDir, matchFilter, name, assemblyNameFilters, new string[] { searchDir })
        {
        }

        public LocalFolderReferencesResolver(string searchDir,
            AssemblyNamePart_e matchFilter,
            string name, string[] assemblyNameFilters, string[] filterDirs) : base(name, filterDirs)
        {
            m_MatchFilter = matchFilter;

            m_SearchDir = searchDir;

            m_AssemblyNameFilters = assemblyNameFilters;
        }

        /// <inheritdoc/>
        protected override AssemblyName GetReplacementAssemblyName(AssemblyName assmName, Assembly requestingAssembly,
            out string searchDir, out bool recursiveSearch)
        {
            searchDir = m_SearchDir;
            recursiveSearch = true;
            return assmName;
        }

        /// <inheritdoc/>
        protected override bool Match(AssemblyName probeAssmName, AssemblyName searchAssmName, Assembly requestingAssembly)
        {
            if (m_AssemblyNameFilters?.Any() != true
                || m_AssemblyNameFilters.Contains(searchAssmName.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                return CompareAssemblyNames(probeAssmName, searchAssmName, m_MatchFilter);
            }
            else 
            {
                return false;
            }
        }
    }
}
