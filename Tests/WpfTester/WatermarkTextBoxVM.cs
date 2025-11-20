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

namespace WpfTester
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

        public Brush Foreground
        {
            get => m_Foreground;
            private set
            {
                m_Foreground = value;
                this.NotifyChanged();
            }
        }

        public double Opacity
        {
            get => m_Opacity;
            private set
            {
                m_Opacity = value;
                this.NotifyChanged();
            }
        }

        public Thickness Margin
        {
            get => m_Margin;
            private set
            {
                m_Margin = value;
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

        public FontStyle FontStyle
        {
            get => m_FontStyle;
            private set
            {
                m_FontStyle = value;
                this.NotifyChanged();
            }
        }

        public ICommand UpdateCommand { get; }
        public ICommand HideShowCommand { get; }

        private string m_Watermark;
        private Brush m_Foreground;
        private double m_Opacity;
        private Thickness m_Margin;
        private Visibility m_Visibility;
        private FontStyle m_FontStyle;

        public WatermarkTextBoxVM() 
        {
            m_Watermark = "Hello World";
            m_Foreground = Brushes.Green;
            m_Opacity = 0.75;
            m_Margin = new Thickness(3);
            m_FontStyle = FontStyles.Normal;

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
            Foreground = Brushes.Blue;
            Opacity = 0.25;
            Margin = new Thickness(0);
            FontStyle = FontStyles.Italic;
        }
    }
}
