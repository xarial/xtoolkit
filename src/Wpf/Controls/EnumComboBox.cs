//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Wpf.Themes;

namespace Xarial.XToolkit.Wpf.Controls
{
    /// <summary>
    /// Item in <see cref="EnumComboBox"/>
    /// </summary>
    public class EnumComboBoxItem
    {
        /// <summary>
        /// Value of this item
        /// </summary>
        public Enum Value { get; }

        /// <summary>
        /// Display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Tooltip
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="dispName">Title</param>
        /// <param name="tooltip">Tooltip</param>
        public EnumComboBoxItem(Enum value, string dispName, string tooltip)
        {
            Value = value;
            DisplayName = dispName;
            Tooltip = tooltip;
        }

        public override string ToString() => DisplayName;
    }

    public class EnumComboBox : Control
    {
        static EnumComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumComboBox),
                new FrameworkPropertyMetadata(typeof(EnumComboBox)));

            StyleProperty.OverrideMetadata(typeof(EnumComboBox),
                new FrameworkPropertyMetadata(StyleLoader.LoadStyle<EnumComboBox>("EnumComboBox.xaml")));
        }

        private Type m_CurBoundType;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
            nameof(Value), typeof(Enum),
            typeof(EnumComboBox), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        /// <summary>
        /// Value
        /// </summary>
        public Enum Value
        {
            get { return (Enum)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private ComboBox m_ComboBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            m_ComboBox = (ComboBox)this.Template.FindName("PART_ComboBox", this);
            m_ComboBox.SelectionChanged += OnSelectionChanged;
            SetValue(Value, null);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbBox = (ComboBox)sender;

            var selItem = (cmbBox.SelectedItem as ComboBoxItem)?.Content as EnumComboBoxItem;
            Value = selItem?.Value;
        }

        private void AddItem(Enum item)
        {
            var enumItem = new EnumComboBoxItem(item, EnumControlHelper.GetTitle(item), EnumControlHelper.GetDescription(item));

            var cmbItem = new ComboBoxItem()
            {
                ToolTip = enumItem.Tooltip,
                Content = enumItem,
            };

            m_ComboBox.Items.Add(cmbItem);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cmb = d as EnumComboBox;

            cmb.SetValue(e.NewValue as Enum, e.OldValue as Enum);
        }

        private void SetValue(Enum val, Enum oldVal)
        {
            if (m_ComboBox != null)
            {
                if (val != null)
                {
                    var enumType = val.GetType();

                    if (enumType != m_CurBoundType)
                    {
                        m_CurBoundType = enumType;

                        m_ComboBox.Items.Clear();

                        var items = Enum.GetValues(enumType);

                        foreach (Enum item in items)
                        {
                            var visible = true;
                            item.TryGetAttribute<BrowsableAttribute>(a => visible = a.Browsable);

                            if (visible)
                            {
                                AddItem(item);
                            }
                        }
                    }

                    if (!Enum.Equals(oldVal, val))
                    {
                        foreach (ComboBoxItem cmbItem in m_ComboBox.Items)
                        {
                            if (Enum.Equals((cmbItem.Content as EnumComboBoxItem).Value, val))
                            {
                                m_ComboBox.SelectedItem = cmbItem;
                                break;
                            }
                        }
                    }
                }
                else if (val == null)
                {
                    m_ComboBox.Items.Clear();
                    m_CurBoundType = null;
                }
            }
        }
    }
}
