using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Xarial.XToolkit.Reporting
{
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
        public virtual void Log(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                if (m_SingleLine)
                {
                    WriteLines(m_SplitLineRegex.Split(msg));
                }
                else
                {
                    WriteLines(msg);
                }
            }
        }

        /// <summary>
        /// Write line to trace
        /// </summary>
        /// <param name="lines">Lines to write</param>
        protected virtual void WriteLines(params string[] lines)
        {
            if (lines != null) 
            {
                foreach (var line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        System.Diagnostics.Trace.WriteLine(line, m_Category);
                    }
                }
            }
        }
    }
}
