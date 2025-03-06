//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
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
using Xarial.XToolkit.Wpf.Delegates;

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

    public class EnumComboBox : ComboBox
    {
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

        public static readonly DependencyProperty ItemCreateCommandProperty =
            DependencyProperty.Register(
            nameof(ItemCreateCommand), typeof(ICommand),
            typeof(EnumComboBox), new FrameworkPropertyMetadata(OnItemCreateCommandPropertyChanged));

        private static void OnItemCreateCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cmb = (EnumComboBox)d;

            var cmd = e.NewValue as ICommand;

            if (cmd != null) 
            {
                foreach (ComboBoxItem cmbItem in cmb.Items) 
                {
                    var enumItem = (EnumComboBoxItem)cmbItem.Content;

                    cmd.Execute(enumItem);

                    cmbItem.ToolTip = enumItem.Tooltip;
                    cmbItem.Content = enumItem;
                }
            }
        }

        /// <summary>
        /// Command for handling the item creation
        /// </summary>
        /// <remarks>Instance of <see cref="EnumComboBoxItem"/> is passed as the command parameter. Display name and description of the item can be modified</remarks>
        public ICommand ItemCreateCommand
        {
            get { return (ICommand)GetValue(ItemCreateCommandProperty); }
            set { SetValue(ItemCreateCommandProperty, value); }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            var selItem = (SelectedItem as ComboBoxItem)?.Content as EnumComboBoxItem;
            Value = selItem?.Value;

            base.OnSelectionChanged(e);
        }

        private void AddItem(Enum item)
        {
            var enumItem = new EnumComboBoxItem(item, EnumControlHelper.GetTitle(item), EnumControlHelper.GetDescription(item));

            if (ItemCreateCommand != null) 
            {
                ItemCreateCommand.Execute(enumItem);
            }

            var cmbItem = new ComboBoxItem()
            {
                ToolTip = enumItem.Tooltip,
                Content = enumItem,
            };

            Items.Add(cmbItem);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cmb = d as EnumComboBox;

            var val = e.NewValue as Enum;
            
            if (val != null)
            {
                var enumType = val.GetType();

                if (enumType != cmb.m_CurBoundType)
                {
                    cmb.m_CurBoundType = enumType;

                    cmb.Items.Clear();

                    var items = Enum.GetValues(enumType);

                    foreach (Enum item in items)
                    {
                        var visible = true;
                        item.TryGetAttribute<BrowsableAttribute>(a => visible = a.Browsable);

                        if (visible)
                        {
                            cmb.AddItem(item);
                        }
                    }
                }

                if (!Enum.Equals(e.OldValue, e.NewValue))
                {
                    foreach (ComboBoxItem cmbItem in cmb.Items) 
                    {
                        if (Enum.Equals((cmbItem.Content as EnumComboBoxItem).Value, val)) 
                        {
                            cmb.SelectedItem = cmbItem;
                            break;
                        }
                    }
                }
            }
            else if (val == null)
            {
                cmb.Items.Clear();
            }
        }
    }
}
