using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Xarial.XToolkit.Reporting
{
    /// <summary>
    /// Options for <see cref="FileLogWriter"/>
    /// </summary>
    public class FileLogOptions 
    {
        internal const string DEFAULT_TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// Fitler for messages (null to filter all)
        /// </summary>
        public LogMessageSeverity_e[] Filter { get; set; }

        /// <summary>
        /// Add time stamp to log message
        /// </summary>
        public bool AddTimeStamp { get; set; }

        /// <summary>
        /// Format of time stamp
        /// </summary>
        public string TimeStampFormat { get; set; }

        /// <summary>
        /// Append to a log file or create new
        /// </summary>
        public bool Append { get; set; }

        /// <param name="addTimeStamp">Add time stamp to log message</param>
        /// <param name="timeStampFormat">Format of time stamp</param>
        /// <param name="append">Append to a log file or create new</param>
        /// <param name="filter">Fitler for messages (null to filter all)</param>
        public FileLogOptions(bool addTimeStamp = true,
            string timeStampFormat = DEFAULT_TIMESTAMP_FORMAT, bool append = false, LogMessageSeverity_e[] filter = null)
        { 
            AddTimeStamp = addTimeStamp;
            TimeStampFormat = timeStampFormat;
            Append = append;
            Filter = filter;
        }
    }

    /// <summary>
    /// File-based logger
    /// </summary>
    public class FileLogWriter : TraceLogWriter
    {
        internal static string GetSignature(Guid appId)
        {
            if (appId.Equals(Guid.Empty))
            {
                throw new ArgumentException("AppId is not specified");
            }

            return string.Format(SIGNATURE, appId);
        }

        internal static bool StartsWithSignature(Stream stream, byte[] signature)
        {
            if (signature.Length > 0)
            {
                var buffer = new byte[signature.Length];

                var read = 0;

                while (read < buffer.Length)
                {
                    var chunk = stream.Read(buffer, read, buffer.Length - read);

                    if (chunk == 0)
                    {
                        break;
                    }

                    read += chunk;
                }

                if (read < buffer.Length)
                {
                    return false;
                }

                for (int i = 0; i < signature.Length; i++)
                {
                    if (buffer[i] != signature[i])
                    {
                        return false;
                    }
                }

                return true;
            }
            else 
            {
                return false;
            }
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

                if (path.IndexOf(':', root.Length) != -1)
                {
                    return false;
                }
            }

            return true;
        }

        internal const string SIGNATURE = "###!!!LOG:{0}!!!###";

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
        /// <param name="opts">Log options</param>
        public FileLogWriter(string filePath, string category, Guid appId, FileLogOptions opts) : base(category, true, opts?.Filter)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            filePath = Environment.ExpandEnvironmentVariables(filePath);

            ValidatePath(filePath);

            m_AddTimeStamp = opts?.AddTimeStamp ?? false;
            m_TimeStampFormat = opts?.TimeStampFormat;

            m_Lock = new object();

            FilePath = filePath;

            m_DirPath = Path.GetDirectoryName(filePath);

            if (string.IsNullOrEmpty(m_DirPath))
            {
                throw new ArgumentException("Log file path must include a directory", nameof(filePath));
            }

            if (string.IsNullOrEmpty(Path.GetExtension(filePath)))
            {
                throw new ArgumentException("Log file path must include a file extension", nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(filePath)))
            {
                throw new ArgumentException("Log file path must include a file name", nameof(filePath));
            }

            m_Append = opts?.Append ?? false;

            m_Signature = GetSignature(appId);

            if (string.IsNullOrEmpty(m_Signature))
            {
                throw new ArgumentNullException(nameof(m_Signature));
            }
        }

        /// <inheritdoc/>
        public FileLogWriter(string filePath, string category, Guid appId)
            : this(filePath, category, appId, new FileLogOptions(true, FileLogOptions.DEFAULT_TIMESTAMP_FORMAT, false))
        {
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

                var stream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                try
                {
                    if (stream.Length > 0 && !StartsWithSignature(stream, Encoding.UTF8.GetBytes(m_Signature)))
                    {
                        throw new IOException($"File '{FilePath}' already exists and is not a log file of this application");
                    }

                    if (m_Append)
                    {
                        stream.Seek(0, SeekOrigin.End);
                    }
                    else
                    {
                        stream.SetLength(0);
                    }
                }
                catch
                {
                    stream.Dispose();
                    throw;
                }

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
        public override void Log(string msg, LogMessageSeverity_e severity)
        {
            if (IsEnabled(severity))
            {
                base.Log(msg, severity);

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
                                    msg = $"[{DateTime.Now.ToString(m_TimeStampFormat, CultureInfo.InvariantCulture)}] [{severity}] {msg}";
                                }

                                m_Writer.WriteLine(msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            base.Log(LogWriterExtension.GetExceptionContent(ex, false), LogMessageSeverity_e.Error);
                        }
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
