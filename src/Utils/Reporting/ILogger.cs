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
            logger.Log(GetExceptionContent(ex, logCallStack));
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
