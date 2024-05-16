using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace WpfTester
{
    public class ProgressPanelVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool m_IsWorkInProgress;
        private double? m_Progress;
        private string m_Message;

        public bool IsWorkInProgress 
        {
            get => m_IsWorkInProgress;
            set 
            {
                m_IsWorkInProgress = value;
                this.NotifyChanged();
            }
        }

        public double? Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }

        public string Message
        {
            get => m_Message;
            set
            {
                m_Message = value;
                this.NotifyChanged();
            }
        }

        public ICommand CancelCommand { get; }

        public ICommand DoWorkCommand => new RelayCommand(DoWorkAsync);

        private CancellationTokenSource m_CancellationTokenSrc;

        public ProgressPanelVM() 
        {
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Cancel()
        {
            m_CancellationTokenSrc.Cancel();
        }

        private async void DoWorkAsync() 
        {
            try
            {
                IsWorkInProgress = true;
                Progress = null;
                Message = "Initializing...";
                m_CancellationTokenSrc = new CancellationTokenSource();
                var cancellationToken = m_CancellationTokenSrc.Token;

                await Task.Delay(2000);

                Message = "Working...";
                Progress = 0;

                for (int i = 0; i < 100; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(100);
                    Progress = (double)(i + 1) / 100d;
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Cancelled");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsWorkInProgress = false;
            }
        }
    }
}
