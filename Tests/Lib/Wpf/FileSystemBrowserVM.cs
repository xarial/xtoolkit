using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Xarial.XToolkit;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Wpf.Services;

namespace Lib.Wpf
{
    public class FileSystemBrowserVM
    {
        public ICommand BrowseFileOpenCommand { get; }
        public ICommand BrowseFilesOpenCommand { get; }
        public ICommand BrowseFileSaveCommand { get; }
        public ICommand BrowseFolderCommand { get; }
        public ICommand BrowseFoldersCommand { get; }
        public ICommand BrowseFileOpenBackgroundCommand { get; }
        public ICommand BrowseFolderBackgroundCommand { get; }

        private readonly IFileSystemBrowser m_Browser;
        private readonly IFileSystemBrowser m_NoParentBrowser;
        private readonly IFileSystemBrowser m_Win32ParentBrowser;

        public FileSystemBrowserVM(Window parentWnd)
        {
            BrowseFileOpenCommand = new RelayCommand(BrowseFileOpen);
            BrowseFilesOpenCommand = new RelayCommand(BrowseFilesOpen);
            BrowseFileSaveCommand = new RelayCommand(BrowseFileSave);
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
            BrowseFoldersCommand = new RelayCommand(BrowseFolders);
            BrowseFileOpenBackgroundCommand = new RelayCommand(BrowseFileOpenBackground);
            BrowseFolderBackgroundCommand = new RelayCommand(BrowseFolderBackground);

            m_Browser = new FileSystemBrowser(ParentWindow.FromWindow(parentWnd));
            m_NoParentBrowser = new FileSystemBrowser();
            m_Win32ParentBrowser = new FileSystemBrowser(ParentWindow.FromHandle(new WindowInteropHelper(parentWnd).EnsureHandle()));
        }

        private void BrowseFileOpen()
        {
            if (m_NoParentBrowser.BrowseFileOpen(out string path, out int filterIndex, "Test",
                FileFilter.BuildFilterString(new FileFilter("Txt1", "*.txt"), new FileFilter("Txt2", "*.txt")), "", "test1.txt"))
            {
                MessageBox.Show($"Path: {path}\nFilter index: {filterIndex}");
            }
        }

        private void BrowseFilesOpen()
        {
            if (m_Win32ParentBrowser.BrowseFilesOpen(out string[] paths, "", "", "", "abc.txt"))
            {
                MessageBox.Show($"Paths:\n{string.Join(Environment.NewLine, paths)}");
            }
        }

        private void BrowseFileSave()
        {
            if (m_NoParentBrowser.BrowseFileSave(out string path, "", FileFilter.BuildFilterString(FileFilter.AllFiles), @"D:\Demo", "mytestfile.txt"))
            {
                MessageBox.Show($"Path: {path}");
            }
        }

        private void BrowseFolder()
        {
            if (m_Win32ParentBrowser.BrowseFolder(out string path, "Test Folder Browser", @"D:\Demo"))
            {
                MessageBox.Show($"Path: {path}");
            }
        }

        private void BrowseFolders()
        {
            if (m_Browser.BrowseFolders(out string[] paths, "Test Folder Browser"))
            {
                MessageBox.Show($"Paths:\n{string.Join(Environment.NewLine, paths)}");
            }
        }

        private async void BrowseFileOpenBackground()
        {
            string path = null;
            var accepted = false;

            try
            {
                await Task.Run(() => accepted = m_Browser.BrowseFileOpen(out path, "Test (background)",
                    FileFilter.BuildFilterString(FileFilter.AllFiles)));

                if (accepted)
                {
                    MessageBox.Show($"Path: {path}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void BrowseFolderBackground()
        {
            string path = null;
            var accepted = false;

            try
            {
                await Task.Run(() => accepted = m_Browser.BrowseFolder(out path, "Test Folder Browser (background)", @"D:\Demo"));

                if (accepted)
                {
                    MessageBox.Show($"Path: {path}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}