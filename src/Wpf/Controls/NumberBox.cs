//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xarial.XToolkit.Wpf.Controls
{
    public abstract class NumberBox : Control 
    {
        public static readonly DependencyProperty ShowIncrementBoxProperty =
            DependencyProperty.Register(
            nameof(ShowIncrementBox), typeof(bool),
            typeof(NumberBox), new PropertyMetadata(true));

        public bool ShowIncrementBox
        {
            get { return (bool)GetValue(ShowIncrementBoxProperty); }
            set { SetValue(ShowIncrementBoxProperty, value); }
        }
    }

    public abstract class NumberBox<T> : NumberBox
    {
        private TextBox m_TextBox;
        private Button m_DecrementButton;
        private Button m_IncrementButton;

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
            typeof(NumberBox<T>), new PropertyMetadata(default(T).ToString(), OnTextValueChanged));

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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_TextBox = (TextBox)this.Template.FindName("PART_TextBox", this);
            m_IncrementButton = (Button)this.Template.FindName("PART_IncrementButton", this);
            m_DecrementButton = (Button)this.Template.FindName("PART_DecrementButton", this);

            m_IncrementButton.Click += OnIncrementButtonClick;
            m_DecrementButton.Click += OnDecrementButtonClick;

            var binding = new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(TextValue)),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };

            binding.ValidationRules.Add(new NumericValidationRule<T>(Validate));
            
            m_TextBox.SetBinding(TextBox.TextProperty, binding);

            m_TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void OnIncrementButtonClick(object sender, RoutedEventArgs e)
        {
            Value = IncrementValue(Value);
        }

        private void OnDecrementButtonClick(object sender, RoutedEventArgs e)
        {
            Value = DecrementValue(Value);
        }

        protected abstract void Validate(string val);
        protected abstract T IncrementValue(T value);
        protected abstract T DecrementValue(T value);
    }

    public class NumericValidationRule<T> : ValidationRule
    {
        private readonly Action<string> m_Validator;

        public NumericValidationRule(Action<string> validator)
        {
            m_Validator = validator;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                m_Validator.Invoke(value?.ToString());

                return new ValidationResult(true, null);
            }
            catch(Exception ex)
            {
                return new ValidationResult(false, ex);
            }
        }
    }

    public class NumberBoxDouble : NumberBox<double>
    {
        static NumberBoxDouble()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBoxDouble),
                new FrameworkPropertyMetadata(typeof(NumberBoxDouble)));
        }

        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register(
            nameof(MinimumValue), typeof(double),
            typeof(NumberBoxDouble), new PropertyMetadata(double.MinValue));

        public double MinimumValue
        {
            get { return (double)GetValue(MinimumValueProperty); }
            set { SetValue(MinimumValueProperty, value); }
        }

        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.Register(
            nameof(MaximumValue), typeof(double),
            typeof(NumberBoxDouble), new PropertyMetadata(double.MaxValue));

        public double MaximumValue
        {
            get { return (double)GetValue(MaximumValueProperty); }
            set { SetValue(MaximumValueProperty, value); }
        }

        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register(
            nameof(Increment), typeof(double),
            typeof(NumberBoxDouble), new PropertyMetadata(1d));

        //NOTE: increment can be negative thus the IncrementValue/DecrementValue functiosn account for potential maximum or minimum value
        public double Increment
        {
            get { return (double)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        protected override void Validate(string val)
        {
            if (!double.TryParse(val, out double dblVal))
            {
                throw new InvalidCastException($"Value '{val}' cannot be converted to number");
            }

            if (dblVal < MinimumValue)
            {
                throw new Exception($"Value {dblVal} is less then minimum {MinimumValue}");
            }
            else if (dblVal > MaximumValue)
            {
                throw new Exception($"Value {dblVal} is more then maximum {MaximumValue}");
            }
        }

        protected override double IncrementValue(double value)
        {
            var newVal = value + Increment;

            if (newVal < MinimumValue)
            {
                newVal = MinimumValue;
            }
            else if (newVal > MaximumValue)
            {
                newVal = MaximumValue;
            }

            return newVal;
        }

        protected override double DecrementValue(double value)
        {
            var newVal = value - Increment;

            if (newVal < MinimumValue)
            {
                newVal = MinimumValue;
            }
            else if (newVal > MaximumValue) 
            {
                newVal = MaximumValue;
            }

            return newVal;
        }
    }

    public class NumberBoxInteger : NumberBox<int>
    {
        static NumberBoxInteger()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBoxInteger),
                new FrameworkPropertyMetadata(typeof(NumberBoxInteger)));
        }

        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register(
            nameof(MinimumValue), typeof(int),
            typeof(NumberBoxInteger), new PropertyMetadata(int.MinValue));

        public int MinimumValue
        {
            get { return (int)GetValue(MinimumValueProperty); }
            set { SetValue(MinimumValueProperty, value); }
        }

        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.Register(
            nameof(MaximumValue), typeof(int),
            typeof(NumberBoxInteger), new PropertyMetadata(int.MaxValue));

        public int MaximumValue
        {
            get { return (int)GetValue(MaximumValueProperty); }
            set { SetValue(MaximumValueProperty, value); }
        }

        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register(
            nameof(Increment), typeof(int),
            typeof(NumberBoxInteger), new PropertyMetadata(1));

        public int Increment
        {
            get { return (int)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        protected override void Validate(string val)
        {
            if (!int.TryParse(val, out int intVal)) 
            {
                throw new InvalidCastException($"Value '{val}' cannot be converted to number");
            }

            if (intVal < MinimumValue)
            {
                throw new Exception($"Value {intVal} is less then minimum {MinimumValue}");
            }
            else if (intVal > MaximumValue)
            {
                throw new Exception($"Value {intVal} is more then maximum {MaximumValue}");
            }
        }

        protected override int IncrementValue(int value)
        {
            var newVal = value + Increment;

            if (newVal < MinimumValue)
            {
                newVal = MinimumValue;
            }
            else if (newVal > MaximumValue)
            {
                newVal = MaximumValue;
            }

            return newVal;
        }

        protected override int DecrementValue(int value)
        {
            var newVal = value - Increment;

            if (newVal < MinimumValue)
            {
                newVal = MinimumValue;
            }
            else if (newVal > MaximumValue)
            {
                newVal = MaximumValue;
            }

            return newVal;
        }
    }
}
