//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xarial.XToolkit.Helpers;

namespace Xarial.XToolkit.Reflection
{
    public abstract class AssemblyNameReferenceResolver : IReferenceResolver
    {
        public class AssemblyInfo
        {
            public AssemblyName Name { get; }
            public string FilePath { get; }
            internal bool IsLoaded { get; }

            internal AssemblyInfo(AssemblyName name, string filePath, bool isLoaded)
            {
                Name = name;
                FilePath = filePath;
                IsLoaded = isLoaded;
            }
        }

        public string Name { get; }

        public AssemblyNameReferenceResolver(string name) 
        {
            if (string.IsNullOrEmpty(name))
            {
                name = this.GetType().FullName;
            }

            Name = name;
        }
        
        public virtual Assembly Resolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
        {
            var searchAssmName = GetReplacementAssemblyName(assmName, requestingAssembly, out string searchDir, out bool recursiveSearch);

            if (searchAssmName != null) 
            {
                var matchedAssmNames = new List<AssemblyInfo>();

                var replacementAssms = appDomain.GetAssemblies().Where(
                                        a => Match(a.GetName(), searchAssmName));

                var exactMatch = replacementAssms.FirstOrDefault(a => CompareAssemblyNames(a.GetName(), searchAssmName));

                if (exactMatch != null)
                {
                    return exactMatch;
                }
                else 
                {
                    matchedAssmNames.AddRange(replacementAssms.Select(a => new AssemblyInfo(a.GetName(), a.Location, true)));
                }

                foreach (var name in EnumerateAssemblyByName(searchDir, recursiveSearch, searchAssmName)) 
                {
                    if (CompareAssemblyNames(name.Name, searchAssmName))
                    {
                        return Assembly.LoadFrom(name.FilePath);
                    }
                    else 
                    {
                        matchedAssmNames.Add(name);
                    }
                }

                var assmInfo = ResolveAmbiguity(matchedAssmNames, searchAssmName);

                if (assmInfo != null) 
                {
                    if (assmInfo.IsLoaded)
                    {
                        return Assembly.Load(assmInfo.Name);
                    }
                    else 
                    {
                        return Assembly.LoadFrom(assmInfo.FilePath);
                    }
                }
            }

            return null;
        }

        protected virtual AssemblyInfo ResolveAmbiguity(
            IReadOnlyList<AssemblyInfo> assmNames, AssemblyName searchAssmName)
        {
            var assmInfo = assmNames.FirstOrDefault(a => CompareAssemblyNames(a.Name, searchAssmName));

            if (assmInfo == null)
            { 
                assmInfo = assmNames.FirstOrDefault(a => a.IsLoaded);

                if (assmInfo == null)
                {
                    assmInfo = assmNames.FirstOrDefault();
                }
            }

            return assmInfo;
        }

        private IEnumerable<AssemblyInfo> EnumerateAssemblyByName(string dir, bool recurse, AssemblyName searchAssmName)
        {
            var probeAssmFilePath = Path.Combine(dir, searchAssmName.Name + ".dll");

            if (File.Exists(probeAssmFilePath))
            {
                var probeAssmName = AssemblyName.GetAssemblyName(probeAssmFilePath);

                if (Match(probeAssmName, searchAssmName))
                {
                    yield return new AssemblyInfo(probeAssmName, probeAssmFilePath, false);
                }
            }

            if (recurse)
            {
                foreach (var subDir in Directory.EnumerateDirectories(dir, "*.*", SearchOption.TopDirectoryOnly))
                {
                    foreach (var res in EnumerateAssemblyByName(subDir, recurse, searchAssmName))
                    {
                        yield return res;
                    }
                }
            }
        }

        protected string GetPublicKeyToken(AssemblyName assmName)
        {
            var bytes = assmName.GetPublicKeyToken();

            if (bytes == null || bytes.Length == 0)
            {
                return "null";
            }

            var publicKeyToken = "";

            for (int i = 0; i < bytes.GetLength(0); i++)
            {
                publicKeyToken += string.Format("{0:x2}", bytes[i]);
            }

            return publicKeyToken;
        }

        protected string GetCulture(AssemblyName assmName)
        {
            if (string.IsNullOrEmpty(assmName.CultureName))
            {
                return "neutral";
            }
            else
            {
                return assmName.CultureName;
            }
        }

        protected abstract AssemblyName GetReplacementAssemblyName(AssemblyName assmName, Assembly requestingAssembly, 
            out string searchDir, out bool recursiveSearch);

        protected virtual bool Match(AssemblyName probeAssmName, AssemblyName searchAssmName)
            => CompareAssemblyNames(probeAssmName, searchAssmName);

        private bool CompareAssemblyNames(AssemblyName firstAssmName, AssemblyName secondAssmName)
            => string.Equals(firstAssmName.FullName, secondAssmName.FullName);
    }
}