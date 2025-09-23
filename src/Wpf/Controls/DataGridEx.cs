//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Controls
{
	/// <summary>
	/// Delegate of <see cref="DataGridEx.ColumnsPreCreated"/> event
	/// </summary>
	/// <param name="columns">List of columns</param>
	public delegate void ColumnsPreCreatedDelegate(List<DataGridColumn> columns);

	/// <summary>
	/// Extended DataGrid control
	/// </summary>
	public class DataGridEx : DataGrid
    {
		/// <summary>
		/// Event is raised when columns are pre-created
		/// </summary>
		/// <remarks>Use this event to change columns (e.g. reorder)</remarks>
		public event ColumnsPreCreatedDelegate ColumnsPreCreated;

        /// <summary>
        /// Generic template for cell
        /// </summary>
        public static readonly DependencyProperty CellTemplateProperty =
			DependencyProperty.Register(
			nameof(CellTemplate), typeof(DataTemplate),
			typeof(DataGridEx));

		/// <summary>
		/// Generic template for cell
		/// </summary>
		public DataTemplate CellTemplate
		{
			get { return (DataTemplate)GetValue(CellTemplateProperty); }
			set { SetValue(CellTemplateProperty, value); }
		}

		/// <summary>
		/// Generic cell template selector
		/// </summary>
		public static readonly DependencyProperty CellTemplateSelectorProperty =
			DependencyProperty.Register(
			nameof(CellTemplateSelector), typeof(DataTemplateSelector),
			typeof(DataGridEx));

        /// <summary>
        /// Generic cell template selector
        /// </summary>
        public DataTemplateSelector CellTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(CellTemplateSelectorProperty); }
			set { SetValue(CellTemplateSelectorProperty, value); }
		}

		/// <summary>
		/// Generic cell editing template
		/// </summary>
		public static readonly DependencyProperty CellEditingTemplateProperty =
            DependencyProperty.Register(
            nameof(CellEditingTemplate), typeof(DataTemplate),
            typeof(DataGridEx));

        /// <summary>
        /// Generic cell editing template
        /// </summary>
        public DataTemplate CellEditingTemplate
        {
            get { return (DataTemplate)GetValue(CellEditingTemplateProperty); }
            set { SetValue(CellEditingTemplateProperty, value); }
        }

        /// <summary>
        /// Generic cell editing template selector
        /// </summary>
        public static readonly DependencyProperty CellEditingTemplateSelectorProperty =
			DependencyProperty.Register(
			nameof(CellEditingTemplateSelector), typeof(DataTemplateSelector),
			typeof(DataGridEx));

        /// <summary>
        /// Generic cell editing template selector
        /// </summary>
        public DataTemplateSelector CellEditingTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(CellEditingTemplateSelectorProperty); }
			set { SetValue(CellEditingTemplateSelectorProperty, value); }
		}

		/// <summary>
		/// Dynamic columns source
		/// </summary>
		public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.Register(
            nameof(ColumnsSource), typeof(IEnumerable),
            typeof(DataGridEx), new PropertyMetadata(OnColumnsSourcePropertyChanged));

        /// <summary>
        /// Dynamic columns source
        /// </summary>
        public IEnumerable ColumnsSource
        {
            get { return (IEnumerable)GetValue(ColumnsSourceProperty); }
            set { SetValue(ColumnsSourceProperty, value); }
        }

		/// <summary>
		/// Static columns
		/// </summary>
		public static readonly DependencyProperty StaticColumnsProperty =
			DependencyProperty.Register(
			nameof(StaticColumns), typeof(List<DataGridColumn>),
			typeof(DataGridEx), new PropertyMetadata(OnStaticColumnsPropertyChanged));

        /// <summary>
        /// Static columns
        /// </summary>
        public List<DataGridColumn> StaticColumns
		{
			get { return (List<DataGridColumn>)GetValue(StaticColumnsProperty); }
			set { SetValue(StaticColumnsProperty, value); }
		}

		/// <summary>
		/// Selector of the data context for cell
		/// </summary>
		public static readonly DependencyProperty CellContentSelectorProperty =
			DependencyProperty.Register(
			nameof(CellContentSelector), typeof(ICellContentSelector),
			typeof(DataGridEx));

        /// <summary>
        /// Selector of the data context for cell
        /// </summary>
        public ICellContentSelector CellContentSelector
		{
			get { return (ICellContentSelector)GetValue(CellContentSelectorProperty); }
			set { SetValue(CellContentSelectorProperty, value); }
		}

		/// <summary>
		/// Generic column header template
		/// </summary>
		public static readonly DependencyProperty ColumnHeaderTemplateProperty =
			DependencyProperty.Register(
			nameof(ColumnHeaderTemplate), typeof(DataTemplate),
			typeof(DataGridEx));

        /// <summary>
        /// Generic column header template
        /// </summary>
        public DataTemplate ColumnHeaderTemplate
		{
			get { return (DataTemplate)GetValue(ColumnHeaderTemplateProperty); }
			set { SetValue(ColumnHeaderTemplateProperty, value); }
		}

        /// <summary>
        /// Generic column header template selector
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty =
			DependencyProperty.Register(
			nameof(ColumnHeaderTemplateSelector), typeof(DataTemplateSelector),
			typeof(DataGridEx));

        /// <summary>
        /// Generic column header template selector
        /// </summary>
        public DataTemplateSelector ColumnHeaderTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(ColumnHeaderTemplateSelectorProperty); }
			set { SetValue(ColumnHeaderTemplateSelectorProperty, value); }
		}
		
		/// <summary>
		/// Binding of column visibility
		/// </summary>
		public BindingBase ColumnVisibilityBinding { get; set; }

        private static readonly DependencyPropertyKey SelectedCellContentsPropertyKey =
			DependencyProperty.RegisterReadOnly(
				nameof(SelectedCellContents),
				typeof(IList<object>),
				typeof(DataGridEx),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Selected cell contents
		/// </summary>
        public static readonly DependencyProperty SelectedCellContentsProperty =
            SelectedCellContentsPropertyKey.DependencyProperty;

        /// <summary>
        /// Selected cell contents
        /// </summary>
        public IList<object> SelectedCellContents
        {
            get => (IList<object>)GetValue(SelectedCellContentsProperty);
            protected set => SetValue(SelectedCellContentsPropertyKey, value);
        }

        private DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
			if (cellContent != null)
			{
				return (DataGridCell)cellContent.Parent;
			}
			else
			{
				return null;
			}
        }

		/// <summary>
		/// Constructor
		/// </summary>
        public DataGridEx() 
		{
			SetValue(StaticColumnsProperty, new List<DataGridColumn>());
		}

        protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
        {
            base.OnSelectedCellsChanged(e);

            if (CellContentSelector != null)
            {
                var contents = new List<object>();

                if (SelectedCells != null)
                {
                    foreach (DataGridCellInfo cellInfo in SelectedCells)
                    {
                        contents.Add(CellContentSelector.SelectContent(cellInfo.Item, cellInfo.Column, GetDataGridCell(cellInfo)));
                    }
                }

                SelectedCellContents = contents;
            }
        }

        private static void OnColumnsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = (DataGridEx)d;
			dataGrid.Dispatcher.Invoke(() =>
			{
				dataGrid.LoadColumns(dataGrid.StaticColumns, e.OldValue as IEnumerable, e.NewValue as IEnumerable);
			});
		}

		private static void OnStaticColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = (DataGridEx)d;
			dataGrid.Dispatcher.Invoke(() =>
			{
				dataGrid.LoadColumns(e.NewValue as IEnumerable<DataGridColumn>, dataGrid.ColumnsSource, dataGrid.ColumnsSource);
			});
		}

		private void LoadColumns(IEnumerable<DataGridColumn> staticColumns, IEnumerable oldColumnsSrc, IEnumerable columnSrc) 
		{
			if (oldColumnsSrc != columnSrc)
			{
				if (oldColumnsSrc is INotifyCollectionChanged)
				{
					((INotifyCollectionChanged)oldColumnsSrc).CollectionChanged -= OnColumnsSourceCollectionChanged;
				}

				if (columnSrc is INotifyCollectionChanged)
				{
					((INotifyCollectionChanged)columnSrc).CollectionChanged += OnColumnsSourceCollectionChanged;
				}
			}

			AutoGenerateColumns = false;

			var columns = new List<DataGridColumn>();

			Columns.Clear();

			if (staticColumns != null) 
			{
				foreach (var col in staticColumns) 
				{
					columns.Add(col);
				}
			}

			if (columnSrc != null)
			{
				foreach (var colSrc in columnSrc)
				{
					var col = new DataGridColumnEx()
					{
						Header = colSrc
					};

					SetBinding(col, nameof(CellTemplate), DataGridColumnEx.CellTemplateProperty);
					SetBinding(col, nameof(CellEditingTemplate), DataGridColumnEx.CellEditingTemplateProperty);
					SetBinding(col, nameof(CellContentSelector), DataGridColumnEx.CellContentSelectorProperty);
					SetBinding(col, nameof(ColumnHeaderTemplate), DataGridColumn.HeaderTemplateProperty);
					SetBinding(col, nameof(ColumnHeaderTemplateSelector), DataGridColumn.HeaderTemplateSelectorProperty);
					SetBinding(col, nameof(CellTemplateSelector), DataGridColumnEx.CellTemplateSelectorProperty);
					SetBinding(col, nameof(CellEditingTemplateSelector), DataGridColumnEx.CellEditingTemplateSelectorProperty);

					if (ColumnVisibilityBinding != null)
					{
						BindingOperations.SetBinding(col, DataGridColumnEx.VisibilityProperty, ColumnVisibilityBinding);
					}

					columns.Add(col);
				}

				ColumnsPreCreated?.Invoke(columns);

				foreach (var column in columns) 
				{
					Columns.Add(column);
				}
			}
		}

        private void OnColumnsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
			Dispatcher.Invoke(() =>
			{
				LoadColumns(StaticColumns, ColumnsSource, ColumnsSource);
			});
		}

        private void SetBinding(DataGridColumnEx targetColumn, 
			string srcPath, DependencyProperty targetPrp) 
		{
			var binding = new Binding
			{
				Source = this,
				Path = new PropertyPath(srcPath)
			};

			BindingOperations.SetBinding(targetColumn,
				targetPrp, binding);
		}
	}

	/// <summary>
	/// Content selector service for the cell
	/// </summary>
	/// <remarks>Set in <see cref="DataGridEx.CellContentSelector"/></remarks>
	public interface ICellContentSelector 
	{
		/// <summary>
		/// Selects content for the cell
		/// </summary>
		/// <param name="dataItem">Bound item</param>
		/// <param name="column">Requesting column</param>
		/// <param name="cell">Target cell</param>
		/// <returns>Cell content</returns>
		object SelectContent(object dataItem, DataGridColumn column, DataGridCell cell);
	}

	public class DataGridColumnEx : DataGridColumn
	{
		public static readonly DependencyProperty CellTemplateProperty =
			DependencyProperty.Register(
			nameof(CellTemplate), typeof(DataTemplate),
			typeof(DataGridColumnEx));

		public DataTemplate CellTemplate
		{
			get { return (DataTemplate)GetValue(CellTemplateProperty); }
			set { SetValue(CellTemplateProperty, value); }
		}

		public static readonly DependencyProperty CellTemplateSelectorProperty =
			DependencyProperty.Register(
			nameof(CellTemplateSelector), typeof(DataTemplateSelector),
			typeof(DataGridColumnEx));

		public DataTemplateSelector CellTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(CellTemplateSelectorProperty); }
			set { SetValue(CellTemplateSelectorProperty, value); }
		}

		public static readonly DependencyProperty CellEditingTemplateProperty =
			DependencyProperty.Register(
			nameof(CellEditingTemplate), typeof(DataTemplate),
			typeof(DataGridColumnEx));

		public DataTemplate CellEditingTemplate
		{
			get { return (DataTemplate)GetValue(CellEditingTemplateProperty); }
			set { SetValue(CellEditingTemplateProperty, value); }
		}

		public static readonly DependencyProperty CellEditingTemplateSelectorProperty =
			DependencyProperty.Register(
			nameof(CellEditingTemplateSelector), typeof(DataTemplateSelector),
			typeof(DataGridColumnEx));

		public DataTemplateSelector CellEditingTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(CellEditingTemplateSelectorProperty); }
			set { SetValue(CellEditingTemplateSelectorProperty, value); }
		}

		public static readonly DependencyProperty CellContentSelectorProperty =
			DependencyProperty.Register(
			nameof(CellContentSelector), typeof(ICellContentSelector),
			typeof(DataGridColumnEx));

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
