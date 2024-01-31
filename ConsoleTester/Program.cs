using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit;
using Xarial.XToolkit.Helpers;
using Xarial.XToolkit.Reflection;

namespace ConsoleTester
{
    class Program
    {
        private static AssemblyResolver m_AssmResolver;

        private class CustomAppConfigBindingRedirectReferenceResolver : AppConfigBindingRedirectReferenceResolver 
        {
            protected override string[] GetAppConfigs(Assembly requestingAssembly)
                => Directory.GetFiles(Path.GetDirectoryName(typeof(Program).Assembly.Location), "*.config");
        }

        static Program() 
        {
            System.Diagnostics.Debugger.Launch();

            m_AssmResolver = new AssemblyResolver(AppDomain.CurrentDomain);
            //m_AssmResolver.RegisterAssemblyReferenceResolver(new CustomAppConfigBindingRedirectReferenceResolver());

            var localPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            m_AssmResolver.RegisterAssemblyReferenceResolver(
                new LocalFolderReferencesResolver(FileSystemUtils.CombinePaths(localPath, @"..\..\..\Lib\bin\Debug"),
                AssemblyMatchFilter_e.PublicKeyToken | AssemblyMatchFilter_e.Culture, "", null,
                new string[] { localPath }));
        }

        static void Main(string[] args)
        {
            Foo();
        }
        
        static void Foo() 
        {
            var f = new Lib.SampleClass();
            f.Foo();
        }
    }
}
