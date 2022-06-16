using System;
using System.Collections.Generic;
using System.Windows;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf.Extensions;
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Xarial.XToolkit.Wpf.Controls
{
    public class ExpressionVariableArgumentTextDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentTextDescriptor() : this("", "")
        {
        }

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

    public class ExpressionVariableArgumentNumericDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentNumericDescriptor() : this("", "")
        {
        }

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

    public class ExpressionVariableArgumentNumericDoubleDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentNumericDoubleDescriptor() : this("", "")
        {
        }

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

    public class ExpressionVariableArgumentToggleDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentToggleDescriptor() : this("", "")
        {
        }

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

    public class ExpressionVariableArgumentOptionsDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentOptionsDescriptor() : this("", "")
        {
        }

        public ExpressionVariableArgumentOptionsDescriptor(string title, string tooltip, params string[] items)
            : base(title, tooltip,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentOptionsTemplate"))
        {
            Items = new Collection<string>();

            if (items != null)
            {
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
        }

        public Collection<string> Items { get; }

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

    public class ExpressionVariableArgumentEnumOptionsDescriptor : ExpressionVariableArgumentDescriptor
    {
        private Type m_EnumType;

        public ExpressionVariableArgumentEnumOptionsDescriptor() 
        {
        }

        public ExpressionVariableArgumentEnumOptionsDescriptor(string title, string tooltip, Type enumType)
            : base(title, tooltip, null)
        {
            EnumType = enumType;
        }

        public Type EnumType 
        {
            get => m_EnumType;
            set 
            {
                if (value == null) 
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (!value.IsEnum) 
                {
                    throw new InvalidCastException("Specified type is not an Enum");
                }

                m_EnumType = value;

                if (m_EnumType.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    Template = typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentEnumFlagOptionsTemplate");
                }
                else 
                {
                    Template = typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentEnumOptionsTemplate");
                }
            }
        }

        protected override IExpressionToken GetToken(object value)
            => new ExpressionTokenText(value?.ToString());

        protected override object GetTokenValue(IExpressionToken token)
        {
            if (token is IExpressionTokenText)
            {
                return Enum.Parse(EnumType, ((IExpressionTokenText)token).Text);
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
                    val = Enum.ToObject(EnumType, 0);
                }

                return val;
            }
            set => base.Value = value;
        }
    }

    public class ExpressionVariableArgumentDescriptor: INotifyPropertyChanged, INotifyDataErrorInfo
    {
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
            where TEnum : Enum => new ExpressionVariableArgumentEnumOptionsDescriptor(title, tooltip, typeof(TEnum));

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
            set 
            {
                m_Title = value;
                this.NotifyChanged();
            }
        }

        public string Description
        {
            get => m_Description;
            set
            {
                m_Description = value;
                this.NotifyChanged();
            }
        }

        public DataTemplate Template 
        {
            get => m_Template;
            set 
            {
                m_Template = value;
                this.NotifyChanged();
            }
        }

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
        private DataTemplate m_Template;
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
            m_Template = template;
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
