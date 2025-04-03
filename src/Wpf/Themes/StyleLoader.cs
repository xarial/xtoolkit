using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.XToolkit.Wpf.Themes
{
    /// <summary>
    /// Utility to load custom control style dynamically from the version-specific assembly
    /// </summary>
    /// <remarks>When multiple versions of library control are loaded into the application domain (e.g. as plugins), correct resource cannot be located
    /// In order to make WPF control support multiple version:
    /// * It must be strong named
    /// * AssemblyVersion attribute with 4 versions components (A.B.C.D) needs to be added to a project
    /// * All merged dictionaries must be loaded using the absolute url which includes assembly version
    /// </remarks>
    internal static class StyleLoader
    {
        internal static Style LoadStyle<T>(string dictName)
            where T : FrameworkElement 
            => (Style)typeof(T).Assembly.LoadFromResources($"Themes/{dictName}", typeof(T));
    }
}
