using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Xarial.XToolkit.Wpf.Dialogs
{
	public static class InputBox 
	{
		public static bool Show(string title, string prompt, Window parentWnd, out string value)
		{
			var dlg = new InputBoxDialog()
			{
				Title = title,
				Prompt = prompt,
				Owner = parentWnd
			};

			if (dlg.ShowDialog() == true)
			{
				value = dlg.Value;
				return true;
			}
			else
			{
				value = null;
				return false;
			}
		}
	}

	public partial class InputBoxDialog : Window
	{
		public InputBoxDialog()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty PromptProperty =
			DependencyProperty.Register(
			nameof(Prompt), typeof(string),
			typeof(InputBoxDialog), new PropertyMetadata("Input value"));

		public string Prompt
		{
			get { return (string)GetValue(PromptProperty); }
			set { SetValue(PromptProperty, value); }
		}

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(
			nameof(Value), typeof(string),
			typeof(InputBoxDialog));

		public string Value
		{
			get { return (string)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		private void OnCancel(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		private void OnOk(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}
	}
}
