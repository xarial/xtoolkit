using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Xarial.XToolkit.Reflection
{
    public class LocalFolderReferencesResolver : AssemblyNameReferenceResolver
    {
        private readonly string m_SearchDir;

        public LocalFolderReferencesResolver(string searchDir)
        {
            m_SearchDir = searchDir;
        }

        protected override AssemblyName GetReplacementAssemblyName(AssemblyName assmName, Assembly requestingAssembly,
            out string searchDir, out bool recursiveSearch)
        {
            searchDir = m_SearchDir;
            recursiveSearch = true;
            return assmName;
        }
    }
}
