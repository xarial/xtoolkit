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
                    .Select(e => FlagEnumComboBox.FlagEnumComboBoxItem.GetTitle(e));

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
    public class EnumItemTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlagEnumComboBox.EnumItemType_e)
            {
                switch ((FlagEnumComboBox.EnumItemType_e)value)
                {
                    case FlagEnumComboBox.EnumItemType_e.Default:
                        return Brushes.Black;

                    case FlagEnumComboBox.EnumItemType_e.Combined:
                        return Brushes.Blue;

                    case FlagEnumComboBox.EnumItemType_e.None:
                        return Brushes.Gray;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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

            internal static string GetTitle(Enum value)
            {
                string title = "";

                if (value != null)
                {
                    if (!value.TryGetAttribute<DisplayNameAttribute>(a => title = a.DisplayName))
                    {
                        if (Convert.ToInt32(value) != 0 || Enum.IsDefined(value.GetType(), value))
                        {
                            title = value.ToString();
                        }
                    }
                }

                return title;
            }

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

                Title = GetTitle(m_Value);
                Description = GetItemDescription(m_Value);
            }

            private string GetItemDescription(Enum value)
            {
                string title = "";

                if (value != null)
                {
                    if (!value.TryGetAttribute<DescriptionAttribute>(a => title = a.Description))
                    {
                        title = GetTitle(value);

                        if (string.IsNullOrEmpty(title))
                        {
                            title = value.ToString();
                        }
                    }
                }

                return title;
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

                        var visible = true;
                        enumVal.TryGetAttribute<BrowsableAttribute>(a => visible = a.Browsable);

                        if (!visible)
                        {
                            enumVal = (Enum)Enum.ToObject(m_Value.GetType(), 0);
                        }

                        m_Parent.Value = enumVal;
                    }

                    this.NotifyChanged();
                }
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

                    var items = Enum.GetValues(enumType);

                    foreach (Enum item in items)
                    {
                        var visible = true;
                        item.TryGetAttribute<BrowsableAttribute>(a => visible = a.Browsable);

                        if (visible)
                        {
                            m_ComboBox.Items.Add(new FlagEnumComboBoxItem(this, item,
                                m_CurFlags.Where(f => item.HasFlag(f)).ToArray()));
                        }
                    }

                    UpdateHeader(this);
                }
            }

            ValueChanged?.Invoke(val);
        }

        private static void UpdateHeader(FlagEnumComboBox cmb)
        {
            if (cmb.m_ComboBox.Items.Count > 0)
            {
                cmb.m_ComboBox.SelectedIndex = 0;
            }
        }
    }
}
