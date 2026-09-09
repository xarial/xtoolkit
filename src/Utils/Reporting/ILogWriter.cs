using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xarial.XToolkit.Reporting
{
    /// <summary>
    /// Message severity
    /// </summary>
    public enum LogMessageSeverity_e
    {
        /// <summary>
        /// Trace
        /// </summary>
        Trace,

        /// <summary>
        /// Information
        /// </summary>
        Information,

        /// <summary>
        /// Warning
        /// </summary>
        Warning,

        /// <summary>
        /// Error
        /// </summary>
        Error,

        /// <summary>
        /// Critical
        /// </summary>
        Critical
    }

    /// <summary>
    /// Message logger
    /// </summary>
    public interface ILogWriter
    {
        /// <summary>
        /// Logs message
        /// </summary>
        /// <param name="msg">Content to log</param>
        /// <param name="severity">Message severity</param>
        void Log(string msg, LogMessageSeverity_e severity);
    }

    /// <summary>
    /// Additional methods of <see cref="TraceLogWriter"/>
    /// </summary>
    public static class LogWriterExtension
    {
        /// <summary>
        /// Logs trace message
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        public static void LogTrace(this ILogWriter logger, string message)
            => logger.Log(message, LogMessageSeverity_e.Trace);

        /// <summary>
        /// Logs information message
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        public static void LogInformation(this ILogWriter logger, string message)
            => logger.Log(message, LogMessageSeverity_e.Information);

        /// <summary>
        /// Logs warning message
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        public static void LogWarning(this ILogWriter logger, string message)
            => logger.Log(message, LogMessageSeverity_e.Warning);

        /// <summary>
        /// Logs exception to log
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="ex">Exception to log</param>
        /// <param name="logCallStack">True to log stack trace</param>
        public static void LogError(this ILogWriter logger, Exception ex, bool logCallStack = true)
            => logger.Log(GetExceptionContent(ex, logCallStack), LogMessageSeverity_e.Error);

        /// <summary>
        /// Logs error to log
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        public static void LogError(this ILogWriter logger, string message)
            => logger.Log(message, LogMessageSeverity_e.Error);

        /// <summary>
        /// Logs critical exception to log
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="ex">Exception to log</param>
        /// <param name="logCallStack">True to log stack trace</param>
        public static void LogCritical(this ILogWriter logger, Exception ex, bool logCallStack = true)
            => logger.Log(GetExceptionContent(ex, logCallStack), LogMessageSeverity_e.Critical);

        /// <summary>
        /// Logs critical error to log
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        public static void LogCritical(this ILogWriter logger, string message)
            => logger.Log(message, LogMessageSeverity_e.Critical);

        /// <summary>
        /// Logs debug message
        /// </summary>
        /// <param name="logger">Source logger</param>
        /// <param name="message">Message</param>
        /// <param name="severity">Severity of the debug message</param>
        /// <param name="member">Caller member</param>
        /// <param name="file">Caller file path</param>
        /// <param name="line">Caller line number</param>
        [Conditional("DEBUG")]
        public static void LogDebug(this ILogWriter logger,
            string message = null, LogMessageSeverity_e severity = LogMessageSeverity_e.Trace,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            var logLine = new StringBuilder("<DEBUG>");

            if (line > 0 && !string.IsNullOrEmpty(file))
            {
                logLine.Append($" {file}({line})");
            }

            if (!string.IsNullOrEmpty(member)) 
            {
                logLine.Append($" {member}");
            }

            if (!string.IsNullOrEmpty(message)) 
            {
                if (logLine.Length > 0) 
                {
                    logLine.Append(": ");
                }

                logLine.Append($" {message}");
            }
            
            logger?.Log(logLine.ToString(), severity);
        }

        internal static string GetExceptionContent(Exception ex, bool logCallStack = true) 
        {
            var exContent = new StringBuilder();

            foreach (var line in ParseException(ex, logCallStack))
            {
                exContent.AppendLine(line);
            }

            return exContent.ToString();
        }

        private static IEnumerable<string> ParseException(Exception ex, bool logCallStack)
        {
            if (ex != null)
            {
                yield return $"{ex.GetType().Name}: {ex.Message}";

                if (logCallStack)
                {
                    var stackTrace = ex.StackTrace;

                    if (!string.IsNullOrEmpty(stackTrace)) 
                    {
                        yield return stackTrace;
                    }
                }

                foreach (var inner in IterateInnerExceptions(ex))
                {
                    foreach (var line in ParseException(inner, logCallStack))
                    {
                        yield return line;
                    }
                }
            }
        }

        private static IEnumerable<Exception> IterateInnerExceptions(Exception ex)
        {
            switch (ex)
            {
                case ReflectionTypeLoadException typeLoadEx:
                    if (typeLoadEx.LoaderExceptions != null)
                    {
                        foreach (var loaderEx in typeLoadEx.LoaderExceptions)
                        {
                            if (loaderEx != null)
                            {
                                yield return loaderEx;
                            }
                        }
                    }
                    break;

                case AggregateException aggEx:
                    foreach (var innerEx in aggEx.Flatten().InnerExceptions)
                    {
                        yield return innerEx;
                    }
                    break;

                default:
                    if (ex.InnerException != null)
                    {
                        yield return ex.InnerException;
                    }
                    break;
            }
        }
    }
}
