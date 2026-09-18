using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Wpf.Services;

namespace Lib.Wpf
{
    public class MessageServiceVM
    {
        public ICommand ShowMessageCommand { get; }
        public ICommand ShowQuestionCommand { get; }
        public ICommand ShowMessageNoParentCommand { get; }
        public ICommand ShowMessageWin32ParentCommand { get; }
        public ICommand ShowMessageBackgroundCommand { get; }
        public ICommand ShowMessageWin32BackgroundCommand { get; }

        private readonly MessageService m_MsgSvc;
        private readonly MessageService m_MsgNoParentSvc;
        private readonly MessageService m_MsgWin32ParentSvc;
        private readonly MessageService m_MsgBackSvc;

        public MessageServiceVM(Window parentWnd)
        {
            ShowMessageCommand = new RelayCommand(ShowMessage);
            ShowQuestionCommand = new RelayCommand(ShowQuestion);
            ShowMessageNoParentCommand = new RelayCommand(ShowMessageNoParent);
            ShowMessageWin32ParentCommand = new RelayCommand(ShowMessageWin32Parent);
            ShowMessageBackgroundCommand = new RelayCommand(ShowMessageBackground);
            ShowMessageWin32BackgroundCommand = new RelayCommand(ShowMessageWin32Background);

            m_MsgSvc = new MessageService("My Message", ParentWindow.FromWindow(parentWnd), null);
            m_MsgNoParentSvc = new MessageService("My Message (No Parent)");
            m_MsgWin32ParentSvc = new MessageService("My Message (Win32 Parent)", ParentWindow.FromHandle(new WindowInteropHelper(parentWnd).EnsureHandle()), null);
            m_MsgBackSvc = new MessageService("My Message (Background)", ParentWindow.FromHandle(new WindowInteropHelper(parentWnd).EnsureHandle(), Dispatcher.CurrentDispatcher), null);
        }

        private void ShowMessage()
        {
            m_MsgSvc.ShowMessage("Information message", MessageServiceIcon_e.Information, MessageServiceButtons_e.Ok);
        }

        private void ShowQuestion()
        {
            var res = m_MsgSvc.ShowMessage("Question message", MessageServiceIcon_e.Question, MessageServiceButtons_e.YesNoCancel);

            MessageBox.Show($"Result: {(res.HasValue ? res.Value.ToString() : "null")}");
        }

        private void ShowMessageNoParent()
        {
            m_MsgNoParentSvc.ShowMessage("Warning message (no parent)", MessageServiceIcon_e.Warning, MessageServiceButtons_e.Ok);
        }

        private void ShowMessageWin32Parent()
        {
            m_MsgWin32ParentSvc.ShowMessage("Error message (Win32 parent)", MessageServiceIcon_e.Error, MessageServiceButtons_e.Ok);
        }

        private async void ShowMessageBackground()
        {
            bool? res = null;

            try
            {
                await Task.Run(() => res = m_MsgSvc.ShowMessage("Question message (background)", MessageServiceIcon_e.Question, MessageServiceButtons_e.YesNoCancel));

                MessageBox.Show($"Result: {(res.HasValue ? res.Value.ToString() : "null")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void ShowMessageWin32Background()
        {
            bool? res = null;

            try
            {
                await Task.Run(() => res = m_MsgBackSvc.ShowMessage("Question message (Win32 background)", MessageServiceIcon_e.Question, MessageServiceButtons_e.YesNoCancel));

                MessageBox.Show($"Result: {(res.HasValue ? res.Value.ToString() : "null")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}