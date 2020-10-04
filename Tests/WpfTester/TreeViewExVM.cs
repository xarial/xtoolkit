using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace WpfTester
{
    public class TreeViewExVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class TreeViewExItem 
        {
            public string Title { get; set; }

            public TreeViewExItem[] Children { get; set; }
        }

        private TreeViewExItem[] m_SelectScope;
        private int m_CurSelIndex;

        public TreeViewExItem[] Items { get; set; }

        public TreeViewExVM() 
        {
            var a = new TreeViewExItem { Title = "A" };
            var b = new TreeViewExItem { Title = "B" };
            var c = new TreeViewExItem { Title = "C" };

            var a1 = new TreeViewExItem { Title = "A.1" };
            var a2 = new TreeViewExItem { Title = "A.2" };

            var b1 = new TreeViewExItem { Title = "B.1" };
            var b2 = new TreeViewExItem { Title = "B.2" };
            var b3 = new TreeViewExItem { Title = "B.3" };

            var b11 = new TreeViewExItem { Title = "B.1.1" };
            var b12 = new TreeViewExItem { Title = "B.1.2" };

            var b21 = new TreeViewExItem { Title = "B.2.1" };
            var b22 = new TreeViewExItem { Title = "B.2.2" };

            var a21 = new TreeViewExItem { Title = "A.2.1" };

            a.Children = new TreeViewExItem[] { a1, a2 };
            a2.Children = new TreeViewExItem[] { a21 };
            b.Children = new TreeViewExItem[] { b1, b2, b3 };

            b1.Children = new TreeViewExItem[] { b11, b12 };
            b2.Children = new TreeViewExItem[] { b21, b22 };

            Items = new TreeViewExItem[] { a, b, c };

            m_SelectScope = new TreeViewExItem[] { b, a21, b1, null };
            m_CurSelIndex = -1;

            SelectNextItemCommand = new RelayCommand(() => 
            {
                m_CurSelIndex++;

                if (m_CurSelIndex > m_SelectScope.Length - 1) 
                {
                    m_CurSelIndex = 0;
                }

                SelectedItem = m_SelectScope[m_CurSelIndex];
            });
        }

        private TreeViewExItem m_SelectedItem;

        public ICommand SelectNextItemCommand { get; }

        public TreeViewExItem SelectedItem 
        {
            get => m_SelectedItem;
            set 
            {
                m_SelectedItem = value;
                this.NotifyChanged();
            }
        }
    }
}
