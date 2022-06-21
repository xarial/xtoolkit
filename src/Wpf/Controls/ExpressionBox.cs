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

    public class ExpressionVariableTokenControl : Control, INotifyPropertyChanged
    {
        internal event Action<ExpressionVariableTokenControl> VariableUpdated;

        static ExpressionVariableTokenControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpressionVariableTokenControl),
                new FrameworkPropertyMetadata(typeof(ExpressionVariableTokenControl)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private DataGrid m_DataGrid;
        private PopupMenu m_PopupEditor;

        private bool m_Edit;

        public override void OnApplyTemplate()
        {
            m_DataGrid = (DataGrid)this.Template.FindName("PART_DataGrid", this);
            m_PopupEditor = (PopupMenu)this.Template.FindName("PART_PopupEditor", this);

            m_PopupEditor.Closed += OnClosed;

            m_DataGrid.AddingNewItem += OnAddingNewItem;

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
            m_Variable = new ExpressionTokenVariable(m_VariableName, Arguments?.Select(a => a.GetToken(Owner.GetExpressionParser())).ToArray());

            UpdateVariableControl(m_Variable);

            VariableUpdated?.Invoke(this);
        }

        private void OnAddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            var argDesc = NewDynamicArgument();
            e.NewItem = argDesc;
        }

        internal IExpressionTokenVariable Variable => m_Variable;

        public ObservableCollection<ExpressionVariableArgumentDescriptor> Arguments
        {
            get => m_Arguments;
            private set
            {
                m_Arguments = value;
                this.NotifyChanged();
            }
        }

        public ExpressionBox Owner { get; }

        private readonly string m_VariableName;

        public bool DynamicArguments
        {
            get => m_DynamicArguments;
            private set
            {
                m_DynamicArguments = value;
                this.NotifyChanged();
            }
        }

        private IExpressionTokenVariable m_Variable;
        private bool m_DynamicArguments;
        private ObservableCollection<ExpressionVariableArgumentDescriptor> m_Arguments;

        internal ExpressionVariableTokenControl(ExpressionBox owner, IExpressionTokenVariable variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            Owner = owner;

            Owner.VariableDescriptorChanged += OnVariableDescriptorChanged;

            m_Variable = variable;

            m_VariableName = variable.Name;

            UpdateVariableControl(variable);
            UpdateVariableArguments(variable);
        }

        private void OnVariableDescriptorChanged(ExpressionBox sender, IExpressionVariableDescriptor varsDesc)
        {
            UpdateVariableArguments(Variable);
            UpdateVariableControl(Variable);
        }

        private ExpressionVariableArgumentExpressionDescriptor NewDynamicArgument()
            => new ExpressionVariableArgumentExpressionDescriptor("", "", null);

        private void UpdateVariableControl(IExpressionTokenVariable variable)
        {
            var descriptor = Owner.GetVariableDescriptor();

            Title = descriptor.GetTitle(variable);
            Icon = descriptor.GetIcon(variable);
            ToolTip = descriptor.GetDescription(variable);
            Background = descriptor.GetBackground(variable);
        }

        private void UpdateVariableArguments(IExpressionTokenVariable variable)
        {
            Arguments = null;

            var args = Owner.GetVariableDescriptor().GetArguments(variable, out bool dynamic);

            DynamicArguments = dynamic;

            if (args?.Any() == true || dynamic)
            {
                Arguments = new ObservableCollection<ExpressionVariableArgumentDescriptor>(args ?? new ExpressionVariableArgumentDescriptor[0]);

                if (variable.Arguments?.Length > Arguments.Count && dynamic)
                {
                    for (int i = Arguments.Count; i < variable.Arguments.Length; i++)
                    {
                        Arguments.Add(NewDynamicArgument());
                    }
                }

                for (int i = 0; i < Arguments.Count; i++)
                {
                    var arg = Arguments[i];

                    if (variable.Arguments != null && variable.Arguments.Length > i)
                    {
                        arg.SetToken(variable.Arguments[i], Owner.GetExpressionParser());
                    }
                }

                if (DynamicArguments)
                {
                    ResolveDynamicArgumentNames();
                    Arguments.CollectionChanged += OnDynamicArgumentsChanged;
                }
            }
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
            nameof(Icon), typeof(ImageSource),
            typeof(ExpressionVariableTokenControl));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
    }

    public class ExpressionBox : Control
    {
        internal event Action<ExpressionBox, IExpressionVariableDescriptor> VariableDescriptorChanged;

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

                return variable.Arguments?.Select(a => new ExpressionVariableArgumentExpressionDescriptor()).ToArray();
            }

            public Brush GetBackground(IExpressionTokenVariable variable) => new SolidColorBrush(Color.FromRgb(221, 221, 221));

            public ImageSource GetIcon(IExpressionTokenVariable variable) => null;

            public string GetTitle(IExpressionTokenVariable variable) => variable.Name;

            public string GetDescription(IExpressionTokenVariable variable) => variable.Name;
        }

        private RichTextBox m_TextBox;
        private FlowDocument m_Doc;
        private PopupMenu m_VariableLinksMenu;

        private readonly InternalChangeTracker m_InternalChangeTracker;

        private bool m_IsInit;

        public ICommand InsertVariableCommand { get; }

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

        private IExpressionVariableDescriptor m_DefaultVariableDescriptor;
        private IExpressionParser m_DefaultParser;

        public ExpressionBox() 
        {
            m_InternalChangeTracker = new InternalChangeTracker();

            InsertVariableCommand = new RelayCommand<IExpressionVariableLink>(InsertVariable);
        }

        public override void OnApplyTemplate()
        {
            m_TextBox = (RichTextBox)this.Template.FindName("PART_RichTextBox", this);
            m_VariableLinksMenu = (PopupMenu)this.Template.FindName("PART_VariableLinksMenu", this);

            m_Doc = m_TextBox.Document;
            m_TextBox.TextChanged += OnTextChanged;

            m_IsInit = true;

            DataObject.AddCopyingHandler(m_TextBox, OnCopy);
            DataObject.AddPastingHandler(m_TextBox, OnPaste);

            RenderExpression(Expression);
        }

        private void InsertVariable(IExpressionVariableLink link)
        {
            var var = link.Factory.Invoke(this);

            if (var != null) 
            {
                Insert(var, link.EnterArguments);
                m_VariableLinksMenu.IsOpen = false;
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

            var parser = GetExpressionParser();

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

            var parser = GetExpressionParser();

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

                UpdateExpression();

                m_TextBox.CaretPosition = pos;

                m_TextBox.Focus();
            }
        }

        public static readonly DependencyProperty ExpressionParserProperty =
            DependencyProperty.Register(
            nameof(ExpressionParser), typeof(IExpressionParser),
            typeof(ExpressionBox));

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
            typeof(ExpressionBox), new PropertyMetadata(OnVariableDescriptorChanged));

        private static void OnVariableDescriptorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExpressionBox)d).RaiseVariableDescriptorChanged((IExpressionVariableDescriptor)e.NewValue);
        }

        private void RaiseVariableDescriptorChanged(IExpressionVariableDescriptor descriptor) 
        {
            VariableDescriptorChanged?.Invoke(this, descriptor);
        }

        public IExpressionVariableDescriptor VariableDescriptor
        {
            get { return (IExpressionVariableDescriptor)GetValue(VariableDescriptorProperty); }
            set { SetValue(VariableDescriptorProperty, value); }
        }

        public static readonly DependencyProperty VariableLinksProperty =
            DependencyProperty.Register(
            nameof(VariableLinks), typeof(Collection<IExpressionVariableLink>),
            typeof(ExpressionBox));

        public Collection<IExpressionVariableLink> VariableLinks
        {
            get { return (Collection<IExpressionVariableLink>)GetValue(VariableLinksProperty); }
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

        public static readonly DependencyProperty VariableLinksMenuItemTemplateProperty =
            DependencyProperty.Register(
            nameof(VariableLinksMenuItemTemplate), typeof(DataTemplate),
            typeof(ExpressionBox),
            new PropertyMetadata(typeof(ExpressionBox).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "VariableLinksMenuItemTemplate")));

        public DataTemplate VariableLinksMenuItemTemplate
        {
            get { return (DataTemplate)GetValue(VariableLinksMenuItemTemplateProperty); }
            set { SetValue(VariableLinksMenuItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty NewArgumentItemTemplateProperty =
            DependencyProperty.Register(
            nameof(NewArgumentItemTemplate), typeof(ControlTemplate),
            typeof(ExpressionBox),
            new PropertyMetadata(typeof(ExpressionBox).Assembly.LoadFromResources<ControlTemplate>("Themes/Generic.xaml", "NewArgumentItemTemplate")));

        public ControlTemplate NewArgumentItemTemplate
        {
            get { return (ControlTemplate)GetValue(NewArgumentItemTemplateProperty); }
            set { SetValue(NewArgumentItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ArgumentLabelTemplateProperty =
            DependencyProperty.Register(
            nameof(ArgumentLabelTemplate), typeof(DataTemplate),
            typeof(ExpressionBox),
            new PropertyMetadata(typeof(ExpressionBox).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "ArgumentLabelTemplate")));

        public DataTemplate ArgumentLabelTemplate
        {
            get { return (DataTemplate)GetValue(ArgumentLabelTemplateProperty); }
            set { SetValue(ArgumentLabelTemplateProperty, value); }
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
            if (e.Changes.Any())
            {
                if (!m_InternalChangeTracker.IsInternalChange)
                {
                    using (var intChange = m_InternalChangeTracker.PerformInternalChange())
                    {
                        UpdateExpression();
                    }
                }
            }
        }

        private void UpdateExpression()
        {
            var parser = GetExpressionParser();
            
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

                    var parser = GetExpressionParser();

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
                    var varCtrl = new ExpressionVariableTokenControl(this, variable);
                    varCtrl.VariableUpdated += OnVariableUpdated;
                    var uiCont = new InlineUIContainer(varCtrl, pos) 
                    {
                        BaselineAlignment = BaselineAlignment.Center 
                    };
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

        private void OnVariableUpdated(ExpressionVariableTokenControl sender)
        {
            UpdateExpression();
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

        internal IExpressionVariableDescriptor GetVariableDescriptor() 
        {
            var desc = VariableDescriptor;

            if (desc == null) 
            {
                desc = m_DefaultVariableDescriptor ?? (m_DefaultVariableDescriptor = new DefaultExpressionVariableDescriptor());
            }

            return desc;
        }

        internal IExpressionParser GetExpressionParser()
        {
            var parser = ExpressionParser;

            if (parser == null)
            {
                parser = m_DefaultParser ?? (m_DefaultParser = new ExpressionParser());
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
