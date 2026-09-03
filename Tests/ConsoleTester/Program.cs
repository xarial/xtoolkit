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
using Xarial.XToolkit.Reporting;
using Xarial.XToolkit.Services;

namespace ConsoleTester
{
    class Program
    {
        public interface ISampleClass
        {
            void Foo(string param);
        }

        private static AssemblyReferenceResolver m_AssmResolver;

        private class CustomAppConfigBindingRedirectReferenceResolver : AppConfigBindingRedirectReferenceResolver 
        {
            public CustomAppConfigBindingRedirectReferenceResolver(AppDomain appDomain, AssemblyReferenceResolverParameters parameters, ILogger logger) 
                : base(appDomain, parameters, logger)
            {
            }

            protected override string[] GetAppConfigs(Assembly requestingAssembly)
                => Directory.GetFiles(Path.GetDirectoryName(typeof(Program).Assembly.Location), "*.config");
        }

        static Program() 
        {
            System.Diagnostics.Debugger.Launch();

            var localPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            m_AssmResolver = new LocalFolderReferencesResolver(AppDomain.CurrentDomain,
                new LocalFolderReferenceResolverParameters()
                {
                    SearchDirectory = FileSystemUtils.CombinePaths(localPath, @"..\..\..\Lib\bin\Debug"),
                    MatchFilter = AssemblyNamePart_e.PublicKeyToken | AssemblyNamePart_e.Culture,
                    RequestingAssemblyDirectories = new string[] { localPath }
                }, new TraceLogger("Test"));
                ;
        }

        static void Main(string[] args)
        {
            TestFileLogger();
            //TestIsolatedInstance();
        }

        private static void TestFileLogger()
        {
            var appGuid = new Guid("{1CA4640E-FC18-454E-93A5-3D815FF8686A}");

            var fileLogger = new FileLogger(
                $@"%appdata%\Xarial\xToolkit\Logs\testlog_{Guid.NewGuid().ToString()}.log", "test", appGuid,
                new FileLoggerRetentionPolicy("testlog_*.log", 3, TimeSpan.FromDays(30)));

            fileLogger.Log("Message 1");
        }

        private static void TestIsolatedInstance()
        {
            var localPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            using (var isoInst = new IsolatedInstance(FileSystemUtils.CombinePaths(localPath, @"..\..\..\Lib\bin\Debug\Lib.dll"), "Lib.SampleClass"))
            {
                isoInst.Call<ISampleClass>(x => x.Foo("ABC"));
            }

            Foo();
        }

        static void Foo() 
        {
            var f = new Lib.SampleClass();
            f.Foo("TEST");
        }
    }
}
