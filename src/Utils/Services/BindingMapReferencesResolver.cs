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
        public static BindingMapReferencesResolver FromType<T>(AssemblyNamePart_e filter, string bindingFileName, ILogger logger)
        {
            var workDir = Path.GetDirectoryName(typeof(T).Assembly.Location);

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
            public bool Equals(AssemblyName x, AssemblyName y) => string.Equals(x.FullName, y.FullName);

            public int GetHashCode(AssemblyName obj) => 0;
        }

        private readonly BindingMapReferenceResolverParameters m_Parameters;

        /// <inheritdoc/>
        public BindingMapReferencesResolver(AppDomain appDomain, BindingMapReferenceResolverParameters parameters, ILogger logger) : base(appDomain, parameters, logger)
        {
            m_Parameters = parameters;
        }

        /// <inheritdoc/>
        public override Assembly Resolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
        {
            if (ShouldResolve(appDomain, assmName, requestingAssembly))
            {
                if (m_Parameters.Map?.TryGetValue(assmName, out var assmFilePath) == true)
                {
                    assmFilePath = Path.Combine(m_Parameters.SearchDirectory, assmFilePath);

                    if (File.Exists(assmFilePath))
                    {
                        var assmInfo = AssemblyInfo.FromFile(assmFilePath);
                        return LoadAssembly(assmInfo);
                    }
                    else 
                    {
                        throw new FileNotFoundException($"Assembly {assmFilePath} is not found");
                    }
                }
            }

            return base.Resolve(appDomain, assmName, requestingAssembly);
        }
    }
}
