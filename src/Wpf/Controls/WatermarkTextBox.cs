using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.XToolkit.Wpf.Controls
{
    /// <summary>
    /// Text box control with watermark
    /// </summary>
    public class WatermarkTextBox : TextBox
    {
        private class WatermarkAdorner : Adorner
        {
            private readonly TextBlock m_Watermark;

            private readonly WatermarkTextBox m_WatermarkTextBox;

            public WatermarkAdorner(WatermarkTextBox watermarkTextBox, string text, Style style)
                : base(watermarkTextBox)
            {
               m_WatermarkTextBox = watermarkTextBox;

                m_Watermark = new TextBlock
                {
                    FontSize = watermarkTextBox.FontSize,
                    FontFamily = watermarkTextBox.FontFamily,
                    Text = text,
                    Style = style,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsHitTestVisible = false
                };

                BindingOperations.SetBinding(this,
                    VisibilityProperty,
                    new Binding()
                    {
                        Path = new PropertyPath(VisibilityProperty),
                        Source = m_WatermarkTextBox
                    });
            }

            protected override int VisualChildrenCount => 1;

            protected override Visual GetVisualChild(int index) => m_Watermark;

            protected override Size MeasureOverride(Size constraint)
            {
                m_Watermark.Measure(m_WatermarkTextBox.RenderSize);
                return m_WatermarkTextBox.RenderSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                m_Watermark.Arrange(new Rect(finalSize));
                return finalSize;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public WatermarkTextBox() 
        {
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateAdorner();
        }

        /// <summary>
        /// Watermark text
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(
            nameof(Watermark), typeof(string),
            typeof(WatermarkTextBox),
            new PropertyMetadata("", OnWatermarkChanged));

        /// <summary>
        /// Watermark text
        /// </summary>
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        /// <summary>
        /// Watermark style
        /// </summary>
        public static readonly DependencyProperty WatermarkStyleProperty =
            DependencyProperty.Register(
            nameof(WatermarkStyle), typeof(Style),
            typeof(WatermarkTextBox),
            new PropertyMetadata(typeof(WatermarkTextBox).Assembly.LoadFromResources<Style>("Themes/WatermarkTextBox.xaml", "WatermarkDefaultStyle"), OnWatermarkChanged));

        /// <summary>
        /// Watermark style
        /// </summary>
        public Style WatermarkStyle
        {
            get { return (Style)GetValue(WatermarkStyleProperty); }
            set { SetValue(WatermarkStyleProperty, value); }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            UpdateAdorner();
        }

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WatermarkTextBox)d).UpdateAdorner();
        }

        private void UpdateAdorner()
        {
            var layer = AdornerLayer.GetAdornerLayer(this);

            if (layer != null)
            {
                var adorners = layer.GetAdorners(this)?.OfType<WatermarkAdorner>().ToArray();
                
                if (adorners != null)
                {
                    foreach (var adorner in adorners)
                    {
                        layer.Remove(adorner);
                    }
                }

                if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Watermark))
                {
                    layer.Add(new WatermarkAdorner(this, Watermark, WatermarkStyle));
                }
            }
        }
    }
}
