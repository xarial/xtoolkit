//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xarial.XToolkit;
using Xarial.XToolkit.Wpf.Controls;
using Xarial.XToolkit.Wpf.Delegates;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Wpf.Utils;

namespace WpfTester
{
    public partial class MainWindow : Window
    {
        private readonly MainVM m_Vm;

        public MainWindow()
        {
            InitializeComponent();
            
            m_Vm = new MainVM();

            this.DataContext = m_Vm;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Debugger.Break();
        }

        private void OnShowAboutClick(object sender, RoutedEventArgs e)
        {
            About.Show(new AboutDialogSpec(this.GetType().Assembly) 
            {
                Edition = new PackageEditionSpec("Test Package", new DateTime(2020, 12, 1))
            }, this);
        }

        private void OnShowInputBoxClick(object sender, RoutedEventArgs e)
        {
            string val = null;

            if (InputBox.Show("My Input Box", "Enter value", this, ref val)) 
            {
                MessageBox.Show($"Entered value: {val}");
            }

            var input = "ABC";

            if (InputBox.Show("My Input Box with default value", "Enter value", this, ref input))
            {
                MessageBox.Show($"Entered value: {input}");
            }
        }
        
        private void OnBrowseFileOpen(object sender, RoutedEventArgs e)
        {
            if (FileSystemBrowser.BrowseFileOpen(out string path, out int filterIndex, "Test",
                FileFilter.BuildFilterString(new FileFilter("Txt1", "*.txt"),
                new FileFilter("Txt2", "*.txt")), "", "test1.txt")) 
            {
            }
        }

        private void OnBrowseFilesOpen(object sender, RoutedEventArgs e)
        {
            if (FileSystemBrowser.BrowseFilesOpen(out string[] path, "", "", "", "abc.txt"))
            {
            }
        }

        private void OnBrowseFileSave(object sender, RoutedEventArgs e)
        {
            if (FileSystemBrowser.BrowseFileSave(out string path, "", FileFilter.BuildFilterString(FileFilter.AllFiles), @"D:\Demo", "mytestfile.txt"))
            {
            }
        }

        private void OnBrowseFolder(object sender, RoutedEventArgs e)
        {
            if (FileSystemBrowser.BrowseFolder(out string path, "Test Folder Browser", @"D:\Demo"))
            {
            }
        }

        private void OnBrowseFolders(object sender, RoutedEventArgs e)
        {
            if (FileSystemBrowser.BrowseFolders(out string[] paths, "Test Folder Browser"))
            {
            }
        }

        private void OnColumnsPreCreated(List<DataGridColumn> columns)
        {
            columns.Sort((c1, c2) => 
            {
                var h1 = c1.Header;
                var h2 = c2.Header;

                if (h1 is ColumnVM && h2 is ColumnVM)
                {
                    return 0;
                }

                if (!(h1 is ColumnVM) && !(h2 is ColumnVM))
                {
                    return 0;
                }

                if (h1 is ColumnVM && !(h2 is ColumnVM))
                {
                    return -1;
                }

                if (!(h1 is ColumnVM) && h2 is ColumnVM)
                {
                    return 1;
                }

                throw new Exception();
            });
        }
    }
}
