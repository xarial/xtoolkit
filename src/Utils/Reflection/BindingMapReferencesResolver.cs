//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
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
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.XToolkit.Reflection
{
    /// <summary>
    /// Resolves assemblies based on the specified binding dictionary
    /// </summary>
    public class BindingMapReferencesResolver : LocalFolderReferencesResolver
    {
        private static IReadOnlyDictionary<AssemblyName, string> LoadBindingMap(string workDir, string filePath)
        {
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(workDir, filePath);
            }

            var ser = new UserSettingsService();

            return ser.ReadSettings<IReadOnlyDictionary<string, string>>(filePath)
                .ToDictionary(x => new AssemblyName(x.Key), y => y.Value, new AssemblyNameEqualityComparer());
        }

        private class AssemblyNameEqualityComparer : IEqualityComparer<AssemblyName>
        {
            public bool Equals(AssemblyName x, AssemblyName y) => string.Equals(x.FullName, y.FullName);

            public int GetHashCode(AssemblyName obj) => 0;
        }

        private readonly string m_WorkDir;
        private readonly IReadOnlyDictionary<AssemblyName, string> m_BindingMap;

        public BindingMapReferencesResolver(string name, string workDir, string bindingMapFilePath) : this(name, workDir, LoadBindingMap(workDir, bindingMapFilePath))
        {
        }

        public BindingMapReferencesResolver(string name, string workDir, IReadOnlyDictionary<AssemblyName, string> bindingMap) : base(workDir, AssemblyNamePart_e.FullName, name)
        {
            m_WorkDir = workDir;
            m_BindingMap = bindingMap;
        }

        /// <inheritdoc/>
        public override Assembly Resolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
        {
            if (ShouldResolve(appDomain, assmName, requestingAssembly))
            {
                if (m_BindingMap.TryGetValue(assmName, out var assmFilePath))
                {
                    assmFilePath = Path.Combine(m_WorkDir, assmFilePath);

                    var assmInfo = AssemblyInfo.FromFile(assmFilePath);

                    return LoadAssembly(assmInfo);
                }
            }

            return base.Resolve(appDomain, assmName, requestingAssembly);
        }
    }
}
