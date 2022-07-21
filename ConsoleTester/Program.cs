using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            m_AssmResolver.RegisterAssemblyReferenceResolver(new CustomAppConfigBindingRedirectReferenceResolver());

            //AppDomain.CurrentDomain.RegisterGlobalAssemblyReferenceResolver(new LocalFolderReferencesResolver(
            //    Path.GetDirectoryName(typeof(Program).Assembly.Location),
            //    AssemblyMatchFilter_e.PublicKeyToken | AssemblyMatchFilter_e.Culture));
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
