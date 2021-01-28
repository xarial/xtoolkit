using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Xarial.XToolkit.Wpf.Controls
{
	public class XListView : ListView
	{
		public XListView()
		{
			this.SelectionChanged += OnSelectionChanged;
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectedItemsSource = this.SelectedItems;
		}

		public static readonly DependencyProperty SelectedItemsSourceProperty =
			DependencyProperty.Register(
			nameof(SelectedItemsSource), typeof(IList),
			typeof(XListView), new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				OnSelectedItemsSourceChanged));

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
