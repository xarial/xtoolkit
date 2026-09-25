//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Xarial.XToolkit.Reporting;

namespace Xarial.XToolkit.Services
{
    /// <summary>
    /// Parameters of <see cref="AppConfigBindingRedirectReferenceResolver"/>
    /// </summary>
    public class AppConfigBindingRedirectReferenceResolverParameters : AssemblyReferenceResolverParameters
    {
        /// <summary>
        /// Assemblies with app.config files
        /// </summary>
        public Assembly[] AppConfigAssemblies { get; set; }

        /// <summary>
        /// Search app.config of calling assemblies
        /// </summary>
        public bool SearchCallingAssemblyAppConfig { get; set; }
    }

    /// <summary>
    /// Resolver allowing to redirect assembly binding based on the .config files
    /// </summary>
    /// <remarks>This resolver can be useful for the plugin applications (.dlls) when the app.config files will not be considered for binding redirects
    /// It can also be useful when the binding redirects specified in the separate file which is not named after the application name (e.g. custom binding redirect without an automatic option)</remarks>
    public class AppConfigBindingRedirectReferenceResolver : AssemblyReferenceResolver
    {
        /// <summary>
        /// Create app.config resolved based on the type fro the main assembly
        /// </summary>
        /// <typeparam name="T">Type of assembly which contains app.config</typeparam>
        /// <param name="logger">Logger</param>
        /// <param name="additionalAssms">Additional assemblies to search app.config</param>
        /// <returns>Resolver</returns>
        public static AppConfigBindingRedirectReferenceResolver FromType<T>(ILogWriter logger = null, params Assembly[] additionalAssms)
        {
            var assm = typeof(T).Assembly;

            var assmFilePath = TryGetLocation(assm);

            if (string.IsNullOrEmpty(assmFilePath))
            {
                throw new InvalidOperationException(
                    $"Assembly '{assm.FullName}' of type '{typeof(T).FullName}' has no location, the app.config directory cannot be resolved from it");
            }

            return new AppConfigBindingRedirectReferenceResolver(AppDomain.CurrentDomain, new AppConfigBindingRedirectReferenceResolverParameters()
            {
                AppConfigAssemblies = new Assembly[] { assm }.Union(additionalAssms ?? Array.Empty<Assembly>()).ToArray(),
                RequestingAssemblyDirectories = new string[] { Path.GetDirectoryName(assmFilePath) },
                SearchCallingAssemblyAppConfig = false
            }, logger);
        }

        private static readonly XNamespace m_AsmNamespace = "urn:schemas-microsoft-com:asm.v1";

        [DebuggerDisplay("oldVersion=\"{" + nameof(OldVersionFrom) + "}-{" + nameof(OldVersionTo) + "}\" newVersion=\"{" + nameof(NewVersion) + "}\"")]
        private class BindingRedirect
        {
            internal Version OldVersionFrom { get; }
            internal Version OldVersionTo { get; }
            internal Version NewVersion { get; }

            internal BindingRedirect(Version oldVersionFrom, Version oldVersionTo, Version newVersion)
            {
                OldVersionFrom = oldVersionFrom;
                OldVersionTo = oldVersionTo;
                NewVersion = newVersion;
            }

            internal bool Matches(Version version)
                => version != null && version >= OldVersionFrom && version <= OldVersionTo;
        }

        [DebuggerDisplay("name=\"{" + nameof(m_Name) + "}\" publicKeyToken=\"{" + nameof(m_PublicKeyToken) + "}\" culture=\"{" + nameof(m_Culture) + "}\"")]
        private readonly struct AssemblyIdentityKey : IEquatable<AssemblyIdentityKey>
        {
            private readonly string m_Name;
            private readonly string m_PublicKeyToken;
            private readonly string m_Culture;

            internal AssemblyIdentityKey(string name, string publicKeyToken, string culture)
            {
                m_Name = name;
                m_PublicKeyToken = publicKeyToken;
                m_Culture = culture;
            }

            public bool Equals(AssemblyIdentityKey other)
                => string.Equals(m_Name, other.m_Name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(m_PublicKeyToken, other.m_PublicKeyToken, StringComparison.OrdinalIgnoreCase)
                && string.Equals(m_Culture, other.m_Culture, StringComparison.OrdinalIgnoreCase);

            public override bool Equals(object obj)
                => obj is AssemblyIdentityKey other && Equals(other);

            public override int GetHashCode()
                => m_Name == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(m_Name);
        }

        private class AppConfigCacheEntry
        {
            internal DateTime LastWriteTimeUtc { get; }
            internal IReadOnlyDictionary<AssemblyIdentityKey, IReadOnlyList<BindingRedirect>> RedirectsByAssembly { get; }

            internal AppConfigCacheEntry(DateTime lastWriteTimeUtc, IReadOnlyDictionary<AssemblyIdentityKey, IReadOnlyList<BindingRedirect>> redirectsByAssembly)
            {
                LastWriteTimeUtc = lastWriteTimeUtc;
                RedirectsByAssembly = redirectsByAssembly;
            }
        }

        private readonly ConcurrentDictionary<string, AppConfigCacheEntry> m_BindingRedirectsCache
            = new ConcurrentDictionary<string, AppConfigCacheEntry>(StringComparer.OrdinalIgnoreCase);

        private AppConfigBindingRedirectReferenceResolverParameters AppConfigParameters
            => (AppConfigBindingRedirectReferenceResolverParameters)Parameters;

        /// <inheritdoc/>
        public AppConfigBindingRedirectReferenceResolver(AppDomain appDomain, AppConfigBindingRedirectReferenceResolverParameters parameters, ILogWriter logger = null)
            : base(appDomain, parameters, logger)
        {
        }

        /// <summary>
        /// Returns app.config files for the requesting assembly
        /// </summary>
        /// <param name="requestingAssembly"></param>
        /// <returns></returns>
        protected virtual string[] GetAppConfigs(Assembly requestingAssembly)
            => IterateAppConfigAssemblies(requestingAssembly)
            .Select(a => TryGetLocation(a))
            .Where(f => !string.IsNullOrEmpty(f))
            .Select(f => f + ".config")
            .Where(f => File.Exists(f)).ToArray();

        private IEnumerable<Assembly> IterateAppConfigAssemblies(Assembly requestingAssembly)
        {
            if (AppConfigParameters.SearchCallingAssemblyAppConfig)
            {
                yield return requestingAssembly;
            }

            if (AppConfigParameters.AppConfigAssemblies != null)
            {
                foreach (var appConfAssm in AppConfigParameters.AppConfigAssemblies)
                {
                    yield return appConfAssm;
                }
            }
        }

        /// <inheritdoc/>
        protected override AssemblyName GetReplacementAssemblyName(AssemblyName assmName, Assembly requestingAssembly,
            out string searchDir, out bool recursiveSearch)
        {
            if (assmName.Version != null)
            {
                var appConfigs = GetAppConfigs(requestingAssembly);

                if (appConfigs?.Any() == true)
                {
                    foreach (var appConfigPath in appConfigs)
                    {
                        try
                        {
                            var redirectsByAssembly = GetBindingRedirects(appConfigPath);

                            var name = assmName.Name;
                            var publicKeyToken = GetPublicKeyToken(assmName);
                            var culture = GetCulture(assmName);

                            var key = new AssemblyIdentityKey(name, publicKeyToken, culture);

                            if (redirectsByAssembly.TryGetValue(key, out var redirects))
                            {
                                var matchingRedirect = redirects.FirstOrDefault(r => r.Matches(assmName.Version));

                                if (matchingRedirect != null)
                                {
                                    var searchAssmName = new AssemblyName(
                                        $"{name}, Version={matchingRedirect.NewVersion}, Culture={culture}, PublicKeyToken={publicKeyToken}");

                                    searchDir = Path.GetDirectoryName(appConfigPath);
                                    recursiveSearch = false;
                                    return searchAssmName;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            m_Logger?.LogError(ex);
                        }
                    }
                }
                else
                {
                    m_Logger?.LogWarning("No app.config files are found");
                }
            }
            else
            {
                m_Logger?.LogTrace($"Cannot resolve binding redirect for '{assmName}' as the assembly version is not specified");
            }

            searchDir = "";
            recursiveSearch = false;
            return null;
        }

        private IReadOnlyDictionary<AssemblyIdentityKey, IReadOnlyList<BindingRedirect>> GetBindingRedirects(string appConfigPath)
        {
            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(appConfigPath);

            if (m_BindingRedirectsCache.TryGetValue(appConfigPath, out var cacheEntry)
                && cacheEntry.LastWriteTimeUtc == lastWriteTimeUtc)
            {
                return cacheEntry.RedirectsByAssembly;
            }

            m_Logger?.LogTrace($"Parsing binding redirects from '{appConfigPath}'");

            var redirectsByAssembly = ParseBindingRedirects(appConfigPath);

            m_BindingRedirectsCache[appConfigPath] = new AppConfigCacheEntry(lastWriteTimeUtc, redirectsByAssembly);

            return redirectsByAssembly;
        }

        private IReadOnlyDictionary<AssemblyIdentityKey, IReadOnlyList<BindingRedirect>> ParseBindingRedirects(string appConfigPath)
        {
            var result = new Dictionary<AssemblyIdentityKey, List<BindingRedirect>>();

            using (var xmlStream = File.OpenRead(appConfigPath))
            {
                var readerSettings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };

                using (var reader = XmlReader.Create(xmlStream, readerSettings))
                {
                    var doc = XDocument.Load(reader);

                    foreach (var dependentAssm in doc.Descendants(m_AsmNamespace + "dependentAssembly"))
                    {
                        var identity = dependentAssm.Element(m_AsmNamespace + "assemblyIdentity");
                        var redirect = dependentAssm.Element(m_AsmNamespace + "bindingRedirect");

                        var name = identity?.Attribute("name")?.Value;
                        var oldVersionVal = redirect?.Attribute("oldVersion")?.Value;
                        var newVersionVal = redirect?.Attribute("newVersion")?.Value;

                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(oldVersionVal) && !string.IsNullOrEmpty(newVersionVal))
                        {
                            if (TryParseVersionRange(oldVersionVal, out var oldVersionMin, out var oldVersionMax))
                            {
                                if (Version.TryParse(newVersionVal, out Version newVersion))
                                {
                                    var publicKeyToken = identity.Attribute("publicKeyToken")?.Value ?? "null";
                                    var culture = identity.Attribute("culture")?.Value ?? "neutral";

                                    var key = new AssemblyIdentityKey(name, publicKeyToken, culture);

                                    if (!result.TryGetValue(key, out var list))
                                    {
                                        list = new List<BindingRedirect>();
                                        result[key] = list;
                                    }

                                    list.Add(new BindingRedirect(oldVersionMin, oldVersionMax, newVersion));
                                }
                                else
                                {
                                    m_Logger?.LogWarning($"Skipping binding redirect for '{name}' as the 'newVersion' value '{newVersionVal}' in '{appConfigPath}' is invalid");
                                }
                            }
                            else
                            {
                                m_Logger?.LogWarning($"Skipping binding redirect for '{name}' as the 'oldVersion' value '{oldVersionVal}' in '{appConfigPath}' is invalid");
                            }
                        }
                    }
                }
            }

            return result.ToDictionary(x => x.Key, x => (IReadOnlyList<BindingRedirect>)x.Value);
        }

        private bool TryParseVersionRange(string versionRange, out Version from, out Version to)
        {
            from = default;
            to = default;

            if (string.IsNullOrEmpty(versionRange))
            {
                return false;
            }

            if (versionRange.Contains("-"))
            {
                var versionParts = versionRange.Split('-');

                if (versionParts.Length == 2
                    && Version.TryParse(versionParts[0], out from)
                    && Version.TryParse(versionParts[1], out to))
                {
                    return true;
                }
            }
            else if (Version.TryParse(versionRange, out from))
            {
                to = from;
                return true;
            }

            from = default;
            to = default;

            return false;
        }
    }
}