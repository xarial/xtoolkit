using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xarial.XToolkit.Reporting;

namespace Utils.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class FileLogRetentionTests
    {
        #region Test infrastructure

        private static class TestScope
        {
            internal static string TestDir { get; private set; }
            internal static Guid AppId { get; private set; }

            internal static List<string> OutOfScope { get; } = new List<string>();
            internal static List<string> Deleted { get; } = new List<string>();
            internal static int CleanerCalls { get; set; }

            internal static void Reset(string testDir, Guid appId)
            {
                TestDir = Path.GetFullPath(testDir);
                AppId = appId;
                OutOfScope.Clear();
                Deleted.Clear();
                CleanerCalls = 0;
            }
        }

        private class ScopedCleaner : FileLogCleaner
        {
            internal ScopedCleaner(string dirPath, string appSignature, string categoryName) : base(dirPath, appSignature, categoryName)
            {
            }

            protected override bool DeleteFile(FileInfo file, string filter)
            {
                var fullPath = Path.GetFullPath(file.FullName);

                if (fullPath.StartsWith(TestScope.TestDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    var res = base.DeleteFile(file, filter);

                    if (res)
                    {
                        TestScope.Deleted.Add(fullPath);
                    }

                    return res;
                }
                else 
                {
                    TestScope.OutOfScope.Add(fullPath);
                    return false;
                }
            }
        }

        private class TestLogWriter : FileLogWriter
        {
            internal TestLogWriter(string filePath, string category, Guid appId, FileLogOptions opts)
                : base(filePath, category, appId, opts)
            {
            }

            protected override void ClearLogFiles(FileLogOptions opts, string signature, string category)
            {
                if (opts?.RetentionPolicy != null)
                {
                    TestScope.CleanerCalls++;

                    new ScopedCleaner(Path.GetDirectoryName(FilePath), signature, category)
                        .TryClear(opts.RetentionPolicy);
                }
            }
        }

        #endregion

        private static readonly Guid m_AppId = Guid.Parse("2B3B3EEB-CD59-4EC3-B52D-6048D02E245D");
        private static readonly Guid m_OtherAppId = Guid.Parse("5C2CF4D2-461B-470A-986D-AED5662BF280");

        private const string FILE_PATTERN = "tstlg_*_tmp.log";
        private const string CATEGORY = "Tests";

        private string m_TempDir;
        private string m_LogDir;
        private string m_OutsideDir;

        private Dictionary<string, byte[]> m_OutsideSnapshot;

        [SetUp]
        public void Setup()
        {
            m_TempDir = Path.Combine(Path.GetTempPath(), "XToolkitLogRetentionTests_" + Guid.NewGuid().ToString("N"));
            m_LogDir = Path.Combine(m_TempDir, "Logs");
            m_OutsideDir = Path.Combine(m_TempDir, "Outside");

            Directory.CreateDirectory(m_LogDir);
            Directory.CreateDirectory(m_OutsideDir);

            CreateLogFile(m_OutsideDir, GetLogFileName("sentinel1"), m_AppId, 100);
            CreateFile(m_OutsideDir, "user.txt", "user content", 100);

            var siblingDir = m_LogDir + "2";
            Directory.CreateDirectory(siblingDir);
            CreateLogFile(siblingDir, GetLogFileName("sentinel2"), m_AppId, 100);

            m_OutsideSnapshot = GetSnapshotOutsideLogFolder();

            TestScope.Reset(m_TempDir, m_AppId);
        }

        [TearDown]
        public void TearDown()
        {
            var outOfScope = TestScope.OutOfScope.ToArray();
            var after = GetSnapshotOutsideLogFolder();

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

        private static string GetLogFileName(string session) => $"tstlg_{session}_tmp.log";

        private static string CreateFile(string dir, string name, string content, int ageDays)
        {
            var path = Path.Combine(dir, name);
            File.WriteAllBytes(path, new UTF8Encoding(false).GetBytes(content));
            File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddDays(-ageDays));
            return path;
        }

        private static string CreateLogFile(string dir, string name, Guid appId, int ageDays)
            => CreateFile(dir, name, GetSignature(appId) + Environment.NewLine + "log line", ageDays);

        private Dictionary<string, byte[]> GetSnapshotOutsideLogFolder()
        {
            var logDir = Path.GetFullPath(m_LogDir) + Path.DirectorySeparatorChar;

            return Directory.EnumerateFiles(m_TempDir, "*", SearchOption.AllDirectories)
                .Where(f => !Path.GetFullPath(f).StartsWith(logDir, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(f => Path.GetFullPath(f), f => File.ReadAllBytes(f), StringComparer.OrdinalIgnoreCase);
        }

        private string RunSession(string session, FileLogRetentionPolicy policy, int ageDays = 0)
        {
            var filePath = Path.Combine(m_LogDir, GetLogFileName(session));

            var writer = new TestLogWriter(filePath, CATEGORY, m_AppId,
                new FileLogOptions(append: false, retentionPolicy: policy));

            try
            {
                writer.Log("message", LogMessageSeverity_e.Error);
            }
            finally
            {
                writer.Dispose();
            }

            File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow.AddDays(-ageDays));

            return filePath;
        }

        private string[] GetLogFileNames()
            => Directory.GetFiles(m_LogDir).Select(f => Path.GetFileName(f)).OrderBy(f => f).ToArray();

        #endregion

        #region Retention rules

        [Test]
        public void NoRetentionPolicy_NothingDeleted()
        {
            var old1 = CreateLogFile(m_LogDir, GetLogFileName("old1"), m_AppId, 100);
            var old2 = CreateLogFile(m_LogDir, GetLogFileName("old2"), m_AppId, 99);

            RunSession("new", null);

            Assert.That(File.Exists(old1), Is.True);
            Assert.That(File.Exists(old2), Is.True);
            Assert.That(TestScope.CleanerCalls, Is.Zero);
            Assert.That(TestScope.Deleted, Is.Empty);
        }

        [Test]
        public void MaxFileCount_KeepsNewestLogsOfEachRun()
        {
            var policy = new FileLogRetentionPolicy(FILE_PATTERN, 3);

            var run1 = RunSession("run1", policy, 5);
            RunSession("run2", policy, 4);
            RunSession("run3", policy, 3);
            RunSession("run4", policy, 2);
            RunSession("run5", policy, 1);

            Assert.That(GetLogFileNames(), Is.EqualTo(new[]
            {
                GetLogFileName("run2"),
                GetLogFileName("run3"),
                GetLogFileName("run4"),
                GetLogFileName("run5")
            }));

            Assert.That(TestScope.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(run1) }));
        }

        [Test]
        public void ExpiryPeriod_DeletesOnlyExpiredLogs()
        {
            var expired = CreateLogFile(m_LogDir, GetLogFileName("expired"), m_AppId, 11);
            var fresh = CreateLogFile(m_LogDir, GetLogFileName("fresh"), m_AppId, 1);

            RunSession("new", new FileLogRetentionPolicy(FILE_PATTERN, null, TimeSpan.FromDays(10)));

            Assert.That(File.Exists(expired), Is.False);
            Assert.That(File.Exists(fresh), Is.True);
            Assert.That(TestScope.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(expired) }));
        }

        [Test]
        public void ClientConfiguration_DeletesOnlyOwnMatchingLogs()
        {
            var ownOldLog = CreateLogFile(m_LogDir, GetLogFileName("old"), m_AppId, 20);
            var unsigned = CreateFile(m_LogDir, GetLogFileName("user"), "user content", 20);
            var otherApp = CreateLogFile(m_LogDir, GetLogFileName("other"), m_OtherAppId, 20);
            var otherName = CreateLogFile(m_LogDir, "report.log", m_AppId, 20);

            RunSession("new", new FileLogRetentionPolicy(FILE_PATTERN, 500, TimeSpan.FromDays(10)));

            Assert.That(File.Exists(ownOldLog), Is.False);
            Assert.That(File.Exists(unsigned), Is.True);
            Assert.That(File.Exists(otherApp), Is.True);
            Assert.That(File.Exists(otherName), Is.True);
            Assert.That(TestScope.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(ownOldLog) }));
        }

        [Test]
        public void MaxFilesSize_DeletesUntilWithinLimit()
        {
            var newest = CreateLogFile(m_LogDir, GetLogFileName("newest"), m_AppId, 1);
            var older = CreateLogFile(m_LogDir, GetLogFileName("older"), m_AppId, 2);
            var oldest = CreateLogFile(m_LogDir, GetLogFileName("oldest"), m_AppId, 3);

            RunSession("new", new FileLogRetentionPolicy(FILE_PATTERN, null, null, new FileInfo(newest).Length));

            Assert.That(File.Exists(newest), Is.True);
            Assert.That(File.Exists(older), Is.False);
            Assert.That(File.Exists(oldest), Is.False);
            Assert.That(TestScope.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(older), Path.GetFullPath(oldest) }));
        }

        #endregion

        #region Cleaner invocation

        [Test]
        public void RetentionPolicy_CleanerInvokedOncePerWriter()
        {
            var policy = new FileLogRetentionPolicy(FILE_PATTERN, 3);

            RunSession("run1", policy);
            RunSession("run2", policy);

            Assert.That(TestScope.CleanerCalls, Is.EqualTo(2));
            Assert.That(TestScope.Deleted, Is.Empty);
        }

        [Test]
        public void CleaningHappensEvenIfNothingIsLogged()
        {
            var oldLog = CreateLogFile(m_LogDir, GetLogFileName("old"), m_AppId, 20);

            var writer = new TestLogWriter(Path.Combine(m_LogDir, GetLogFileName("new")), CATEGORY, m_AppId,
                new FileLogOptions(retentionPolicy: new FileLogRetentionPolicy(FILE_PATTERN, 0)));

            writer.Dispose();

            Assert.That(File.Exists(oldLog), Is.False);
            Assert.That(GetLogFileNames(), Is.Empty);
            Assert.That(TestScope.CleanerCalls, Is.EqualTo(1));
            Assert.That(TestScope.Deleted, Is.EquivalentTo(new[] { Path.GetFullPath(oldLog) }));
        }

        [Test]
        public void InvalidRetentionPolicy_ThrowsAndDeletesNothing()
        {
            var oldLog = CreateLogFile(m_LogDir, GetLogFileName("old"), m_AppId, 20);

            Assert.Catch<ArgumentException>(() => new TestLogWriter(Path.Combine(m_LogDir, GetLogFileName("new")),
                CATEGORY, m_AppId, new FileLogOptions(retentionPolicy: new FileLogRetentionPolicy(FILE_PATTERN, null))));

            Assert.That(File.Exists(oldLog), Is.True);
            Assert.That(TestScope.Deleted, Is.Empty);
        }

        #endregion

        #region Concurrent writers

        [Test]
        public void ActiveLogOfAnotherWriter_NotDeleted()
        {
            var activeFilePath = Path.Combine(m_LogDir, GetLogFileName("active"));

            var activeWriter = new TestLogWriter(activeFilePath, CATEGORY, m_AppId, new FileLogOptions());

            try
            {
                activeWriter.Log("message", LogMessageSeverity_e.Error);

                RunSession("new", new FileLogRetentionPolicy(FILE_PATTERN, 0));

                Assert.That(File.Exists(activeFilePath), Is.True);
                Assert.That(TestScope.Deleted, Is.Empty);
            }
            finally
            {
                activeWriter.Dispose();
            }
        }

        [Test]
        public void CurrentLogFile_NotDeletedByNextMessages()
        {
            var filePath = Path.Combine(m_LogDir, GetLogFileName("run"));

            var writer = new TestLogWriter(filePath, CATEGORY, m_AppId,
                new FileLogOptions(append: false, retentionPolicy: new FileLogRetentionPolicy(FILE_PATTERN, 0)));

            try
            {
                writer.Log("first", LogMessageSeverity_e.Error);
                writer.Log("second", LogMessageSeverity_e.Error);

                string content;

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        content = reader.ReadToEnd();
                    }
                }

                Assert.That(content, Does.Contain("first"));
                Assert.That(content, Does.Contain("second"));
                Assert.That(TestScope.Deleted, Is.Empty);
            }
            finally
            {
                writer.Dispose();
            }
        }

        #endregion
    }
}