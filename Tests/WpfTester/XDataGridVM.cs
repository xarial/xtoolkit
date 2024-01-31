using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Controls;
using Xarial.XToolkit.Wpf.Extensions;

namespace WpfTester
{
    public class CellVM
    {
        public string Value { get; set; }
    }

    public class RowVM 
    {
        public Dictionary<string, CellVM> Cells { get; set; }
    }

    public class ColumnVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Visibility m_Visibility;

        public string Title { get; }

        public Visibility Visibility 
        {
            get => m_Visibility;
            set 
            {
                m_Visibility = value;
                this.NotifyChanged();
            }
        }

        public ColumnVM(string title) 
        {
            Title = title;
            m_Visibility = Visibility.Visible;
        }
    }

    public class CellContentSelector : ICellContentSelector
    {
        public CellContentSelector() 
        {
        }

        public object SelectContent(object dataItem, DataGridColumn column, DataGridCell cell)
        {
            return (dataItem as RowVM).Cells[((ColumnVM)column.Header).Title];
        }
    }
    
    public class MyCellTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AColumn { get; set; }
        public DataTemplate Default { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if ((item as CellVM).Value == "Val1-1")
            {
                return AColumn;
            }
            else
            {
                return Default;
            }
        }
    }

    public class MyCellEditingTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AColumn { get; set; }
        public DataTemplate Default { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if ((item as CellVM).Value == "Val1-1")
            {
                return AColumn;
            }
            else
            {
                return Default;
            }
        }
    }

    public class MyColumnTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AColumn { get; set; }
        public DataTemplate Default { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (((ColumnVM)item).Title == "A")
            {
                return AColumn;
            }
            else
            {
                return Default;
            }
        }
    }
    
    public class XDataGridVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public RowVM[] Rows { get; set; }
        public ObservableCollection<RowVM> DynamicRows { get; set; }

        public ColumnVM[] ColumnsSource { get; set; }

        public ObservableCollection<ColumnVM> DynamicColumnsSource { get; set; }

        private bool m_ShowColumns;

        public ICommand AddColumnCommand { get; }
        public ICommand AddColumnAsyncCommand { get; }

        public bool ShowColumns 
        {
            get => m_ShowColumns;
            set 
            {
                m_ShowColumns = value;
                this.NotifyChanged();

                if (m_ShowColumns)
                {
                    foreach (var col in ColumnsSource)
                    {
                        col.Visibility = Visibility.Visible;
                    }
                }
                else 
                {
                    ColumnsSource[0].Visibility = Visibility.Collapsed;
                    ColumnsSource[2].Visibility = Visibility.Collapsed;
                }
            }
        }

        public XDataGridVM() 
        {
            Rows = new RowVM[]
            {
                new RowVM()
                {
                    Cells = new Dictionary<string, CellVM>()
                    {
                        { "A", new CellVM() { Value = "Val1-1" } },
                        { "B", new CellVM() { Value = "Val1-2" } },
                        { "C", new CellVM() { Value = "Val1-3" } }
                    }
                },
                new RowVM()
                {
                    Cells = new Dictionary<string, CellVM>()
                    {
                        { "A", new CellVM() { Value = "Val2-1" } },
                        { "B", new CellVM() { Value = "Val2-2" } },
                        { "C", new CellVM() { Value = "Val2-3" } }
                    }
                }
            };

            DynamicRows = new ObservableCollection<RowVM>();

            DynamicRows.Add(new RowVM()
            {
                Cells = new Dictionary<string, CellVM>()
                {
                    { "A", new CellVM() { Value = "Val1-1" } },
                    { "B", new CellVM() { Value = "Val1-2" } }
                }
            });

            DynamicRows.Add(new RowVM()
            {
                Cells = new Dictionary<string, CellVM>()
                {
                    { "A", new CellVM() { Value = "Val2-1" } },
                    { "B", new CellVM() { Value = "Val2-2" } }
                }
            });

            ColumnsSource = new ColumnVM[] 
            {
                new ColumnVM("A"),
                new ColumnVM("B"),
                new ColumnVM("C")
            };

            DynamicColumnsSource = new ObservableCollection<ColumnVM>(new ColumnVM[]
            {
                new ColumnVM("A"),
                new ColumnVM("B")
            });

            m_ShowColumns = true;

            AddColumnCommand = new RelayCommand(AddColumn);
            AddColumnAsyncCommand = new RelayCommand(AddColumnAsync);
        }

        private async void AddColumnAsync()
        {
            await Task.Run(AddColumn);
        }

        private void AddColumn()
        {
            var colName = $"C{DynamicColumnsSource.Count - 1}";

            for (int i = 0; i < DynamicRows.Count; i++)
            {
                var row = DynamicRows[i];

                row.Cells.Add(colName, new CellVM() { Value = $"Val{i + 1}-{DynamicColumnsSource.Count + 1}" });
            }

            DynamicColumnsSource.Add(new ColumnVM(colName));
        }
    }
}
