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
using Xarial.XToolkit.Wpf.Controls;
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
            if (InputBox.Show("My Input Box", "Enter value", this, out string val)) 
            {
                MessageBox.Show($"Entered value: {val}");
            }
        }
        
        private void OnBrowseFileOpen(object sender, RoutedEventArgs e)
        {
            if (FileSystemBrowser.BrowseFileOpen(out string path, out int filterIndex, "Test",
                FileSystemBrowser.BuildFilterString(new FileFilter("Txt1", "*.txt"),
                new FileFilter("Txt2", "*.txt")))) 
            {
            }
        }

        private void OnBrowseFilesOpen(object sender, RoutedEventArgs e)
        {
            if (FileSystemBrowser.BrowseFilesOpen(out string[] path))
            {
            }
        }

        private void OnBrowseFileSave(object sender, RoutedEventArgs e)
        {
            if (FileSystemBrowser.BrowseFileSave(out string path))
            {
            }
        }

        private void OnBrowseFolder(object sender, RoutedEventArgs e)
        {
            if (FileSystemBrowser.BrowseFolder(out string path))
            {
            }
        }

        private void OnColumnsPreCreated(List<DataGridColumn> columns)
        {
            columns.Sort((c1, c2) => 
            {
                if (c1 is XDataGridColumn && c2 is XDataGridColumn)
                {
                    return 0;
                }

                if (!(c1 is XDataGridColumn) && !(c2 is XDataGridColumn))
                {
                    return 0;
                }

                if (c1 is XDataGridColumn && !(c2 is XDataGridColumn))
                {
                    return -1;
                }

                if (!(c1 is XDataGridColumn) && c2 is XDataGridColumn)
                {
                    return 1;
                }

                throw new Exception();
            });
        }
    }
}
