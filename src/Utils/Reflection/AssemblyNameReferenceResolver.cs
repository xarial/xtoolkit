//*********************************************************************
//xToolkit
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xarial.XToolkit.Helpers;

namespace Xarial.XToolkit.Reflection
{
    /// <summary>
    /// Assembly name match filter
    /// </summary>
    [Flags]
    public enum AssemblyNamePart_e
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
        Version = 4,

        /// <summary>
        /// Full name
        /// </summary>
        FullName = PublicKeyToken | Culture | Version
    }

    /// <summary>
    /// Assembly information
    /// </summary>
    public class AssemblyInfo
    {
        /// <summary>
        /// Loads assembly info from file
        /// </summary>
        /// <param name="filePath">Path to assembly file</param>
        /// <returns>Assembly info</returns>
        public static AssemblyInfo FromFile(string filePath)
            => new AssemblyInfo(AssemblyName.GetAssemblyName(filePath), filePath, false);

        /// <summary>
        /// File path to the assembly
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Assembly name
        /// </summary>
        public AssemblyName Name { get; }

        internal bool IsLoaded { get; }

        internal AssemblyInfo(AssemblyName name, string filePath, bool isLoaded)
        {
            Name = name;
            FilePath = filePath;
            IsLoaded = isLoaded;
        }
    }

    /// <summary>
    /// Filter for the requesting assembly
    /// </summary>
    public class RequestingAssemblyFilter 
    {
        /// <summary>
        /// Creates instance of the assembly filter
        /// </summary>
        /// <param name="assmName">Name of the assembly</param>
        /// <param name="matchFilter">Match filter</param>
        /// <returns>Requesting assembly filter</returns>
        public static RequestingAssemblyFilter Create(string assmName, AssemblyNamePart_e matchFilter = AssemblyNamePart_e.FullName)
            => new RequestingAssemblyFilter(new AssemblyName(assmName), matchFilter);

        /// <summary>
        /// Name of the assembly
        /// </summary>
        public AssemblyName Name { get; }

        /// <summary>
        /// Filter
        /// </summary>
        public AssemblyNamePart_e MatchFilter { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the requesting assembly</param>
        /// <param name="matchFilter">Match filter</param>
        public RequestingAssemblyFilter(AssemblyName name, AssemblyNamePart_e matchFilter)
        {
            Name = name;
            MatchFilter = matchFilter;
        }
    }

    /// <summary>
    /// Default assembly name resolver
    /// </summary>
    public abstract class AssemblyNameReferenceResolver : IReferenceResolver
    {
        /// <summary>
        /// Name of the resolver
        /// </summary>
        /// <remarks>Used in the logs</remarks>
        public string Name { get; }

        private readonly string[] m_FilterDirs;

        private readonly RequestingAssemblyFilter[] m_RequestingAssmsFilter;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Name of the resolver</param>
        /// <param name="filterDirs">Only resolve the assembly if requesting assembly is in the specified directories</param>
        /// <param name="requestingAssmsFilter">Only resolve assemblies which match the requesting filter</param>
        public AssemblyNameReferenceResolver(string name, string[] filterDirs, RequestingAssemblyFilter[] requestingAssmsFilter)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = this.GetType().FullName;
            }

            Name = name;

            m_FilterDirs = filterDirs;
            m_RequestingAssmsFilter = requestingAssmsFilter;
        }
        
        /// <inheritdoc/>>
        public virtual Assembly Resolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
        {
            if (ShouldResolve(appDomain, assmName, requestingAssembly))
            {
                var searchAssmName = GetReplacementAssemblyName(assmName, requestingAssembly, out string searchDir, out bool recursiveSearch);

                if (searchAssmName != null)
                {
                    var matchedAssmNames = new List<AssemblyInfo>();

                    var replacementAssms = appDomain.GetAssemblies().Where(
                                            a => Match(a.GetName(), searchAssmName, requestingAssembly));

                    var exactMatch = replacementAssms.FirstOrDefault(a => CompareAssemblyNames(a.GetName(), searchAssmName));

                    if (exactMatch != null)
                    {
                        Trace.WriteLine($"Assembly '{searchAssmName}' is resolved to '{exactMatch.Location}' as exact match via '{Name}' resolver", Name);

                        return exactMatch;
                    }
                    else
                    {
                        matchedAssmNames.AddRange(replacementAssms.Select(a => new AssemblyInfo(a.GetName(), a.Location, true)));
                    }

                    foreach (var name in EnumerateAssemblyByName(searchDir, recursiveSearch, searchAssmName, requestingAssembly))
                    {
                        if (CompareAssemblyNames(name.Name, searchAssmName))
                        {
                            Trace.WriteLine($"Loading '{searchAssmName}' from '{name.FilePath}' as exact match via '{Name}' resolver", Name);

                            return LoadAssembly(AssemblyInfo.FromFile(name.FilePath));
                        }
                        else
                        {
                            matchedAssmNames.Add(name);
                        }
                    }

                    var assmInfo = ResolveAmbiguity(matchedAssmNames, searchAssmName);

                    if (assmInfo != null)
                    {
                        return LoadAssembly(assmInfo);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if assembly should be resolved by this resolver
        /// </summary>
        /// <param name="appDomain">App Domain</param>
        /// <param name="assmName">Assembly to resolve</param>
        /// <param name="requestingAssembly">Requesting assembly</param>
        /// <returns></returns>
        protected virtual bool ShouldResolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly) 
        {
            if (requestingAssembly != null)
            {
                var reqAssmName = requestingAssembly.GetName();
                var reqAssmFilePath = requestingAssembly.Location;

                return EmptyOrAny(m_RequestingAssmsFilter, a => CompareAssemblyNames(reqAssmName, a.Name, a.MatchFilter))
                    || EmptyOrAny(m_FilterDirs, f => FileSystemUtils.IsInDirectory(reqAssmFilePath, f));
            }
            else
            {
                return true;
            }
        }

        private bool EmptyOrAny<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) 
        {
            if (source?.Any() == true)
            {
                return source.Any(predicate);
            }
            else 
            {
                return true;
            }
        }

        /// <summary>
        /// Load the assembly
        /// </summary>
        /// <param name="assmInfo">Assembly information</param>
        /// <returns>Loaded assembly</returns>
        protected Assembly LoadAssembly(AssemblyInfo assmInfo) 
        {
            Trace.WriteLine($"Loading '{assmInfo.Name}' from file '{assmInfo.FilePath}' [Loaded={assmInfo.IsLoaded}] via '{Name}' resolver", Name);

            return Assembly.Load(assmInfo.Name);
        }

        /// <summary>
        /// Gets the culture of the assembly name
        /// </summary>
        /// <param name="assmName">Assembly name</param>
        /// <returns>Text of the culture</returns>
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

        /// <summary>
        /// Gets the public key token from the assembly name
        /// </summary>
        /// <param name="assmName">Assembly name</param>
        /// <returns>Text version of public key token</returns>
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

        /// <summary>
        /// Provides the name of the assembly to replace for this missing reference
        /// </summary>
        /// <param name="assmName">Missing assembly name</param>
        /// <param name="requestingAssembly">Assembly that requests this missing reference</param>
        /// <param name="searchDir">Search directory</param>
        /// <param name="recursiveSearch">True to search in sub-directories recursievely</param>
        /// <returns>Assemly to replace</returns>
        protected abstract AssemblyName GetReplacementAssemblyName(AssemblyName assmName, Assembly requestingAssembly,
            out string searchDir, out bool recursiveSearch);

        /// <summary>
        /// Compares two assemblies to see if those match
        /// </summary>
        /// <param name="probeAssmName">Assembly candidate</param>
        /// <param name="searchAssmName">Target assembly</param>
        /// <param name="requestingAssembly">Requesting assembly</param>
        /// <returns>True if assembly names are matching</returns>
        /// <remarks>Use this method to override logic for matching (e.g. full match or only match by file name, version, public key token etc.)</remarks>
        protected virtual bool Match(AssemblyName probeAssmName, AssemblyName searchAssmName, Assembly requestingAssembly)
            => CompareAssemblyNames(probeAssmName, searchAssmName);

        /// <summary>
        /// Provides the assembly to use if multiple options available
        /// </summary>
        /// <param name="assmNames">Assembly candidates</param>
        /// <param name="searchAssmName">Target assembly name</param>
        /// <returns>Assembly to use</returns>
        protected virtual AssemblyInfo ResolveAmbiguity(
            IReadOnlyList<AssemblyInfo> assmNames, AssemblyName searchAssmName)
        {
            Trace.WriteLine($"Resolving ambiguity for '{searchAssmName}' via '{Name}' resolver", Name);

            var assmInfo = assmNames.FirstOrDefault(a => CompareAssemblyNames(a.Name, searchAssmName));

            if (assmInfo == null)
            {
                Trace.WriteLine($"Ambiguity for '{searchAssmName}' is not resolved via exact match via '{Name}' resolver", Name);

                assmInfo = assmNames.FirstOrDefault(a => a.IsLoaded);

                if (assmInfo == null)
                {
                    assmInfo = assmNames.FirstOrDefault();

                    if (assmInfo != null)
                    {
                        Trace.WriteLine($"Ambiguity for '{searchAssmName}' is resolved by first assembly via '{Name}' resolver", Name);
                    }
                    else 
                    {
                        Trace.WriteLine($"Ambiguity for '{searchAssmName}' is not resolved via '{Name}' resolver", Name);
                    }
                }
                else 
                {
                    Trace.WriteLine($"Ambiguity for '{searchAssmName}' is resolved by first loaded assembly via '{Name}' resolver", Name);
                }
            }

            return assmInfo;
        }

        /// <summary>
        /// Compares two assembly names by filter
        /// </summary>
        /// <param name="firstAssmName">First assembly name</param>
        /// <param name="secondAssmName">Second asembly name</param>
        /// <param name="filter">Filter</param>
        /// <returns>True if matched</returns>
        protected bool CompareAssemblyNames(AssemblyName firstAssmName, AssemblyName secondAssmName, AssemblyNamePart_e filter = AssemblyNamePart_e.FullName)
        {
            if (filter == AssemblyNamePart_e.FullName)
            {
                return CaseInsensitiveCompare(firstAssmName.FullName, secondAssmName.FullName);
            }
            else
            {
                return CaseInsensitiveCompare(firstAssmName.Name, secondAssmName.Name)
                    && (!filter.HasFlag(AssemblyNamePart_e.PublicKeyToken) || CaseInsensitiveCompare(GetPublicKeyToken(firstAssmName), GetPublicKeyToken(secondAssmName)))
                    && (!filter.HasFlag(AssemblyNamePart_e.Culture) || CaseInsensitiveCompare(firstAssmName.CultureName, firstAssmName.CultureName))
                    && (!filter.HasFlag(AssemblyNamePart_e.Version) || firstAssmName.Version == secondAssmName.Version);
            }
        }

        private bool CaseInsensitiveCompare(string firstText, string secondText) => string.Equals(firstText, secondText, StringComparison.CurrentCultureIgnoreCase);

        private IEnumerable<AssemblyInfo> EnumerateAssemblyByName(string dir, bool recurse, AssemblyName searchAssmName, Assembly requestingAssembly)
        {
            foreach (var probeAssmFilePath in ProvideProbeAssemblyFilePaths(dir, searchAssmName))
            {
                if (File.Exists(probeAssmFilePath))
                {
                    var probeAssmName = AssemblyName.GetAssemblyName(probeAssmFilePath);

                    if (Match(probeAssmName, searchAssmName, requestingAssembly))
                    {
                        yield return new AssemblyInfo(probeAssmName, probeAssmFilePath, false);
                    }
                }
            }

            if (recurse)
            {
                foreach (var subDir in Directory.EnumerateDirectories(dir, "*.*", SearchOption.TopDirectoryOnly))
                {
                    foreach (var res in EnumerateAssemblyByName(subDir, recurse, searchAssmName, requestingAssembly))
                    {
                        yield return res;
                    }
                }
            }
        }

        /// <summary>
        /// Returnes probe assemly file paths
        /// </summary>
        /// <param name="dir">Directory to search in</param>
        /// <param name="searchAssmName">Target assembly name</param>
        /// <returns>Possible file paths of the assembly file</returns>
        protected virtual IEnumerable<string> ProvideProbeAssemblyFilePaths(string dir, AssemblyName searchAssmName) 
        {
            yield return Path.Combine(dir, searchAssmName.Name + ".dll");
        }
    }
}