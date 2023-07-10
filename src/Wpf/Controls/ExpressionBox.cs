//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
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
using static System.Windows.Forms.LinkLabel;

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

    //NOTE: IValueConverter on just the HasAdvancedEditor does not work and for the NewItemPlaceholder (e.g. new row header) the advanced editor toggle would be always shown
    public class AdvancedEditorVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values[0] is ExpressionVariableArgumentDescriptor && values[1] is bool && (bool)values[1] == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SupportsArgumentsVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var args = (IEnumerable)values[0];
            var dynArgs = (bool)values[1];

            return (dynArgs || (args != null && args.GetEnumerator().MoveNext())) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ExpressionVariableTokenControl : Control, INotifyPropertyChanged
    {
        static ExpressionVariableTokenControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpressionVariableTokenControl),
                new FrameworkPropertyMetadata(typeof(ExpressionVariableTokenControl)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal event Action<ExpressionVariableTokenControl> VariableUpdated;
        internal event Action<ExpressionVariableTokenControl> EditingStarted;
        internal event Action<ExpressionVariableTokenControl> EditingCompleted;

        public ICommand ApplyRowChangesCommand { get; }
        public ICommand CancelRowChangesCommand { get; }

        private FrameworkElement m_Main;
        private FrameworkElement m_Error;
        private DataGrid m_DataGrid;
        private PopupMenu m_PopupEditor;
        private TextBlock m_ClosePopupButton;

        private string m_VariableName;
        public IExpressionTokenVariable Variable 
        {
            get => m_Variable;
            private set 
            {
                m_Variable = value;
                this.NotifyChanged();
                this.NotifyChanged(nameof(AllowEditName));
            }
        }

        private bool m_DynamicArguments;
        private ObservableCollection<ExpressionVariableArgumentDescriptor> m_Arguments;

        private bool m_Edit;
        public bool AllowEditName => Variable is IExpressionTokenCustomVariable;

        private Exception m_Exception;

        public string VariableName 
        {
            get => m_VariableName;
            set 
            {
                m_VariableName = value;
                this.NotifyChanged();
            }
        }

        private IExpressionTokenVariable m_Variable;

        internal ExpressionVariableTokenControl(ExpressionBox owner, IExpressionTokenVariable variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            Owner = owner;

            Owner.VariableDescriptorChanged += OnVariableDescriptorChanged;

            Variable = variable;

            VariableName = variable.Name;

            ApplyRowChangesCommand = new RelayCommand(ApplyRowChanges);
            CancelRowChangesCommand = new RelayCommand(CancelRowChanges);

            UpdateVariableControl(variable);
            UpdateVariableArguments(variable);
        }

        public override void OnApplyTemplate()
        {
            m_DataGrid = (DataGrid)this.Template.FindName("PART_DataGrid", this);
            m_PopupEditor = (PopupMenu)this.Template.FindName("PART_PopupEditor", this);
            m_Main = (FrameworkElement)this.Template.FindName("PART_Main", this);
            m_Error = (FrameworkElement)this.Template.FindName("PART_Error", this);
            m_ClosePopupButton = (TextBlock)this.Template.FindName("PART_CloseArgumentsWindow", this);
            m_ClosePopupButton.MouseDown += OnClosePopupButtonMouseDown;

            m_PopupEditor.Opened += OnPopupEditorOpened;
            m_PopupEditor.Closed += OnClosed;

            m_DataGrid.AddingNewItem += OnAddingNewItem;

            SetError(m_Exception);

            if (m_Exception == null)
            {
                if (m_Edit)
                {
                    Edit();
                    m_Edit = false;
                }
            }
        }

        private void ApplyRowChanges()
        {
            m_DataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        private void CancelRowChanges()
        {
            m_DataGrid.CancelEdit(DataGridEditingUnit.Row);
        }

        private void OnClosePopupButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            Commit();
        }

        public void Edit() 
        {
            if (Arguments != null || AllowEditName)
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

        internal bool IsEditing
        {
            get => m_PopupEditor.IsOpen;
            set => m_PopupEditor.IsOpen = value;
        }

        internal void Commit()
        {
            CommitChanges();

            IsEditing = false;
        }

        private void OnPopupEditorOpened(PopupMenu sender)
        {
            m_DataGrid.Focus();
            EditingStarted?.Invoke(this);
        }

        private void OnClosed(PopupMenu popupMenu)
        {
            CommitChanges();

            EditingCompleted?.Invoke(this);
        }

        private void CommitChanges()
        {
            var oldVar = Variable;

            Variable = new ExpressionTokenVariable(VariableName, Arguments?.Select(a => a.GetToken(Owner.GetExpressionParser())).ToArray());

            if (oldVar is IExpressionTokenCustomVariable || !Variable.IsSame(oldVar))
            {
                UpdateVariableControl(Variable);

                VariableUpdated?.Invoke(this);
            }
        }

        private void OnAddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            var argDesc = NewDynamicArgument();
            e.NewItem = argDesc;
        }

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

        public bool DynamicArguments
        {
            get => m_DynamicArguments;
            private set
            {
                m_DynamicArguments = value;
                this.NotifyChanged();
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

        private void OnVariableDescriptorChanged(ExpressionBox sender, IExpressionVariableDescriptor varsDesc)
        {
            UpdateVariableArguments(Variable);
            UpdateVariableControl(Variable);
        }

        private ExpressionVariableArgumentExpressionDescriptor NewDynamicArgument()
            => new ExpressionVariableArgumentExpressionDescriptor("", "", null);

        private void UpdateVariableControl(IExpressionTokenVariable variable)
        {
            try
            {
                var descriptor = Owner.GetVariableDescriptor();

                Title = descriptor.GetTitle(variable);
                Icon = descriptor.GetIcon(variable);
                ToolTip = descriptor.GetDescription(variable);
                Background = descriptor.GetBackground(variable);

                SetError(null);
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
        }

        private void UpdateVariableArguments(IExpressionTokenVariable variable)
        {
            try
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

                SetError(null);
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
        }

        private void SetError(Exception ex)
        {
            m_Exception = ex;

            if (m_Main != null && m_Error != null)
            {
                if (ex != null)
                {
                    m_Main.Visibility = Visibility.Collapsed;
                    m_Error.Visibility = Visibility.Visible;
                    m_Error.ToolTip = ex.ParseUserError(out _, $"Variable '{Variable?.Name}' error");
                }
                else
                {
                    m_Main.Visibility = Visibility.Visible;
                    m_Error.Visibility = Visibility.Collapsed;
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

                return variable.Arguments?.Select(a => new ExpressionVariableArgumentExpressionDescriptor()).ToArray();
            }

            public Brush GetBackground(IExpressionTokenVariable variable) => new SolidColorBrush(Color.FromRgb(221, 221, 221));

            public ImageSource GetIcon(IExpressionTokenVariable variable) => null;

            public string GetTitle(IExpressionTokenVariable variable) => variable.Name;

            public string GetDescription(IExpressionTokenVariable variable) => variable.Name;
        }

        internal event Action<ExpressionBox, IExpressionVariableDescriptor> VariableDescriptorChanged;

        private RichTextBox m_TextBox;
        private FlowDocument m_Doc;
        private PopupMenu m_VariableLinksMenu;

        private readonly InternalChangeTracker m_InternalChangeTracker;

        private bool m_IsInit;

        private IExpressionVariableDescriptor m_DefaultVariableDescriptor;
        private IExpressionParser m_DefaultParser;

        private string m_CachedText;
        private int m_CachedVariablesCount;

        private ExpressionVariableTokenControl m_EditingVariable;

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

            SetSingleLineOptions(SingleLine);

            m_IsInit = true;

            CommandManager.AddPreviewExecutedHandler(m_TextBox, OnPreviewCommandExecuted);

            RenderExpression(Expression);
        }

        private void OnPreviewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Cut)
            {
                if (CopySelectionToClipboard())
                {
                    m_TextBox.Selection.Text = "";
                    e.Handled = true;
                }
            }
            else if (e.Command == ApplicationCommands.Copy) 
            {
                if (CopySelectionToClipboard())
                {
                    e.Handled = true;
                }
            }
            else if (e.Command == ApplicationCommands.Paste)
            {
                if (PasteFromClipboard())
                {
                    e.Handled = true;
                }
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

                if (enterArgs || expressionToken is IExpressionTokenCustomVariable)
                {
                    var cont = inlines.OfType<InlineUIContainer>().FirstOrDefault();

                    if (cont?.Child is ExpressionVariableTokenControl)
                    {
                        var varCtrl = (ExpressionVariableTokenControl)cont.Child;

                        varCtrl.Edit();
                    }
                }

                UpdateExpression();

                m_CachedText = GetCachedText();
                m_CachedVariablesCount = GetCachedVariablesCount();

                m_TextBox.CaretPosition = pos;

                m_TextBox.Focus();
            }
        }

        public void CommitEditingVariable()
        {
            if (m_EditingVariable != null)
            {
                m_EditingVariable.Commit();
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

        public static readonly DependencyProperty VariableLinksBoxDecorationTemplateProperty =
            DependencyProperty.Register(
            nameof(VariableLinksBoxDecorationTemplate), typeof(DataTemplate),
            typeof(ExpressionBox),
            new PropertyMetadata(typeof(ExpressionBox).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "VariableLinksBoxDecorationTemplate")));

        public DataTemplate VariableLinksBoxDecorationTemplate
        {
            get { return (DataTemplate)GetValue(VariableLinksBoxDecorationTemplateProperty); }
            set { SetValue(VariableLinksBoxDecorationTemplateProperty, value); }
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
            nameof(NewArgumentItemTemplate), typeof(DataTemplate),
            typeof(ExpressionBox),
            new PropertyMetadata(typeof(ExpressionBox).Assembly.LoadFromResources<DataTemplate>("Themes/Generic.xaml", "NewArgumentItemTemplate")));

        public DataTemplate NewArgumentItemTemplate
        {
            get { return (DataTemplate)GetValue(NewArgumentItemTemplateProperty); }
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

        public static readonly DependencyProperty VariableLinksBoxVisibilityProperty =
            DependencyProperty.Register(
            nameof(VariableLinksBoxVisibility), typeof(Visibility),
            typeof(ExpressionBox), new PropertyMetadata(Visibility.Visible));

        public Visibility VariableLinksBoxVisibility
        {
            get { return (Visibility)GetValue(VariableLinksBoxVisibilityProperty); }
            set { SetValue(VariableLinksBoxVisibilityProperty, value); }
        }

        public static readonly DependencyProperty SingleLineProperty =
            DependencyProperty.Register(
            nameof(SingleLine), typeof(bool),
            typeof(ExpressionBox), new PropertyMetadata(true, new PropertyChangedCallback(OnSingleLinePropertyChanged)));

        public bool SingleLine
        {
            get { return (bool)GetValue(SingleLineProperty); }
            set { SetValue(SingleLineProperty, value); }
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
                var cachedText = GetCachedText();
                var cachedVarsCount = GetCachedVariablesCount();

                //NOTE: cached text will have variables replaced with empty string whch means if the varible replaced with the empty string in the UI - then text will not be changed
                if (!string.Equals(m_CachedText, cachedText) || cachedVarsCount != m_CachedVariablesCount)
                {
                    m_CachedText = cachedText;
                    m_CachedVariablesCount = cachedVarsCount;

                    if (!m_InternalChangeTracker.IsInternalChange)
                    {
                        using (var intChange = m_InternalChangeTracker.PerformInternalChange())
                        {
                            UpdateExpression();
                        }
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

        private string GetCachedText() => new TextRange(m_Doc.ContentStart, m_Doc.ContentEnd).Text;

        private int GetCachedVariablesCount() => Inlines.OfType<InlineUIContainer>().Count();

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
                    varCtrl.EditingStarted += OnVariableEditingStarted;
                    varCtrl.EditingCompleted += OnVariableEditingCompleted;

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

        private void OnVariableEditingStarted(ExpressionVariableTokenControl sender)
        {
            m_EditingVariable = sender;
        }

        private void OnVariableEditingCompleted(ExpressionVariableTokenControl sender)
        {
            m_EditingVariable = null;
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
                        return new ExpressionTokenText("");
                    }

                default:
                    throw new NotSupportedException("Not supported inline");
            }
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

        private bool CopySelectionToClipboard() 
        {
            if (m_EditingVariable == null)//if variable is editing no need to handle the copying as it will be hijacked
            {
                IEnumerable<Tuple<Inline, int?, int?>> GetSelectedInlines()
                {
                    var sel = m_TextBox.Selection;

                    if (!sel.IsEmpty)
                    {
                        foreach (var inline in Inlines)
                        {
                            if (sel.Start.CompareTo(inline.ContentEnd) < 0 && sel.End.CompareTo(inline.ContentStart) > 0)
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

                return true;
            }
            else 
            {
                return false;
            }
        }

        private bool PasteFromClipboard() 
        {
            if (m_EditingVariable == null)//if variable is editing no need to handle the pasting as it will be hijacked
            {
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

                return true;
            }
            else 
            {
                return false;
            }
        }

        private void SetExpressionError(Exception ex)
        {
            var bindingExpression = this.GetBindingExpression(ExpressionProperty);

            if (bindingExpression != null)
            {
                var validationError = new ValidationError(new DataErrorValidationRule(), bindingExpression);
                validationError.ErrorContent = ex.ParseUserError(out _, "Expression parsing error");

                Validation.MarkInvalid(bindingExpression, validationError);
            }
        }

        private void SetSingleLineOptions(bool singleLine)
        {
            var width = double.NaN;

            if (singleLine)
            {
                width = 2000;
            }

            m_TextBox.Document.PageWidth = width;
        }

        private static void OnSingleLinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var exprBox = (ExpressionBox)d;

            if (exprBox.m_IsInit)
            {
                exprBox.SetSingleLineOptions((bool)e.NewValue);
            }
        }
    }
}
