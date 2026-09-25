using Microsoft.CSharp;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Xarial.XToolkit.Reporting;
using Xarial.XToolkit.Services;

namespace Utils.Tests
{
    [TestFixture]
    public class AssemblyReferenceResolverTests
    {
        private class AppConfigBindingRedirectReferenceResolverTest : AppConfigBindingRedirectReferenceResolver
        {
            private readonly string m_AppConfigPath;

            internal AppConfigBindingRedirectReferenceResolverTest(string appConfigPath)
                : base(AppDomain.CurrentDomain, new AppConfigBindingRedirectReferenceResolverParameters())
            {
                m_AppConfigPath = appConfigPath;
            }

            internal Assembly ResolveTest(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
                => base.Resolve(appDomain, assmName, requestingAssembly);

            protected override string[] GetAppConfigs(Assembly requestingAssembly) => new string[] { m_AppConfigPath };
        }

        private class LocalFolderReferencesResolverTest : LocalFolderReferencesResolver
        {
            internal LocalFolderReferencesResolverTest(AppDomain appDomain, LocalFolderReferenceResolverParameters parameters, ILogWriter logger = null)
                : base(appDomain, parameters, logger)
            {
            }

            internal Assembly ResolveTest(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
                => base.Resolve(appDomain, assmName, requestingAssembly);
        }

        private class BindingMapReferencesResolverTest : BindingMapReferencesResolver
        {
            public BindingMapReferencesResolverTest(AppDomain appDomain, BindingMapReferenceResolverParameters parameters, ILogWriter logger = null) : base(appDomain, parameters, logger)
            {
            }

            internal Assembly ResolveTest(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
                => base.Resolve(appDomain, assmName, requestingAssembly);
        }

        private const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x4;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);

        private static readonly string m_TestsRootDir = Path.Combine(Path.GetTempPath(), "xToolkitTests_A62C1F26-E4AF-41CA-B80D-5700DCB8E241");

        private string m_TestDir;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (Directory.Exists(m_TestsRootDir))
            {
                foreach (var dir in Directory.EnumerateDirectories(m_TestsRootDir))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch
                    {
                    }
                }
            }
        }

        [SetUp]
        public void Setup()
        {
            m_TestDir = Path.Combine(m_TestsRootDir, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(m_TestDir);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                Directory.Delete(m_TestDir, true);
            }
            catch
            {
                MarkForDeletionOnReboot(m_TestDir);
            }
        }

        private static void MarkForDeletionOnReboot(string dir)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                {
                    MoveFileEx(file, null, MOVEFILE_DELAY_UNTIL_REBOOT);
                }

                foreach (var subDir in Directory.EnumerateDirectories(dir, "*", SearchOption.AllDirectories)
                    .OrderByDescending(d => d.Length))
                {
                    MoveFileEx(subDir, null, MOVEFILE_DELAY_UNTIL_REBOOT);
                }

                MoveFileEx(dir, null, MOVEFILE_DELAY_UNTIL_REBOOT);
            }
            catch
            {
            }
        }

        private static string NewAssemblyName() => "TestAssm_" + Guid.NewGuid().ToString("N");

        private static AssemblyName CreateName(string name, string version)
            => new AssemblyName($"{name}, Version={version}, Culture=neutral, PublicKeyToken=null");

        private string CompileAssembly(string name, string version, string relDir = "", string fileName = null)
        {
            var dir = Path.Combine(m_TestDir, relDir);
            Directory.CreateDirectory(dir);

            var outPath = Path.Combine(dir, (fileName ?? name) + ".dll");

            using (var provider = new CSharpCodeProvider())
            {
                var parameters = new CompilerParameters()
                {
                    GenerateExecutable = false,
                    GenerateInMemory = false,
                    OutputAssembly = outPath
                };

                var src = $"[assembly: System.Reflection.AssemblyVersion(\"{version}\")] namespace {name} {{ public class Dummy {{ }} }}";

                var res = provider.CompileAssemblyFromSource(parameters, src);

                if (res.Errors.HasErrors)
                {
                    throw new Exception(string.Join(Environment.NewLine, res.Output));
                }
            }

            return outPath;
        }

        private LocalFolderReferencesResolverTest CreateLocalFolderResolver(AssemblyNamePart_e filter,
            AssemblyFilter[] assmFilter = null, string[] reqAssmDirs = null, AssemblyFilter[] reqAssmFilter = null)
            => new LocalFolderReferencesResolverTest(AppDomain.CurrentDomain, new LocalFolderReferenceResolverParameters()
            {
                SearchDirectory = m_TestDir,
                MatchFilter = filter,
                AssemblyFilter = assmFilter,
                RequestingAssemblyDirectories = reqAssmDirs,
                RequestingAssemblyFilter = reqAssmFilter
            });

        [Test]
        public void LocalFolder_ExactMatchInSubFolderTest()
        {
            var name = NewAssemblyName();
            var path = CompileAssembly(name, "1.0.0.0", Path.Combine("sub1", "sub2"));

            using (var resolver = CreateLocalFolderResolver(AssemblyNamePart_e.FullName))
            {
                var assm = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name, "1.0.0.0"), null);

                Assert.IsNotNull(assm);
                Assert.AreEqual("1.0.0.0", assm.GetName().Version.ToString());
                Assert.That(string.Equals(path, assm.Location, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Test]
        public void LocalFolder_FullNameVersionMismatchTest()
        {
            var name = NewAssemblyName();
            CompileAssembly(name, "1.0.0.0");

            using (var resolver = CreateLocalFolderResolver(AssemblyNamePart_e.FullName))
            {
                var assm = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name, "2.0.0.0"), null);

                Assert.IsNull(assm);
            }
        }

        [Test]
        public void LocalFolder_VersionAllowNewerTest()
        {
            var name = NewAssemblyName();
            CompileAssembly(name, "2.0.0.0");

            using (var resolver = CreateLocalFolderResolver(AssemblyNamePart_e.VersionAllowNewer))
            {
                var newerAssm = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name, "3.0.0.0"), null);
                var olderAssm = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name, "1.0.0.0"), null);

                Assert.IsNull(newerAssm);
                Assert.IsNotNull(olderAssm);
                Assert.AreEqual("2.0.0.0", olderAssm.GetName().Version.ToString());
            }
        }

        [Test]
        public void LocalFolder_AssemblyFilterTest()
        {
            var name1 = NewAssemblyName();
            var name2 = NewAssemblyName();
            CompileAssembly(name1, "1.0.0.0");
            CompileAssembly(name2, "1.0.0.0");

            using (var resolver = CreateLocalFolderResolver(AssemblyNamePart_e.FullName,
                new AssemblyFilter[] { new AssemblyFilter(new AssemblyName(name1), 0) }))
            {
                var assm1 = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name1, "1.0.0.0"), null);
                var assm2 = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name2, "1.0.0.0"), null);

                Assert.IsNotNull(assm1);
                Assert.IsNull(assm2);
            }
        }

        [Test]
        public void LocalFolder_RequestingAssemblyDirectoryTest()
        {
            var name = NewAssemblyName();
            CompileAssembly(name, "1.0.0.0");

            var reqAssm = GetType().Assembly;
            var reqAssmDir = Path.GetDirectoryName(reqAssm.Location);

            Assembly assmOtherDir;
            Assembly assmReqDir;

            using (var resolver = CreateLocalFolderResolver(AssemblyNamePart_e.FullName, reqAssmDirs: new string[] { m_TestDir }))
            {
                assmOtherDir = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name, "1.0.0.0"), reqAssm);
            }

            using (var resolver = CreateLocalFolderResolver(AssemblyNamePart_e.FullName, reqAssmDirs: new string[] { reqAssmDir }))
            {
                assmReqDir = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name, "1.0.0.0"), reqAssm);
            }

            Assert.IsNull(assmOtherDir);
            Assert.IsNotNull(assmReqDir);
        }

        [Test]
        public void LocalFolder_RequestingAssemblyFilterTest()
        {
            var depName = NewAssemblyName();
            var myAddInName = NewAssemblyName();
            var thirdPartyAddInName = NewAssemblyName();

            CompileAssembly(depName, "1.0.0.0");

            var myAddIn = Assembly.LoadFrom(CompileAssembly(myAddInName, "1.0.0.0", Path.Combine("addins", "my")));
            var thirdPartyAddIn = Assembly.LoadFrom(CompileAssembly(thirdPartyAddInName, "1.0.0.0", Path.Combine("addins", "thirdparty")));

            using (var resolver = CreateLocalFolderResolver(AssemblyNamePart_e.FullName,
                reqAssmFilter: new AssemblyFilter[] { new AssemblyFilter(new AssemblyName(myAddInName), 0) }))
            {
                var thirdPartyAssm = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(depName, "1.0.0.0"), thirdPartyAddIn);
                var myAssm = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(depName, "1.0.0.0"), myAddIn);

                Assert.IsNull(thirdPartyAssm);
                Assert.IsNotNull(myAssm);
                Assert.AreEqual(depName, myAssm.GetName().Name);
            }
        }

        [Test]
        public void LocalFolder_AlreadyLoadedAssemblyTest()
        {
            var name = NewAssemblyName();
            CompileAssembly(name, "1.0.0.0");

            using (var resolver = CreateLocalFolderResolver(AssemblyNamePart_e.FullName))
            {
                var assm1 = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name, "1.0.0.0"), null);
                var assm2 = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name, "1.0.0.0"), null);

                Assert.IsNotNull(assm1);
                Assert.AreSame(assm1, assm2);
            }
        }

        [Test]
        public void LocalFolder_AppDomainAssemblyResolveEventTest()
        {
            var name1 = NewAssemblyName();
            var name2 = NewAssemblyName();
            CompileAssembly(name1, "1.0.0.0");
            CompileAssembly(name2, "1.0.0.0");

            Assembly assm1;

            using (var resolver = CreateLocalFolderResolver(AssemblyNamePart_e.FullName))
            {
                assm1 = Assembly.Load(CreateName(name1, "1.0.0.0"));
            }

            Assert.IsNotNull(assm1);
            Assert.AreEqual(name1, assm1.GetName().Name);

            Assert.Throws<FileNotFoundException>(() => Assembly.Load(CreateName(name2, "1.0.0.0")));
        }

        [Test]
        public void BindingMap_MappedFileTest()
        {
            var name = NewAssemblyName();
            var path = CompileAssembly(name, "1.0.0.0", "mapped", "renamed");

            var assmName = CreateName(name, "1.0.0.0");

            using (var resolver = new BindingMapReferencesResolverTest(AppDomain.CurrentDomain, new BindingMapReferenceResolverParameters()
            {
                SearchDirectory = m_TestDir,
                MatchFilter = AssemblyNamePart_e.FullName,
                Map = new Dictionary<AssemblyName, string>()
                {
                    { assmName, Path.Combine("mapped", "renamed.dll") }
                }
            }))
            {
                var assm = resolver.ResolveTest(AppDomain.CurrentDomain, assmName, null);

                Assert.IsNotNull(assm);
                Assert.That(string.Equals(path, assm.Location, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Test]
        public void AppConfig_BindingRedirectTest()
        {
            var name = NewAssemblyName();
            CompileAssembly(name, "2.0.0.0");

            var appConfigPath = Path.Combine(m_TestDir, "app.config");

            File.WriteAllText(appConfigPath,
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <dependentAssembly>
        <assemblyIdentity name=""{name}"" culture=""neutral"" />
        <bindingRedirect oldVersion=""0.0.0.0-1.9.9.9"" newVersion=""2.0.0.0"" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>");

            using (var resolver = new AppConfigBindingRedirectReferenceResolverTest(appConfigPath))
            {
                var outOfRangeAssm = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name, "3.0.0.0"), null);
                var redirectedAssm = resolver.ResolveTest(AppDomain.CurrentDomain, CreateName(name, "1.5.0.0"), null);

                Assert.IsNull(outOfRangeAssm);
                Assert.IsNotNull(redirectedAssm);
                Assert.AreEqual("2.0.0.0", redirectedAssm.GetName().Version.ToString());
            }
        }
    }
}
