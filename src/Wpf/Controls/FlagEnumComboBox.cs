//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Wpf.Extensions;
using System.Windows.Media;
using System.Reflection;

namespace Xarial.XToolkit.Wpf.Controls
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class FlagEnumValueToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumVal = value as Enum;
            
            if (enumVal != null)
            {
                var enumType = enumVal.GetType();

                //TODO: this is a simple fix - need to implement more robust solution

                var val = enumVal.ToString();
                var vals = val.Split(',')
                    .Select(v => (Enum)Enum.Parse(enumType, v.Trim()))
                    .Select(e => EnumControlHelper.GetTitle(e));

                return string.Join(", ", vals.ToArray());
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
    
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class FlagEnumComboBoxItemTemplateSelector : DataTemplateSelector
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

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumItemTypeToForegroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var defaultColor = values[1] as Brush;
            var combinedColor = values[2] as Brush;
            var noneColor = values[3] as Brush;

            if (values[0] is FlagEnumComboBox.EnumItemType_e)
            {
                switch ((FlagEnumComboBox.EnumItemType_e)values[0])
                {
                    case FlagEnumComboBox.EnumItemType_e.Default:
                        return defaultColor;

                    case FlagEnumComboBox.EnumItemType_e.Combined:
                        return combinedColor;

                    case FlagEnumComboBox.EnumItemType_e.None:
                        return noneColor;
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FlagEnumComboBox : Control
    {
        public enum EnumItemType_e
        {
            Default,
            Combined,
            None
        }

        internal class FlagEnumComboBoxItem : INotifyPropertyChanged
        {
#pragma warning disable CS0067
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

            private readonly FlagEnumComboBox m_Parent;
            private readonly Enum m_Value;
            private readonly Enum[] m_AffectedFlags;

            internal FlagEnumComboBoxItem(FlagEnumComboBox parent, Enum value, Enum[] affectedFlags)
            {
                m_Parent = parent;
                m_Parent.ValueChanged += OnValueChanged;
                m_Value = value;
                m_AffectedFlags = affectedFlags;

                if (m_AffectedFlags.Length > 1)
                {
                    Type = EnumItemType_e.Combined;
                }
                else if (m_AffectedFlags.Length == 0)
                {
                    Type = EnumItemType_e.None;
                }
                else
                {
                    Type = EnumItemType_e.Default;
                }

                Title = EnumControlHelper.GetTitle(m_Value);
                Description = EnumControlHelper.GetDescription(m_Value);
            }

            public EnumItemType_e Type { get; private set; }

            public bool IsSelected
            {
                get
                {
                    if (m_Parent.Value != null)
                    {
                        if (Type == EnumItemType_e.None)
                        {
                            return IsNone(m_Parent.Value);
                        }
                        else
                        {
                            return m_Parent.Value.HasFlag(m_Value);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                set
                {
                    if (Type == EnumItemType_e.None)
                    {
                        m_Parent.Value = (Enum)Enum.ToObject(m_Value.GetType(), 0);
                    }
                    else
                    {
                        int val = Convert.ToInt32(m_Parent.Value);

                        if (value)
                        {
                            foreach (var flag in m_AffectedFlags)
                            {
                                if (!m_Parent.Value.HasFlag(flag))
                                {
                                    val += Convert.ToInt32(flag);
                                }
                            }
                        }
                        else
                        {
                            foreach (var flag in m_AffectedFlags)
                            {
                                if (m_Parent.Value.HasFlag(flag))
                                {
                                    val -= Convert.ToInt32(flag);
                                }
                            }
                        }

                        var enumVal = (Enum)Enum.ToObject(m_Value.GetType(), val);
                        enumVal = RemoveDanglingHiddentEnumValues(enumVal);

                        m_Parent.Value = enumVal;
                    }

                    this.NotifyChanged();
                }
            }
            
            private Enum RemoveDanglingHiddentEnumValues(Enum enumVal)
            {
                foreach (var hiddenItem in m_Parent.m_HiddenItems)
                {
                    var hiddenItemsGroup = m_Parent.m_Items.Where(i => i.HasFlag(hiddenItem));

                    if (enumVal.HasFlag(hiddenItem) && !hiddenItemsGroup.Any(g => enumVal.HasFlag(g)))
                    {
                        var val = Convert.ToInt32(enumVal);
                        val -= Convert.ToInt32(hiddenItem);
                        enumVal = (Enum)Enum.ToObject(m_Value.GetType(), val);
                    }
                }

                return enumVal;
            }

            public string Title { get; private set; }
            public string Description { get; private set; }

            private void OnValueChanged(Enum value)
            {
                this.NotifyChanged(nameof(IsSelected));
            }

            private bool IsNone(Enum val)
            {
                return Convert.ToInt32(val) == 0;
            }

            public override string ToString()
            {
                return m_Value.ToString();
            }
        }

        internal event Action<Enum> ValueChanged;

        private Type m_CurBoundType;
        private Enum[] m_CurFlags;

        private Enum[] m_Items;
        private Enum[] m_HiddenItems;

        private ComboBox m_ComboBox;

        static FlagEnumComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlagEnumComboBox),
                new FrameworkPropertyMetadata(typeof(FlagEnumComboBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            m_ComboBox = (ComboBox)this.Template.FindName("PART_ComboBox", this);
            TryResolveItems(Value);
        }

        public static readonly DependencyProperty GroupItemForegroundProperty =
            DependencyProperty.Register(
            nameof(GroupItemForeground), typeof(Brush),
            typeof(FlagEnumComboBox), new PropertyMetadata(Brushes.Blue));

        public Brush GroupItemForeground
        {
            get { return (Brush)GetValue(GroupItemForegroundProperty); }
            set { SetValue(GroupItemForegroundProperty, value); }
        }

        public static readonly DependencyProperty ItemForegroundProperty =
            DependencyProperty.Register(
            nameof(ItemForeground), typeof(Brush),
            typeof(FlagEnumComboBox), new PropertyMetadata(Brushes.Black));

        public Brush ItemForeground
        {
            get { return (Brush)GetValue(ItemForegroundProperty); }
            set { SetValue(ItemForegroundProperty, value); }
        }

        public static readonly DependencyProperty NoneItemForegroundProperty =
            DependencyProperty.Register(
            nameof(NoneItemForeground), typeof(Brush),
            typeof(FlagEnumComboBox), new PropertyMetadata(Brushes.Gray));

        public Brush NoneItemForeground
        {
            get { return (Brush)GetValue(NoneItemForegroundProperty); }
            set { SetValue(NoneItemForegroundProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(Enum),
            typeof(FlagEnumComboBox), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public Enum Value
        {
            get { return (Enum)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cmb = d as FlagEnumComboBox;

            var val = e.NewValue as Enum;

            cmb.TryResolveItems(val);
        }

        private void TryResolveItems(Enum val) 
        {
            if (val != null && m_ComboBox != null)
            {
                var enumType = val.GetType();

                if (enumType != m_CurBoundType)
                {
                    m_CurFlags = enumType.GetEnumFlags();

                    m_ComboBox.Items.Clear();

                    m_CurBoundType = enumType;

                    var items = GetEnumValueOrderAsDefined(enumType);

                    var itemsList = new List<Enum>();
                    var hiddenItemsList = new List<Enum>();

                    foreach (Enum item in items)
                    {
                        var visible = true;
                        item.TryGetAttribute<BrowsableAttribute>(a => visible = a.Browsable);

                        if (visible)
                        {
                            itemsList.Add(item);
                            m_ComboBox.Items.Add(new FlagEnumComboBoxItem(this, item,
                                m_CurFlags.Where(f => item.HasFlag(f)).ToArray()));
                        }
                        else 
                        {
                            hiddenItemsList.Add(item);
                        }
                    }

                    m_Items = itemsList.ToArray();
                    m_HiddenItems = hiddenItemsList.ToArray();

                    UpdateHeader(this);
                }
            }

            ValueChanged?.Invoke(val);
        }

        private Array GetEnumValueOrderAsDefined(Type enumType)
        {
            var fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
            return Array.ConvertAll(fields, x => (Enum)x.GetValue(null));
        }

        private void UpdateHeader(FlagEnumComboBox cmb)
        {
            if (cmb.m_ComboBox.Items.Count > 0)
            {
                cmb.m_ComboBox.SelectedIndex = 0;
            }
        }
    }
}
