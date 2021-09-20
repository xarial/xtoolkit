//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Xarial.XToolkit
{
    public static class FileSystemUtils
    {
        /// <summary>
        /// Checks if the specified file matches any of the provided filters
        /// </summary>
        /// <param name="filPath">FilePath</param>
        /// <param name="filters">Filters</param>
        /// <returns></returns>
        public static bool MatchesAnyFilter(string filePath, params string[] filters)
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

                    return Regex.IsMatch(filePath, regex, RegexOptions.IgnoreCase);
                });
            }
        }

        /// <summary>
        /// Combines the directory paths
        /// </summary>
        /// <param name="srcPath">Start path</param>
        /// <param name="additionalPaths">Additional path parts</param>
        /// <returns>Combined path</returns>
        /// <remarks>This method works with relative path, including moving the upper fodlers via ..</remarks>
        public static string CombinePaths(string srcPath, params string[] additionalPaths) 
        {
            var pathParts = new List<string>();

            var addedRoot = "";

            if (!Path.IsPathRooted(srcPath))
            {
                addedRoot = @"C:\";
                pathParts.Add(Path.Combine(addedRoot, srcPath));
            }
            else 
            {
                pathParts.Add(srcPath);
            }

            foreach (var path in additionalPaths) 
            {
                pathParts.Add(path.TrimStart('\\'));
            }

            var combinedPath = new Uri(Path.Combine(pathParts.ToArray())).LocalPath;

            if (!string.IsNullOrEmpty(addedRoot))
            {
                combinedPath = combinedPath.Substring(addedRoot.Length);
            }

            return combinedPath;
        }

        /// <summary>
        /// Excludes all sub level folders and only returns top level folders
        /// </summary>
        /// <param name="paths">Input directory paths</param>
        /// <returns>Top level folders paths</returns>
        public static string[] GetTopFolders(IEnumerable<string> paths)
        {
            var result = new List<string>();

            foreach (var path in paths.OrderBy(p => p))
            {
                if (!result.Any(r => IsInDirectory(path, r)))
                {
                    result.Add(path);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Checks if the specified directoryis in the other directory
        /// </summary>
        /// <param name="thisDir">Directory to check</param>
        /// <param name="parentDir">Directory to check agains</param>
        /// <returns>True of directory is within another directory</returns>
        public static bool IsInDirectory(string thisDir, string parentDir)
        {
            string NormalizePath(string path) => path.TrimEnd('\\') + "\\";

            return NormalizePath(thisDir).StartsWith(NormalizePath(parentDir),
                    StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Opens file explorer at the specified folder
        /// </summary>
        /// <param name="path"></param>
        public static void BrowseFolderInExplorer(string path) 
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        /// <summary>
        /// Opens file explorer and selects specified file
        /// </summary>
        /// <param name="path"></param>
        public static void BrowseFileInExplorer(string path)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                UseShellExecute = true,
                Arguments = $"/select, \"{path}\""
            });
        }
    }
}
