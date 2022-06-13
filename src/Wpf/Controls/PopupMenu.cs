using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.XToolkit.Wpf.Controls
{
    [ContentProperty(nameof(Content))]
    public class PopupMenu : Control
    {
        static PopupMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupMenu),
                new FrameworkPropertyMetadata(typeof(PopupMenu)));
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content), typeof(FrameworkElement),
                typeof(PopupMenu));

        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ToggleButtonStyleProperty =
            DependencyProperty.Register(
            nameof(ToggleButtonStyle), typeof(Style),
            typeof(PopupMenu),
            new PropertyMetadata(typeof(PopupMenu).Assembly.LoadFromResources<Style>("Themes/Generic.xaml", "ExpandToggleButtonStyle")));

        public Style ToggleButtonStyle
        {
            get { return (Style)GetValue(ToggleButtonStyleProperty); }
            set { SetValue(ToggleButtonStyleProperty, value); }
        }
    }
}
