using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

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

            public WatermarkAdorner(WatermarkTextBox watermarkTextBox, string text, Brush foreground, double opacity, Thickness margin, FontStyle style)
                : base(watermarkTextBox)
            {
                m_WatermarkTextBox = watermarkTextBox;

                m_Watermark = new TextBlock
                {
                    FontSize = watermarkTextBox.FontSize,
                    FontFamily = watermarkTextBox.FontFamily,
                    Text = text,
                    FontStyle = style,
                    Foreground = foreground,
                    Opacity = opacity,
                    Margin = margin,
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
        /// Watermark foreground
        /// </summary>
        public static readonly DependencyProperty WatermarkForegroundProperty =
            DependencyProperty.Register(
            nameof(WatermarkForeground), typeof(Brush),
            typeof(WatermarkTextBox),
            new PropertyMetadata(Brushes.Gray, OnWatermarkChanged));

        /// <summary>
        /// Watermark foreground
        /// </summary>
        public Brush WatermarkForeground
        {
            get { return (Brush)GetValue(WatermarkForegroundProperty); }
            set { SetValue(WatermarkForegroundProperty, value); }
        }

        /// <summary>
        /// Watermark opacity
        /// </summary>
        public static readonly DependencyProperty WatermarkOpacityProperty =
            DependencyProperty.Register(
            nameof(WatermarkOpacity), typeof(double),
            typeof(WatermarkTextBox),
            new PropertyMetadata(0.5, OnWatermarkChanged));

        /// <summary>
        /// Watermark opacity
        /// </summary>
        public double WatermarkOpacity
        {
            get { return (double)GetValue(WatermarkOpacityProperty); }
            set { SetValue(WatermarkOpacityProperty, value); }
        }

        /// <summary>
        /// Watermark margin
        /// </summary>
        public static readonly DependencyProperty WatermarkMarginProperty =
            DependencyProperty.Register(
            nameof(WatermarkMargin), typeof(Thickness),
            typeof(WatermarkTextBox),
            new PropertyMetadata(new Thickness(2), OnWatermarkChanged));

        /// <summary>
        /// Watermark margin
        /// </summary>
        public Thickness WatermarkMargin
        {
            get { return (Thickness)GetValue(WatermarkMarginProperty); }
            set { SetValue(WatermarkMarginProperty, value); }
        }

        /// <summary>
        /// Watermark font style
        /// </summary>
        public static readonly DependencyProperty WatermarkFontStyleProperty =
            DependencyProperty.Register(
            nameof(WatermarkFontStyle), typeof(FontStyle),
            typeof(WatermarkTextBox),
            new PropertyMetadata(FontStyles.Italic, OnWatermarkChanged));

        /// <summary>
        /// Watermark font style
        /// </summary>
        public FontStyle WatermarkFontStyle
        {
            get { return (FontStyle)GetValue(WatermarkFontStyleProperty); }
            set { SetValue(WatermarkFontStyleProperty, value); }
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
                    layer.Add(new WatermarkAdorner(this, Watermark, WatermarkForeground, WatermarkOpacity, WatermarkMargin, WatermarkFontStyle));
                }
            }
        }
    }
}
