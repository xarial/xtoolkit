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
using System.Security.Cryptography;
using System.Text;
using Xarial.XToolkit.Reporting;
using Xarial.XToolkit.Services;

namespace Xarial.XToolkit.Services
{
    /// <summary>
    /// Parameters of <see cref="LocalFolderReferencesResolver"/>
    /// </summary>
    public class LocalFolderReferenceResolverParameters : AssemblyReferenceResolverParameters
    {
        /// <summary>
        /// Directory to search for the assemblies
        /// </summary>
        public string SearchDirectory { get; set; }

        /// <summary>
        /// Filter to match assemblies
        /// </summary>
        public AssemblyNamePart_e MatchFilter { get; set; }

        /// <summary>
        /// Filter of assemblies to search for replacement
        /// </summary>
        /// <remarks>Oonly matched assemblies will be searched or all if null or empty</remarks>
        public AssemblyFilter[] AssemblyFilter { get; set; }
    }

    /// <summary>
    /// Resolver to load referenced from the local folder
    /// </summary>
    public class LocalFolderReferencesResolver : AssemblyReferenceResolver
    {
        /// <summary>
        /// Creates a resolver for this type in the current application domain
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="filter">Filter</param>
        /// <param name="logger">Logger</param>
        /// <returns>References resolvedr</returns>
        public static LocalFolderReferencesResolver FromType<T>(AssemblyNamePart_e filter, ILogger logger)
        {
            var workDir = Path.GetDirectoryName(typeof(T).Assembly.Location);

            return new LocalFolderReferencesResolver(AppDomain.CurrentDomain, new LocalFolderReferenceResolverParameters()
            {
                MatchFilter = filter,
                SearchDirectory = workDir,
                RequestingAssemblyDirectories = new string[] { workDir }
            }, logger);
        }

        private readonly LocalFolderReferenceResolverParameters m_Parameters;

        /// <inheritdoc/>
        public LocalFolderReferencesResolver(AppDomain appDomain, LocalFolderReferenceResolverParameters parameters, ILogger logger)
            : base(appDomain, parameters, logger)
        {
            m_Parameters = parameters;
        }

        /// <inheritdoc/>
        protected override AssemblyName GetReplacementAssemblyName(AssemblyName assmName, Assembly requestingAssembly,
            out string searchDir, out bool recursiveSearch)
        {
            searchDir = m_Parameters.SearchDirectory;
            recursiveSearch = true;
            return assmName;
        }

        /// <inheritdoc/>
        protected override bool Match(AssemblyName probeAssmName, AssemblyName searchAssmName, Assembly requestingAssembly)
        {
            if (m_Parameters.AssemblyFilter?.Any() != true
                || m_Parameters.AssemblyFilter.Any(a => CompareAssemblyNames(searchAssmName, a.Name, a.MatchFilter)))
            {
                return CompareAssemblyNames(probeAssmName, searchAssmName, m_Parameters.MatchFilter);
            }
            else
            {
                return false;
            }
        }
    }
}
