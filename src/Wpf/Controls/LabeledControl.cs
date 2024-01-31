//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.XToolkit.Wpf.Controls
{
	[ContentProperty(nameof(Content))]
	public class LabeledControl : Control
    {
        static LabeledControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabeledControl), new FrameworkPropertyMetadata(typeof(LabeledControl)));
        }

		public static readonly DependencyProperty LabelProperty =
			DependencyProperty.Register(
			nameof(Label), typeof(string),
			typeof(LabeledControl));

		public string Label
		{
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}

		public static readonly DependencyProperty GridSharedSizeGroupProperty =
			DependencyProperty.Register(
			nameof(GridSharedSizeGroup), typeof(string),
			typeof(LabeledControl));

		public string GridSharedSizeGroup
		{
			get { return (string)GetValue(GridSharedSizeGroupProperty); }
			set { SetValue(GridSharedSizeGroupProperty, value); }
		}

		public static readonly DependencyProperty ContentProperty =
			DependencyProperty.Register(
				nameof(Content), typeof(FrameworkElement),
				typeof(LabeledControl));

		public FrameworkElement Content
		{
			get { return (FrameworkElement)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public static readonly DependencyProperty LabelStyleProperty =
			DependencyProperty.Register(
			nameof(LabelStyle), typeof(Style),
			typeof(LabeledControl),
			new PropertyMetadata(typeof(LabeledControl).Assembly.LoadFromResources<Style>("Themes/Generic.xaml", "DefaultLabeledControlTextBlockStyle")));

		public Style LabelStyle
		{
			get { return (Style)GetValue(LabelStyleProperty); }
			set { SetValue(LabelStyleProperty, value); }
		}

        public static readonly DependencyProperty LabelMarginProperty =
            DependencyProperty.Register(
            nameof(LabelMargin), typeof(Thickness),
            typeof(LabeledControl));

        public Thickness LabelMargin
        {
            get { return (Thickness)GetValue(LabelMarginProperty); }
            set { SetValue(LabelMarginProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
            nameof(Icon), typeof(ImageSource),
            typeof(LabeledControl));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
    }
}
