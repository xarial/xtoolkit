using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        public Func<object, DataGridColumn, DataGridCell, object> CellContentSelector => SelectCellContent;

        private object SelectCellContent(object dataItem, DataGridColumn column, DataGridCell cell) 
        {
            return (dataItem as RowVM).Cells[(string)column.Header];
        }
    }
}
