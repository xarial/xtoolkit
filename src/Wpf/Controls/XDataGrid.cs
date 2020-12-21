using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Controls
{
	public class XDataGrid : DataGrid
    {
		public static readonly DependencyProperty CellTemplateProperty =
			DependencyProperty.Register(
			nameof(CellTemplate), typeof(DataTemplate),
			typeof(XDataGrid));

		public DataTemplate CellTemplate
		{
			get { return (DataTemplate)GetValue(CellTemplateProperty); }
			set { SetValue(CellTemplateProperty, value); }
		}

		public static readonly DependencyProperty CellEditingTemplateProperty =
            DependencyProperty.Register(
            nameof(CellEditingTemplate), typeof(DataTemplate),
            typeof(XDataGrid));

        public DataTemplate CellEditingTemplate
        {
            get { return (DataTemplate)GetValue(CellEditingTemplateProperty); }
            set { SetValue(CellEditingTemplateProperty, value); }
        }

        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.Register(
            nameof(ColumnsSource), typeof(IEnumerable),
            typeof(XDataGrid), new PropertyMetadata(OnColumnsSourcePropertyChanged));

        public string ColumnsSource
        {
            get { return (string)GetValue(ColumnsSourceProperty); }
            set { SetValue(ColumnsSourceProperty, value); }
        }

		public static readonly DependencyProperty CellContentSelectorProperty =
			DependencyProperty.Register(
			nameof(CellContentSelector), typeof(Func<object, DataGridColumn, DataGridCell, object>),
			typeof(XDataGrid));

		public Func<object, DataGridColumn, DataGridCell, object> CellContentSelector
		{
			get { return (Func<object, DataGridColumn, DataGridCell, object>)GetValue(CellContentSelectorProperty); }
			set { SetValue(CellContentSelectorProperty, value); }
		}

		private static void OnColumnsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = (XDataGrid)d;
			dataGrid.AutoGenerateColumns = false;

			var columnSrcs = e.NewValue as IEnumerable;

			dataGrid.Columns.Clear();

			if (columnSrcs != null)
			{
				foreach (var colSrc in columnSrcs)
				{
					var col = new XDataGridColumn()
					{
						Header = colSrc
					};

					BindingOperations.SetBinding(col,
						XDataGridColumn.CellTemplateProperty,
						new Binding
						{
							Source = dataGrid,
							Path = new PropertyPath(nameof(dataGrid.CellTemplate)),
						});

					BindingOperations.SetBinding(col,
						XDataGridColumn.CellEditingTemplateProperty,
						new Binding
						{
							Source = dataGrid,
							Path = new PropertyPath(nameof(dataGrid.CellEditingTemplate)),
						});

					BindingOperations.SetBinding(col,
						XDataGridColumn.CellContentSelectorProperty,
						new Binding
						{
							Source = dataGrid,
							Path = new PropertyPath(nameof(dataGrid.CellContentSelector)),
						});

					dataGrid.Columns.Add(col);
				}
			}
		}
	}

	public class XDataGridColumn : DataGridColumn
	{
		public static readonly DependencyProperty CellTemplateProperty =
			DependencyProperty.Register(
			nameof(CellTemplate), typeof(DataTemplate),
			typeof(XDataGridColumn));

		public DataTemplate CellTemplate
		{
			get { return (DataTemplate)GetValue(CellTemplateProperty); }
			set { SetValue(CellTemplateProperty, value); }
		}

		public static readonly DependencyProperty CellEditingTemplateProperty =
			DependencyProperty.Register(
			nameof(CellEditingTemplate), typeof(DataTemplate),
			typeof(XDataGridColumn));

		public DataTemplate CellEditingTemplate
		{
			get { return (DataTemplate)GetValue(CellEditingTemplateProperty); }
			set { SetValue(CellEditingTemplateProperty, value); }
		}

		public static readonly DependencyProperty CellContentSelectorProperty =
			DependencyProperty.Register(
			nameof(CellContentSelector), typeof(Func<object, DataGridColumn, DataGridCell, object>),
			typeof(XDataGridColumn));

		public Func<object, DataGridColumn, DataGridCell, object> CellContentSelector
		{
			get { return (Func<object, DataGridColumn, DataGridCell, object>)GetValue(CellContentSelectorProperty); }
			set { SetValue(CellContentSelectorProperty, value); }
		}

		protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
			=> CreateCellControl(dataItem, CellEditingTemplate, cell);

		protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
			=> CreateCellControl(dataItem, CellTemplate, cell);

		private FrameworkElement CreateCellControl(object dataItem, DataTemplate template, DataGridCell cell)
		{
			var ctrl = new ContentPresenter()
			{
				ContentTemplate = template,
				Content = CellContentSelector?.Invoke(dataItem, this, cell)
			};

			return ctrl;
		}
	}
}
