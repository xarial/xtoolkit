//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

namespace Xarial.XToolkit.Services
{
    /// <summary>
    /// Service for browsing files and folders in the system
    /// </summary>
    public interface IFileSystemBrowser
    {
        /// <summary>
        /// Browse folder
        /// </summary>
        /// <param name="path">Path to the folder</param>
        /// <param name="title">Title of the browse dialog</param>
        /// <param name="initialDir">Initial directory</param>
        /// <returns>True if folder is browsed</returns>
        bool BrowseFolder(out string path, string title = "", string initialDir = "");

        /// <summary>
        /// Browse multiple folders
        /// </summary>
        /// <param name="paths">Paths to the folders</param>
        /// <param name="title">Title of the browse dialog</param>
        /// <param name="initialDir">Initial directory</param>
        /// <returns>True if folders are browsed</returns>
        bool BrowseFolders(out string[] paths, string title = "", string initialDir = "");

        /// <summary>
        /// Browse file for open
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="filterIndex">Index of selected filter</param>
        /// <param name="title">Title of the browse dialog</param>
        /// <param name="filter">File filter. Use <see cref="FileFilter"/></param>
        /// <param name="initialDir">Initial directory</param>
        /// <param name="initialFile">Initial file name</param>
        /// <returns>True if file is browsed</returns>
        /// <remarks>Useful if multiple filters with same extension are used</remarks>
        bool BrowseFileOpen(out string path, out int filterIndex, string title = "", string filter = "",
            string initialDir = "", string initialFile = "");

        /// <summary>
        /// Browse multiple files for open
        /// </summary>
        /// <param name="paths">Paths to the files</param>
        /// <param name="filterIndex">Index of selected filter</param>
        /// <param name="title">Title of the browse dialog</param>
        /// <param name="filter">File filter. Use <see cref="FileFilter"/></param>
        /// <param name="initialDir">Initial directory</param>
        /// <param name="initialFile">Initial file name</param>
        /// <returns>True if files are browsed</returns>
        bool BrowseFilesOpen(out string[] paths, out int filterIndex, string title = "", string filter = "",
            string initialDir = "", string initialFile = "");

        /// <summary>
        /// Browse file for save
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="filterIndex">Index of selected filter</param>
        /// <param name="title">Title of the browse dialog</param>
        /// <param name="filter">File filter. Use <see cref="FileFilter"/></param>
        /// <param name="initialDir">Initial directory</param>
        /// <param name="initialFile">Initial file name</param>
        /// <returns>True if file is browsed</returns>
        /// <remarks>Useful if multiple filters with same extension are used</remarks>
        bool BrowseFileSave(out string path, out int filterIndex, string title = "", string filter = "",
            string initialDir = "", string initialFile = "");
    }

    /// <summary>
    /// Additional methods of <see cref="IFileSystemBrowser"/>
    /// </summary>
    public static class FileSystemBrowserExtension
    {
        /// <summary>
        /// Browse file for open
        /// </summary>
        /// <param name="browser">File system browser</param>
        /// <param name="path">Path to the file</param>
        /// <param name="title">Title of the browse dialog</param>
        /// <param name="filter">File filter</param>
        /// <param name="initialDir">Initial directory</param>
        /// <param name="initialFile">Initial file name</param>
        /// <returns>True if file is browsed</returns>
        public static bool BrowseFileOpen(this IFileSystemBrowser browser, out string path, string title = "", string filter = "",
            string initialDir = "", string initialFile = "")
            => browser.BrowseFileOpen(out path, out _, title, filter, initialDir, initialFile);

        /// <summary>
        /// Browse multiple files for open
        /// </summary>
        /// <param name="browser">File system browser</param>
        /// <param name="paths">Paths to the files</param>
        /// <param name="title">Title of the browse dialog</param>
        /// <param name="filter">File filter</param>
        /// <param name="initialDir">Initial directory</param>
        /// <param name="initialFile">Initial file name</param>
        /// <returns>True if files are browsed</returns>
        public static bool BrowseFilesOpen(this IFileSystemBrowser browser, out string[] paths, string title = "", string filter = "",
            string initialDir = "", string initialFile = "")
            => browser.BrowseFilesOpen(out paths, out _, title, filter, initialDir, initialFile);

        /// <summary>
        /// Browse file for save
        /// </summary>
        /// <param name="browser">File system browser</param>
        /// <param name="path">Path to the file</param>
        /// <param name="title">Title of the browse dialog</param>
        /// <param name="filter">File filter</param>
        /// <param name="initialDir">Initial directory</param>
        /// <param name="initialFile">Initial file name</param>
        /// <returns>True if file is browsed</returns>
        public static bool BrowseFileSave(this IFileSystemBrowser browser, out string path, string title = "", string filter = "",
            string initialDir = "", string initialFile = "")
            => browser.BrowseFileSave(out path, out _, title, filter, initialDir, initialFile);
    }
}