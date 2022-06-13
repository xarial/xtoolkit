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

namespace Xarial.XToolkit.Wpf.Controls
{
    public class ExpressionVariableTokenControl : Control
    {
        static ExpressionVariableTokenControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpressionVariableTokenControl),
                new FrameworkPropertyMetadata(typeof(ExpressionVariableTokenControl)));
        }

        internal IExpressionTokenVariable Variable { get; }

        internal ExpressionVariableTokenControl(IExpressionTokenVariable variable) 
        {
            Variable = variable;
        }

        public override void OnApplyTemplate()
        {
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
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 171, 173, 179))));
        }

        public ExpressionBox() 
        {
            m_InternalChangeTracker = new InternalChangeTracker();
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

                Insert(token);
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
            var size = base.MeasureOverride(constraint);
            return new Size(size.Width, size.Height + 4);
        }

        public void Insert(IExpressionToken expressionToken) 
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

                AddExpressionToken(Inlines, expressionToken, ref pos);

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
        private void AddExpressionToken(InlineCollection inlines, IExpressionToken expressionToken, ref TextPointer pos) 
        {
            switch (expressionToken)
            {
                case IExpressionTokenVariable variable:
                    var uiCont = new InlineUIContainer(new ExpressionVariableTokenControl(variable)
                    {
                        Title = variable.Name
                    }, pos);
                    pos = uiCont.ContentEnd.GetInsertionPosition(LogicalDirection.Forward);
                    break;

                case IExpressionTokenText text:
                    var run = new Run(text.Text, pos);
                    pos = run.ContentEnd.GetInsertionPosition(LogicalDirection.Forward);
                    break;

                case IExpressionTokenGroup group:
                    if (group.Children != null)
                    {
                        foreach (var child in group.Children)
                        {
                            AddExpressionToken(inlines, child, ref pos);
                        }
                    }
                    break;

                default:
                    throw new NotSupportedException("Expression token is not supported");
            }
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
