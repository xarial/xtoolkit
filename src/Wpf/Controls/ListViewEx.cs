//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Xarial.XToolkit.Wpf.Controls
{
	/// <summary>
	/// Extended ListView control
	/// </summary>
	public class ListViewEx : ListView
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public ListViewEx()
		{
			this.SelectionChanged += OnSelectionChanged;
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectedItemsSource = this.SelectedItems;
		}

        /// <summary>
        /// Items source for selected items
        /// </summary>
        public static readonly DependencyProperty SelectedItemsSourceProperty =
			DependencyProperty.Register(
			nameof(SelectedItemsSource), typeof(IList),
			typeof(ListViewEx), new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				OnSelectedItemsSourceChanged));

		/// <summary>
		/// Items source for selected items
		/// </summary>
		public IList SelectedItemsSource
		{
			get { return (IList)GetValue(SelectedItemsSourceProperty); }
			set { SetValue(SelectedItemsSourceProperty, value); }
		}

		private static void OnSelectedItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//TODO: update selected items in list
			//TODO: subscribe if IObservable
		}
	}
}
