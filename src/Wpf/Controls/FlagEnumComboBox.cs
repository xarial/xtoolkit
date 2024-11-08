//*********************************************************************
//xToolkit
//Copyright(C) 2024 Xarial Pty Limited
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
using Xarial.XToolkit.Wpf.Delegates;
using System.Windows.Input;

namespace Xarial.XToolkit.Wpf.Controls
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class FlagEnumValueToHeaderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var enumVal = values[0] as Enum;
            var items = values[1] as ItemCollection;

            if (enumVal != null && items != null)
            {
                //TODO: fix when sub-combined items still shown in the title
                var selItems = items.Cast<FlagEnumComboBox.FlagEnumComboBoxItem>().Where(i => i.IsSelected);
                var swallabedFlags = selItems.Where(i => i.Type == FlagEnumComboBox.EnumItemType_e.Combined).SelectMany(i => i.AffectedFlags);
                var visItems = selItems.Where(i => !swallabedFlags.Any(f => Enum.Equals(f, i.Value)));

                return string.Join(", ", visItems.Select(i => i.DisplayName));
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
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

        internal class FlagEnumComboBoxItem : EnumComboBoxItem, INotifyPropertyChanged
        {
#pragma warning disable CS0067
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

            private readonly FlagEnumComboBox m_Parent;
            internal Enum[] AffectedFlags { get; }

            internal FlagEnumComboBoxItem(FlagEnumComboBox parent, Enum value, Enum[] affectedFlags, string dispName, string tooltip) : base(value, dispName, tooltip)
            {
                m_Parent = parent;
                m_Parent.ValueChanged += OnValueChanged;
                AffectedFlags = affectedFlags;

                if (AffectedFlags.Length > 1)
                {
                    Type = EnumItemType_e.Combined;
                }
                else if (AffectedFlags.Length == 0)
                {
                    Type = EnumItemType_e.None;
                }
                else
                {
                    Type = EnumItemType_e.Default;
                }
            }

            public EnumItemType_e Type { get; }

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
                            return m_Parent.Value.HasFlag(Value);
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
                        m_Parent.Value = (Enum)Enum.ToObject(Value.GetType(), 0);
                    }
                    else
                    {
                        int val = Convert.ToInt32(m_Parent.Value);

                        if (value)
                        {
                            foreach (var flag in AffectedFlags)
                            {
                                if (!m_Parent.Value.HasFlag(flag))
                                {
                                    val += Convert.ToInt32(flag);
                                }
                            }
                        }
                        else
                        {
                            foreach (var flag in AffectedFlags)
                            {
                                if (m_Parent.Value.HasFlag(flag))
                                {
                                    val -= Convert.ToInt32(flag);
                                }
                            }
                        }

                        var enumVal = (Enum)Enum.ToObject(Value.GetType(), val);
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
                        enumVal = (Enum)Enum.ToObject(Value.GetType(), val);
                    }
                }

                return enumVal;
            }

            private void OnValueChanged(Enum value)
                => this.NotifyChanged(nameof(IsSelected));

            private bool IsNone(Enum val) => Convert.ToInt32(val) == 0;

            public override string ToString() => Value.ToString();
        }

        public static readonly DependencyProperty ItemCreateCommandProperty =
            DependencyProperty.Register(
            nameof(ItemCreateCommand), typeof(ICommand),
            typeof(FlagEnumComboBox));

        /// <summary>
        /// Command for handling the item creation
        /// </summary>
        /// <remarks>Instance of <see cref="EnumComboBoxItem"/> is passed as the command parameter. Display name and description of the item can be modified</remarks>
        public ICommand ItemCreateCommand
        {
            get { return (ICommand)GetValue(ItemCreateCommandProperty); }
            set { SetValue(ItemCreateCommandProperty, value); }
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

                            var enumItem = new FlagEnumComboBoxItem(this, item, 
                                m_CurFlags.Where(f => item.HasFlag(f)).ToArray(), 
                                EnumControlHelper.GetTitle(item), EnumControlHelper.GetDescription(item));

                            if (ItemCreateCommand != null)
                            {
                                ItemCreateCommand.Execute(enumItem);
                            }

                            m_ComboBox.Items.Add(enumItem);
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
