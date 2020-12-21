using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xarial.XToolkit.Wpf.Controls;

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

    public class CellContentSelector : ICellContentSelector
    {
        public CellContentSelector() 
        {
        }

        public object SelectContent(object dataItem, DataGridColumn column, DataGridCell cell)
        {
            return (dataItem as RowVM).Cells[(string)column.Header];
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
            if ((string)item == "A")
            {
                return AColumn;
            }
            else
            {
                return Default;
            }
        }
    }

    public class XDataGridVM
    {
        public RowVM[] Rows { get; set; }

        public string[] ColumnNames { get; set; }

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

            ColumnNames = new string[] { "A", "B", "C" };
        }
    }
}
