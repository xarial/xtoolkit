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
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reporting;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Services.Data;

namespace Xarial.XToolkit.Services
{
    /// <summary>
    /// Parameters of <see cref="BindingMapReferencesResolver"/>
    /// </summary>
    public class BindingMapReferenceResolverParameters : LocalFolderReferenceResolverParameters
    {
        /// <summary>
        /// Binding map
        /// </summary>
        /// <remarks>Use <see cref="BindingMapReferencesResolver.LoadBindingMap(string)"/> to load binding map from file</remarks>
        public IReadOnlyDictionary<AssemblyName, string> Map { get; set; }
    }

    /// <summary>
    /// Resolves assemblies based on the specified binding dictionary
    /// </summary>
    public class BindingMapReferencesResolver : LocalFolderReferencesResolver
    {
        /// <inheritdoc/>
        /// <param name="bindingFileName">Name of bidning file</param>
        /// <param name="filter">Match filter</param>
        /// <param name="logger">Logger</param>
        public static BindingMapReferencesResolver FromType<T>(AssemblyNamePart_e filter, string bindingFileName, ILogWriter logger = null)
        {
            var assm = typeof(T).Assembly;

            var assmFilePath = TryGetLocation(assm);

            if (string.IsNullOrEmpty(assmFilePath))
            {
                throw new InvalidOperationException(
                    $"Assembly '{assm.FullName}' of type '{typeof(T).FullName}' has no location, the search directory cannot be resolved from it");
            }

            var workDir = Path.GetDirectoryName(assmFilePath);

            return new BindingMapReferencesResolver(AppDomain.CurrentDomain, new BindingMapReferenceResolverParameters()
            {
                Map = LoadBindingMap(Path.Combine(workDir, bindingFileName)),
                MatchFilter = filter,
                SearchDirectory = workDir,
                RequestingAssemblyDirectories = new string[] { workDir }
            }, logger);
        }

        /// <summary>
        /// Loads binding map from the json file
        /// </summary>
        /// <param name="filePath">File path to Binding JSON map</param>
        /// <returns>Binding map</returns>
        public static IReadOnlyDictionary<AssemblyName, string> LoadBindingMap(string filePath)
        {
            var ser = new NsJsonDataSerializer<IReadOnlyDictionary<string, string>>();

            return ser.Read(filePath)
                .ToDictionary(x => new AssemblyName(x.Key), y => y.Value, new AssemblyNameEqualityComparer());
        }

        private class AssemblyNameEqualityComparer : IEqualityComparer<AssemblyName>
        {
            public bool Equals(AssemblyName x, AssemblyName y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                return string.Equals(x.FullName, y.FullName, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(AssemblyName obj)
                => obj?.FullName == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FullName);
        }

        private BindingMapReferenceResolverParameters MapParameters
            => (BindingMapReferenceResolverParameters)Parameters;

        /// <inheritdoc/>
        public BindingMapReferencesResolver(AppDomain appDomain, BindingMapReferenceResolverParameters parameters, ILogWriter logger = null) : base(appDomain, parameters, logger)
        {
        }

        /// <inheritdoc/>
        protected override Assembly Resolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
        {
            if (ShouldResolve(appDomain, assmName, requestingAssembly))
            {
                var searchDir = MapParameters.SearchDirectory;

                if (MapParameters.Map?.TryGetValue(assmName, out var assmFilePath) == true
                    && !string.IsNullOrEmpty(assmFilePath))
                {
                    if (!string.IsNullOrEmpty(searchDir))
                    {
                        assmFilePath = Path.Combine(searchDir, assmFilePath);
                    }

                    if (File.Exists(assmFilePath))
                    {
                        return LoadAssembly(AssemblyInfo.FromFile(assmFilePath));
                    }
                    else
                    {
                        m_Logger?.LogWarning($"Mapped assembly '{assmFilePath}' for '{assmName}' is not found - falling back to the local folder search");
                    }
                }
            }

            return base.Resolve(appDomain, assmName, requestingAssembly);
        }
    }
}
