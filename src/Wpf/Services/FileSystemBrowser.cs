//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using System.Windows.Forms;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.XToolkit.Wpf.Services
{
    /// <summary>
    /// Represents the instance of the <see cref="IFileSystemBrowser"/> based on the Windows file and folder dialogs
    /// </summary>
    public class FileSystemBrowser : IFileSystemBrowser
    {
        private readonly IParentWindow m_Parent;

        /// <summary>
        /// Constructor
        /// </summary>
        public FileSystemBrowser()
            : this(null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Parent window of the dialogs or null</param>
        public FileSystemBrowser(IParentWindow parent)
        {
            m_Parent = parent;
        }

        /// <inheritdoc/>
        public bool BrowseFolder(out string path, string title = "", string initialDir = "")
        {
            var res = ShowFolderDialog(title, initialDir, false, out var paths);
            path = paths?.FirstOrDefault();
            return res;
        }

        /// <inheritdoc/>
        public bool BrowseFolders(out string[] paths, string title = "", string initialDir = "")
            => ShowFolderDialog(title, initialDir, true, out paths);

        /// <inheritdoc/>
        public bool BrowseFileOpen(out string path, out int filterIndex, string title = "", string filter = "",
            string initialDir = "", string initialFile = "")
        {
            var res = ShowFileDialog(() => new OpenFileDialog(), title, filter, initialDir, initialFile, out var paths, out filterIndex);
            path = paths?.FirstOrDefault();
            return res;
        }

        /// <inheritdoc/>
        public bool BrowseFilesOpen(out string[] paths, out int filterIndex, string title = "", string filter = "",
            string initialDir = "", string initialFile = "")
            => ShowFileDialog(() => new OpenFileDialog() 
            {
                Multiselect = true 
            }, title, filter, initialDir, initialFile, out paths, out filterIndex);

        /// <inheritdoc/>
        public bool BrowseFileSave(out string path, out int filterIndex, string title = "", string filter = "",
            string initialDir = "", string initialFile = "")
        {
            var res = ShowFileDialog(() => new SaveFileDialog(), title, filter, initialDir, initialFile, out var paths, out filterIndex);
            path = paths?.FirstOrDefault();
            return res;
        }

        private bool ShowFileDialog(Func<FileDialog> dlgFact, string title, string filter,
            string initialDir, string initialFile, out string[] paths, out int filterIndex)
        {
            string[] resPaths = null;
            var resFilterIndex = -1;

            var res = m_Parent.InvokeOnWindowThread(() =>
            {
                using (var dlg = dlgFact.Invoke())
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

                    if (dlg.ShowDialog(new Win32Window(GetOwnerHandle())) == DialogResult.OK)
                    {
                        resFilterIndex = dlg.FilterIndex - 1;
                        resPaths = dlg.FileNames;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            });

            paths = resPaths;
            filterIndex = resFilterIndex;

            return res;
        }

        private bool ShowFolderDialog(string title, string initialDir, bool multiselect, out string[] paths)
        {
            string[] resPaths = null;

            var res = m_Parent.InvokeOnWindowThread(() =>
            {
                using (var dlg = new AdvancedFolderBrowseDialog())
                {
                    dlg.Title = title;
                    dlg.Multiselect = multiselect;

                    if (!string.IsNullOrEmpty(initialDir))
                    {
                        dlg.InitialDirectory = initialDir;
                    }

                    if (dlg.ShowDialog(GetOwnerHandle()) == DialogResult.OK)
                    {
                        resPaths = dlg.FolderNames;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            });

            paths = resPaths;

            return res;
        }

        private IntPtr GetOwnerHandle()
            => m_Parent.IsValidWindow() ? m_Parent.Handle : IntPtr.Zero;
    }
}