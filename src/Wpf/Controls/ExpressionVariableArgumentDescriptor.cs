using System;
using System.Collections.Generic;
using System.Windows;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf.Extensions;
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Xarial.XToolkit.Wpf.Controls
{
    public abstract class ExpressionVariableArgumentDescriptor : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public static ExpressionVariableArgumentDescriptor CreateText(string title, string tooltip, ImageSource icon)
            => new ExpressionVariableArgumentTextDescriptor(title, tooltip, icon);

        public static ExpressionVariableArgumentDescriptor CreateNumeric(string title, string tooltip, ImageSource icon)
            => new ExpressionVariableArgumentNumericDescriptor(title, tooltip, icon);

        public static ExpressionVariableArgumentDescriptor CreateNumericDouble(string title, string tooltip, ImageSource icon)
            => new ExpressionVariableArgumentNumericDoubleDescriptor(title, tooltip, icon);

        public static ExpressionVariableArgumentDescriptor CreateToggle(string title, string tooltip, ImageSource icon)
            => new ExpressionVariableArgumentToggleDescriptor(title, tooltip, icon);

        public static ExpressionVariableArgumentDescriptor CreateOptions(string title, string tooltip, ImageSource icon, params string[] items)
            => new ExpressionVariableArgumentOptionsDescriptor(title, tooltip, icon, items);

        public static ExpressionVariableArgumentDescriptor CreateOptions<TEnum>(string title, string tooltip, ImageSource icon)
            where TEnum : Enum => new ExpressionVariableArgumentEnumOptionsDescriptor(title, tooltip, icon, typeof(TEnum));

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public event PropertyChangedEventHandler PropertyChanged;

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

        public ImageSource Icon
        {
            get => m_Icon;
            set
            {
                m_Icon = value;
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

        public bool HasAdvancedEditor
        {
            get => m_HasAdvancedEditor;
            set
            {
                m_HasAdvancedEditor = value;
                this.NotifyChanged();
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsAdvancedEditor
        {
            get => m_IsAdvancedEditor;
            set
            {
                m_IsAdvancedEditor = value;
                this.NotifyChanged();
            }
        }

        internal IExpressionToken GetToken(IExpressionParser expParser) => CreateToken(Value, expParser);

        internal void SetToken(IExpressionToken token, IExpressionParser expParser) 
        {
            try
            {
                Value = GetTokenValue(token, expParser);
            }
            catch (Exception ex)
            {
                m_Error = ex;
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
            }
        }

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
        private ImageSource m_Icon;

        private DataTemplate m_Template;
        private bool m_IsAdvancedEditor;
        private bool m_HasAdvancedEditor;

        private object m_Value;

        private Exception m_Error;

        //NOTE: need to keey the public parameterless constructor so DataGrid allows to create new items
        public ExpressionVariableArgumentDescriptor() 
        {
        }

        protected ExpressionVariableArgumentDescriptor(string title, string desc, ImageSource icon, DataTemplate template)
        {
            m_Title = title;
            m_Description = desc;
            m_Icon = icon;
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

        protected abstract IExpressionToken CreateToken(object value, IExpressionParser expParser);

        protected abstract object GetTokenValue(IExpressionToken token, IExpressionParser expParser);
    }

    public class ExpressionVariableArgumentExpressionDescriptor : ExpressionVariableArgumentDescriptor 
    {
        public ExpressionVariableArgumentExpressionDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentExpressionDescriptor(string title, string tooltip, ImageSource icon)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentExpressionTemplate"))
        {
        }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser) => expParser.Parse(value?.ToString());

        protected override object GetTokenValue(IExpressionToken token, IExpressionParser expParser) => expParser.CreateExpression(token);
    }

    public class ExpressionVariableArgumentTextDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentTextDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentTextDescriptor(string title, string tooltip, ImageSource icon)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentTextTemplate"))
        {
        }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

        protected override object GetTokenValue(IExpressionToken token, IExpressionParser expParser)
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
        public ExpressionVariableArgumentNumericDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentNumericDescriptor(string title, string tooltip, ImageSource icon)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentNumericTemplate"))
        {
        }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

        protected override object GetTokenValue(IExpressionToken token, IExpressionParser expParser)
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
        public ExpressionVariableArgumentNumericDoubleDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentNumericDoubleDescriptor(string title, string tooltip, ImageSource icon)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentNumericDoubleTemplate"))
        {
        }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

        protected override object GetTokenValue(IExpressionToken token, IExpressionParser expParser)
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
        public ExpressionVariableArgumentToggleDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentToggleDescriptor(string title, string tooltip, ImageSource icon)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ExpressionVariableArgumentToggleTemplate"))
        {
        }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

        protected override object GetTokenValue(IExpressionToken token, IExpressionParser expParser)
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
        public ExpressionVariableArgumentOptionsDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentOptionsDescriptor(string title, string tooltip, ImageSource icon, params string[] items)
            : base(title, tooltip, icon,
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

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

        protected override object GetTokenValue(IExpressionToken token, IExpressionParser expParser)
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

        public ExpressionVariableArgumentEnumOptionsDescriptor(string title, string tooltip, ImageSource icon, Type enumType)
            : base(title, tooltip, icon, null)
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

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

        protected override object GetTokenValue(IExpressionToken token, IExpressionParser expParser)
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
}
