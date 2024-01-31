//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
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
        /// <param name="initalDir">Initial directory</param>
        /// <returns>True if folder is browsed</returns>
        public static bool BrowseFolder(out string path, string desc = "", string initalDir = "")
        {
            if (BrowseFolder(out var paths, desc, false, initalDir))
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

        public static bool BrowseFolders(out string[] paths, string desc = "", string initalDir = "")
            => BrowseFolder(out paths, desc, true, initalDir);

        public static bool BrowseFileOpen(out string path, string title = "", string filter = "", string initalDir = "", string initialFile = "")
            => BrowseFileOpen(out path, out _, title, filter, initalDir, initialFile);

        public static bool BrowseFileOpen(out string path, out int filterIndex, string title = "", string filter = "", string initalDir = "", string initialFile = "")
        {
            var res = BrowseForFile(out string[] paths, new OpenFileDialog(), title, filter, initalDir, initialFile, out filterIndex);
            path = paths?.FirstOrDefault();
            return res;
        }

        public static bool BrowseFilesOpen(out string[] paths, string title = "", string filter = "", string initalDir = "", string initialFile = "")
            => BrowseFilesOpen(out paths, out _, title, filter, initalDir, initialFile);

        public static bool BrowseFilesOpen(out string[] paths, out int filterIndex, string title = "", string filter = "", string initalDir = "", string initialFile = "")
            => BrowseForFile(out paths, new OpenFileDialog() { Multiselect = true }, title, filter, initalDir, initialFile, out filterIndex);

        public static bool BrowseFileSave(out string path, string title = "", string filter = "", string initalDir = "", string initialFile = "")
            => BrowseFileSave(out path, out _, title, filter, initalDir, initialFile);

        public static bool BrowseFileSave(out string path, out int filterIndex, string title = "", string filter = "", string initalDir = "", string initialFile = "")
        {
            var res = BrowseForFile(out string[] paths, new SaveFileDialog(), title, filter, initalDir, initialFile, out filterIndex);
            path = paths?.FirstOrDefault();
            return res;
        }

        private static bool BrowseForFile(out string[] paths, FileDialog dlg, string title, string filter,
            string initialDir, string initialFile, out int filterIndex)
        {
            using (dlg)
            {
                dlg.Filter = filter;
                dlg.Title = title;
                
                if (!string.IsNullOrEmpty(initialDir))
                {
                    dlg.InitialDirectory = initialDir;
                }

                if (!string.IsNullOrEmpty(initialFile))
                {
                    dlg.FileName = initialFile;
                }

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

        private static bool BrowseFolder(out string[] paths, string desc, bool multiselect, string initialDir)
        {
            using (var dlg = new AdvancedFolderBrowseDialog())
            {
                dlg.Title = desc;
                dlg.Multiselect = multiselect;
                
                if (!string.IsNullOrEmpty(initialDir))
                {
                    dlg.InitialDirectory = initialDir;
                }

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
