using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xarial.XToolkit.Reporting;

namespace Utils.Tests
{
    [TestFixture]
    public class FileLogWriterTests
    {
        private static readonly Guid m_AppId = Guid.Parse("6C1E9E4A-2B7D-4F0B-9C55-3A1D2E8F7B10");
        private static readonly Guid m_OtherAppId = Guid.Parse("A0F3B6C2-8D41-4E7A-B2C9-5F6E1D3A4B20");

        private string m_TempDir;
        private string m_LogDir;

        [SetUp]
        public void Setup()
        {
            m_TempDir = Path.Combine(Path.GetTempPath(), "XToolkitLogWriterTests_" + Guid.NewGuid().ToString("N"));
            m_LogDir = Path.Combine(m_TempDir, "Logs");

            Directory.CreateDirectory(m_LogDir);
        }

        [TearDown]
        public void TearDown()
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

        #region Helpers

        private static string GetSignature(Guid appId) => $"###!!!LOG:{appId}!!!###";

        private static string CreateFile(string dir, string name, string content)
        {
            var path = Path.Combine(dir, name);
            File.WriteAllBytes(path, new UTF8Encoding(false).GetBytes(content));
            File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddDays(-30));
            return path;
        }

        private static string CreateLogFile(string dir, string name, Guid appId)
            => CreateFile(dir, name, GetSignature(appId) + Environment.NewLine + "log line");

        private FileLogWriter CreateWriter(string filePath, bool append)
            => new FileLogWriter(filePath, "Tests", m_AppId, new FileLogOptions(append: append));

        private static void LogAndDispose(FileLogWriter writer, string msg)
        {
            try
            {
                writer.Log(msg, LogMessageSeverity_e.Error);
            }
            finally
            {
                writer.Dispose();
            }
        }

        #endregion

        #region Existing files

        [TestCase(false)]
        [TestCase(true)]
        public void ExistingNonLogFile_NotModified(bool append)
        {
            var file = CreateFile(m_LogDir, "report.log", "important user data");
            var before = File.ReadAllBytes(file);

            LogAndDispose(CreateWriter(file, append), "message");

            Assert.That(File.ReadAllBytes(file), Is.EqualTo(before));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExistingOtherAppLog_NotModified(bool append)
        {
            var file = CreateLogFile(m_LogDir, "app.log", m_OtherAppId);
            var before = File.ReadAllBytes(file);

            LogAndDispose(CreateWriter(file, append), "message");

            Assert.That(File.ReadAllBytes(file), Is.EqualTo(before));
        }

        [Test]
        public void ExistingOwnLog_Overwrite_Replaced()
        {
            var file = CreateLogFile(m_LogDir, "app.log", m_AppId);

            LogAndDispose(CreateWriter(file, false), "new message");

            var content = File.ReadAllText(file);

            Assert.That(content, Does.StartWith(GetSignature(m_AppId)));
            Assert.That(content, Does.Contain("new message"));
            Assert.That(content, Does.Not.Contain("log line"));
        }

        [Test]
        public void ExistingOwnLog_Append_Appended()
        {
            var file = CreateLogFile(m_LogDir, "app.log", m_AppId);

            LogAndDispose(CreateWriter(file, true), "new message");

            var content = File.ReadAllText(file);

            Assert.That(content, Does.StartWith(GetSignature(m_AppId)));
            Assert.That(content, Does.Contain("log line"));
            Assert.That(content, Does.Contain("new message"));
        }

        [Test]
        public void ExistingEmptyFile_SignatureWritten()
        {
            var file = CreateFile(m_LogDir, "app.log", "");

            LogAndDispose(CreateWriter(file, true), "message");

            Assert.That(File.ReadAllText(file), Does.StartWith(GetSignature(m_AppId)));
        }

        [Test]
        public void ExistingReadOnlyNonLogFile_NotModified()
        {
            var file = CreateFile(m_LogDir, "report.log", "important user data");
            File.SetAttributes(file, FileAttributes.ReadOnly);

            var before = File.ReadAllBytes(file);

            Assert.DoesNotThrow(() => LogAndDispose(CreateWriter(file, false), "message"));

            Assert.That(File.ReadAllBytes(file), Is.EqualTo(before));
        }

        #endregion

        #region Short names

        [TestCase(false)]
        [TestCase(true)]
        public void ExistingNonLogFileAddressedByShortName_NotModified(bool append)
        {
            var file = CreateFile(m_LogDir, "important_user_report.log", "important user data");
            var shortPath = ShortPathHelper.GetShortPathOrIgnore(file);

            var before = File.ReadAllBytes(file);

            LogAndDispose(CreateWriter(shortPath, append), "message");

            Assert.That(File.ReadAllBytes(file), Is.EqualTo(before));
        }

        [Test]
        public void ExistingOwnLogAddressedByShortName_Append_Appended()
        {
            var file = CreateLogFile(m_LogDir, "application_session.log", m_AppId);
            var shortPath = ShortPathHelper.GetShortPathOrIgnore(file);

            LogAndDispose(CreateWriter(shortPath, true), "new message");

            var content = File.ReadAllText(file);

            Assert.That(content, Does.StartWith(GetSignature(m_AppId)));
            Assert.That(content, Does.Contain("log line"));
            Assert.That(content, Does.Contain("new message"));
        }

        [Test]
        public void NewFileInShortDirPath_Created()
        {
            var shortTempDir = ShortPathHelper.GetShortPathOrIgnore(m_TempDir);

            LogAndDispose(CreateWriter(Path.Combine(shortTempDir, "Logs", "app.log"), false), "message");

            Assert.That(File.ReadAllText(Path.Combine(m_LogDir, "app.log")), Does.Contain("message"));
        }

        #endregion

        #region New files

        [Test]
        public void NewFile_SignatureAndMessageWritten()
        {
            var file = Path.Combine(m_LogDir, "app.log");

            LogAndDispose(CreateWriter(file, false), "message");

            var content = File.ReadAllText(file);

            Assert.That(content, Does.StartWith(GetSignature(m_AppId)));
            Assert.That(content, Does.Contain("message"));
        }

        [Test]
        public void NewFileInMissingFolder_FolderCreated()
        {
            var file = Path.Combine(m_LogDir, "Sub", "app.log");

            LogAndDispose(CreateWriter(file, false), "message");

            Assert.That(File.Exists(file), Is.True);
        }

        [Test]
        public void FileNotCreatedUntilFirstMessage()
        {
            var file = Path.Combine(m_LogDir, "app.log");

            var writer = CreateWriter(file, false);

            try
            {
                Assert.That(File.Exists(file), Is.False);
            }
            finally
            {
                writer.Dispose();
            }
        }

        #endregion

        #region Other files in folder

        [TestCase(false)]
        [TestCase(true)]
        public void ExistingOtherLogsInFolder_NotModified(bool append)
        {
            var ownOld = CreateLogFile(m_LogDir, "rt_old_x.log", m_AppId);
            var unsigned = CreateFile(m_LogDir, "rt_user_x.log", "user content");
            var otherApp = CreateLogFile(m_LogDir, "rt_other_x.log", m_OtherAppId);

            var before = new[] { ownOld, unsigned, otherApp }.ToDictionary(f => f, f => File.ReadAllBytes(f));

            LogAndDispose(CreateWriter(Path.Combine(m_LogDir, "rt_new_x.log"), append), "message");

            foreach (var file in before)
            {
                Assert.That(File.Exists(file.Key), Is.True, file.Key);
                Assert.That(File.ReadAllBytes(file.Key), Is.EqualTo(file.Value), file.Key);
            }
        }

        #endregion

        #region Arguments

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("app.log")]
        [TestCase(@"Logs\app.log")]
        [TestCase(@"\Logs\app.log")]
        [TestCase("/Logs/app.log")]
        [TestCase("C:app.log")]
        [TestCase(@"C:\Logs\app.log:stream")]
        [TestCase(@"C:\Logs\app.log::$DATA")]
        [TestCase(@"C:\Logs:x\app.log")]
        [TestCase(@"C:\Logs\a:b.log")]
        [TestCase(@"%XT_UNDEFINED_TEST_VAR%\app.log")]
        public void InvalidPath_Throws(string filePath)
        {
            Assert.Catch<ArgumentException>(() => new FileLogWriter(filePath, "Tests", m_AppId));
        }

        [TestCase("app")]
        [TestCase("app.")]
        [TestCase("app.log.")]
        [TestCase(@"Sub\")]
        public void PathWithoutExtension_Throws(string fileName)
        {
            var filePath = Path.Combine(m_LogDir, fileName);

            Assert.Catch<ArgumentException>(() => new FileLogWriter(filePath, "Tests", m_AppId));
            Assert.That(File.Exists(filePath), Is.False);
        }

        [TestCase(".log")]
        [TestCase(" .log")]
        [TestCase("  .log")]
        [TestCase(@"Sub\.log")]
        public void PathWithoutFileName_Throws(string fileName)
        {
            var filePath = Path.Combine(m_LogDir, fileName);

            Assert.Catch<ArgumentException>(() => new FileLogWriter(filePath, "Tests", m_AppId));
            Assert.That(File.Exists(filePath), Is.False);
        }

        [TestCase("app.log")]
        [TestCase("app.txt")]
        [TestCase("app.1.log")]
        [TestCase("a.log")]
        public void PathWithExtension_DoesNotThrow(string fileName)
        {
            Assert.DoesNotThrow(() => LogAndDispose(CreateWriter(Path.Combine(m_LogDir, fileName), false), "message"));
        }

        [Test]
        public void EmptyAppId_Throws()
        {
            Assert.Catch<ArgumentException>(() => new FileLogWriter(Path.Combine(m_LogDir, "app.log"), "Tests", Guid.Empty));
        }

        #endregion
    }
}