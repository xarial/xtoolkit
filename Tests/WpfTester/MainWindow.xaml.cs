//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Lib.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using Xarial.XToolkit.Wpf.Utils;

namespace WpfTester
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoadFromFile(object sender, RoutedEventArgs e)
        {
            if (FileSystemBrowser.BrowseFileOpen(out var path, "Library dll file",
                FileFilter.BuildFilterString(FileFilter.Create("DLL", "*.dll")),
                System.IO.Path.GetDirectoryName(typeof(WpfControls).Assembly.Location),
                System.IO.Path.GetFileName(typeof(WpfControls).Assembly.Location)))
            {
                var assm = Assembly.LoadFrom(path);

                var wpfCtrls = (UserControl)Activator.CreateInstance(assm.GetType(typeof(WpfControls).FullName));
                
                var wnd = new Window()
                {
                    Content = wpfCtrls,
                    Title = assm.GetName().Version.ToString()
                };

                wnd.Show();
            }
        }
    }
}
