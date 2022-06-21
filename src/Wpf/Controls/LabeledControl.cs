//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
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

		public static readonly DependencyProperty TextBlockStyleProperty =
			DependencyProperty.Register(
			nameof(TextBlockStyle), typeof(Style),
			typeof(LabeledControl),
			new PropertyMetadata(typeof(LabeledControl).Assembly.LoadFromResources<Style>("Themes/Generic.xaml", "DefaultLabeledControlTextBlockStyle")));

		public Style TextBlockStyle
		{
			get { return (Style)GetValue(TextBlockStyleProperty); }
			set { SetValue(TextBlockStyleProperty, value); }
		}
	}
}
