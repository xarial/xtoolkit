using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTester
{
    public class ListViewExVM
    {
        public List<string> Items { get; set; }
        public IList SelectedItems { get; set; }

        public ListViewExVM() 
        {
            Items = new List<string>(new string[] { "A", "B", "C", "D" });
        }
    }
}
