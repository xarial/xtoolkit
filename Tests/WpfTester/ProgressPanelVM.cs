using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ICommand DoWorkCommand => new RelayCommand(DoWorkAsync);

        private async void DoWorkAsync() 
        {
            try
            {
                IsWorkInProgress = true;
                Progress = null;
                Message = "Initializing...";

                await Task.Delay(2000);

                Message = "Working...";
                Progress = 0;

                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(100);
                    Progress = (double)(i + 1) / 100d;
                }
            }
            catch
            {
            }
            finally 
            {
                IsWorkInProgress = false;
            }
        }
    }
}
