using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Xarial.XToolkit
{
    /// <summary>
    /// Provides utilities working with text
    /// </summary>
    public static class TextUtils
    {
        /// <summary>
        /// Checks if the specified text matches any of the provided filters
        /// </summary>
        /// <param name="text">FilePath</param>
        /// <param name="filters">Filters</param>
        /// <returns></returns>
        public static bool MatchesAnyFilter(string text, params string[] filters)
        {
            if (filters?.Any() == false)
            {
                return true;
            }
            else
            {
                const string ANY_FILTER = "*";

                return filters.Any(f =>
                {
                    var regex = (f.StartsWith(ANY_FILTER) ? "" : "^")
                        + Regex.Escape(f).Replace($"\\{ANY_FILTER}", ".*").Replace("\\?", ".")
                        + (f.EndsWith(ANY_FILTER) ? "" : "$");

                    return Regex.IsMatch(text, regex, RegexOptions.IgnoreCase);
                });
            }
        }
    }
}
