//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
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
    /// Options of the <see cref="TextUtils.Like(string, string, LikeOptions_e)"/>
    /// </summary>
    [Flags]
    public enum LikeOptions_e
    {
        /// <summary>
        /// Case sensitive, only <c>*</c> and <c>?</c> are special symbols
        /// </summary>
        None = 0,

        /// <summary>
        /// Compare characters case insensitively
        /// </summary>
        IgnoreCase = 1,

        /// <summary>
        /// <c>#</c> matches any single digit (0-9)
        /// </summary>
        DigitWildcard = 2,

        /// <summary>
        /// <c>[x]</c> matches the literal character x, e.g. <c>[#]</c>, <c>[*]</c>, <c>[?]</c>, <c>[[]</c>
        /// </summary>
        LiteralEscape = 4,

        /// <summary>
        /// Case insensitive with all special symbols
        /// </summary>
        Default = IgnoreCase | DigitWildcard | LiteralEscape
    }

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

        /// <inheritdoc/>
        public static bool MatchesAnyFilter(string text, bool ignoreCase, params string[] filters)
            => MatchesAnyFilter(text, ignoreCase, default(TimeSpan?), filters);

        /// <summary>
        /// Checks if the specified text matches any of the provided filters
        /// </summary>
        /// <param name="text">Text to match</param>
        /// <param name="ignoreCase">Ignore the case</param>
        /// <param name="filters">Filters</param>
        /// <param name="timeout">Timeout or null if no timeout</param>
        /// <returns>True if any of the fitler match the text</returns>
        /// <remarks>This method supports wildcards *. If no filters specified this method returns true</remarks>
        /// <exception cref="RegexMatchTimeoutException">Matching exceeded <paramref name="timeout"/></exception>
        public static bool MatchesAnyFilter(string text, bool ignoreCase, TimeSpan? timeout, params string[] filters)
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

                    if (timeout.HasValue)
                    {
                        return Regex.IsMatch(text, regex, regexOpts, timeout.Value);
                    }
                    else 
                    {
                        return Regex.IsMatch(text, regex, regexOpts);
                    }
                });
            }
        }

        /// <inheritdoc/>
        public static bool MatchesAnyFilter(string text, params string[] filters)
            => MatchesAnyFilter(text, true, filters);

        /// <summary>
        /// Checks if the text matches the pattern
        /// </summary>
        /// <param name="text">Text to match</param>
        /// <param name="pattern">Pattern to match against</param>
        /// <param name="options">Matching options</param>
        /// <returns>True if the text matches the pattern</returns>
        public static bool Like(string text, string pattern, LikeOptions_e options = LikeOptions_e.Default)
        {
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(pattern))
            {
                var ignoreCase = (options & LikeOptions_e.IgnoreCase) != 0;
                var digitWildcard = (options & LikeOptions_e.DigitWildcard) != 0;
                var literalEscape = (options & LikeOptions_e.LiteralEscape) != 0;

                var textIndex = 0;
                var patternIndex = 0;

                var starPatternIndex = -1;
                var starTextIndex = -1;

                while (textIndex < text.Length)
                {
                    if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                    {
                        starPatternIndex = patternIndex++;
                        starTextIndex = textIndex;
                    }
                    else if (patternIndex < pattern.Length && 
                        IsTokenMatch(pattern, patternIndex, text[textIndex],
                        ignoreCase, digitWildcard, literalEscape, out var tokenLength))
                    {
                        patternIndex += tokenLength;
                        textIndex++;
                    }
                    else if (starPatternIndex != -1)
                    {
                        patternIndex = starPatternIndex + 1;
                        textIndex = ++starTextIndex;
                    }
                    else
                    {
                        return false;
                    }
                }

                while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                {
                    patternIndex++;
                }

                return patternIndex == pattern.Length;
            }
            else
            {
                return false;
            }
        }

        private static bool IsTokenMatch(string pattern, int patternIndex, char textChar,
            bool ignoreCase, bool digitWildcard, bool literalEscape, out int tokenLength)
        {
            var patternChar = pattern[patternIndex];

            if (patternChar == '[' && literalEscape
                && patternIndex + 2 < pattern.Length && pattern[patternIndex + 2] == ']')
            {
                tokenLength = 3;
                return CharsEqual(pattern[patternIndex + 1], textChar, ignoreCase);
            }
            else
            {
                tokenLength = 1;

                switch (patternChar)
                {
                    case '?':
                        return true;

                    case '#' when digitWildcard:
                        return textChar >= '0' && textChar <= '9';

                    default:
                        return CharsEqual(patternChar, textChar, ignoreCase);
                }
            }
        }

        private static bool CharsEqual(char a, char b, bool ignoreCase)
            => a == b || (ignoreCase && char.ToUpperInvariant(a) == char.ToUpperInvariant(b));
    }
}
