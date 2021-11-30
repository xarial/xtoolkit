using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTester
{
    [Description("Custom Item")]
    public class Item 
    {
        public string Title { get; }

        public Item(string title) 
        {
            Title = title;
        }

        public override string ToString()
            => Title;
    }

    public enum EnumItems_e
    {
        Item1,
        
        [EnumDisplayName("Item #2")]
        [EnumDescription("Second Item")]
        Item2,

        Item3
    }

    public class CheckableComboBoxVM
    {
        public string[] ItemsSource1 { get; }
        public Item[] ItemsSource2 { get; }
        public EnumItems_e[] ItemsSource3 { get; }

        public List<string> SelectedItems1 { get; }
        public List<Item> SelectedItems2 { get; }
        public List<EnumItems_e> SelectedItems3 { get; }

        public CheckableComboBoxVM() 
        {
            ItemsSource1 = new string[] { "SItem1", "SItem2", "SItem3" };
            ItemsSource2 = new Item[] { new Item("OItem1"), new Item("OItem2"), new Item("OItem3") };
            ItemsSource3 = Enum.GetValues(typeof(EnumItems_e)).Cast<EnumItems_e>().ToArray();

            SelectedItems1 = new List<string>(new string[] { "SItem1", "SItem3" });
            SelectedItems2 = new List<Item>();
            SelectedItems3 = new List<EnumItems_e>();
        }
    }
}
