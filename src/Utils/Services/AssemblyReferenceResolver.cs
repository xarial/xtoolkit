//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xarial.XToolkit.Reporting;

namespace Xarial.XToolkit.Services
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
        /// Match by version (or newer version)
        /// </summary>
        VersionAllowNewer = 8,

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
        /// <remarks>Null or empty for dynamic and in-memory assemblies</remarks>
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
    public class AssemblyFilter
    {
        /// <summary>
        /// Creates instance of the assembly filter
        /// </summary>
        /// <param name="assmName">Name of the assembly</param>
        /// <param name="matchFilter">Match filter</param>
        /// <returns>Requesting assembly filter</returns>
        public static AssemblyFilter Create(string assmName, AssemblyNamePart_e matchFilter = AssemblyNamePart_e.FullName)
            => new AssemblyFilter(new AssemblyName(assmName), matchFilter);

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
        public AssemblyFilter(AssemblyName name, AssemblyNamePart_e matchFilter)
        {
            Name = name;
            MatchFilter = matchFilter;
        }
    }

    /// <summary>
    /// Parameters for <see cref="AssemblyReferenceResolver"/>
    /// </summary>
    public class AssemblyReferenceResolverParameters
    {
        /// <summary>
        /// Only resolve the assembly if requesting assembly is in the specified directories
        /// </summary>
        public string[] RequestingAssemblyDirectories { get; set; }

        /// <summary>
        /// Only resolve assemblies whose requesting assembly match the filter
        /// </summary>
        public AssemblyFilter[] RequestingAssemblyFilter { get; set; }
    }

    /// <summary>
    /// Service to resolve the missing assembly references
    /// </summary>
    public abstract class AssemblyReferenceResolver : IDisposable
    {
        [ThreadStatic]
        private static HashSet<string> m_ResolvingAssmNames;

        private static readonly byte[][] m_FrameworkPublicKeyTokens = new byte[][]
        {
            new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 }, //b77a5c561934e089 - mscorlib, System
            new byte[] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a }, //b03f5f7f11d50a3a - System.Configuration
            new byte[] { 0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0x4e, 0x35 }, //31bf3856ad364e35 - WPF, WCF
            new byte[] { 0x7c, 0xec, 0x85, 0xd7, 0xbe, 0xa7, 0x79, 0x8e }  //7cec85d7bea7798e - .NET Core / portable
        };

        private readonly AppDomain m_AppDomain;
        private readonly AssemblyReferenceResolverParameters m_Parameters;

        /// <summary>
        /// Current logger
        /// </summary>
        protected readonly ILogWriter m_Logger;

        /// <summary>
        /// Parameters of this resolver
        /// </summary>
        protected AssemblyReferenceResolverParameters Parameters => m_Parameters;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="appDomain">Application domain</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="logger">Logger</param>
        protected AssemblyReferenceResolver(AppDomain appDomain, AssemblyReferenceResolverParameters parameters, ILogWriter logger = null)
        {
            if (appDomain == null)
            {
                throw new ArgumentNullException(nameof(appDomain));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            m_AppDomain = appDomain;

            m_Parameters = parameters;

            m_Logger = logger;

            m_AppDomain.AssemblyResolve += OnResolveMissingAssembly;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private Assembly OnResolveMissingAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                var assmName = new AssemblyName(args.Name);

                if (!string.IsNullOrEmpty(assmName.Name)
                    && !assmName.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase)
                    && !assmName.Name.EndsWith(".XmlSerializers", StringComparison.OrdinalIgnoreCase))
                {
                    //NOTE: do not move in other method to ensure Assembly::GetCallingAssembly returns the actual caller
                    var requestingAssm = args.RequestingAssembly ?? Assembly.GetCallingAssembly();

                    if (!BeginResolve(args.Name))
                    {
                        m_Logger?.LogTrace($"Already resolving '{args.Name}' on this thread - skipped to avoid recursion");
                        return null;
                    }

                    try
                    {
                        m_Logger?.LogTrace($"Resolving '{args.Name}' for requesting assembly '{requestingAssm?.FullName}' [exact={args.RequestingAssembly != null}]");

                        var assm = Resolve(m_AppDomain, assmName, requestingAssm);

                        if (assm != null)
                        {
                            m_Logger?.LogInformation($"Assembly '{args.Name}' is resolved to '{assm.FullName}' in '{TryGetLocation(assm) ?? "<dynamic or in-memory>"}'");
                            return assm;
                        }
                        else
                        {
                            m_Logger?.LogInformation($"Assembly '{args.Name}' is not resolved");
                        }
                    }
                    finally
                    {
                        EndResolve(args.Name);
                    }
                }
                else 
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                m_Logger?.LogError(ex);
            }

            return null;
        }

        private static bool BeginResolve(string assmName)
        {
            if (m_ResolvingAssmNames == null)
            {
                m_ResolvingAssmNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return m_ResolvingAssmNames.Add(assmName);
        }

        private static void EndResolve(string assmName)
        {
            m_ResolvingAssmNames?.Remove(assmName);
        }

        /// <summary>
        /// Returns the location of the assembly
        /// </summary>
        /// <param name="assm">Assembly</param>
        /// <returns>File path of the assembly or null</returns>
        protected static string TryGetLocation(Assembly assm)
        {
            try
            {
                if (assm == null || assm.IsDynamic)
                {
                    return null;
                }

                var location = assm.Location;

                return string.IsNullOrEmpty(location) ? null : location;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resolves the missing assembly reference
        /// </summary>
        /// <param name="appDomain">Application domain</param>
        /// <param name="assmName">Assembly name to resolve</param>
        /// <param name="requestingAssembly">Assembly which requests the missing reference</param>
        /// <returns>Replacement assembly</returns>
        protected virtual Assembly Resolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
        {
            if (ShouldResolve(appDomain, assmName, requestingAssembly))
            {
                var searchAssmName = GetReplacementAssemblyName(assmName, requestingAssembly, out string searchDir, out bool recursiveSearch);

                if (searchAssmName != null)
                {
                    var matchedAssmNames = new List<AssemblyInfo>();

                    var replacementAssms = appDomain.GetAssemblies().Where(
                                            a => Match(a.GetName(), searchAssmName, requestingAssembly)).ToArray();

                    var exactMatch = replacementAssms.FirstOrDefault(a => CompareAssemblyNames(a.GetName(), searchAssmName));

                    if (exactMatch != null)
                    {
                        m_Logger?.LogInformation($"Assembly '{searchAssmName}' is resolved to '{TryGetLocation(exactMatch) ?? "<dynamic or in-memory>"}' as exact match");

                        return exactMatch;
                    }
                    else
                    {
                        matchedAssmNames.AddRange(replacementAssms.Select(a => new AssemblyInfo(a.GetName(), TryGetLocation(a), true)));
                    }

                    if (!string.IsNullOrEmpty(searchDir) && Directory.Exists(searchDir))
                    {
                        foreach (var name in EnumerateAssemblyByName(searchDir, recursiveSearch, searchAssmName, requestingAssembly))
                        {
                            if (CompareAssemblyNames(name.Name, searchAssmName))
                            {
                                m_Logger?.LogTrace($"Loading '{searchAssmName}' from '{name.FilePath}' as exact match");

                                return LoadAssembly(AssemblyInfo.FromFile(name.FilePath));
                            }
                            else
                            {
                                matchedAssmNames.Add(name);
                            }
                        }
                    }
                    else
                    {
                        m_Logger?.LogWarning($"Search directory '{searchDir}' for '{searchAssmName}' does not exist - only already loaded assemblies are considered");
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

                var reqAssmFilters = Parameters.RequestingAssemblyFilter?.Where(a => a?.Name != null);

                if (EmptyOrAny(reqAssmFilters, f => CompareAssemblyNames(reqAssmName, f.Name, f.MatchFilter)))
                {
                    if (!IsFrameworkAssembly(reqAssmName))
                    {
                        var reqAssmFilePath = TryGetLocation(requestingAssembly);

                        if (reqAssmFilePath != null)
                        {
                            var reqAssmDirs = Parameters.RequestingAssemblyDirectories?.Where(f => !string.IsNullOrEmpty(f));

                            return EmptyOrAny(reqAssmDirs, f => FileSystemUtils.IsInDirectory(reqAssmFilePath, f));
                        }
                        else
                        {
                            m_Logger?.LogTrace($"Requesting assembly '{reqAssmName}' has no location - directory filter is not applied");
                            return true;
                        }
                    }
                    else
                    {
                        m_Logger?.LogTrace($"Requesting assembly '{reqAssmName}' is framework assembly, attempting to resolve");
                        return true;
                    }
                }
                else 
                {
                    m_Logger?.LogTrace($"Requesting assembly '{reqAssmName}' filter mismatch, ignoring resolve");
                    return false;
                }
            }
            else 
            {
                m_Logger?.LogTrace($"Requesting assembly is null, attempting to resolve");
                return true;
            }
        }

        private bool IsFrameworkAssembly(AssemblyName assmName)
        {
            var token = assmName?.GetPublicKeyToken();

            if (token == null || token.Length == 0)
            {
                return false;
            }

            return m_FrameworkPublicKeyTokens.Any(t => t.SequenceEqual(token));
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
        protected virtual Assembly LoadAssembly(AssemblyInfo assmInfo)
        {
            if (assmInfo.IsLoaded || string.IsNullOrEmpty(assmInfo.FilePath))
            {
                m_Logger?.LogTrace($"Resolving '{assmInfo.Name}' to the already loaded assembly");

                return Assembly.Load(assmInfo.Name);
            }

            m_Logger?.LogTrace($"Loading '{assmInfo.Name}' from file '{assmInfo.FilePath}'");

            return Assembly.LoadFrom(assmInfo.FilePath);
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
            m_Logger?.LogTrace($"Resolving ambiguity for '{searchAssmName}'");

            var assmInfo = assmNames.FirstOrDefault(a => CompareAssemblyNames(a.Name, searchAssmName));

            if (assmInfo == null)
            {
                m_Logger?.LogTrace($"Ambiguity for '{searchAssmName}' is not resolved via exact match");

                assmInfo = assmNames.FirstOrDefault(a => a.IsLoaded);

                if (assmInfo == null)
                {
                    assmInfo = assmNames.FirstOrDefault(a => !string.IsNullOrEmpty(a.FilePath));

                    if (assmInfo != null)
                    {
                        m_Logger?.LogWarning($"Ambiguity for '{searchAssmName}' is resolved by first assembly '{assmInfo.Name}', version differs from the requested one");
                    }
                    else
                    {
                        m_Logger?.LogInformation($"Ambiguity for '{searchAssmName}' is not resolved");
                    }
                }
                else
                {
                    m_Logger?.LogInformation($"Ambiguity for '{searchAssmName}' is resolved by first loaded assembly");
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
            if (firstAssmName == null || secondAssmName == null)
            {
                return false;
            }

            if (filter == AssemblyNamePart_e.FullName)
            {
                return CaseInsensitiveCompare(firstAssmName.FullName, secondAssmName.FullName);
            }
            else
            {
                return CaseInsensitiveCompare(firstAssmName.Name, secondAssmName.Name)
                    && (!filter.HasFlag(AssemblyNamePart_e.PublicKeyToken) || CaseInsensitiveCompare(GetPublicKeyToken(firstAssmName), GetPublicKeyToken(secondAssmName)))
                    && (!filter.HasFlag(AssemblyNamePart_e.Culture) || CaseInsensitiveCompare(firstAssmName.CultureName, secondAssmName.CultureName))
                    && MatchVersion(firstAssmName.Version, secondAssmName.Version, filter);
            }
        }

        private bool MatchVersion(Version firstAssmVers, Version secondAssmVers, AssemblyNamePart_e filter)
        {
            if (filter.HasFlag(AssemblyNamePart_e.VersionAllowNewer))
            {
                if (firstAssmVers == null)
                {
                    return secondAssmVers == null;
                }

                return secondAssmVers == null || firstAssmVers >= secondAssmVers;
            }
            else if (filter.HasFlag(AssemblyNamePart_e.Version))
            {
                return firstAssmVers == secondAssmVers;
            }
            else
            {
                return true;
            }
        }

        private bool CaseInsensitiveCompare(string firstText, string secondText)
            => string.Equals(firstText, secondText, StringComparison.OrdinalIgnoreCase);

        private IEnumerable<AssemblyInfo> EnumerateAssemblyByName(string dir, bool recurse, AssemblyName searchAssmName, Assembly requestingAssembly)
        {
            IEnumerable<string> probeAssmFilePaths;

            try
            {
                probeAssmFilePaths = ProvideProbeAssemblyFilePaths(dir, searchAssmName).ToArray();
            }
            catch (Exception ex)
            {
                m_Logger?.LogWarning($"Failed to enumerate probe assembly paths in '{dir}': {ex.Message}");
                probeAssmFilePaths = Array.Empty<string>();
            }

            foreach (var probeAssmFilePath in probeAssmFilePaths)
            {
                AssemblyInfo probeAssmInfo = null;

                try
                {
                    if (File.Exists(probeAssmFilePath))
                    {
                        var probeAssmName = AssemblyName.GetAssemblyName(probeAssmFilePath);

                        if (Match(probeAssmName, searchAssmName, requestingAssembly))
                        {
                            probeAssmInfo = new AssemblyInfo(probeAssmName, probeAssmFilePath, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_Logger?.LogWarning($"Failed to probe assembly candidate '{probeAssmFilePath}': {ex.Message}");
                }

                if (probeAssmInfo != null)
                {
                    yield return probeAssmInfo;
                }
            }

            if (recurse)
            {
                string[] subDirs;

                try
                {
                    subDirs = Directory.EnumerateDirectories(dir, "*.*", SearchOption.TopDirectoryOnly).ToArray();
                }
                catch (Exception ex)
                {
                    m_Logger?.LogWarning($"Failed to enumerate sub-directories of '{dir}': {ex.Message}");
                    subDirs = Array.Empty<string>();
                }

                foreach (var subDir in subDirs)
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

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            m_AppDomain.AssemblyResolve -= OnResolveMissingAssembly;
        }
    }
}