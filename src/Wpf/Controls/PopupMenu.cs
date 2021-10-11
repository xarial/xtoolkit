using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

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
    }
}
