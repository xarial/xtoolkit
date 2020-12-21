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

		public static readonly DependencyProperty CellTemplateSelectorProperty =
			DependencyProperty.Register(
			nameof(CellTemplateSelector), typeof(DataTemplateSelector),
			typeof(XDataGrid));

		public DataTemplateSelector CellTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(CellTemplateSelectorProperty); }
			set { SetValue(CellTemplateSelectorProperty, value); }
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

		public static readonly DependencyProperty CellEditingTemplateSelectorProperty =
			DependencyProperty.Register(
			nameof(CellEditingTemplateSelector), typeof(DataTemplateSelector),
			typeof(XDataGrid));

		public DataTemplateSelector CellEditingTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(CellEditingTemplateSelectorProperty); }
			set { SetValue(CellEditingTemplateSelectorProperty, value); }
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
			nameof(CellContentSelector), typeof(ICellContentSelector),
			typeof(XDataGrid));

		public ICellContentSelector CellContentSelector
		{
			get { return (ICellContentSelector)GetValue(CellContentSelectorProperty); }
			set { SetValue(CellContentSelectorProperty, value); }
		}

		public static readonly DependencyProperty ColumnHeaderTemplateProperty =
			DependencyProperty.Register(
			nameof(ColumnHeaderTemplate), typeof(DataTemplate),
			typeof(XDataGrid));

		public DataTemplate ColumnHeaderTemplate
		{
			get { return (DataTemplate)GetValue(ColumnHeaderTemplateProperty); }
			set { SetValue(ColumnHeaderTemplateProperty, value); }
		}

		public static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty =
			DependencyProperty.Register(
			nameof(ColumnHeaderTemplateSelector), typeof(DataTemplateSelector),
			typeof(XDataGrid));

		public DataTemplateSelector ColumnHeaderTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(ColumnHeaderTemplateSelectorProperty); }
			set { SetValue(ColumnHeaderTemplateSelectorProperty, value); }
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

					SetBinding(col, dataGrid, nameof(dataGrid.CellTemplate), XDataGridColumn.CellTemplateProperty);
					SetBinding(col, dataGrid, nameof(dataGrid.CellEditingTemplate), XDataGridColumn.CellEditingTemplateProperty);
					SetBinding(col, dataGrid, nameof(dataGrid.CellContentSelector), XDataGridColumn.CellContentSelectorProperty);
					SetBinding(col, dataGrid, nameof(dataGrid.ColumnHeaderTemplate), DataGridColumn.HeaderTemplateProperty);
					SetBinding(col, dataGrid, nameof(dataGrid.ColumnHeaderTemplateSelector), DataGridColumn.HeaderTemplateSelectorProperty);
					SetBinding(col, dataGrid, nameof(dataGrid.CellTemplateSelector), XDataGridColumn.CellTemplateSelectorProperty);
					SetBinding(col, dataGrid, nameof(dataGrid.CellEditingTemplateSelector), XDataGridColumn.CellEditingTemplateSelectorProperty);

					dataGrid.Columns.Add(col);
				}
			}
		}

		private static void SetBinding(XDataGridColumn targetColumn, XDataGrid srcDataGrid, 
			string srcPath, DependencyProperty targetPrp) 
		{
			BindingOperations.SetBinding(targetColumn,
				targetPrp,
				new Binding
				{
					Source = srcDataGrid,
					Path = new PropertyPath(srcPath),
				});
		}
	}

	public interface ICellContentSelector 
	{
		object SelectContent(object dataItem, DataGridColumn column, DataGridCell cell);
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

		public static readonly DependencyProperty CellTemplateSelectorProperty =
			DependencyProperty.Register(
			nameof(CellTemplateSelector), typeof(DataTemplateSelector),
			typeof(XDataGridColumn));

		public DataTemplateSelector CellTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(CellTemplateSelectorProperty); }
			set { SetValue(CellTemplateSelectorProperty, value); }
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

		public static readonly DependencyProperty CellEditingTemplateSelectorProperty =
			DependencyProperty.Register(
			nameof(CellEditingTemplateSelector), typeof(DataTemplateSelector),
			typeof(XDataGridColumn));

		public DataTemplateSelector CellEditingTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(CellEditingTemplateSelectorProperty); }
			set { SetValue(CellEditingTemplateSelectorProperty, value); }
		}

		public static readonly DependencyProperty CellContentSelectorProperty =
			DependencyProperty.Register(
			nameof(CellContentSelector), typeof(ICellContentSelector),
			typeof(XDataGridColumn));

		public ICellContentSelector CellContentSelector
		{
			get { return (ICellContentSelector)GetValue(CellContentSelectorProperty); }
			set { SetValue(CellContentSelectorProperty, value); }
		}

		protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
			=> CellEditingTemplateSelector != null 
			? CreateCellControl(dataItem, CellEditingTemplateSelector, cell)
			: CreateCellControl(dataItem, CellEditingTemplate, cell);

		protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
			=> CellTemplateSelector != null
			? CreateCellControl(dataItem, CellTemplateSelector, cell)
			: CreateCellControl(dataItem, CellTemplate, cell);

		private FrameworkElement CreateCellControl(object dataItem, DataTemplateSelector templateSelector, DataGridCell cell)
		{
			var content = CellContentSelector?.SelectContent(dataItem, this, cell);

			var ctrl = new ContentPresenter()
			{
				Content = content
			};

			ctrl.ContentTemplate = templateSelector.SelectTemplate(content, ctrl);

			return ctrl;
		}

		private FrameworkElement CreateCellControl(object dataItem, DataTemplate template, DataGridCell cell)
		{
			var ctrl = new ContentPresenter()
			{
				ContentTemplate = template,
				Content = CellContentSelector?.SelectContent(dataItem, this, cell)
			};
			
			return ctrl;
		}
	}
}
