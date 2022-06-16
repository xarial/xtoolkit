using System;
using System.Collections.Generic;
using System.Windows;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf.Extensions;
using System.ComponentModel;
using System.Collections;

namespace Xarial.XToolkit.Wpf.Controls
{
    public class ExpressionVariableArgumentDescriptor: INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private class ExpressionVariableArgumentTextDescriptor : ExpressionVariableArgumentDescriptor
        {
            public ExpressionVariableArgumentTextDescriptor(string title, string tooltip)
                : base(title, tooltip,
                      typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentTextTemplate"))
            {
            }

            protected override IExpressionToken GetToken(object value)
                => new ExpressionTokenText(value?.ToString());

            protected override object GetTokenValue(IExpressionToken token)
            {
                if (token is IExpressionTokenText)
                {
                    return ((IExpressionTokenText)token).Text;
                }
                else
                {
                    throw new NotSupportedException($"Only {typeof(IExpressionTokenText)} is supported");
                }
            }
        }

        private class ExpressionVariableArgumentNumericDescriptor : ExpressionVariableArgumentDescriptor
        {
            public ExpressionVariableArgumentNumericDescriptor(string title, string tooltip)
                : base(title, tooltip,
                      typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentNumericTemplate"))
            {
            }

            protected override IExpressionToken GetToken(object value)
                => new ExpressionTokenText(value?.ToString());

            protected override object GetTokenValue(IExpressionToken token)
            {
                if (token is IExpressionTokenText)
                {
                    return int.Parse(((IExpressionTokenText)token).Text);
                }
                else
                {
                    throw new NotSupportedException($"Only {typeof(IExpressionTokenText)} is supported");
                }
            }
        }

        private class ExpressionVariableArgumentNumericDoubleDescriptor : ExpressionVariableArgumentDescriptor
        {
            public ExpressionVariableArgumentNumericDoubleDescriptor(string title, string tooltip)
                : base(title, tooltip,
                      typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentNumericDoubleTemplate"))
            {
            }

            protected override IExpressionToken GetToken(object value)
                => new ExpressionTokenText(value?.ToString());

            protected override object GetTokenValue(IExpressionToken token)
            {
                if (token is IExpressionTokenText)
                {
                    return double.Parse(((IExpressionTokenText)token).Text);
                }
                else
                {
                    throw new NotSupportedException($"Only {typeof(IExpressionTokenText)} is supported");
                }
            }
        }

        private class ExpressionVariableArgumentToggleDescriptor : ExpressionVariableArgumentDescriptor
        {
            public ExpressionVariableArgumentToggleDescriptor(string title, string tooltip)
                : base(title, tooltip,
                      typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentToggleTemplate"))
            {
            }

            protected override IExpressionToken GetToken(object value)
                => new ExpressionTokenText(value?.ToString());

            protected override object GetTokenValue(IExpressionToken token)
            {
                if (token is IExpressionTokenText)
                {
                    return bool.Parse(((IExpressionTokenText)token).Text);
                }
                else
                {
                    throw new NotSupportedException($"Only {typeof(IExpressionTokenText)} is supported");
                }
            }
        }

        private class ExpressionVariableArgumentOptionsDescriptor : ExpressionVariableArgumentDescriptor
        {
            public ExpressionVariableArgumentOptionsDescriptor(string title, string tooltip, string[] items)
                : base(title, tooltip,
                      typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentOptionsTemplate"))
            {
                Items = items;
            }

            public string[] Items { get; }

            protected override IExpressionToken GetToken(object value)
                => new ExpressionTokenText(value?.ToString());

            protected override object GetTokenValue(IExpressionToken token)
            {
                if (token is IExpressionTokenText)
                {
                    return ((IExpressionTokenText)token).Text;
                }
                else
                {
                    throw new NotSupportedException($"Only {typeof(IExpressionTokenText)} is supported");
                }
            }
        }

        private class ExpressionVariableArgumentEnumOptionsDescriptor<TEnum> : ExpressionVariableArgumentDescriptor
            where TEnum : Enum
        {
            public ExpressionVariableArgumentEnumOptionsDescriptor(string title, string tooltip, bool multi)
                : base(title, tooltip, multi 
                      ? typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentEnumFlagOptionsTemplate")
                      : typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentEnumOptionsTemplate"))
            {
            }

            protected override IExpressionToken GetToken(object value)
                => new ExpressionTokenText(value?.ToString());

            protected override object GetTokenValue(IExpressionToken token)
            {
                if (token is IExpressionTokenText)
                {
                    return Enum.Parse(typeof(TEnum), ((IExpressionTokenText)token).Text);
                }
                else
                {
                    throw new NotSupportedException($"Only {typeof(IExpressionTokenText)} is supported");
                }
            }

            public override object Value
            {
                get
                {
                    var val = base.Value;
                    
                    if (val == null) 
                    {
                        val = default(TEnum);
                    }

                    return val;
                }
                set => base.Value = value; 
            }
        }

        public static ExpressionVariableArgumentDescriptor CreateText(string title, string tooltip)
            => new ExpressionVariableArgumentTextDescriptor(title, tooltip);

        public static ExpressionVariableArgumentDescriptor CreateNumeric(string title, string tooltip)
            => new ExpressionVariableArgumentNumericDescriptor(title, tooltip);

        public static ExpressionVariableArgumentDescriptor CreateNumericDouble(string title, string tooltip)
            => new ExpressionVariableArgumentNumericDoubleDescriptor(title, tooltip);

        public static ExpressionVariableArgumentDescriptor CreateToggle(string title, string tooltip)
            => new ExpressionVariableArgumentToggleDescriptor(title, tooltip);

        public static ExpressionVariableArgumentDescriptor CreateOptions(string title, string tooltip, params string[] items) 
            => new ExpressionVariableArgumentOptionsDescriptor(title, tooltip, items);

        public static ExpressionVariableArgumentDescriptor CreateOptions<TEnum>(string title, string tooltip)
            where TEnum : Enum => new ExpressionVariableArgumentEnumOptionsDescriptor<TEnum>(title, tooltip, false);

        public static ExpressionVariableArgumentDescriptor CreateMultipleOptions<TEnum>(string title, string tooltip)
            where TEnum : Enum => new ExpressionVariableArgumentEnumOptionsDescriptor<TEnum>(title, tooltip, true);

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public IExpressionParser Parser 
        {
            get => m_Parser;
            internal set 
            {
                m_Parser = value;
                this.NotifyChanged();
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public IExpressionVariableDescriptor VariableDescriptor 
        {
            get => m_VariableDescriptor;
            internal set 
            {
                m_VariableDescriptor = value;
                this.NotifyChanged();
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<IExpressionVariableLink> VariableLinks
        {
            get => m_VariableLinks;
            internal set
            {
                m_VariableLinks = value;
                this.NotifyChanged();
            }
        }

        public string Title 
        {
            get => m_Title;
            internal set 
            {
                m_Title = value;
                this.NotifyChanged();
            }
        }

        public string Description
        {
            get => m_Description;
            internal set
            {
                m_Description = value;
                this.NotifyChanged();
            }
        }

        public DataTemplate Template { get; }

        public IExpressionToken Token
        {
            get => GetToken(Value);
            set
            {
                try
                {
                    Value = GetTokenValue(value);
                }
                catch (Exception ex)
                {
                    m_Error = ex;
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual object Value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                m_Error = null;
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));

                //NOTE: workaround, this needs to be called otherwise error will not be cleared
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
            }
        }

        public bool HasErrors => m_Error != null;

        private string m_Title;
        private string m_Description;
        private object m_Value;

        private IExpressionParser m_Parser;
        private IExpressionVariableDescriptor m_VariableDescriptor;
        private IEnumerable<IExpressionVariableLink> m_VariableLinks;

        private Exception m_Error;

        public ExpressionVariableArgumentDescriptor() : this("", "")
        {
        }

        public ExpressionVariableArgumentDescriptor(string title, string desc) 
            : this(title, desc, typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>(
                "Themes/Generic.xaml", "ExpressionVariableArgumentExpressionTemplate")) 
        {
        }

        public ExpressionVariableArgumentDescriptor(string title, string desc, DataTemplate template)
        {
            m_Title = title;
            m_Description = desc;
            Template = template;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (m_Error != null)
            {
                yield return m_Error;
            }
            else
            {
                yield break;
            }
        }

        protected virtual IExpressionToken GetToken(object value) => Parser.Parse(value?.ToString());

        protected virtual object GetTokenValue(IExpressionToken token) => Parser.CreateExpression(token);
    }
}
