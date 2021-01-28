using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTester
{
    public class XListViewVM
    {
        public List<string> Items { get; set; }
        public IList SelectedItems { get; set; }

        public XListViewVM() 
        {
            Items = new List<string>(new string[] { "A", "B", "C", "D" });
        }
    }
}
