using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;

namespace Xarial.XToolkit.Reporting
{
    /// <summary>
    /// File-based logger
    /// </summary>
    public class FileLogger : TraceLogger
    {
        internal static string GetSignature(Guid appId)
        {
            if (appId.Equals(Guid.Empty))
            {
                throw new ArgumentException("AppId is not specified");
            }

            return string.Format(SIGNATURE, appId);
        }

        internal static void ValidatePath(string path)
        {
            if (!IsValidPath(path))
            {
                throw new ArgumentException(
                    $@"Path '{path}' must be a full (rooted) path, e.g. 'C:\Logs\app.log'. Relative paths, drive-relative paths ('\Logs\app.log') and unresolved environment variables are not supported");
            }
        }

        private static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (path.IndexOf('%') != -1)
            {
                return false;
            }

            if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                return false;
            }

            if (!Path.IsPathRooted(path))
            {
                return false;
            }

            if (Path.DirectorySeparatorChar == '\\')
            {
                var root = Path.GetPathRoot(path);

                if (root == "\\" || root == "/")
                {
                    return false;
                }

                if (root.Length == 2 && root[1] == ':')
                {
                    return false;
                }
            }

            return true;
        }

        internal const string SIGNATURE = "###!!!LOG:{0}!!!###";

        private const string DEFAULT_TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// Path to log file
        /// </summary>
        public string FilePath { get; }

        private readonly object m_Lock;

        private readonly bool m_AddTimeStamp;
        private readonly string m_TimeStampFormat;

        private readonly bool m_Append;
        private readonly string m_Signature;
        private readonly string m_DirPath;

        private StreamWriter m_Writer;

        private bool m_WriterInitFailed;

        private bool m_IsDisposed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path to log file</param>
        /// <param name="category">Log category</param>
        /// <param name="appId">Application id</param>
        /// <param name="addTimeStamp">Add time stamp to log message</param>
        /// <param name="timeStampFormat">Format of tiem stamp</param>
        /// <param name="append">Append to a log file or create new</param>
        /// <param name="retentionPolicy">retention policy for log files</param>
        public FileLogger(string filePath, string category, Guid appId, bool addTimeStamp = true,
            string timeStampFormat = DEFAULT_TIMESTAMP_FORMAT, bool append = false, FileLoggerRetentionPolicy retentionPolicy = null) : base(category, true)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            filePath = Environment.ExpandEnvironmentVariables(filePath);

            ValidatePath(filePath);

            m_AddTimeStamp = addTimeStamp;
            m_TimeStampFormat = timeStampFormat;

            m_Lock = new object();

            FilePath = filePath;

            m_DirPath = Path.GetDirectoryName(FilePath);

            if (string.IsNullOrEmpty(m_DirPath))
            {
                throw new ArgumentException("Log file path must include a directory", nameof(filePath));
            }

            m_Append = append;
            m_Signature = GetSignature(appId);

            if (retentionPolicy != null)
            {
                var logCleaner = new FileLoggerCleaner(m_DirPath, m_Signature, category);
                logCleaner.TryClear(retentionPolicy);
            }
        }

        private void EnsureWriter()
        {
            if (m_Writer != null || m_WriterInitFailed)
            {
                return;
            }

            StreamWriter writer = null;

            try
            {
                Directory.CreateDirectory(m_DirPath);

                var stream = new FileStream(FilePath,
                    m_Append ? FileMode.Append : FileMode.Create,
                    FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);

                writer = new StreamWriter(stream, new UTF8Encoding(false))
                {
                    AutoFlush = true
                };

                if (stream.Length == 0)
                {
                    writer.WriteLine(m_Signature);
                }

                m_Writer = writer;
            }
            catch
            {
                m_WriterInitFailed = true;
                writer?.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public FileLogger(string filePath, string category, Guid appId,
            FileLoggerRetentionPolicy retentionPolicy) 
            : this(filePath, category, appId, true, DEFAULT_TIMESTAMP_FORMAT, false, retentionPolicy) 
        {
        }

        /// <inheritdoc/>
        public override void Log(string msg)
        {
            base.Log(msg);

            lock (m_Lock)
            {
                if (!m_IsDisposed)
                {
                    try
                    {
                        EnsureWriter();

                        if (m_Writer != null)
                        {
                            if (m_AddTimeStamp)
                            {
                                msg = $"[{DateTime.Now.ToString(m_TimeStampFormat, CultureInfo.InvariantCulture)}] {msg}";
                            }

                            m_Writer.WriteLine(msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        base.Log(LoggerExtension.GetExceptionContent(ex, false));
                    }
                }
            }
        }

        /// <summary>
        /// Disaposing logger
        /// </summary>
        public void Dispose()
        {
            lock (m_Lock)
            {
                if (!m_IsDisposed)
                {
                    m_IsDisposed = true;
                    m_Writer?.Dispose();
                }
            }
        }
    }
}
