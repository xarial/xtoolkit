//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Xarial.XToolkit.Wpf.Extensions;

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
        private class CellContentsList : IList<object>
        {
            #region NotSupported
            public void Add(object item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Contains(object item) => m_CellContents.Contains(item);
            public void Insert(int index, object item) => throw new NotSupportedException();
            public bool Remove(object item) => throw new NotSupportedException();
            public void RemoveAt(int index) => throw new NotSupportedException();
            #endregion

            public object this[int index] 
			{
				get => m_CellContents.ElementAt(index);
				set => throw new NotSupportedException(); 
			}

            public int Count { get; }

            public bool IsReadOnly => true;

            private readonly IEnumerable<object> m_CellContents;

            internal CellContentsList(IEnumerable<object> cellContents, int count)
            {
                m_CellContents = cellContents;
                Count = count;
            }

            public void CopyTo(object[] array, int arrayIndex) => m_CellContents.ToArray().CopyTo(array, arrayIndex);

            public IEnumerator<object> GetEnumerator() => m_CellContents.GetEnumerator();

            public int IndexOf(object item)
            {
                var index = -1;

                foreach (var content in m_CellContents)
                {
                    index++;

                    if (object.Equals(content, item))
                    {
                        break;
                    }
                }

                return index;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class DragState
        {
            internal Point StartPoint { get; set; }
            internal object Item { get; set; }
            internal ScrollViewer ScrollViewer { get; set; }
            internal InsertionAdorner Adorner { get; set; }
            internal AdornerLayer AdornerLayer { get; set; }
        }

        private class DragPayload
        {
            internal DataGrid Grid { get; }
            internal object Item { get; }

            internal DragPayload(DataGrid grid, object item)
            {
                Grid = grid;
                Item = item;
            }
        }

        private class InsertionAdorner : Adorner
        {
            private readonly Pen m_Pen;

            private bool m_IsAfter;

            internal InsertionAdorner(UIElement adornedElement) : base(adornedElement)
            {
                IsHitTestVisible = false;

                m_Pen = new Pen(SystemColors.HighlightBrush, 2);
                m_Pen.Freeze();
            }

            internal bool IsAfter
            {
                get => m_IsAfter;
                set
                {
                    if (m_IsAfter != value)
                    {
                        m_IsAfter = value;
                        InvalidateVisual();
                    }
                }
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                var size = AdornedElement.RenderSize;
                var y = m_IsAfter ? size.Height : 0;

                drawingContext.DrawLine(m_Pen, new Point(0, y), new Point(size.Width, y));
            }
        }

        private const string DRAG_FORMAT = "Xarial.XToolkit.Wpf.Controls.DataGridRowReorderItem";

        private const double AUTO_SCROLL_MARGIN = 20;

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
            private set => SetValue(SelectedCellContentsPropertyKey, value);
        }

        private IEnumerable<object> IterateSelectedCellContents()
		{
            if (SelectedCells != null)
            {
                foreach (DataGridCellInfo cellInfo in SelectedCells)
                {
                    if (cellInfo.Column is DataGridColumnEx)
                    {
						yield return CellContentSelector?.SelectContent(cellInfo.Item, cellInfo.Column, GetDataGridCell(cellInfo));
                    }
                    else
                    {
						yield return GetCellValue(cellInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Automatically commit row when cell value is changed
        /// </summary>
        /// <remarks>This allows to avoid the need of pressing Enter button to commit row</remarks>
        public static readonly DependencyProperty AutoCommitRowProperty =
			DependencyProperty.Register(
			nameof(AutoCommitRow), typeof(bool),
			typeof(DataGridEx), new PropertyMetadata(false));

		/// <summary>
		/// Automatically commit row when cell value is changed
		/// </summary>
		/// <remarks>This allows to avoid the need of pressing Enter button to commit row</remarks>
		public bool AutoCommitRow
		{
			get { return (bool)GetValue(AutoCommitRowProperty); }
			set { SetValue(AutoCommitRowProperty, value); }
		}

        /// <summary>
        /// Gets or sets a value that indicates whether the user can change the rows display order by dragging rows with the mouse
        /// </summary>
		public static readonly DependencyProperty CanUserReorderRowsProperty =
			DependencyProperty.Register(
			nameof(CanUserReorderRows), typeof(bool),
			typeof(DataGridEx), new PropertyMetadata(false, OnCanUserReorderRowsChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can change the rows display order by dragging rows with the mouse
        /// </summary>
		public bool CanUserReorderRows
		{
			get { return (bool)GetValue(CanUserReorderRowsProperty); }
			set { SetValue(CanUserReorderRowsProperty, value); }
		}

		private DragState m_DragState;

        private bool m_IsCommitting;

        /// <summary>
        /// Constructor
        /// </summary>
        public DataGridEx() 
		{
			SetValue(StaticColumnsProperty, new List<DataGridColumn>());
		}

		private static void OnCanUserReorderRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var grid = (DataGridEx)d;

			if ((bool)e.NewValue)
			{
				grid.m_DragState = new DragState();
				grid.AllowDrop = true;

				grid.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
				grid.PreviewMouseMove += OnPreviewMouseMove;
				grid.DragOver += OnDragOver;
				grid.DragLeave += OnDragLeave;
				grid.Drop += OnDrop;
			}
			else
			{
				grid.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
				grid.PreviewMouseMove -= OnPreviewMouseMove;
				grid.DragOver -= OnDragOver;
				grid.DragLeave -= OnDragLeave;
				grid.Drop -= OnDrop;

				grid.ClearValue(AllowDropProperty);
				grid.m_DragState = null;
			}
		}

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = (DataGridEx)sender;
            var state = grid.m_DragState;

            var row = FindRow(grid, e.OriginalSource as DependencyObject);

            if (row != null && !row.IsEditing && row.Item != CollectionView.NewItemPlaceholder && TryGetReorderableList(grid, out _))
            {
                state.StartPoint = e.GetPosition(grid);
                state.Item = row.Item;
            }
            else
            {
                state.Item = null;
            }
        }

        private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var grid = (DataGridEx)sender;
            var state = grid.m_DragState;

            if (e.LeftButton == MouseButtonState.Pressed && state.Item != null)
            {
                var offset = e.GetPosition(grid) - state.StartPoint;

                if (Math.Abs(offset.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(offset.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var item = state.Item;
                    state.Item = null;

                    try
                    {
                        DragDrop.DoDragDrop(grid, new DataObject(DRAG_FORMAT, new DragPayload(grid, item)), DragDropEffects.Move);
                    }
                    finally
                    {
                        HideAdorner(state);
                    }
                }
            }
            else
            {
                state.Item = null;
            }
        }

        private static void OnDragOver(object sender, DragEventArgs e)
        {
            var grid = (DataGridEx)sender;
            var state = grid.m_DragState;

            e.Effects = DragDropEffects.None;

            if (TryGetPayloadItem(grid, e.Data, out var item))
            {
                AutoScroll(grid, state, e);

                if (TryGetDropTarget(grid, item, e, out var targetRow, out var isAfter))
                {
                    e.Effects = DragDropEffects.Move;
                    ShowAdorner(state, targetRow, isAfter);
                }
                else
                {
                    HideAdorner(state);
                }
            }

            e.Handled = true;
        }

        private static void OnDragLeave(object sender, DragEventArgs e)
        {
            var grid = (DataGridEx)sender;

            if (!new Rect(grid.RenderSize).Contains(e.GetPosition(grid)))
            {
                HideAdorner(grid.m_DragState);
            }
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            var grid = (DataGridEx)sender;
            var state = grid.m_DragState;

            HideAdorner(state);

            if (TryGetPayloadItem(grid, e.Data, out var item)
                && TryGetDropTarget(grid, item, e, out var targetRow, out var isAfter)
                && TryGetReorderableList(grid, out var list))
            {
                MoveItem(list, item, targetRow.Item, isAfter);

                grid.SelectedItem = item;
                grid.ScrollIntoView(item);
            }

            e.Handled = true;
        }

        private static void MoveItem(IList list, object item, object targetItem, bool isAfter)
        {
            var oldIndex = list.IndexOf(item);

            int newIndex;

            if (targetItem == CollectionView.NewItemPlaceholder)
            {
                newIndex = list.Count - 1;
            }
            else
            {
                newIndex = list.IndexOf(targetItem);

                if (isAfter)
                {
                    newIndex++;
                }

                if (oldIndex < newIndex)
                {
                    newIndex--;
                }
            }

            if (oldIndex != -1 && newIndex != -1 && oldIndex != newIndex)
            {
                list.RemoveAt(oldIndex);
                list.Insert(newIndex, item);
            }
        }

        private static bool TryGetReorderableList(DataGrid grid, out IList list)
        {
            var view = (IEditableCollectionView)grid.Items;

            if (grid.ItemsSource is IList srcList
                && !srcList.IsReadOnly
                && !srcList.IsFixedSize
                && grid.Items.SortDescriptions.Count == 0
                && (grid.Items.GroupDescriptions == null || grid.Items.GroupDescriptions.Count == 0)
                && !view.IsAddingNew
                && !view.IsEditingItem)
            {
                list = srcList;
                return true;
            }
            else
            {
                list = null;
                return false;
            }
        }

        private static bool TryGetPayloadItem(DataGrid grid, IDataObject data, out object item)
        {
            if (data.GetDataPresent(DRAG_FORMAT) && data.GetData(DRAG_FORMAT) is DragPayload payload && payload.Grid == grid)
            {
                item = payload.Item;
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }

        private static bool TryGetDropTarget(DataGrid grid, object draggedItem, DragEventArgs e, out DataGridRow targetRow, out bool isAfter)
        {
            var row = FindRow(grid, e.OriginalSource as DependencyObject);

            if (row != null && row.Item != draggedItem)
            {
                targetRow = row;
                isAfter = row.Item != CollectionView.NewItemPlaceholder && e.GetPosition(row).Y > row.ActualHeight / 2;
                return true;
            }
            else
            {
                targetRow = null;
                isAfter = false;
                return false;
            }
        }

        private static void AutoScroll(DataGrid grid, DragState state, DragEventArgs e)
        {
            if (state.ScrollViewer == null)
            {
                state.ScrollViewer = grid.TryFindChildOfType<ScrollViewer>();
            }

            if (state.ScrollViewer != null)
            {
                var y = e.GetPosition(grid).Y;

                if (y < AUTO_SCROLL_MARGIN)
                {
                    state.ScrollViewer.LineUp();
                }
                else if (y > grid.ActualHeight - AUTO_SCROLL_MARGIN)
                {
                    state.ScrollViewer.LineDown();
                }
            }
        }

        private static void ShowAdorner(DragState state, DataGridRow row, bool isAfter)
        {
            if (state.Adorner == null || state.Adorner.AdornedElement != row)
            {
                HideAdorner(state);

                var layer = AdornerLayer.GetAdornerLayer(row);

                if (layer != null)
                {
                    state.Adorner = new InsertionAdorner(row);
                    state.AdornerLayer = layer;
                    layer.Add(state.Adorner);
                }
            }

            if (state.Adorner != null)
            {
                state.Adorner.IsAfter = isAfter;
            }
        }

        private static void HideAdorner(DragState state)
        {
            if (state.Adorner != null)
            {
                state.AdornerLayer.Remove(state.Adorner);
                state.Adorner = null;
                state.AdornerLayer = null;
            }
        }

        private static DataGridRow FindRow(DataGrid grid, DependencyObject elem)
        {
            while (elem != null && elem != grid)
            {
                if (elem is DataGridRow row && ItemsControl.ItemsControlFromItemContainer(row) == grid)
                {
                    return row;
                }

                elem = elem is Visual || elem is Visual3D ? VisualTreeHelper.GetParent(elem) : LogicalTreeHelper.GetParent(elem);
            }

            return null;
        }


        /// <inheritdoc/>
        protected override void OnCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            base.OnCellEditEnding(e);

			if (AutoCommitRow)
			{
				if (!m_IsCommitting && e.EditAction == DataGridEditAction.Commit)
				{
					m_IsCommitting = true;
					Dispatcher.BeginInvoke(new Action(() =>
					{
						CommitEdit(DataGridEditingUnit.Row, true);
						m_IsCommitting = false;
					}), System.Windows.Threading.DispatcherPriority.Background);
				}
			}
		}

        /// <inheritdoc/>
        protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
        {
            base.OnSelectedCellsChanged(e);

			SelectedCellContents = new CellContentsList(IterateSelectedCellContents(), SelectedCells.Count);
        }

        private object GetCellValue(DataGridCellInfo cellInfo)
        {
            if (cellInfo.Column is DataGridBoundColumn boundColumn)
            {
                var binding = (Binding)boundColumn.Binding;

                if (binding != null)
                {
					return ResolvePropertyPath(cellInfo.Item, binding.Path);
                }
            }

            var content = cellInfo.Column.GetCellContent(cellInfo.Item);

			switch (content) 
			{
				case TextBlock tb:
					return tb.Text;

				case CheckBox cb:
					return cb.IsChecked;
			}

            return null;
        }

        private object ResolvePropertyPath(object item, PropertyPath path)
        {
			if (item != null && !string.IsNullOrWhiteSpace(path.Path))
			{
				object current = item;

				foreach (var part in path.Path.Split('.'))
				{
					if (current != null)
					{
						if (current is DataRowView drv)
						{
							current = drv[part];
						}
						else
						{
							var prop = current.GetType().GetProperty(part);

							if (prop != null)
							{
								current = prop.GetValue(current);
							}
							else
							{
								return null;
							}
						}
					}
					else
					{
						return null;
					}
				}

                return current;
            }
			else 
			{
				return null;
			}
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

			var oldColumns = Columns.ToArray();

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
					var col = oldColumns.OfType<DataGridColumnEx>().FirstOrDefault(c => object.Equals(c.Header, colSrc));

					if (col == null)
					{
						col = new DataGridColumnEx()
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

	internal class DataGridColumnEx : DataGridColumn
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
