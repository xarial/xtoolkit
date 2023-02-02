//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.XToolkit.Wpf.Utils
{
    /// <summary>
    /// Utilities for browsing fils in the system
    /// </summary>
    public static class FileSystemBrowser
    {
        /// <summary>
        /// Browse folder
        /// </summary>
        /// <param name="path">Path to the folder</param>
        /// <param name="desc">Title of the browse dialog</param>
        /// <returns>True if folder is browsed</returns>
        public static bool BrowseFolder(out string path, string desc = "")
        {
            if (BrowseFolder(out var paths, desc, false))
            {
                path = paths.First();
                return true;
            }
            else 
            {
                path = null;
                return false;
            }
        }

        public static bool BrowseFolders(out string[] paths, string desc = "")
            => BrowseFolder(out paths, desc, true);

        public static bool BrowseFileOpen(out string path, string title = "", string filter = "")
            => BrowseFileOpen(out path, out _, title, filter);

        public static bool BrowseFileOpen(out string path, out int filterIndex, string title = "", string filter = "")
        {
            var res = BrowseForFile(out string[] paths, new OpenFileDialog(), title, filter, out filterIndex);
            path = paths?.FirstOrDefault();
            return res;
        }

        public static bool BrowseFilesOpen(out string[] paths, string title = "", string filter = "")
            => BrowseFilesOpen(out paths, out _, title, filter);

        public static bool BrowseFilesOpen(out string[] paths, out int filterIndex, string title = "", string filter = "")
            => BrowseForFile(out paths, new OpenFileDialog() { Multiselect = true }, title, filter, out filterIndex);

        public static bool BrowseFileSave(out string path, string title = "", string filter = "")
            => BrowseFileSave(out path, out _, title, filter);

        public static bool BrowseFileSave(out string path, out int filterIndex, string title = "", string filter = "")
        {
            var res = BrowseForFile(out string[] paths, new SaveFileDialog(), title, filter, out filterIndex);
            path = paths?.FirstOrDefault();
            return res;
        }

        private static bool BrowseForFile(out string[] paths, FileDialog dlg, string title, string filter, out int filterIndex)
        {
            using (dlg)
            {
                dlg.Filter = filter;
                dlg.Title = title;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    filterIndex = dlg.FilterIndex - 1;
                    paths = dlg.FileNames;
                    return true;
                }
                else
                {
                    filterIndex = -1;
                    paths = null;
                    return false;
                }
            }
        }

        private static bool BrowseFolder(out string[] paths, string desc, bool multiselect)
        {
            using (var dlg = new AdvancedFolderBrowseDialog())
            {
                dlg.Title = desc;
                dlg.Multiselect = multiselect;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    paths = dlg.FolderNames;
                    return true;
                }
                else
                {
                    paths = null;
                    return false;
                }
            }
        }
    }
}
