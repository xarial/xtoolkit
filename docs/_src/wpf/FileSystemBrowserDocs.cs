using System;
using Xarial.XToolkit;
using Xarial.XToolkit.Wpf.Utils;

namespace Wpf.Docs
{
    public static class FileSystemBrowserDocs
    {
        public static void BrowseFolder() 
        {
            //--- browse-folder
            if (FileSystemBrowser.BrowseFolder(out string path, "Select sample folder"))
            {
                Console.WriteLine($"Selected path: {path}");
            }
            else 
            {
                Console.WriteLine("User has cancelled the folder browsing");
            }
            //---
        }

        public static void BrowseFile() 
        {
            //--- browse-file-filter
            var filters = FileFilter.BuildFilterString(
                new FileFilter("Text Files", "*.txt", "*.doc", "*.md"), 
                FileFilter.ImageFiles, 
                FileFilter.AllFiles);
            //---

            //--- browse-file-open
            if (FileSystemBrowser.BrowseFileOpen(out string fileIn, "Select input file", filters)) 
            {
                Console.WriteLine($"Selected path: {fileIn}");
            }
            else
            {
                Console.WriteLine("User has cancelled the file browsing");
            }
            //---

            //--- browse-file-save
            if (FileSystemBrowser.BrowseFileSave(out string fileOut, "Select output file", filters))
            {
                Console.WriteLine($"Selected path: {fileOut}");
            }
            else
            {
                Console.WriteLine("User has cancelled the file browsing");
            }
            //---
        }
    }
}
