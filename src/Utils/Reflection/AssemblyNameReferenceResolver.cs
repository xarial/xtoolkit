//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Xarial.XToolkit.Reflection
{
    public abstract class AssemblyNameReferenceResolver : IReferenceResolver
    {
        public virtual Assembly Resolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
        {
            var searchAssmName = GetReplacementAssemblyName(assmName, requestingAssembly, out string searchDir, out bool recursiveSearch);

            if (searchAssmName != null) 
            {
                var replacementAssm = appDomain.GetAssemblies().FirstOrDefault(
                                        a => string.Equals(a.GetName().FullName, searchAssmName.FullName));

                if (replacementAssm == null)
                {
                    if (TryFindAssemblyByName(searchDir, recursiveSearch, searchAssmName, out string probeAssmFile))
                    {
                        replacementAssm = Assembly.LoadFrom(probeAssmFile);
                    }
                }

                if (replacementAssm != null)
                {
                    return replacementAssm;
                }
            }

            return null;
        }

        private bool TryFindAssemblyByName(string dir, bool recurse, AssemblyName searchAssmName, out string filePath)
        {
            var probeAssmFilePath = Path.Combine(dir, searchAssmName.Name + ".dll");

            if (File.Exists(probeAssmFilePath))
            {
                var probeAssmName = AssemblyName.GetAssemblyName(probeAssmFilePath);

                if (string.Equals(probeAssmName.FullName, searchAssmName.FullName))
                {
                    filePath = probeAssmFilePath;
                    return true;
                }
            }

            if (recurse)
            {
                foreach (var subDir in Directory.EnumerateDirectories(dir, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if (TryFindAssemblyByName(subDir, recurse, searchAssmName, out filePath))
                    {
                        return true;
                    }
                }
            }

            filePath = "";
            return false;
        }

        protected abstract AssemblyName GetReplacementAssemblyName(AssemblyName assmName, Assembly requestingAssembly, 
            out string searchDir, out bool recursiveSearch);
    }
}