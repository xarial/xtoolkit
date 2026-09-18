using System;
using Xarial.XToolkit;
using Xarial.XToolkit.Wpf.Services;
using Xarial.XToolkit.Services;

namespace Wpf.Docs
{
    public static class FileSystemBrowserDocs
    {
        public static void BrowseFolder() 
        {
            //--- browse-folder
            var fsb = new FileSystemBrowser();

            if (fsb.BrowseFolder(out string path, "Select sample folder"))
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
            var fsb = new FileSystemBrowser();

            if (fsb.BrowseFileOpen(out string fileIn, "Select input file", filters)) 
            {
                Console.WriteLine($"Selected path: {fileIn}");
            }
            else
            {
                Console.WriteLine("User has cancelled the file browsing");
            }
            //---

            //--- browse-file-save
            if (fsb.BrowseFileSave(out string fileOut, "Select output file", filters))
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
