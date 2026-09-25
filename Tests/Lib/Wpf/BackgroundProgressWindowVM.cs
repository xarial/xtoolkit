using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Wpf.Services;

namespace Lib.Wpf
{
    public class TestProgressWindow : Window
    {
        private readonly TextBlock m_Message;
        private readonly ProgressBar m_Progress;

        public TestProgressWindow(string title, CancellationTokenSource cancellation)
        {
            Title = title;
            Width = 350;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            ShowInTaskbar = false;

            m_Message = new TextBlock() { Margin = new Thickness(5) };
            m_Progress = new ProgressBar() { Margin = new Thickness(5), Height = 20, Maximum = 1 };

            var cancelBtn = new Button() { Content = "Cancel", Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Right, Width = 75 };
            cancelBtn.Click += (s, e) => cancellation.Cancel();

            var panel = new StackPanel() { Orientation = Orientation.Vertical };
            panel.Children.Add(m_Message);
            panel.Children.Add(m_Progress);
            panel.Children.Add(cancelBtn);

            Content = panel;
        }

        public void Report(double progress, string msg)
        {
            m_Progress.Value = progress;
            m_Message.Text = msg;
        }
    }

    public class BackgroundProgressWindowVM
    {
        private const int STEPS = 100;
        private const int STEP_DURATION = 30;

        public ICommand ShowProgressCommand { get; }
        public ICommand ShowProgressNoParentCommand { get; }
        public ICommand ShowProgressWin32ParentCommand { get; }
        public ICommand ShowProgressBackgroundCommand { get; }
        public ICommand ShowProgressWin32BackgroundCommand { get; }

        private readonly IParentWindow m_Parent;
        private readonly IParentWindow m_Win32Parent;
        private readonly IParentWindow m_BackParent;

        public BackgroundProgressWindowVM(Window parentWnd)
        {
            ShowProgressCommand = new RelayCommand(ShowProgress);
            ShowProgressNoParentCommand = new RelayCommand(ShowProgressNoParent);
            ShowProgressWin32ParentCommand = new RelayCommand(ShowProgressWin32Parent);
            ShowProgressBackgroundCommand = new RelayCommand(ShowProgressBackground);
            ShowProgressWin32BackgroundCommand = new RelayCommand(ShowProgressWin32Background);

            m_Parent = ParentWindow.FromWindow(parentWnd);
            m_Win32Parent = ParentWindow.FromHandle(new WindowInteropHelper(parentWnd).EnsureHandle());
            m_BackParent = ParentWindow.FromHandle(new WindowInteropHelper(parentWnd).EnsureHandle(), Dispatcher.CurrentDispatcher);
        }

        private void ShowProgress()
            => RunBlocking("Progress (WPF Parent)", m_Parent);

        private void ShowProgressNoParent()
            => RunBlocking("Progress (No Parent)", null);

        private void ShowProgressWin32Parent()
            => RunBlocking("Progress (Win32 Parent)", m_Win32Parent);

        private async void ShowProgressBackground()
            => await RunBackground("Progress (Background)", m_Parent);

        private async void ShowProgressWin32Background()
            => await RunBackground("Progress (Win32 Background)", m_BackParent);

        private void RunBlocking(string title, IParentWindow parent)
        {
            try
            {
                string res;

                using (var cts = new CancellationTokenSource())
                {
                    using (var prg = new BackgroundProgressWindow<TestProgressWindow>(() => new TestProgressWindow(title, cts), parent))
                    {
                        res = DoWork(prg, cts.Token);
                    }
                }

                MessageBox.Show(res);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Cancelled");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task RunBackground(string title, IParentWindow parent)
        {
            try
            {
                var res = await Task.Run(() =>
                {
                    using (var cts = new CancellationTokenSource())
                    {
                        using (var prg = new BackgroundProgressWindow<TestProgressWindow>(() => new TestProgressWindow(title, cts), parent))
                        {
                            return DoWork(prg, cts.Token);
                        }
                    }
                });

                MessageBox.Show(res);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Cancelled");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string DoWork(IBackgroundProgressWindow<TestProgressWindow> prg, CancellationToken cancellationToken)
        {
            for (int i = 0; i < STEPS; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var step = i + 1;

                prg.BeginInvoke(w => w.Report((double)step / STEPS, $"Processing {step} of {STEPS}"));

                Thread.Sleep(STEP_DURATION);
            }

            return "Completed";
        }
    }
}
