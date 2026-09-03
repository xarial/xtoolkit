using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XToolkit.Reporting
{
    /// <summary>
    /// Utility to clear log files
    /// </summary>
    public interface IFileLoggerCleaner
    {
        /// <summary>
        /// Retention policy for clearing logs
        /// </summary>
        /// <param name="policy">Policy</param>
        void TryClear(FileLoggerRetentionPolicy policy);
    }

    /// <summary>
    /// Clear log retention policy
    /// </summary>
    public class FileLoggerRetentionPolicy
    {
        /// <summary>
        /// Search pattern of the log files
        /// </summary>
        public string SearchPattern { get; set; }

        /// <summary>
        /// Maximum number of files to keep (null - unlimited)
        /// </summary>
        public int? MaxFileCount { get; set; }

        /// <summary>
        /// Delete files older than this (null - unlimited)
        /// </summary>
        public TimeSpan? ExpiryPeriod { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public FileLoggerRetentionPolicy()
        {
        }

        /// <summary>
        /// Constructor with retention policy parameters
        /// </summary>
        /// <param name="searchPattern">Search pattern of the log files</param>
        /// <param name="maxFilesCount">Maximum number of files to keep (null - unlimited)</param>
        /// <param name="expiryPeriod">Delete files older than this (null - unlimited)</param>
        public FileLoggerRetentionPolicy(string searchPattern, int? maxFilesCount = 10, TimeSpan? expiryPeriod = null)
        {
            SearchPattern = searchPattern;
            MaxFileCount = maxFilesCount;
            ExpiryPeriod = expiryPeriod;
        }
    }

    /// <inheritdoc/>
    public class FileLoggerCleaner : IFileLoggerCleaner
    {
        private readonly string m_DirPath;
        private readonly string m_CategoryName;

        private readonly byte[] m_Signature;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dirPath">Log directory file path</param>
        /// <param name="appId">Application id</param>
        /// <param name="categoryName">category name</param>
        public FileLoggerCleaner(string dirPath, Guid appId, string categoryName)
            : this(dirPath, FileLogger.GetSignature(appId), categoryName)
        {
        }

        internal FileLoggerCleaner(string dirPath, string appSignature, string categoryName)
        {
            if (string.IsNullOrEmpty(appSignature))
            {
                throw new ArgumentNullException(nameof(appSignature));
            }

            m_DirPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(dirPath));
            m_CategoryName = categoryName;

            m_Signature = Encoding.ASCII.GetBytes(appSignature);
        }

        /// <inheritdoc/>
        public void TryClear(FileLoggerRetentionPolicy policy)
        {
            if (!policy.MaxFileCount.HasValue && !policy.ExpiryPeriod.HasValue)
            {
                throw new ArgumentException("Specify maximum log files count and/or maximum age");
            }

            try
            {
                if (!string.IsNullOrEmpty(m_DirPath) && Directory.Exists(m_DirPath))
                {
                    var files = new DirectoryInfo(m_DirPath).EnumerateFiles(policy.SearchPattern)
                        .OrderByDescending(f => f.LastWriteTimeUtc)
                        .ToArray();

                    var expiryDate = policy.ExpiryPeriod.HasValue ? DateTime.UtcNow - policy.ExpiryPeriod.Value : (DateTime?)null;

                    for (int i = 0; i < files.Length; i++)
                    {
                        var file = files[i];

                        var filePath = file.FullName;

                        var isExcessive = policy.MaxFileCount.HasValue && i >= policy.MaxFileCount.Value;
                        var isExpired = expiryDate.HasValue && file.LastWriteTimeUtc < expiryDate.Value;

                        if (isExcessive || isExpired)
                        {
                            try
                            {
                                if (HasSignature(filePath))
                                {
                                    Trace($"Deleting log file '{filePath}' [excessive: {isExcessive}, expired: {isExpired}]");
                                    DeleteFile(file);
                                }
                                else
                                {
                                    Trace($"Retaining log file '{filePath}' [excessive: {isExcessive}, expired: {isExpired}] - signature mismatch");
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace($"Failed to delete file '{filePath}': {ex.Message}");
                            }
                        }
                        else
                        {
                            Trace($"Retaining log file '{filePath}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace($"Failed to clean log files in '{m_DirPath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Delete log file
        /// </summary>
        /// <param name="file">File</param>
        protected virtual void DeleteFile(FileInfo file)
            => file.Delete();

        private bool HasSignature(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete))
                {
                    var buffer = new byte[m_Signature.Length];

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

                    for (int i = 0; i < buffer.Length; i++)
                    {
                        if (buffer[i] != m_Signature[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void Trace(string message) => System.Diagnostics.Trace.WriteLine(message, m_CategoryName);
    }
}
