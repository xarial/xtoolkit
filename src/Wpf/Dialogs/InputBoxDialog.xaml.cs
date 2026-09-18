//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Xarial.XToolkit.Wpf.Dialogs
{
	/// <summary>
	/// Input box dialog
	/// </summary>
	public partial class InputBoxDialog : Window
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public InputBoxDialog()
		{
			InitializeComponent();
            this.Loaded += OnWindowLoaded;
		}

        /// <summary>
        /// User prompt
        /// </summary>
        public static readonly DependencyProperty PromptProperty =
			DependencyProperty.Register(
			nameof(Prompt), typeof(string),
			typeof(InputBoxDialog), new PropertyMetadata("Input value"));

		/// <summary>
		/// User prompt
		/// </summary>
		public string Prompt
		{
			get { return (string)GetValue(PromptProperty); }
			set { SetValue(PromptProperty, value); }
		}

        /// <summary>
        /// Input value
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(
			nameof(Value), typeof(string),
			typeof(InputBoxDialog));

		/// <summary>
		/// Input value
		/// </summary>
		public string Value
		{
			get { return (string)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Value))
            {
				txtValue.CaretIndex = Value.Length;
            }
        }

        private void OnCancel(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void OnOk(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
    }
}
