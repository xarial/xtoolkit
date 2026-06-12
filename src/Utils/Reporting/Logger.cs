using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xarial.XToolkit.Reporting
{
    /// <summary>
    /// Message logger
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs message
        /// </summary>
        /// <param name="msg">Content to log</param>
        void Log(string msg);
    }

    /// <summary>
    /// Simple logger to output messages to trace window
    /// </summary>
    public class TraceLogger : ILogger
    {
        private readonly string m_Category;
        private readonly bool m_SingleLine;

        private readonly Regex m_SplitLineRegex;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="category">Trace category</param>
        /// <param name="singleLine">Force single line</param>
        public TraceLogger(string category, bool singleLine = true) 
        {
            m_Category = category;
            m_SingleLine = singleLine;

            m_SplitLineRegex = new Regex(@"\r\n?|\n", RegexOptions.Compiled);
        }

        /// <inheritdoc/>
        public void Log(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                if (m_SingleLine)
                {
                    foreach (var line in m_SplitLineRegex.Split(msg))
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            WriteLine(line);
                        }
                    }
                }
                else
                {
                    WriteLine(msg);
                }
            }
        }

        /// <summary>
        /// Write line to trace
        /// </summary>
        /// <param name="line">Line to write</param>
        protected virtual void WriteLine(string line) => System.Diagnostics.Trace.WriteLine(line, m_Category);
    }

    /// <summary>
    /// Additional methods of <see cref="TraceLogger"/>
    /// </summary>
    public static class LoggerExtension
    {
        /// <summary>
        /// Logs exception to log
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="ex">Exception to log</param>
        /// <param name="logCallStack">True to log stack trace</param>
        public static void Log(this ILogger logger, Exception ex, bool logCallStack = true)
        {
            var exContent = new StringBuilder();

            foreach (var line in ParseException(ex, logCallStack)) 
            {
                exContent.AppendLine(line);
            }

            logger.Log(exContent.ToString());
        }

        private static IEnumerable<string> ParseException(Exception ex, bool logCallStack)
        {
            if (ex != null)
            {
                yield return ex.Message;

                if (logCallStack)
                {
                    var stackTrace = ex.StackTrace;

                    if (!string.IsNullOrEmpty(stackTrace)) 
                    {
                        yield return stackTrace;
                    }
                }

                if (ex.InnerException != null)
                {
                    foreach (var log in ParseException(ex.InnerException, logCallStack))
                    {
                        yield return log;
                    }
                }
            }
        }
    }
}
