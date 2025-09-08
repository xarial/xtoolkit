//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Xarial.XToolkit
{
    /// <summary>
    /// Provides utilities working with text
    /// </summary>
    public static class TextUtils
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string x, string y);

        /// <summary>
        /// Compares two strings logically
        /// </summary>
        /// <param name="firstText">First text to compare</param>
        /// <param name="secondText">Second text to compare</param>
        /// <returns>-1 - the firstText precedes the secondText in the sort order.
        ///0 - firstText and secondText in the same position in the sort order
        ///1 - The firstText follows the secondText in the sort order.
        ///</returns>
        public static int CompareLogical(string firstText, string secondText) => StrCmpLogicalW(firstText, secondText);

        /// <summary>
        /// Checks if the specified text matches any of the provided filters
        /// </summary>
        /// <param name="text">Text to match</param>
        /// <param name="ignoreCase">Ignore the case</param>
        /// <param name="filters">Filters</param>
        /// <returns>True if any of the fitler match the text</returns>
        /// <remarks>This method supports wildcards *. If no filters specified this method returns true</remarks>
        public static bool MatchesAnyFilter(string text, bool ignoreCase, params string[] filters)
        {
            if (filters?.Any() != true)
            {
                return true;
            }
            else
            {
                const string ANY_FILTER = "*";

                var regexOpts = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

                return filters.Any(f =>
                {
                    if (string.IsNullOrEmpty(text) && f == ANY_FILTER) 
                    {
                        return false;
                    }

                    var regex = (f.StartsWith(ANY_FILTER) ? "" : "^")
                        + Regex.Escape(f).Replace($"\\{ANY_FILTER}", ".*").Replace("\\?", ".")
                        + (f.EndsWith(ANY_FILTER) ? "" : "$");

                    return Regex.IsMatch(text, regex, regexOpts);
                });
            }
        }

        /// <inheritdoc/>
        public static bool MatchesAnyFilter(string text, params string[] filters)
            => MatchesAnyFilter(text, true, filters);
    }
}
