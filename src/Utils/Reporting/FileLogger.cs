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
            => string.Format(SIGNATURE, appId);

        internal const string SIGNATURE = "###!!!LOG:{0}!!!###";

        private const string DEFAULT_TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// Path to log file
        /// </summary>
        public string FilePath { get; }

        private readonly StreamWriter m_Writer;
        private readonly object m_Lock;

        private readonly bool m_AddTimeStamp;
        private readonly string m_TimeStampFormat;

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
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            m_AddTimeStamp = addTimeStamp;
            m_TimeStampFormat = timeStampFormat;

            m_Lock = new object();

            FilePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(filePath));

            var dir = Path.GetDirectoryName(FilePath);

            if (!string.IsNullOrEmpty(dir))
            {
                var signature = GetSignature(appId);

                if (retentionPolicy != null)
                {
                    var logCleaner = new FileLoggerCleaner(dir, signature, category);
                    logCleaner.TryClear(retentionPolicy);
                }

                Directory.CreateDirectory(dir);

                var stream = new FileStream(FilePath,
                    append ? FileMode.Append : FileMode.Create,
                    FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);

                m_Writer = new StreamWriter(stream, new UTF8Encoding(false))
                {
                    AutoFlush = true
                };

                if (stream.Length == 0)
                {
                    m_Writer.WriteLine(signature);
                }
            }
            else
            {
                throw new ArgumentNullException("No directory for log file");
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
                        if (m_AddTimeStamp)
                        {
                            msg = $"[{DateTime.Now.ToString(m_TimeStampFormat, CultureInfo.InvariantCulture)}] {msg}";
                        }

                        m_Writer.WriteLine(msg);
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
                    m_Writer.Dispose();
                }
            }
        }
    }
}
