//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

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
    /// <summary>
    /// Variable argument descriptor
    /// </summary>
    public abstract class ExpressionVariableArgumentDescriptor : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        /// <summary>
        /// Create text argument
        /// </summary>
        /// <param name="title">Name of the argument</param>
        /// <param name="tooltip">Description</param>
        /// <param name="icon">Icon</param>
        /// <returns>Argument descriptor</returns>
        public static ExpressionVariableArgumentDescriptor CreateText(string title, string tooltip, ImageSource icon)
            => new ExpressionVariableArgumentTextDescriptor(title, tooltip, icon);

        /// <summary>
        /// Create numeric argument
        /// </summary>
        /// <param name="title">Name of the argument</param>
        /// <param name="tooltip">Description</param>
        /// <param name="icon">Icon</param>
        /// <returns>Argument descriptor</returns>
        public static ExpressionVariableArgumentDescriptor CreateNumeric(string title, string tooltip, ImageSource icon)
            => new ExpressionVariableArgumentNumericDescriptor(title, tooltip, icon);

        /// <summary>
        /// Create floating number argument
        /// </summary>
        /// <param name="title">Name of the argument</param>
        /// <param name="tooltip">Description</param>
        /// <param name="icon">Icon</param>
        /// <returns>Argument descriptor</returns>
        public static ExpressionVariableArgumentDescriptor CreateNumericDouble(string title, string tooltip, ImageSource icon)
            => new ExpressionVariableArgumentNumericDoubleDescriptor(title, tooltip, icon);

        /// <summary>
        /// Create toggle argument
        /// </summary>
        /// <param name="title">Name of the argument</param>
        /// <param name="tooltip">Description</param>
        /// <param name="icon">Icon</param>
        /// <returns>Argument descriptor</returns>
        public static ExpressionVariableArgumentDescriptor CreateToggle(string title, string tooltip, ImageSource icon)
            => new ExpressionVariableArgumentToggleDescriptor(title, tooltip, icon);

        /// <summary>
        /// Create text options argument
        /// </summary>
        /// <param name="items">List of items</param>
        /// <param name="title">Name of the argument</param>
        /// <param name="tooltip">Description</param>
        /// <param name="icon">Icon</param>
        /// <returns>Argument descriptor</returns>
        public static ExpressionVariableArgumentDescriptor CreateOptions(string title, string tooltip, ImageSource icon, params string[] items)
            => new ExpressionVariableArgumentOptionsDescriptor(title, tooltip, icon, items);

        /// <summary>
        /// Create enumeration based options
        /// </summary>
        /// <typeparam name="TEnum">Enumeration type</typeparam>
        /// <param name="title">Name of the argument</param>
        /// <param name="tooltip">Description</param>
        /// <param name="icon">Icon</param>
        /// <returns>Argument descriptor</returns>
        public static ExpressionVariableArgumentDescriptor CreateOptions<TEnum>(string title, string tooltip, ImageSource icon)
            where TEnum : Enum => new ExpressionVariableArgumentEnumOptionsDescriptor(title, tooltip, icon, typeof(TEnum));

        /// <summary>
        /// Create expression argument
        /// </summary>
        /// <param name="title">Name of the argument</param>
        /// <param name="tooltip">Description</param>
        /// <param name="icon">Icon</param>
        /// <returns>Argument descriptor</returns>
        public static ExpressionVariableArgumentDescriptor CreateExpression(string title, string tooltip, ImageSource icon)
            => new ExpressionVariableArgumentExpressionDescriptor(title, tooltip, icon);

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Name of the argument
        /// </summary>
        public string Title
        {
            get => m_Title;
            set
            {
                m_Title = value;
                this.NotifyChanged();
            }
        }

        /// <summary>
        /// Argument detailed description
        /// </summary>
        public string Description
        {
            get => m_Description;
            set
            {
                m_Description = value;
                this.NotifyChanged();
            }
        }

        /// <summary>
        /// Argument icon
        /// </summary>
        public ImageSource Icon
        {
            get => m_Icon;
            set
            {
                m_Icon = value;
                this.NotifyChanged();
            }
        }

        /// <summary>
        /// Argument template
        /// </summary>
        public DataTemplate Template
        {
            get => IsAdvancedEditor ? m_AdvancedEditorTemplate.Value : m_Template;
            set
            {
                m_Template = value;
                this.NotifyChanged();
            }
        }

        /// <summary>
        /// Identifies if advanmced editor is used
        /// </summary>
        public virtual bool HasAdvancedEditor
        {
            get => m_HasAdvancedEditor;
            set
            {
                m_HasAdvancedEditor = value;
                this.NotifyChanged();
            }
        }

        /// <summary>
        /// Identifies if advanced editor is used
        /// </summary>
        public virtual bool IsAdvancedEditor
        {
            get => m_IsAdvancedEditor;
            set
            {
                m_IsAdvancedEditor = value;
                this.NotifyChanged();
                this.NotifyChanged(nameof(Template));
            }
        }

        internal IExpressionToken GetToken(IExpressionParser expParser)
        {
            if (IsAdvancedEditor)
            {
                return expParser.Parse(Value?.ToString());
            }
            else
            {
                return CreateToken(Value, expParser);
            }
        }

        internal void SetToken(IExpressionToken token, IExpressionParser expParser)
        {
            try
            {
                if (IsAdvancedEditor)
                {
                    Value = expParser.CreateExpression(token);
                }
                else
                {
                    Value = GetTokenValue(token, expParser);
                }
            }
            catch (Exception ex)
            {
                m_Error = ex;
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
            }
        }

        /// <summary>
        /// Argument value
        /// </summary>
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

        /// <summary>
        /// Type of the value of this argument
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        /// Checks if argument has errors
        /// </summary>
        public bool HasErrors => m_Error != null;

        private string m_Title;
        private string m_Description;
        private ImageSource m_Icon;

        private DataTemplate m_Template;
        private bool m_IsAdvancedEditor;
        private bool m_HasAdvancedEditor;

        private object m_Value;

        private Exception m_Error;

        private Lazy<DataTemplate> m_AdvancedEditorTemplate;

        //NOTE: need to keep the public parameterless constructor so DataGrid allows to create new items
        /// <summary>
        /// Constructor
        /// </summary>
        public ExpressionVariableArgumentDescriptor()
        {
            m_AdvancedEditorTemplate = new Lazy<DataTemplate>(() => typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>(
                "Themes/ExpressionBox.xaml", "ExpressionVariableArgumentExpressionTemplate"));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Name of the argument</param>
        /// <param name="desc">Detailed description of the argument</param>
        /// <param name="icon">Icon of the argument</param>
        /// <param name="template">Editor template</param>
        /// <param name="valType">Value type</param>
        protected ExpressionVariableArgumentDescriptor(string title, string desc, ImageSource icon, DataTemplate template, Type valType) : this()
        {
            m_Title = title;
            m_Description = desc;
            m_Icon = icon;
            m_Template = template;
            ValueType = valType;
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

        /// <summary>
        /// Get the value from the token
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="expParser">Expression parser</param>
        /// <returns>Value</returns>
        /// <exception cref="NotSupportedException">Not supported token</exception>
        protected virtual object GetTokenValue(IExpressionToken token, IExpressionParser expParser) 
        {
            if (token is IExpressionTokenText)
            {
                var textVal = ((IExpressionTokenText)token).Text;

                if (ValueType.IsEnum)
                {
                    return Enum.Parse(ValueType, textVal);
                }
                else
                {
                    return Convert.ChangeType(textVal, ValueType);
                }
            }
            else if (HasAdvancedEditor)
            {
                IsAdvancedEditor = true;
                return expParser.CreateExpression(token);
            }
            else
            {
                throw new NotSupportedException($"Only {typeof(IExpressionTokenText)} is supported");
            }
        }

        /// <summary>
        /// Clones this argument
        /// </summary>
        /// <returns>Cloned argument</returns>
        public abstract ExpressionVariableArgumentDescriptor Clone();
    }

    public class ExpressionVariableArgumentExpressionDescriptor : ExpressionVariableArgumentDescriptor 
    {
        public ExpressionVariableArgumentExpressionDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentExpressionDescriptor(string title, string tooltip, ImageSource icon)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/ExpressionBox.xaml", "ExpressionVariableArgumentExpressionTemplate"),
                  typeof(string))
        {
        }

        public override bool HasAdvancedEditor { get => false; set { } }
        public override bool IsAdvancedEditor { get => false; set { } }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser) => expParser.Parse(value?.ToString());

        protected override object GetTokenValue(IExpressionToken token, IExpressionParser expParser) => expParser.CreateExpression(token);

        public override ExpressionVariableArgumentDescriptor Clone() => new ExpressionVariableArgumentExpressionDescriptor(Title, Description, Icon);
    }

    public class ExpressionVariableArgumentTextDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentTextDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentTextDescriptor(string title, string tooltip, ImageSource icon)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/ExpressionBox.xaml", "ExpressionVariableArgumentTextTemplate"),
                  typeof(string))
        {
        }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

        public override ExpressionVariableArgumentDescriptor Clone() => new ExpressionVariableArgumentTextDescriptor(Title, Description, Icon)
        {
            HasAdvancedEditor = HasAdvancedEditor
        };
    }

    public class ExpressionVariableArgumentNumericDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentNumericDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentNumericDescriptor(string title, string tooltip, ImageSource icon)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/ExpressionBox.xaml", "ExpressionVariableArgumentNumericTemplate"),
                  typeof(int))
        {
        }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

        public override ExpressionVariableArgumentDescriptor Clone() => new ExpressionVariableArgumentNumericDescriptor(Title, Description, Icon)
        {
            HasAdvancedEditor = HasAdvancedEditor
        };
    }

    public class ExpressionVariableArgumentNumericDoubleDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentNumericDoubleDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentNumericDoubleDescriptor(string title, string tooltip, ImageSource icon)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/ExpressionBox.xaml", "ExpressionVariableArgumentNumericDoubleTemplate"),
                  typeof(double))
        {
        }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

        public override ExpressionVariableArgumentDescriptor Clone() => new ExpressionVariableArgumentNumericDoubleDescriptor(Title, Description, Icon)
        {
            HasAdvancedEditor = HasAdvancedEditor
        };
    }

    public class ExpressionVariableArgumentToggleDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentToggleDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentToggleDescriptor(string title, string tooltip, ImageSource icon)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/ExpressionBox.xaml", "ExpressionVariableArgumentToggleTemplate"),
                  typeof(bool))
        {
        }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

        public override ExpressionVariableArgumentDescriptor Clone() => new ExpressionVariableArgumentToggleDescriptor(Title, Description, Icon)
        {
            HasAdvancedEditor = HasAdvancedEditor
        };
    }

    public class ExpressionVariableArgumentOptionsDescriptor : ExpressionVariableArgumentDescriptor
    {
        public ExpressionVariableArgumentOptionsDescriptor() : this("", "", null)
        {
        }

        public ExpressionVariableArgumentOptionsDescriptor(string title, string tooltip, ImageSource icon, params string[] items)
            : base(title, tooltip, icon,
                  typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/ExpressionBox.xaml", "ExpressionVariableArgumentOptionsTemplate"),
                  typeof(string))
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

        public override ExpressionVariableArgumentDescriptor Clone() => new ExpressionVariableArgumentOptionsDescriptor(Title, Description, Icon)
        {
            HasAdvancedEditor = HasAdvancedEditor
        };
    }

    public class ExpressionVariableArgumentEnumOptionsDescriptor : ExpressionVariableArgumentDescriptor
    {
        private Type m_EnumType;

        public ExpressionVariableArgumentEnumOptionsDescriptor()
        {
        }

        public ExpressionVariableArgumentEnumOptionsDescriptor(string title, string tooltip, ImageSource icon, Type enumType)
            : base(title, tooltip, icon, null, enumType)
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
                    Template = typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/ExpressionBox.xaml", "ExpressionVariableArgumentEnumFlagOptionsTemplate");
                }
                else 
                {
                    Template = typeof(ExpressionVariableArgumentTextDescriptor).Assembly.LoadFromResources<DataTemplate>("Themes/ExpressionBox.xaml", "ExpressionVariableArgumentEnumOptionsTemplate");
                }
            }
        }

        protected override IExpressionToken CreateToken(object value, IExpressionParser expParser)
            => new ExpressionTokenText(value?.ToString());

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

        public override ExpressionVariableArgumentDescriptor Clone() => new ExpressionVariableArgumentEnumOptionsDescriptor(Title, Description, Icon, EnumType)
        {
            HasAdvancedEditor = HasAdvancedEditor 
        };
    }
}
