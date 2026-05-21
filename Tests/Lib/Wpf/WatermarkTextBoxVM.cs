//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Lib.Wpf
{
    public class WatermarkTextBoxVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Watermark 
        {
            get => m_Watermark;
            private set 
            {
                m_Watermark = value;
                this.NotifyChanged();
            }
        }

        public Visibility Visibility
        {
            get => m_Visibility;
            private set
            {
                m_Visibility = value;
                this.NotifyChanged();
            }
        }

        public ICommand UpdateCommand { get; }
        public ICommand HideShowCommand { get; }

        private string m_Watermark;
        private Visibility m_Visibility;

        public WatermarkTextBoxVM() 
        {
            m_Watermark = "Hello World";

            UpdateCommand = new RelayCommand(Update);
            HideShowCommand = new RelayCommand(HideShow);
        }

        private void HideShow()
        {
            switch (Visibility) 
            {
                case Visibility.Visible:
                    Visibility = Visibility.Hidden;
                    break;

                case Visibility.Hidden:
                    Visibility = Visibility.Collapsed;
                    break;

                case Visibility.Collapsed:
                    Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Update()
        {
            Watermark = "Hello World (Updated)";
        }
    }
}
