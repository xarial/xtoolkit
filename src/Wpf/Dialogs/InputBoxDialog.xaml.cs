//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Xarial.XToolkit.Wpf.Dialogs
{
	/// <summary>
	/// Simple dialog to collect input value
	/// </summary>
	public static class InputBox 
	{
		/// <summary>
		/// Shows the input box at the mouse cursor
		/// </summary>
		/// <param name="title">Title of the box</param>
		/// <param name="prompt">User prompt</param>
		/// <param name="value">Input value</param>
		/// <returns>True if user clicked OK</returns>
		public static bool ShowAtCursor(string title, string prompt, ref string value) 
		{
			var cursorPos = System.Windows.Forms.Cursor.Position;

			Point pos;

			using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
			{
				const int DPI = 96;

				var scaleX = graphics.DpiX / DPI;
				var scaleY = graphics.DpiY / DPI;

				pos = new Point(cursorPos.X / scaleX, cursorPos.Y / scaleY);
			}

			return Show(title, prompt, pos, ref value);
		}

		public static bool Show(string title, string prompt, Point pos, ref string value)
			=> Show(title, prompt, null, WindowStartupLocation.Manual, pos, ref value);

		public static bool Show(string title, string prompt, Window parentWnd, ref string value)
			=> Show(title, prompt, parentWnd, parentWnd != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen, null, ref value);

		private static bool Show(string title, string prompt, Window parentWnd, WindowStartupLocation startupLocation, Point? pos, ref string value)
		{
			var dlg = new InputBoxDialog()
			{
				Title = title,
				Prompt = prompt,
				Value = value,
				Owner = parentWnd,
				WindowStartupLocation = startupLocation
			};

			if (pos.HasValue) 
			{
				dlg.Left = pos.Value.X;
				dlg.Top = pos.Value.Y;
			}

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
            this.Loaded += OnWindowLoaded;
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
