//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

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
using Xarial.XToolkit.Wpf.Themes;

namespace Xarial.XToolkit.Wpf.Controls
{
    /// <summary>
    /// Color picker control
    /// </summary>
    public class ColorPicker : Control
    {
        private const float COLORS_DIFFERENT_THRESHOLD = 0.25f;

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker),
                    new FrameworkPropertyMetadata(typeof(ColorPicker)));

            StyleProperty.OverrideMetadata(typeof(ColorPicker),
                new FrameworkPropertyMetadata(StyleLoader.LoadStyle<ColorPicker>("ColorPicker.xaml")));
        }

        private Grid m_MainGrid;
        private Hyperlink m_RemoveColorLink;
        private Hyperlink m_SelectColorLink;
        private TextBlock m_ColorTextBlock;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            m_MainGrid = (Grid)this.Template.FindName("PART_MainGrid", this);
            m_MainGrid.MouseDown += OnMainGridMouseDown;
            
            m_RemoveColorLink = (Hyperlink)this.Template.FindName("PART_RemoveColorLink", this);
            m_RemoveColorLink.Click += OnRemoveColorLinkClick;

            m_SelectColorLink = (Hyperlink)this.Template.FindName("PART_SelectColorLink", this);
            m_SelectColorLink.Click += OnSelectColorButtonClick;

            m_ColorTextBlock = (TextBlock)this.Template.FindName("PART_ColorTextBlock", this);

            SetAppearance(Color);
        }

        private void OnMainGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            PickColor();
        }

        private void OnRemoveColorLinkClick(object sender, RoutedEventArgs e)
        {
            Color = null;
        }

        private void OnSelectColorButtonClick(object sender, RoutedEventArgs e)
        {
            PickColor();
        }

        private void PickColor()
        {
            var colorDlg = new System.Windows.Forms.ColorDialog();

            colorDlg.Color = Color.HasValue ? ConvertColor(Color.Value) : System.Drawing.Color.White;

            if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = colorDlg.Color;

                Color = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            }
        }

        private System.Drawing.Color ConvertColor(Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Color
        /// </summary>
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(
            nameof(Color), typeof(Color?),
            typeof(ColorPicker), new FrameworkPropertyMetadata(default(Color?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker)d).SetAppearance((Color?)e.NewValue);
        }

        public static readonly DependencyProperty RemoveColorButtonTooltipProperty =
            DependencyProperty.Register(
            nameof(RemoveColorButtonTooltip), typeof(object),
            typeof(ColorPicker), new PropertyMetadata("Remove Color"));

        /// <summary>
        /// Tooltip for the remove color button
        /// </summary>
        public object RemoveColorButtonTooltip
        {
            get { return (object)GetValue(RemoveColorButtonTooltipProperty); }
            set { SetValue(RemoveColorButtonTooltipProperty, value); }
        }

        public static readonly DependencyProperty UnsetColorTitleProperty =
            DependencyProperty.Register(
            nameof(UnsetColorTitle), typeof(string),
            typeof(ColorPicker), new PropertyMetadata("<None>", OnUnsetColorTitlePropertyChanged));

        /// <summary>
        /// Text to display when color is null
        /// </summary>
        public string UnsetColorTitle
        {
            get { return (string)GetValue(UnsetColorTitleProperty); }
            set { SetValue(UnsetColorTitleProperty, value); }
        }

        public static readonly DependencyProperty UnsetColorForegroundProperty =
            DependencyProperty.Register(
            nameof(UnsetColorForeground), typeof(Brush),
            typeof(ColorPicker), new PropertyMetadata(Brushes.Gray, OnUnsetColorForegroundPropertyChanged));

        /// <summary>
        /// Foreground for the label when color is null
        /// </summary>
        public Brush UnsetColorForeground
        {
            get { return (Brush)GetValue(UnsetColorForegroundProperty); }
            set { SetValue(UnsetColorForegroundProperty, value); }
        }

        private static void OnUnsetColorTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;

            if (!colorPicker.Color.HasValue)
            {
                if (colorPicker.m_ColorTextBlock != null)
                {
                    colorPicker.m_ColorTextBlock.Text = (string)e.NewValue;
                }
            }
        }

        private static void OnUnsetColorForegroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;

            if (!colorPicker.Color.HasValue)
            {
                if (colorPicker.m_ColorTextBlock != null)
                {
                    colorPicker.m_ColorTextBlock.Foreground = (Brush)e.NewValue;
                }
            }
        }

        private void SetAppearance(Color? color)
        {
            if (m_MainGrid != null) 
            {
                if (color.HasValue)
                {
                    m_MainGrid.Background = new SolidColorBrush(color.Value);
                }
                else
                {
                    m_MainGrid.Background = Brushes.White;
                }
            }

            if (m_ColorTextBlock != null) 
            {
                if (color.HasValue) 
                {
                    var drwColor = ConvertColor(color.Value);

                    m_ColorTextBlock.Text = $"{color.Value.R} {color.Value.G} {color.Value.B} {color.Value.A}";
                    m_ColorTextBlock.ToolTip = $"R={color.Value.R} G={color.Value.G} B={color.Value.B} A={color.Value.A}";

                    if (Math.Abs(drwColor.GetBrightness() - System.Drawing.Color.Black.GetBrightness()) < COLORS_DIFFERENT_THRESHOLD)
                    {
                        //Changing background color to white
                        m_ColorTextBlock.Foreground = Brushes.White;
                    }
                    else
                    {
                        m_ColorTextBlock.Foreground = Brushes.Black;
                    }
                }
                else
                {
                    m_ColorTextBlock.Text = UnsetColorTitle;
                    m_ColorTextBlock.Foreground = UnsetColorForeground;
                }
            }
        }

        /// <summary>
        /// Color
        /// </summary>
        public Color? Color
        {
            get { return (Color?)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
    }
}
