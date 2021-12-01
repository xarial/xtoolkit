using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Reflection;
using System.Text;

namespace Xarial.XToolkit.Wpf.Controls
{
    public class CheckableComboBoxItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Item { get; set; }
        public DataTemplate Header { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var elem = container as FrameworkElement;

            if (elem?.TemplatedParent is ComboBox)
            {
                return Header;
            }
            else
            {
                return Item;
            }
        }
    }

    public class CheckableComboBoxItemTitleConverter : IValueConverter
    {
        public static string GetTitle(object item) 
        {
            if (item != null)
            {
                string title = "";

                if (item is Enum)
                {
                    if (!((Enum)item).TryGetAttribute<DisplayNameAttribute>(a => title = a.DisplayName))
                    {
                        title = item.ToString();
                    }
                }
                else
                {
                    if (item.GetType().TryGetAttribute<DisplayNameAttribute>(out DisplayNameAttribute att))
                    {
                        title = att.DisplayName;
                    }
                    else
                    {
                        title = item.ToString();
                    }
                }

                return title;
            }
            else
            {
                return "";
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => GetTitle(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CheckableComboBoxItemTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                string tooltip = "";

                if (value is Enum)
                {
                    if (!((Enum)value).TryGetAttribute<DescriptionAttribute>(a => tooltip = a.Description))
                    {
                        tooltip = value.ToString();
                    }
                }
                else
                {
                    if (value.GetType().TryGetAttribute<DescriptionAttribute>(out DescriptionAttribute att))
                    {
                        tooltip = att.Description;
                    }
                    else
                    {
                        tooltip = value.ToString();
                    }
                }

                return tooltip;
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CheckableComboBoxHeaderTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList)
            {
                var header = new StringBuilder();

                foreach (var item in (IList)value) 
                {
                    if (header.Length > 0) 
                    {
                        header.Append(", ");
                    }

                    header.Append(CheckableComboBoxItemTitleConverter.GetTitle(item));
                }

                return header.ToString();
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CheckableComboBox : Control
    {
        public class CheckableComboBoxItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public event Action<CheckableComboBoxItem, object, bool> SelectedChanged;

            private bool m_IsSelected;

            public object Value { get; }
            
            public bool IsSelected 
            {
                get => m_IsSelected;
                set 
                {
                    m_IsSelected = value;
                    this.NotifyChanged();
                    SelectedChanged?.Invoke(this, Value, value);
                }
            }

            internal CheckableComboBoxItem(object value) 
            {
                Value = value;
            }

            internal void SetSelected(bool value) 
            {
                m_IsSelected = value;
                this.NotifyChanged();
            }
        }

        private ComboBox m_ComboBox;

        private readonly List<CheckableComboBoxItem> m_Items;

        static CheckableComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckableComboBox),
                new FrameworkPropertyMetadata(typeof(CheckableComboBox)));
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable),
            typeof(CheckableComboBox), new PropertyMetadata(null, OnItemsSourcePropertyChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
            nameof(SelectedItems), typeof(IList),
            typeof(CheckableComboBox), new PropertyMetadata(null, OnSelectedItemsPropertyChanged));

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }
        
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
            nameof(ItemTemplate), typeof(DataTemplate),
            typeof(CheckableComboBox),
            new PropertyMetadata(typeof(CheckableComboBox).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "CheckableComboBoxItemTemplate")));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
            nameof(HeaderTemplate), typeof(DataTemplate),
            typeof(CheckableComboBox),
            new PropertyMetadata(typeof(CheckableComboBox).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "CheckableComboBoxHeaderTemplate")));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            m_ComboBox = (ComboBox)this.Template.FindName("PART_ComboBox", this);
            LoadItems(ItemsSource);
            SetItemsChecked(SelectedItems ?? new List<object>());
        }

        public CheckableComboBox() 
        {
            m_Items = new List<CheckableComboBoxItem>();
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            var cmbBox = (CheckableComboBox)d;

            cmbBox.SetItems((IEnumerable)e.NewValue ?? Enumerable.Empty<object>());
        }

        private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cmbBox = (CheckableComboBox)d;

            cmbBox.SetItemsChecked((IList)e.NewValue ?? new List<object>());
        }

        private void SetItems(IEnumerable items) 
        {
            if (m_ComboBox != null)
            {
                m_Items.Clear();
                m_ComboBox.Items.Clear();
                SelectedItems?.Clear();

                LoadItems(items);
            }
        }

        private void LoadItems(IEnumerable items)
        {
            foreach (var item in items)
            {
                var cmbItem = new CheckableComboBoxItem(item);
                cmbItem.SelectedChanged += OnItemSelectedChanged;
                m_Items.Add(cmbItem);
                m_ComboBox.Items.Add(cmbItem);
            }
        }

        private void OnItemSelectedChanged(CheckableComboBoxItem sender, object item, bool isSelected)
        {
            if (isSelected)
            {
                SelectedItems?.Add(item);
            }
            else 
            {
                SelectedItems?.Remove(item);
            }

            UpdateHeader();
        }

        private void SetItemsChecked(IList checkedItems)
        {
            foreach (var item in m_Items) 
            {
                item.SetSelected(checkedItems.Contains(item.Value));
            }

            UpdateHeader();
        }

        private void UpdateHeader() 
        {
            if (m_ComboBox != null)
            {
                m_ComboBox.SelectedIndex = -1;
                m_ComboBox.SelectedIndex = 0;
            }
        }
    }
}
