//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
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

namespace Xarial.XToolkit.Wpf.Controls
{
    public abstract class NumberBox<T> : Control
    {
        private TextBox m_TextBox;
        private object m_PevVal;

        public NumberBox()
        {
            m_PevVal = default(T);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
            nameof(Value), typeof(T),
            typeof(NumberBox<T>),
            new FrameworkPropertyMetadata(default(T), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty TextValueProperty =
            DependencyProperty.Register(
            nameof(TextValue), typeof(string),
            typeof(NumberBox<T>), new PropertyMetadata(default(T).ToString(), OnTextValueChanged, OnCoerceTextValue));

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string TextValue
        {
            get { return (string)GetValue(TextValueProperty); }
            set { SetValue(TextValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as NumberBox<T>).TextValue = e.NewValue.ToString();
        }

        private static void OnTextValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as NumberBox<T>).Value = (T)Convert.ChangeType(e.NewValue, typeof(T));
        }

        private static object OnCoerceTextValue(DependencyObject d, object baseValue)
        {
            try
            {
                Convert.ChangeType(baseValue, typeof(T));
                (d as NumberBox<T>).m_PevVal = baseValue;
                return baseValue;
            }
            catch
            {
                return (d as NumberBox<T>).m_PevVal;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_TextBox = (TextBox)this.Template.FindName("PART_TextBox", this);

            var binding = new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(TextValue)),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            m_TextBox.SetBinding(TextBox.TextProperty, binding);
        }
    }

    public class NumberBoxDouble : NumberBox<double>
    {
        static NumberBoxDouble()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBoxDouble),
                new FrameworkPropertyMetadata(typeof(NumberBoxDouble)));
        }
    }

    public class NumberBoxInteger : NumberBox<int>
    {
        static NumberBoxInteger()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBoxInteger),
                new FrameworkPropertyMetadata(typeof(NumberBoxInteger)));
        }
    }
}
