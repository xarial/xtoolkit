using Lib.Properties;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Attributes;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Wpf.Services;

namespace Lib.Wpf
{
    public class AboutServiceVM
    {
        public ICommand ShowAboutCommand { get; }
        public ICommand ShowAboutNoParentCommand { get; }
        public ICommand ShowAboutWin32ParentCommand { get; }
        public ICommand ShowAboutBackgroundCommand { get; }
        public ICommand ShowAboutWin32BackgroundCommand { get; }

        private readonly IAboutService m_AboutSvc;
        private readonly IAboutService m_AboutNoParentSvc;
        private readonly IAboutService m_AboutWin32ParentSvc;
        private readonly IAboutService m_AboutBackSvc;

        public AboutServiceVM(Window parentWnd)
        {
            ShowAboutCommand = new RelayCommand(ShowAbout);
            ShowAboutNoParentCommand = new RelayCommand(ShowAboutNoParent);
            ShowAboutWin32ParentCommand = new RelayCommand(ShowAboutWin32Parent);
            ShowAboutBackgroundCommand = new RelayCommand(ShowAboutBackground);
            ShowAboutWin32BackgroundCommand = new RelayCommand(ShowAboutWin32Background);

            var assm = Assembly.GetExecutingAssembly();
            var hwnd = new WindowInteropHelper(parentWnd).EnsureHandle();

            m_AboutSvc = new AboutService(assm, Resources.icon, ParentWindow.FromWindow(parentWnd));
            m_AboutNoParentSvc = new AboutService(assm);
            m_AboutWin32ParentSvc = new AboutService(new AboutDialogSpec(assm, Resources.icon, new LicenseInfo[] 
            {
                new LicenseInfo()
                {
                    Title = "xToolkit",
                    Url ="https://xtoolkit.xarial.com/"
                }
            })
            {
                Edition = new PackageEditionSpec("Test Package", new DateTime(2020, 12, 1)),
                Eula = Resources.eula
            }, ParentWindow.FromHandle(hwnd));
            m_AboutBackSvc = new AboutService(assm, ParentWindow.FromHandle(hwnd, Dispatcher.CurrentDispatcher));
        }

        private void ShowAbout()
        {
            m_AboutSvc.ShowAbout();
        }

        private void ShowAboutNoParent()
        {
            m_AboutNoParentSvc.ShowAbout();
        }

        private void ShowAboutWin32Parent()
        {
            m_AboutWin32ParentSvc.ShowAbout();
        }

        private async void ShowAboutBackground()
        {
            try
            {
                await Task.Run(() => m_AboutSvc.ShowAbout());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void ShowAboutWin32Background()
        {
            try
            {
                await Task.Run(() => m_AboutBackSvc.ShowAbout());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}