//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
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

namespace Xarial.XToolkit.Wpf.Controls
{
    public class EnumComboBox : ComboBox
    {
        public class EnumComboBoxItem 
        {
            public Enum Value { get; }

            private readonly string m_Title;

            public EnumComboBoxItem(Enum value, string title) 
            {
                Value = value;
                m_Title = title;
            }

            public override string ToString() => m_Title;
        }

        private Type m_CurBoundType;

        public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(
			nameof(Value), typeof(Enum),
			typeof(EnumComboBox), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public Enum Value
        {
            get { return (Enum)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            var selItem = (SelectedItem as ComboBoxItem)?.Content as EnumComboBoxItem;
            Value = selItem?.Value;

            base.OnSelectionChanged(e);
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
                            var cmbItem = new ComboBoxItem()
                            {
                                ToolTip = EnumControlHelper.GetDescription(item),
                                Content = new EnumComboBoxItem(item, EnumControlHelper.GetTitle(item)),
                            };
                            
                            cmb.Items.Add(cmbItem);
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
