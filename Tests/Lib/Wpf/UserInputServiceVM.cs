using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Wpf.Services;

namespace Lib.Wpf
{
    public class UserInputServiceVM
    {
        public ICommand ShowInputBoxCommand { get; }
        public ICommand ShowInputBoxDefaultValueCommand { get; }
        public ICommand ShowInputBoxNoParentCommand { get; }
        public ICommand ShowInputBoxWin32ParentCommand { get; }
        public ICommand ShowInputBoxBackgroundCommand { get; }
        public ICommand ShowInputBoxWin32BackgroundCommand { get; }

        private readonly UserInputService m_InputSvc;
        private readonly UserInputService m_InputNoParentSvc;
        private readonly UserInputService m_InputWin32ParentSvc;
        private readonly UserInputService m_InputBackSvc;

        public UserInputServiceVM(Window parentWnd)
        {
            ShowInputBoxCommand = new RelayCommand(ShowInputBox);
            ShowInputBoxDefaultValueCommand = new RelayCommand(ShowInputBoxDefaultValue);
            ShowInputBoxNoParentCommand = new RelayCommand(ShowInputBoxNoParent);
            ShowInputBoxWin32ParentCommand = new RelayCommand(ShowInputBoxWin32Parent);
            ShowInputBoxBackgroundCommand = new RelayCommand(ShowInputBoxBackground);
            ShowInputBoxWin32BackgroundCommand = new RelayCommand(ShowInputBoxWin32Background);

            m_InputSvc = new UserInputService("My Input Box", ParentWindow.FromWindow(parentWnd));
            m_InputNoParentSvc = new UserInputService("My Input Box (No Parent)");
            m_InputWin32ParentSvc = new UserInputService("My Input Box (No Parent)", ParentWindow.FromHandle(new WindowInteropHelper(parentWnd).EnsureHandle()));
            m_InputBackSvc = new UserInputService("My Input Box (Background)", ParentWindow.FromHandle(new WindowInteropHelper(parentWnd).EnsureHandle(), Dispatcher.CurrentDispatcher));
        }

        private void ShowInputBox()
        {
            string val = null;

            if (m_InputSvc.TryGetInput("Enter value", ref val))
            {
                MessageBox.Show($"Entered value: {val}");
            }
        }

        private void ShowInputBoxDefaultValue()
        {
            var input = "ABC";

            if (m_InputSvc.TryGetInput("Enter value (default)", ref input))
            {
                MessageBox.Show($"Entered value: {input}");
            }
        }

        private void ShowInputBoxNoParent()
        {
            var input = "xyz";

            if (m_InputNoParentSvc.TryGetInput("Enter value (no parent)", ref input))
            {
                MessageBox.Show($"Entered value: {input}");
            }
        }

        private void ShowInputBoxWin32Parent()
        {
            var input = "123";

            if (m_InputWin32ParentSvc.TryGetInput("Enter value (Win32 parent)", ref input))
            {
                MessageBox.Show($"Entered value: {input}");
            }
        }

        private async void ShowInputBoxBackground()
        {
            string val = null;
            var accepted = false;

            try
            {
                await Task.Run(() => accepted = m_InputSvc.TryGetInput("Enter value (background)", ref val));

                if (accepted)
                {
                    MessageBox.Show($"Entered value: {val}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void ShowInputBoxWin32Background()
        {
            string val = null;
            var accepted = false;

            try
            {
                await Task.Run(() => accepted = m_InputBackSvc.TryGetInput("Enter value (background)", ref val));

                if (accepted)
                {
                    MessageBox.Show($"Entered value: {val}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
