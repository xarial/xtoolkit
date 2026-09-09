using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Xarial.XToolkit.Reporting
{
    /// <summary>
    /// Simple logger to output messages to trace window
    /// </summary>
    public class TraceLogWriter : ILogWriter
    {
        private readonly string m_Category;
        private readonly bool m_SingleLine;

        private static readonly Regex m_SplitLineRegex
            = new Regex(@"\r\n?|\n", RegexOptions.Compiled);

        private readonly LogMessageSeverity_e[] m_Filter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="category">Trace category</param>
        /// <param name="singleLine">Force single line</param>
        /// <param name="filter">Filter for messages (null to filter all)</param>
        public TraceLogWriter(string category, bool singleLine = true, LogMessageSeverity_e[] filter = null) 
        {
            m_Category = category;
            m_SingleLine = singleLine;

            m_Filter = filter;
        }

        /// <inheritdoc/>
        public virtual void Log(string msg, LogMessageSeverity_e severity)
        {
            if (IsEnabled(severity))
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    if (m_SingleLine)
                    {
                        WriteLines(severity, m_SplitLineRegex.Split(msg));
                    }
                    else
                    {
                        WriteLines(severity, msg);
                    }
                }
            }
        }

        /// <summary>
        /// Is severity filtered
        /// </summary>
        /// <param name="severity">Severity</param>
        /// <returns>True if severity is enabled</returns>
        protected bool IsEnabled(LogMessageSeverity_e severity) => m_Filter?.Contains(severity) != false;

        /// <summary>
        /// Write line to trace
        /// </summary>
        /// <param name="lines">Lines to write</param>
        /// <param name="severity">Message severity</param>
        protected virtual void WriteLines(LogMessageSeverity_e severity, params string[] lines)
        {
            if (lines != null) 
            {
                foreach (var line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        switch (severity) 
                        {
                            case LogMessageSeverity_e.Trace:
                                Trace.WriteLine(line, m_Category);
                                break;
                            case LogMessageSeverity_e.Information:
                                Trace.TraceInformation(Format(line));
                                break;
                            case LogMessageSeverity_e.Warning:
                                Trace.TraceWarning(Format(line));
                                break;
                            case LogMessageSeverity_e.Error:
                                Trace.TraceError(Format(line));
                                break;

                            case LogMessageSeverity_e.Critical:
                                Trace.TraceError(Format("[Critical] " + line));
                                break;

                            default:
                                Trace.WriteLine(line, m_Category);
                                break;
                        }
                    }
                }
            }
        }

        private string Format(string line)
            => string.IsNullOrEmpty(m_Category) ? line : $"{m_Category}: {line}";
    }
}
