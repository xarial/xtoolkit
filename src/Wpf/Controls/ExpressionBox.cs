﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Reporting;
using Xarial.XToolkit.Services.Expressions.Exceptions;
using Xarial.XToolkit.Wpf.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.XToolkit.Wpf.Controls
{
    public class DataGridCellIsNewItemPlaceholderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => CollectionView.NewItemPlaceholder.GetType() == value?.GetType();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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

        public IExpressionParser Parser 
        {
            get => m_Parser;
            internal set 
            {
                m_Parser = value;
                this.NotifyChanged();
            }
        }

        public IExpressionVariableDescriptor VariableDescriptor 
        {
            get => m_VariableDescriptor;
            internal set 
            {
                m_VariableDescriptor = value;
                this.NotifyChanged();
            }
        }

        public IEnumerable<ExpressionVariableLink> VariableLinks
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
        private IEnumerable<ExpressionVariableLink> m_VariableLinks;

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

    public interface IExpressionVariableDescriptor 
    {
        string GetTitle(IExpressionTokenVariable variable);
        BitmapImage GetIcon(IExpressionTokenVariable variable);
        string GetDescription(IExpressionTokenVariable variable);
        Brush GetBackground(IExpressionTokenVariable variable);
        ExpressionVariableArgumentDescriptor[] GetArguments(IExpressionTokenVariable variable, out bool dynamic);
    }

    public class ExpressionVariableTokenControl : Control
    {
        private DataGrid m_DataGrid;
        private PopupMenu m_PopupEditor;

        private bool m_Edit;

        static ExpressionVariableTokenControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpressionVariableTokenControl),
                new FrameworkPropertyMetadata(typeof(ExpressionVariableTokenControl)));
        }

        public override void OnApplyTemplate()
        {
            m_DataGrid = (DataGrid)this.Template.FindName("PART_DataGrid", this);
            m_PopupEditor = (PopupMenu)this.Template.FindName("PART_PopupEditor", this);

            m_PopupEditor.Closed += OnClosed;

            m_DataGrid.InitializingNewItem += OnInitializingNewItem;

            if (m_Edit) 
            {
                Edit();
                m_Edit = false;
            }
        }

        public void Edit() 
        {
            if (Arguments != null)
            {
                if (m_PopupEditor != null)
                {
                    m_PopupEditor.IsOpen = true;
                }
                else 
                {
                    m_Edit = true;
                }
            }
        }

        private void OnClosed(PopupMenu popupMenu)
        {
            m_Variable = new ExpressionTokenVariable(m_VariableName, Arguments?.Select(a => a.Token).ToArray());

            UpdateVariableControl(m_Variable);
        }

        private void OnInitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            var argDesc = (ExpressionVariableArgumentDescriptor)e.NewItem;
            InitArgumentDescriptor(argDesc);
        }

        internal IExpressionTokenVariable Variable => m_Variable;

        private readonly IExpressionParser m_Parser;
        private readonly IExpressionVariableDescriptor m_Descriptor;
        private readonly IEnumerable<ExpressionVariableLink> m_VariableLinks;

        public ObservableCollection<ExpressionVariableArgumentDescriptor> Arguments { get; }

        private readonly string m_VariableName;

        public bool DynamicArguments { get; }

        private IExpressionTokenVariable m_Variable;

        internal ExpressionVariableTokenControl(IExpressionTokenVariable variable,
            IExpressionVariableDescriptor descriptor, IExpressionParser parser, IEnumerable<ExpressionVariableLink> variableLinks)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            m_Parser = parser;

            m_Variable = variable;

            m_VariableName = variable.Name;

            m_Descriptor = descriptor;

            m_VariableLinks = variableLinks;

            UpdateVariableControl(variable);

            var args = m_Descriptor.GetArguments(variable, out bool dynamic);

            DynamicArguments = dynamic;

            if (args?.Any() == true || dynamic)
            {
                Arguments = new ObservableCollection<ExpressionVariableArgumentDescriptor>(args ?? new ExpressionVariableArgumentDescriptor[0]);

                for (int i = 0; i < Arguments.Count; i++)
                {
                    var arg = Arguments[i];

                    InitArgumentDescriptor(arg);

                    if (variable.Arguments != null && variable.Arguments.Length > i)
                    {
                        arg.Token = variable.Arguments[i];
                    }
                }

                if (DynamicArguments)
                {
                    ResolveDynamicArgumentNames();
                    Arguments.CollectionChanged += OnDynamicArgumentsChanged;
                }
            }
        }

        private void UpdateVariableControl(IExpressionTokenVariable variable)
        {
            Title = m_Descriptor.GetTitle(variable);
            Icon = m_Descriptor.GetIcon(variable);
            ToolTip = m_Descriptor.GetDescription(variable);
            Background = m_Descriptor.GetBackground(variable);
        }

        private void OnDynamicArgumentsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ResolveDynamicArgumentNames();
        }

        private void ResolveDynamicArgumentNames() 
        {
            for (int i = 0; i < Arguments.Count; i++) 
            {
                var arg = Arguments[i];
                arg.Title = $"#{i + 1}";
                arg.Description = $"Argument #{i + 1}";
            }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
            nameof(Title), typeof(string),
            typeof(ExpressionVariableTokenControl));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
            nameof(Icon), typeof(BitmapImage),
            typeof(ExpressionVariableTokenControl));

        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        private void InitArgumentDescriptor(ExpressionVariableArgumentDescriptor argDesc)
        {
            argDesc.Parser = m_Parser;
            argDesc.VariableDescriptor = m_Descriptor;
            argDesc.VariableLinks = m_VariableLinks;
        }
    }

    public delegate IExpressionTokenVariable ExpressionVariableFactoryDelegate(ExpressionBox sender);

    public class ExpressionVariableLink 
    {
        public static ExpressionVariableLink Custom { get; } = new ExpressionVariableLink("Insert New Variable...", "Insert a new variable with the specified name", null, sender =>
        {
            if (InputBox.ShowAtCursor("ExpressionBox", "Variable Name", out string varName))
            {
                return new ExpressionTokenVariable(varName, null);
            }
            else
            {
                return null;
            }
        }, true);

        public string Title { get; }
        public string Description { get; }
        public BitmapImage Icon { get; }
        public ExpressionVariableFactoryDelegate Factory { get; }
        public bool EnterArgs { get; }

        public ExpressionVariableLink(string title, string description, BitmapImage icon, ExpressionVariableFactoryDelegate factory, bool enterArgs) 
        {
            Title = title;
            Description = description;
            Icon = icon;
            Factory = factory;
            EnterArgs = enterArgs;
        }
    }

    public class ExpressionBox : Control
    {
        private class InternalChangeTracker 
        {
            private class InternalChange : IDisposable
            {
                private readonly Action<bool> m_Setter;

                internal InternalChange(Action<bool> setter)
                {
                    m_Setter = setter;
                    m_Setter.Invoke(true);
                }

                public void Dispose()
                {
                    m_Setter.Invoke(false);
                }
            }

            internal bool IsInternalChange { get; private set; }

            internal InternalChangeTracker() 
            {
                IsInternalChange = false;
            }

            internal IDisposable PerformInternalChange() => new InternalChange(x => IsInternalChange = x);
        }

        private class DefaultExpressionVariableDescriptor : IExpressionVariableDescriptor
        {
            public ExpressionVariableArgumentDescriptor[] GetArguments(IExpressionTokenVariable variable, out bool dynamic) 
            {   
                dynamic = true;

                return variable.Arguments?.Select(a => new ExpressionVariableArgumentDescriptor()).ToArray();
            }

            public Brush GetBackground(IExpressionTokenVariable variable) => new SolidColorBrush(Color.FromRgb(221, 221, 221));

            public BitmapImage GetIcon(IExpressionTokenVariable variable) => null;

            public string GetTitle(IExpressionTokenVariable variable) => variable.Name;

            public string GetDescription(IExpressionTokenVariable variable) => variable.Name;
        }

        private RichTextBox m_TextBox;
        private FlowDocument m_Doc;
        private readonly InternalChangeTracker m_InternalChangeTracker;

        private bool m_IsInit;

        static ExpressionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpressionBox),
                new FrameworkPropertyMetadata(typeof(ExpressionBox)));

            BorderThicknessProperty.OverrideMetadata(
                typeof(ExpressionBox), new FrameworkPropertyMetadata(new Thickness(1)));

            BorderBrushProperty.OverrideMetadata(
                typeof(ExpressionBox),
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(171, 173, 179))));
        }

        public ICommand InsertVariableCommand { get; }

        public ExpressionBox() 
        {
            m_InternalChangeTracker = new InternalChangeTracker();

            InsertVariableCommand = new RelayCommand<ExpressionVariableLink>(InsertVariable);
        }

        private void InsertVariable(ExpressionVariableLink link)
        {
            var var = link.Factory.Invoke(this);

            if (var != null) 
            {
                Insert(var, link.EnterArgs);
            }
        }

        private void OnCopy(object sender, DataObjectCopyingEventArgs e)
        {
            e.Handled = true;
            e.CancelCommand();

            IEnumerable<Tuple<Inline, int?, int?>> GetSelectedInlines() 
            {
                var sel = m_TextBox.Selection;

                if (!sel.IsEmpty)
                {
                    foreach (var inline in Inlines)
                    {
                        if (sel.Start.CompareTo(inline.ContentEnd) <= 0 && sel.End.CompareTo(inline.ContentStart) >= 0)
                        {
                            var hasStart = sel.Contains(inline.ContentStart);
                            var hasEnd = sel.Contains(inline.ContentEnd);

                            int? start = null;
                            int? end = null;

                            if (!hasStart)
                            {
                                start = inline.ContentStart.GetOffsetToPosition(sel.Start);
                            }

                            if (!hasEnd)
                            {
                                end = sel.End.GetOffsetToPosition(inline.ContentEnd);
                            }

                            yield return new Tuple<Inline, int?, int?>(inline, start, end);
                        }
                    }
                }
                else
                {
                    yield break;
                }
            }

            var expression = new StringBuilder();

            var parser = GetParser();

            foreach (var inline in GetSelectedInlines()) 
            {
                var token = GetExpressionToken(inline.Item1);

                var tokenExpression = parser.CreateExpression(token);

                if (inline.Item3.HasValue)
                {
                    tokenExpression = tokenExpression.Substring(0, tokenExpression.Length - inline.Item3.Value);
                }

                if (inline.Item2.HasValue) 
                {
                    tokenExpression = tokenExpression.Substring(inline.Item2.Value);
                }

                expression.Append(tokenExpression);
            }

            Clipboard.SetText(expression.ToString());
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            e.Handled = true;
            e.CancelCommand();

            var parser = GetParser();

            var expression = Clipboard.GetText();

            try
            {
                var token = parser.Parse(expression);

                Insert(token, false);
            }
            catch (Exception ex)
            {
                SetExpressionError(ex);
            }
        }

        public override void OnApplyTemplate()
        {
            m_TextBox = (RichTextBox)this.Template.FindName("PART_RichTextBox", this);
            m_Doc = m_TextBox.Document;
            m_TextBox.TextChanged += OnTextChanged;
            
            m_IsInit = true;

            DataObject.AddCopyingHandler(m_TextBox, OnCopy);
            DataObject.AddPastingHandler(m_TextBox, OnPaste);

            RenderExpression(Expression);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            //adding extra space for the height so the varibale controls are fit inside otherwise they can be cropped
            const int OFFSET = 4;

            var size = base.MeasureOverride(constraint);
            return new Size(size.Width, size.Height + OFFSET);
        }

        public void Insert(IExpressionToken expressionToken, bool enterArgs) 
        {
            using (var intChange = m_InternalChangeTracker.PerformInternalChange()) 
            {
                TextPointer pos;

                var sel = m_TextBox.Selection;

                if (!sel.IsEmpty)
                {
                    pos = sel.Start;
                    sel.Text = "";
                }
                else
                {
                    pos = m_TextBox.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
                }

                var inlines = AddExpressionToken(Inlines, expressionToken, ref pos);

                if (enterArgs)
                {
                    var cont = inlines.OfType<InlineUIContainer>().FirstOrDefault();

                    if (cont?.Child is ExpressionVariableTokenControl)
                    {
                        var varCtrl = (ExpressionVariableTokenControl)cont.Child;

                        varCtrl.Edit();
                    }
                }

                m_TextBox.CaretPosition = pos;

                UpdateExpression();

                m_TextBox.Focus();
            }
        }

        public static readonly DependencyProperty ExpressionParserProperty =
            DependencyProperty.Register(
            nameof(ExpressionParser), typeof(IExpressionParser),
            typeof(ExpressionBox), new PropertyMetadata(new ExpressionParser()));

        public IExpressionParser ExpressionParser
        {
            get { return (IExpressionParser)GetValue(ExpressionParserProperty); }
            set { SetValue(ExpressionParserProperty, value); }
        }

        public static readonly DependencyProperty ExpressionProperty =
            DependencyProperty.Register(
            nameof(Expression), typeof(string),
            typeof(ExpressionBox),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnExpressionChanged));

        public string Expression
        {
            get { return (string)GetValue(ExpressionProperty); }
            set { SetValue(ExpressionProperty, value); }
        }

        public static readonly DependencyProperty VariableDescriptorProperty =
            DependencyProperty.Register(
            nameof(VariableDescriptor), typeof(IExpressionVariableDescriptor),
            typeof(ExpressionBox), new PropertyMetadata(new DefaultExpressionVariableDescriptor()));

        public IExpressionVariableDescriptor VariableDescriptor
        {
            get { return (IExpressionVariableDescriptor)GetValue(VariableDescriptorProperty); }
            set { SetValue(VariableDescriptorProperty, value); }
        }

        public static readonly DependencyProperty VariableLinksProperty =
            DependencyProperty.Register(
            nameof(VariableLinks), typeof(IEnumerable<ExpressionVariableLink>),
            typeof(ExpressionBox), new PropertyMetadata(new ExpressionVariableLink[] { ExpressionVariableLink.Custom }));

        public IEnumerable<ExpressionVariableLink> VariableLinks
        {
            get { return (IEnumerable<ExpressionVariableLink>)GetValue(VariableLinksProperty); }
            set { SetValue(VariableLinksProperty, value); }
        }


        public static readonly DependencyProperty VariableLinksMenuTemplateProperty =
            DependencyProperty.Register(
            nameof(VariableLinksMenuTemplate), typeof(DataTemplate),
            typeof(ExpressionBox),
            new PropertyMetadata(typeof(ExpressionBox).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "VariableLinksMenuTemplate")));

        public DataTemplate VariableLinksMenuTemplate
        {
            get { return (DataTemplate)GetValue(VariableLinksMenuTemplateProperty); }
            set { SetValue(VariableLinksMenuTemplateProperty, value); }
        }

        private InlineCollection Inlines 
        {
            get 
            {
                if (m_Doc.Blocks.Count == 1)
                {
                    return ((Paragraph)m_Doc.Blocks.FirstBlock).Inlines;
                }
                else if (m_Doc.Blocks.Count == 0)
                {
                    var par = new Paragraph()
                    {
                        LineStackingStrategy = LineStackingStrategy.MaxHeight
                    };
                    m_Doc.Blocks.Add(par);
                    return par.Inlines;
                }
                else 
                {
                    throw new NotSupportedException("Only one paragraph is supported");
                }
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!m_InternalChangeTracker.IsInternalChange)
            {
                using (var intChange = m_InternalChangeTracker.PerformInternalChange())
                {
                    UpdateExpression();
                }
            }
        }

        private void UpdateExpression()
        {
            var parser = GetParser();
            
            IExpressionToken token;

            if (m_Doc.Blocks.Count == 1)
            {
                token = GetExpressionToken(Inlines);
            }
            else
            {
                token = new ExpressionTokenText("");
            }

            Expression = parser.CreateExpression(token);
        }

        private static void OnExpressionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            var expBox = (ExpressionBox)d;

            expBox.RenderExpression((string)e.NewValue);
        }

        private void RenderExpression(string expression) 
        {
            if (m_IsInit && !m_InternalChangeTracker.IsInternalChange)
            {
                using (var intChnage = m_InternalChangeTracker.PerformInternalChange()) 
                {
                    m_Doc.Blocks.Clear();

                    var parser = GetParser();

                    try
                    {
                        var expressionToken = parser.Parse(expression);

                        var pos = m_Doc.ContentStart;

                        AddExpressionToken(Inlines, expressionToken, ref pos);
                    }
                    catch (Exception ex)
                    {
                        SetExpressionError(ex);
                    }
                }
            }
        }

        /// <remarks><paramref name="inlines"/> parameter is not used directly, but keeping it to enforce updating the inline and creating new ones if needed</remarks>
        private IReadOnlyList<Inline> AddExpressionToken(InlineCollection inlines, IExpressionToken expressionToken, ref TextPointer pos) 
        {
            var res = new List<Inline>();

            switch (expressionToken)
            {
                case IExpressionTokenVariable variable:
                    var uiCont = new InlineUIContainer(new ExpressionVariableTokenControl(variable, VariableDescriptor, GetParser(), VariableLinks), pos);
                    pos = uiCont.ContentEnd.GetInsertionPosition(LogicalDirection.Forward);
                    res.Add(uiCont);
                    break;

                case IExpressionTokenText text:
                    var run = new Run(text.Text, pos);
                    pos = run.ContentEnd.GetInsertionPosition(LogicalDirection.Forward);
                    res.Add(run);
                    break;

                case IExpressionTokenGroup group:
                    if (group.Children != null)
                    {
                        foreach (var child in group.Children)
                        {
                            res.AddRange(AddExpressionToken(inlines, child, ref pos));
                        }
                    }
                    break;

                default:
                    throw new NotSupportedException("Expression token is not supported");
            }

            return res;
        }

        private IExpressionToken GetExpressionToken(IEnumerable<Inline> inlines) 
        {
            var tokens = new List<IExpressionToken>();

            foreach (var inline in inlines)
            {
                tokens.Add(GetExpressionToken(inline));
            }

            if (tokens.Count == 0)
            {
                return new ExpressionTokenText("");
            }
            else if (tokens.Count == 1)
            {
                return tokens.First();
            }
            else 
            {
                return new ExpressionTokenGroup(tokens.ToArray());
            }
        }

        private IExpressionToken GetExpressionToken(Inline inline)
        {
            switch (inline)
            {
                case Run run:
                    return new ExpressionTokenText(run.Text);

                case InlineUIContainer cont:
                    if (cont.Child is ExpressionVariableTokenControl)
                    {
                        var varCtrl = (ExpressionVariableTokenControl)cont.Child;
                        return varCtrl.Variable;
                    }
                    else
                    {
                        throw new Exception("Not supported UI token");
                    }

                default:
                    throw new NotSupportedException("Not supported inline");
            }
        }

        private IExpressionParser GetParser()
        {
            var parser = ExpressionParser;

            if (parser == null)
            {
                throw new NullReferenceException("Expression parser is not set");
            }

            return parser;
        }

        private void SetExpressionError(Exception ex)
        {
            var bindingExpression = this.GetBindingExpression(ExpressionProperty);
            
            if (bindingExpression != null)
            {
                var validationError = new ValidationError(new DataErrorValidationRule(), bindingExpression);
                validationError.ErrorContent = ex;

                Validation.MarkInvalid(bindingExpression, validationError);
            }
        }
    }
}
