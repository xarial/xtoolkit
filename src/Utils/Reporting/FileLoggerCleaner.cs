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
        /// Maximum aggregate size (in bytes) of the kept files (null - unlimited).
        /// </summary>
        public long? MaxFilesSize { get; set; }

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
        /// <param name="maxFilesSize">Maximum aggregate size (in bytes) of the kept files (null - unlimited)</param>
        public FileLoggerRetentionPolicy(string searchPattern, int? maxFilesCount = 10, TimeSpan? expiryPeriod = null, long? maxFilesSize = null)
        {
            SearchPattern = searchPattern;
            MaxFileCount = maxFilesCount;
            ExpiryPeriod = expiryPeriod;
            MaxFilesSize = maxFilesSize;
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

            if (string.IsNullOrWhiteSpace(dirPath))
            {
                throw new ArgumentNullException(nameof(dirPath));
            }

            dirPath = Environment.ExpandEnvironmentVariables(dirPath);

            FileLogger.ValidatePath(dirPath);

            m_DirPath = dirPath;

            m_CategoryName = categoryName;

            m_Signature = Encoding.ASCII.GetBytes(appSignature);
        }

        /// <inheritdoc/>
        public void TryClear(FileLoggerRetentionPolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            if (!policy.MaxFileCount.HasValue && !policy.ExpiryPeriod.HasValue && !policy.MaxFilesSize.HasValue)
            {
                throw new ArgumentException("Specify maximum log files count, maximum age and/or maximum aggregate size");
            }

            if (policy.MaxFileCount.HasValue && policy.MaxFileCount.Value < 0)
            {
                throw new ArgumentException("Maximum log files count must not be negative");
            }

            if (policy.MaxFilesSize.HasValue && policy.MaxFilesSize.Value < 0)
            {
                throw new ArgumentException("Maximum aggregate size must not be negative");
            }

            if (string.IsNullOrWhiteSpace(policy.SearchPattern))
            {
                throw new ArgumentException("Empty search pattern is not supported");
            }

            try
            {
                Trace($"Clearing log folder: '{m_DirPath}'");

                if (Directory.Exists(m_DirPath))
                {
                    var files = new DirectoryInfo(m_DirPath).EnumerateFiles(policy.SearchPattern)
                        .OrderByDescending(f => f.LastWriteTimeUtc)
                        .ToArray();

                    var expiryDate = policy.ExpiryPeriod.HasValue ? DateTime.UtcNow - policy.ExpiryPeriod.Value : (DateTime?)null;

                    long retainedSize = 0;

                    for (int i = 0; i < files.Length; i++)
                    {
                        var file = files[i];

                        var filePath = file.FullName;

                        long fileSize;

                        try
                        {
                            fileSize = file.Length;
                        }
                        catch
                        {
                            fileSize = 0;
                        }

                        var isExcessive = policy.MaxFileCount.HasValue && i >= policy.MaxFileCount.Value;
                        var isExpired = expiryDate.HasValue && file.LastWriteTimeUtc < expiryDate.Value;
                        var isOversized = policy.MaxFilesSize.HasValue && retainedSize + fileSize > policy.MaxFilesSize.Value;

                        if (isExcessive || isExpired || isOversized)
                        {
                            try
                            {
                                if (HasSignature(filePath))
                                {
                                    Trace($"Deleting log file '{filePath}' [excessive: {isExcessive}, expired: {isExpired}, oversized: {isOversized}]");
                                    DeleteFile(file);
                                }
                                else
                                {
                                    retainedSize += fileSize;
                                    Trace($"Retaining log file '{filePath}' [excessive: {isExcessive}, expired: {isExpired}, oversized: {isOversized}] - signature mismatch");
                                }
                            }
                            catch (Exception ex)
                            {
                                retainedSize += fileSize;
                                Trace($"Failed to delete file '{filePath}': {ex.Message}");
                            }
                        }
                        else
                        {
                            retainedSize += fileSize;
                            Trace($"Retaining log file '{filePath}'");
                        }
                    }
                }
                else
                {
                    Trace($"Log folder '{m_DirPath}' does not exist - nothing to clean");
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
