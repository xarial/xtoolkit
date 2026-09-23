using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reporting;

namespace Utils.Tests
{
    [TestFixture]
    public class FileLogCleanerTests
    {
        private class TestCleaner : FileLogCleaner
        {
            private readonly string m_TestDir;
            private readonly List<string> m_OutOfScope;
            private readonly List<string> m_Deleted;

            internal IReadOnlyList<string> OutOfScope => m_OutOfScope;

            internal IReadOnlyList<string> Deleted => m_Deleted;

            internal TestCleaner(string dirPath, Guid appId, string testDir) : base(dirPath, appId, "Tests")
            {
                m_TestDir = testDir;
                m_OutOfScope = new List<string>();
                m_Deleted = new List<string>();
            }

            protected override bool DeleteFile(FileInfo file, string filter)
            {
                var fullPath = Path.GetFullPath(file.FullName);

                if (fullPath.StartsWith(Path.GetFullPath(m_TestDir) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    var res = base.DeleteFile(file, filter);

                    if (res)
                    {
                        m_Deleted.Add(fullPath);
                    }

                    return res;
                }
                else 
                {
                    m_OutOfScope.Add(fullPath);
                    return false;
                }
            }

            internal bool CallDeleteFile(FileInfo file, string filter) => DeleteFile(file, filter);
        }

        private static readonly Guid m_AppId = Guid.Parse("13A28660-35CD-4D91-964B-2931CF3D70D5");
        private static readonly Guid m_OtherAppId = Guid.Parse("008676EE-5325-4DC9-B824-EBA53D8C2E00");

        private string m_TempDir;
        private string m_LogDir;
        private string m_OutsideDir;

        [SetUp]
        public void Setup()
        {
            m_TempDir = Path.Combine(Path.GetTempPath(), "XToolkitLogTests_" + Guid.NewGuid().ToString("N"));
            m_LogDir = Path.Combine(m_TempDir, "Logs");
            m_OutsideDir = Path.Combine(m_TempDir, "Outside");

            Directory.CreateDirectory(m_LogDir);
            Directory.CreateDirectory(m_OutsideDir);
        }

        [TearDown]
        public void TearDown()
        {
            var outOfScope = m_Cleaners.SelectMany(c => c.OutOfScope).ToArray();

            m_Cleaners.Clear();

            try
            {
                if (Directory.Exists(m_TempDir))
                {
                    foreach (var file in Directory.EnumerateFiles(m_TempDir, "*", SearchOption.AllDirectories))
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                    }

                    Directory.Delete(m_TempDir, true);
                }
            }
            finally
            {
                Assert.That(outOfScope, Is.Empty, "Cleaner attempted to delete files outside of the test folder");
            }
        }

        #region Helpers

        private static string GetSignature(Guid appId) => $"###!!!LOG:{appId}!!!###";

        private static string CreateFile(string dir, string name, byte[] content)
        {
            var path = Path.Combine(dir, name);
            File.WriteAllBytes(path, content);
            File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddDays(-30));
            return path;
        }

        private static string CreateFile(string dir, string name, string content)
            => CreateFile(dir, name, new UTF8Encoding(false).GetBytes(content));

        private static string CreateLogFile(string dir, string name, Guid appId)
            => CreateFile(dir, name, GetSignature(appId) + Environment.NewLine + "log line");

        
        private readonly List<TestCleaner> m_Cleaners = new List<TestCleaner>();

        private TestCleaner CreateCleaner() 
        {
            var cleaner = new TestCleaner(m_LogDir, m_AppId, m_TempDir);
            m_Cleaners.Add(cleaner);
            return cleaner;
        }

        private static FileLogRetentionPolicy DeleteAllPolicy(string pattern) => new FileLogRetentionPolicy(pattern, 0);

        private static bool TryMkLink(string args)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", "/c mklink " + args)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var prc = Process.Start(psi))
                {
                    prc.WaitForExit();
                    return prc.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Signature

        [Test]
        public void DeleteAllPolicy_DeletesOnlyOwnSignedFilesInFolder()
        {
            var ownLog = CreateLogFile(m_LogDir, "log_a_x.log", m_AppId);
            var unsigned = CreateFile(m_LogDir, "log_b_x.log", "some user content");
            var otherApp = CreateLogFile(m_LogDir, "log_c_x.log", m_OtherAppId);

            var subDir = Path.Combine(m_LogDir, "Sub");
            Directory.CreateDirectory(subDir);
            var ownInSubDir = CreateLogFile(subDir, "log_d_x.log", m_AppId);

            CreateCleaner().TryClear(DeleteAllPolicy("log_*_x.log"));

            Assert.That(File.Exists(ownLog), Is.False);
            Assert.That(File.Exists(unsigned), Is.True);
            Assert.That(File.Exists(otherApp), Is.True);
            Assert.That(File.Exists(ownInSubDir), Is.True);
        }

        [TestCase("")]
        [TestCase("#")]
        [TestCase("###!!!LOG:")]
        public void FileShorterThanSignature_NotDeleted(string content)
        {
            var file = CreateFile(m_LogDir, "a.log", content);

            CreateCleaner().TryClear(DeleteAllPolicy("a.log"));

            Assert.That(File.Exists(file), Is.True);
        }

        [Test]
        public void SignatureNotAtStart_NotDeleted()
        {
            var file = CreateFile(m_LogDir, "a.log", "header" + Environment.NewLine + GetSignature(m_AppId));

            CreateCleaner().TryClear(DeleteAllPolicy("a.log"));

            Assert.That(File.Exists(file), Is.True);
        }

        [Test]
        public void SignatureWithBom_NotDeleted()
        {
            var file = CreateFile(m_LogDir, "a.log", new UTF8Encoding(true).GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(GetSignature(m_AppId))).ToArray());

            CreateCleaner().TryClear(DeleteAllPolicy("a.log"));

            Assert.That(File.Exists(file), Is.True);
        }

        [Test]
        public void SignatureWithUpperCaseGuid_NotDeleted()
        {
            var file = CreateFile(m_LogDir, "a.log", $"###!!!LOG:{m_AppId.ToString().ToUpperInvariant()}!!!###");

            CreateCleaner().TryClear(DeleteAllPolicy("a.log"));

            Assert.That(File.Exists(file), Is.True);
        }

        #endregion

        #region Pattern

        [Test]
        public void SignedFileNotMatchingPattern_NotDeleted()
        {
            var matching = CreateLogFile(m_LogDir, "a.log", m_AppId);
            var notMatching = CreateLogFile(m_LogDir, "b.txt", m_AppId);

            var longerExt = CreateLogFile(m_LogDir, "c.log2", m_AppId);

            CreateCleaner().TryClear(DeleteAllPolicy("a.log"));

            Assert.That(File.Exists(matching), Is.False);
            Assert.That(File.Exists(notMatching), Is.True);
            Assert.That(File.Exists(longerExt), Is.True);
        }

        [Test]
        public void ClientPattern_DeletesOnlyMatchingLogFiles()
        {
            var log = CreateLogFile(m_LogDir, "tstlg_2026-09-18-10-15-30-123_tmp.log", m_AppId);
            var otherName = CreateLogFile(m_LogDir, "backup_tstlg_x_tmp.log", m_AppId);
            var otherExt = CreateLogFile(m_LogDir, "tstlg_x_tmp.log2", m_AppId);

            CreateCleaner().TryClear(DeleteAllPolicy("tstlg_*_tmp.log"));

            Assert.That(File.Exists(log), Is.False);
            Assert.That(File.Exists(otherName), Is.True);
            Assert.That(File.Exists(otherExt), Is.True);
        }

        [TestCase(@"Sub\*.log")]
        [TestCase(@"..\*.log")]
        [TestCase(@"..\Outside\*.log")]
        [TestCase("/*.log")]
        [TestCase("C:*.log")]
        [TestCase(@"C:\*.log")]
        public void PatternWithPath_DeletesNothing(string pattern)
        {
            var own = CreateLogFile(m_LogDir, "a.log", m_AppId);
            var outside = CreateLogFile(m_OutsideDir, "b.log", m_AppId);

            var subDir = Path.Combine(m_LogDir, "Sub");
            Directory.CreateDirectory(subDir);
            var inSubDir = CreateLogFile(subDir, "c.log", m_AppId);

            var cleaner = CreateCleaner();

            try
            {
                cleaner.TryClear(DeleteAllPolicy(pattern));
            }
            catch (ArgumentException)
            {
            }

            Assert.That(File.Exists(own), Is.True);
            Assert.That(File.Exists(outside), Is.True);
            Assert.That(File.Exists(inSubDir), Is.True);
            Assert.That(cleaner.Deleted, Is.Empty);
        }

        [TestCase("*")]
        [TestCase("*.*")]
        [TestCase("*.log")]
        [TestCase("log*.txt")]
        [TestCase("log")]
        [TestCase("log.")]
        [TestCase("a.??")]
        public void OverlyBroadOrMissingExtensionPattern_ThrowsAndDeletesNothing(string pattern)
        {
            var own = CreateLogFile(m_LogDir, "a.log", m_AppId);

            var cleaner = CreateCleaner();

            Assert.Catch<ArgumentException>(() => cleaner.TryClear(DeleteAllPolicy(pattern)));

            Assert.That(File.Exists(own), Is.True);
            Assert.That(cleaner.Deleted, Is.Empty);
        }

        [TestCase("a.log")]
        [TestCase("tstlg_*_tmp.log")]
        [TestCase("log_*_x.log")]
        public void StrictPattern_DoesNotThrow(string pattern)
        {
            Assert.DoesNotThrow(() => CreateCleaner().TryClear(DeleteAllPolicy(pattern)));
        }

        #endregion

        #region Arguments

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("Logs")]
        [TestCase(@"Logs\Sub")]
        [TestCase(@"\Logs")]
        [TestCase("/Logs")]
        [TestCase("C:Logs")]
        [TestCase(@"%XT_UNDEFINED_TEST_VAR%\Logs")]
        public void InvalidDirPath_Throws(string dirPath)
        {
            Assert.Catch<ArgumentException>(() => new FileLogCleaner(dirPath, m_AppId, "Tests"));
        }

        [Test]
        public void EmptyAppId_Throws()
        {
            Assert.Catch<ArgumentException>(() => new FileLogCleaner(m_LogDir, Guid.Empty, "Tests"));
        }

        [Test]
        public void NullPolicy_Throws()
        {
            Assert.Catch<ArgumentNullException>(() => CreateCleaner().TryClear(null));
        }

        [Test]
        public void InvalidPolicy_ThrowsAndDeletesNothing()
        {
            var own = CreateLogFile(m_LogDir, "a.log", m_AppId);

            var cleaner = CreateCleaner();

            Assert.Catch<ArgumentException>(() => cleaner.TryClear(new FileLogRetentionPolicy("*", null)));
            Assert.Catch<ArgumentException>(() => cleaner.TryClear(new FileLogRetentionPolicy("*", -1)));
            Assert.Catch<ArgumentException>(() => cleaner.TryClear(new FileLogRetentionPolicy("*", null, null, -1)));
            Assert.Catch<ArgumentException>(() => cleaner.TryClear(new FileLogRetentionPolicy("", 0)));
            Assert.Catch<ArgumentException>(() => cleaner.TryClear(new FileLogRetentionPolicy("  ", 0)));
            Assert.Catch<ArgumentException>(() => cleaner.TryClear(new FileLogRetentionPolicy(null, 0)));

            Assert.That(File.Exists(own), Is.True);
            Assert.That(cleaner.Deleted, Is.Empty);
        }

        [Test]
        public void NonExistingFolder_DoesNotThrow()
        {
            var cleaner = new FileLogCleaner(Path.Combine(m_TempDir, "Missing"), m_AppId, "Tests");

            Assert.DoesNotThrow(() => cleaner.TryClear(DeleteAllPolicy("a.log")));
        }

        [Test]
        public void PathWithDotDot_CleansOnlySignedFiles()
        {
            var own = CreateLogFile(m_LogDir, "log_a_x.log", m_AppId);
            var unsigned = CreateFile(m_LogDir, "log_b_x.log", "user content");

            var cleaner = new FileLogCleaner(Path.Combine(m_LogDir, "Sub", ".."), m_AppId, "Tests");
            cleaner.TryClear(DeleteAllPolicy("log_*_x.log"));

            Assert.That(File.Exists(own), Is.False);
            Assert.That(File.Exists(unsigned), Is.True);
        }

        #endregion

        #region Retention rules

        [Test]
        public void MaxFileCount_KeepsNewestFiles()
        {
            var oldest = CreateLogFile(m_LogDir, "log_a_x.log", m_AppId);
            var middle = CreateLogFile(m_LogDir, "log_b_x.log", m_AppId);
            var newest = CreateLogFile(m_LogDir, "log_c_x.log", m_AppId);

            File.SetLastWriteTimeUtc(oldest, DateTime.UtcNow.AddDays(-3));
            File.SetLastWriteTimeUtc(middle, DateTime.UtcNow.AddDays(-2));
            File.SetLastWriteTimeUtc(newest, DateTime.UtcNow.AddDays(-1));

            CreateCleaner().TryClear(new FileLogRetentionPolicy("log_*_x.log", 2));

            Assert.That(File.Exists(oldest), Is.False);
            Assert.That(File.Exists(middle), Is.True);
            Assert.That(File.Exists(newest), Is.True);
        }

        [Test]
        public void ExpiryPeriod_DeletesOnlyExpiredFiles()
        {
            var expired = CreateLogFile(m_LogDir, "log_a_x.log", m_AppId);
            var fresh = CreateLogFile(m_LogDir, "log_b_x.log", m_AppId);

            File.SetLastWriteTimeUtc(expired, DateTime.UtcNow.AddDays(-11));
            File.SetLastWriteTimeUtc(fresh, DateTime.UtcNow.AddDays(-1));

            CreateCleaner().TryClear(new FileLogRetentionPolicy("log_*_x.log", null, TimeSpan.FromDays(10)));

            Assert.That(File.Exists(expired), Is.False);
            Assert.That(File.Exists(fresh), Is.True);
        }

        #endregion

        #region Locked and protected files

        [Test]
        public void LockedFile_NotDeletedAndDoesNotThrow()
        {
            var file = CreateLogFile(m_LogDir, "a.log", m_AppId);

            using (new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Assert.DoesNotThrow(() => CreateCleaner().TryClear(DeleteAllPolicy("a.log")));
            }

            Assert.That(File.Exists(file), Is.True);
        }

        [Test]
        public void ReadOnlyFile_NotDeletedAndDoesNotThrow()
        {
            var file = CreateLogFile(m_LogDir, "a.log", m_AppId);
            File.SetAttributes(file, FileAttributes.ReadOnly);

            Assert.DoesNotThrow(() => CreateCleaner().TryClear(DeleteAllPolicy("a.log")));

            Assert.That(File.Exists(file), Is.True);
        }

        [Test]
        public void FileOpenByActiveWriter_NotDeleted()
        {
            var logFile = Path.Combine(m_LogDir, "active.log");

            var writer = new FileLogWriter(logFile, "Tests", m_AppId);

            try
            {
                writer.Log("message", LogMessageSeverity_e.Error);

                CreateCleaner().TryClear(DeleteAllPolicy("active.log"));

                Assert.That(File.Exists(logFile), Is.True);
            }
            finally
            {
                writer.Dispose();
            }
        }

        #endregion

        #region Links

        [Test]
        public void SymlinkToSignedFileOutside_DeletesLinkOnly()
        {
            var target = CreateLogFile(m_OutsideDir, "target.log", m_AppId);
            var link = Path.Combine(m_LogDir, "link.log");

            if (!TryMkLink($"\"{link}\" \"{target}\""))
            {
                Assert.Ignore("Creating symbolic links requires administrator rights or Developer Mode");
            }

            CreateCleaner().TryClear(DeleteAllPolicy("link.log"));

            Assert.That(File.Exists(link), Is.False);
            Assert.That(File.Exists(target), Is.True);
            Assert.That(File.ReadAllText(target), Does.StartWith(GetSignature(m_AppId)));
        }

        [Test]
        public void SymlinkToUnsignedFileOutside_NotDeleted()
        {
            var target = CreateFile(m_OutsideDir, "target.log", "user content");
            var link = Path.Combine(m_LogDir, "link.log");

            if (!TryMkLink($"\"{link}\" \"{target}\""))
            {
                Assert.Ignore("Creating symbolic links requires administrator rights or Developer Mode");
            }

            CreateCleaner().TryClear(DeleteAllPolicy("link.log"));

            Assert.That(File.Exists(link), Is.True);
            Assert.That(File.Exists(target), Is.True);
        }

        [Test]
        public void HardLinkToSignedFileOutside_TargetDataPreserved()
        {
            var target = CreateLogFile(m_OutsideDir, "target.log", m_AppId);
            var link = Path.Combine(m_LogDir, "link.log");

            if (!TryMkLink($"/H \"{link}\" \"{target}\""))
            {
                Assert.Ignore("Failed to create hard link");
            }

            CreateCleaner().TryClear(DeleteAllPolicy("link.log"));

            Assert.That(File.Exists(link), Is.False);
            Assert.That(File.Exists(target), Is.True);
            Assert.That(File.ReadAllText(target), Does.StartWith(GetSignature(m_AppId)));
        }

        #endregion

        #region DeleteFile guards

        [Test]
        public void DeleteFile_FileInSiblingFolderWithSamePrefix_NotDeleted()
        {
            var siblingDir = m_LogDir + "2";
            Directory.CreateDirectory(siblingDir);

            var file = CreateLogFile(siblingDir, "a.log", m_AppId);

            var res = CreateCleaner().CallDeleteFile(new FileInfo(file), "*");

            Assert.That(res, Is.False);
            Assert.That(File.Exists(file), Is.True);
        }

        [Test]
        public void DeleteFile_FileOutsideFolder_NotDeleted()
        {
            var file = CreateLogFile(m_OutsideDir, "a.log", m_AppId);

            var res = CreateCleaner().CallDeleteFile(new FileInfo(file), "*");

            Assert.That(res, Is.False);
            Assert.That(File.Exists(file), Is.True);
        }

        [Test]
        public void DeleteFile_NameNotMatchingFilter_NotDeleted()
        {
            var file = CreateLogFile(m_LogDir, "a.txt", m_AppId);

            var res = CreateCleaner().CallDeleteFile(new FileInfo(file), "*.log");

            Assert.That(res, Is.False);
            Assert.That(File.Exists(file), Is.True);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("  ")]
        [TestCase(null)]
        public void DeleteFile_EmptyFilter_NotDeleted(string filter)
        {
            var file = CreateLogFile(m_LogDir, "a.txt", m_AppId);

            var res = CreateCleaner().CallDeleteFile(new FileInfo(file), "");

            Assert.That(res, Is.False);
            Assert.That(File.Exists(file), Is.True);
        }

        #endregion
    }
}
