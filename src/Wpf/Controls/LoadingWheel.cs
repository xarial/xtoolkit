using System;
using System.Collections.Generic;
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

namespace Xarial.XToolkit.Wpf.Controls
{
    public class LoadingWheel : Control
    {
        static LoadingWheel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingWheel), 
                new FrameworkPropertyMetadata(typeof(LoadingWheel)));
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
            nameof(StrokeThickness), typeof(double),
            typeof(LoadingWheel), new PropertyMetadata(10d));

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register(
            nameof(Diameter), typeof(double),
            typeof(LoadingWheel), new PropertyMetadata(100d));

        public double Diameter
        {
            get { return (double)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(
            nameof(Color), typeof(Color),
            typeof(LoadingWheel), new PropertyMetadata(Colors.Green));

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
    }
}
