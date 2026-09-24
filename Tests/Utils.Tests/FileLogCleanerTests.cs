using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit;
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

        private Dictionary<string, byte[]> m_OutsideSnapshot;

        [SetUp]
        public void Setup()
        {
            m_TempDir = Path.Combine(Path.GetTempPath(), "XToolkitLogTests_" + Guid.NewGuid().ToString("N"));
            m_LogDir = Path.Combine(m_TempDir, "Logs");
            m_OutsideDir = Path.Combine(m_TempDir, "Outside");

            Directory.CreateDirectory(m_LogDir);
            Directory.CreateDirectory(m_OutsideDir);

            CreateLogFile(m_OutsideDir, "sentinel1.log", m_AppId);
            CreateFile(m_OutsideDir, "user.txt", "user content");

            var siblingDir = m_LogDir + "2";
            Directory.CreateDirectory(siblingDir);
            CreateLogFile(siblingDir, "sentinel2.log", m_AppId);

            m_OutsideSnapshot = GetSnapshotOutsideLogFolder();
        }

        [TearDown]
        public void TearDown()
        {
            var outOfScope = m_Cleaners.SelectMany(c => c.OutOfScope).ToArray();
            var after = GetSnapshotOutsideLogFolder();

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

                var deleted = m_OutsideSnapshot.Keys.Where(f => !after.ContainsKey(f)).ToArray();

                Assert.That(deleted, Is.Empty, "Files outside of the log folder were deleted");

                var modified = m_OutsideSnapshot
                    .Where(f => after.ContainsKey(f.Key) && !after[f.Key].SequenceEqual(f.Value))
                    .Select(f => f.Key)
                    .ToArray();

                Assert.That(modified, Is.Empty, "Files outside of the log folder were modified");
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

        private Dictionary<string, byte[]> GetSnapshotOutsideLogFolder()
        {
            var logDir = Path.GetFullPath(m_LogDir) + Path.DirectorySeparatorChar;

            return Directory.EnumerateFiles(m_TempDir, "*", SearchOption.AllDirectories)
                .Where(f => !Path.GetFullPath(f).StartsWith(logDir, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(f => Path.GetFullPath(f), f => File.ReadAllBytes(f), StringComparer.OrdinalIgnoreCase);
        }

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

            var cleaner = CreateCleaner();
            cleaner.TryClear(DeleteAllPolicy("log_*_x.log"));

            Assert.That(File.Exists(ownLog), Is.False);
            Assert.That(File.Exists(unsigned), Is.True);
            Assert.That(File.Exists(otherApp), Is.True);
            Assert.That(File.Exists(ownInSubDir), Is.True);
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(ownLog) }));
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

            var cleaner = CreateCleaner();
            cleaner.TryClear(DeleteAllPolicy("a.log"));

            Assert.That(File.Exists(matching), Is.False);
            Assert.That(File.Exists(notMatching), Is.True);
            Assert.That(File.Exists(longerExt), Is.True);
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(matching) }));
        }

        [Test]
        public void ClientPattern_DeletesOnlyMatchingLogFiles()
        {
            var log = CreateLogFile(m_LogDir, "tstlg_2026-09-18-10-15-30-123_tmp.log", m_AppId);
            var otherName = CreateLogFile(m_LogDir, "backup_tstlg_x_tmp.log", m_AppId);
            var otherExt = CreateLogFile(m_LogDir, "tstlg_x_tmp.log2", m_AppId);

            var cleaner = CreateCleaner();
            cleaner.TryClear(DeleteAllPolicy("tstlg_*_tmp.log"));

            Assert.That(File.Exists(log), Is.False);
            Assert.That(File.Exists(otherName), Is.True);
            Assert.That(File.Exists(otherExt), Is.True);
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(log) }));
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
        [TestCase(@"C:\Logs:stream")]
        [TestCase(@"C:\Logs:x\Sub")]
        [TestCase(@"C:\Lo:gs")]
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

            var cleaner = new TestCleaner(Path.Combine(m_LogDir, "Sub", ".."), m_AppId, m_TempDir);
            m_Cleaners.Add(cleaner);

            cleaner.TryClear(DeleteAllPolicy("log_*_x.log"));

            Assert.That(File.Exists(own), Is.False);
            Assert.That(File.Exists(unsigned), Is.True);
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(own) }));
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

            var cleaner = CreateCleaner();
            cleaner.TryClear(new FileLogRetentionPolicy("log_*_x.log", 2));

            Assert.That(File.Exists(oldest), Is.False);
            Assert.That(File.Exists(middle), Is.True);
            Assert.That(File.Exists(newest), Is.True);
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(oldest) }));
        }

        [Test]
        public void ExpiryPeriod_DeletesOnlyExpiredFiles()
        {
            var expired = CreateLogFile(m_LogDir, "log_a_x.log", m_AppId);
            var fresh = CreateLogFile(m_LogDir, "log_b_x.log", m_AppId);

            File.SetLastWriteTimeUtc(expired, DateTime.UtcNow.AddDays(-11));
            File.SetLastWriteTimeUtc(fresh, DateTime.UtcNow.AddDays(-1));

            var cleaner = CreateCleaner();
            cleaner.TryClear(new FileLogRetentionPolicy("log_*_x.log", null, TimeSpan.FromDays(10)));

            Assert.That(File.Exists(expired), Is.False);
            Assert.That(File.Exists(fresh), Is.True);
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(expired) }));
        }

        [Test]
        public void ExpiryPeriod_DeletesOnlyOwnSignedFiles()
        {
            var own = CreateLogFile(m_LogDir, "log_a_x.log", m_AppId);
            var unsigned = CreateFile(m_LogDir, "log_b_x.log", "user content");
            var otherApp = CreateLogFile(m_LogDir, "log_c_x.log", m_OtherAppId);
            var otherName = CreateLogFile(m_LogDir, "report.log", m_AppId);

            var cleaner = CreateCleaner();
            cleaner.TryClear(new FileLogRetentionPolicy("log_*_x.log", 500, TimeSpan.FromDays(10)));

            Assert.That(File.Exists(own), Is.False);
            Assert.That(File.Exists(unsigned), Is.True);
            Assert.That(File.Exists(otherApp), Is.True);
            Assert.That(File.Exists(otherName), Is.True);
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(own) }));
        }

        [Test]
        public void MaxFilesSize_DeletesUntilWithinLimit()
        {
            var newest = CreateLogFile(m_LogDir, "log_a_x.log", m_AppId);
            var older = CreateLogFile(m_LogDir, "log_b_x.log", m_AppId);
            var oldest = CreateLogFile(m_LogDir, "log_c_x.log", m_AppId);

            File.SetLastWriteTimeUtc(newest, DateTime.UtcNow.AddDays(-1));
            File.SetLastWriteTimeUtc(older, DateTime.UtcNow.AddDays(-2));
            File.SetLastWriteTimeUtc(oldest, DateTime.UtcNow.AddDays(-3));

            var cleaner = CreateCleaner();
            cleaner.TryClear(new FileLogRetentionPolicy("log_*_x.log", null, null, new FileInfo(newest).Length));

            Assert.That(File.Exists(newest), Is.True);
            Assert.That(File.Exists(older), Is.False);
            Assert.That(File.Exists(oldest), Is.False);
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(older), Path.GetFullPath(oldest) }));
        }

        [Test]
        public void MaxFilesSize_RetainedUnsignedFilesCountTowardsLimit()
        {
            var unsigned = CreateFile(m_LogDir, "log_a_x.log", "user content which is larger than the limit");
            var own = CreateLogFile(m_LogDir, "log_b_x.log", m_AppId);

            File.SetLastWriteTimeUtc(unsigned, DateTime.UtcNow.AddDays(-1));
            File.SetLastWriteTimeUtc(own, DateTime.UtcNow.AddDays(-2));

            var cleaner = CreateCleaner();
            cleaner.TryClear(new FileLogRetentionPolicy("log_*_x.log", null, null, new FileInfo(own).Length));

            Assert.That(File.Exists(unsigned), Is.True);
            Assert.That(File.Exists(own), Is.False);
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(own) }));
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
                writer.Log("first", LogMessageSeverity_e.Error);

                var cleaner = CreateCleaner();
                cleaner.TryClear(DeleteAllPolicy("active.log"));

                writer.Log("second", LogMessageSeverity_e.Error);

                Assert.That(File.Exists(logFile), Is.True);
                Assert.That(cleaner.Deleted, Is.Empty);

                string content;

                using (var stream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        content = reader.ReadToEnd();
                    }
                }

                Assert.That(content, Does.Contain("first"));
                Assert.That(content, Does.Contain("second"));
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

            var cleaner = CreateCleaner();
            cleaner.TryClear(DeleteAllPolicy("link.log"));

            Assert.That(File.Exists(link), Is.False);
            Assert.That(File.Exists(target), Is.True);
            Assert.That(File.ReadAllText(target), Does.StartWith(GetSignature(m_AppId)));
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(link) }));
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

            var cleaner = CreateCleaner();
            cleaner.TryClear(DeleteAllPolicy("link.log"));

            Assert.That(File.Exists(link), Is.False);
            Assert.That(File.Exists(target), Is.True);
            Assert.That(File.ReadAllText(target), Does.StartWith(GetSignature(m_AppId)));
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(link) }));
        }

        #endregion

        #region Short names

        [Test]
        public void PatternMatchingOnlyShortName_NotDeleted()
        {
            var file = CreateLogFile(m_LogDir, "abcdefgh_report.log2", m_AppId);

            var shortName = Path.GetFileName(ShortPathHelper.GetShortPathOrIgnore(file));

            Assert.That(TextUtils.MatchesAnyFilter(shortName, "abc*1.log"), Is.True, $"Unexpected short name '{shortName}'");

            var cleaner = CreateCleaner();
            cleaner.TryClear(DeleteAllPolicy("abc*1.log"));

            Assert.That(File.Exists(file), Is.True);
            Assert.That(cleaner.Deleted, Is.Empty);
        }

        [Test]
        public void PatternMatchingOnlyShortName_NotCountedTowardsMaxFileCount()
        {
            var shortOnly = CreateLogFile(m_LogDir, "abcdefgh_report.log2", m_AppId);
            var own = CreateLogFile(m_LogDir, "abc_run_1.log", m_AppId);

            var shortName = Path.GetFileName(ShortPathHelper.GetShortPathOrIgnore(shortOnly));

            Assert.That(TextUtils.MatchesAnyFilter(shortName, "abc*1.log"), Is.True, $"Unexpected short name '{shortName}'");

            File.SetLastWriteTimeUtc(shortOnly, DateTime.UtcNow.AddDays(-1));
            File.SetLastWriteTimeUtc(own, DateTime.UtcNow.AddDays(-2));

            var cleaner = CreateCleaner();
            cleaner.TryClear(new FileLogRetentionPolicy("abc*1.log", 1));

            Assert.That(File.Exists(shortOnly), Is.True);
            Assert.That(File.Exists(own), Is.True);
            Assert.That(cleaner.Deleted, Is.Empty);
        }

        [Test]
        public void ShortDirPath_DeletesOnlyOwnSignedFiles()
        {
            var own = CreateLogFile(m_LogDir, "log_a_x.log", m_AppId);
            var unsigned = CreateFile(m_LogDir, "log_b_x.log", "user content");

            var shortTempDir = ShortPathHelper.GetShortPathOrIgnore(m_TempDir);
            var shortLogDir = Path.Combine(shortTempDir, Path.GetFileName(m_LogDir));

            var cleaner = new TestCleaner(shortLogDir, m_AppId, shortTempDir);
            m_Cleaners.Add(cleaner);

            cleaner.TryClear(DeleteAllPolicy("log_*_x.log"));

            Assert.That(File.Exists(own), Is.False);
            Assert.That(File.Exists(unsigned), Is.True);
            Assert.That(cleaner.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(Path.Combine(shortLogDir, "log_a_x.log")) }));
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

            var res = CreateCleaner().CallDeleteFile(new FileInfo(file), filter);

            Assert.That(res, Is.False);
            Assert.That(File.Exists(file), Is.True);
        }

        #endregion
    }
}
