using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reflection;

namespace ConsoleTester
{
    class Program
    {
        static Program() 
        {
            AppDomain.CurrentDomain.RegisterGlobalAssemblyReferenceResolver(new LocalFolderReferencesResolver(
                Path.GetDirectoryName(typeof(Program).Assembly.Location),
                AssemblyMatchFilter_e.PublicKeyToken | AssemblyMatchFilter_e.Culture));
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
