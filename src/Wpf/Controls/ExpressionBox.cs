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
        }

        public ExpressionBox() 
        {
            m_InternalChangeTracker = new InternalChangeTracker();
        }

        private void OnCopy(object sender, DataObjectCopyingEventArgs e)
        {
            if (e.DataObject != null)
            {
                e.DataObject.SetData(DataFormats.UnicodeText, "XYZ");
                e.Handled = true;
            }
        }

        public override void OnApplyTemplate()
        {
            m_TextBox = (RichTextBox)this.Template.FindName("PART_RichTextBox", this);
            m_Doc = m_TextBox.Document;
            m_TextBox.TextChanged += OnTextChanged;
            m_IsInit = true;

            DataObject.AddCopyingHandler(m_TextBox, OnCopy);

            RenderExpression(Expression);
        }

        public void Insert(IExpressionToken expressionToken) 
        {
            var pos = m_TextBox.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);

            using (var intChange = m_InternalChangeTracker.PerformInternalChange()) 
            {
                AddExpressionToken(Inlines, expressionToken, ref pos);

                UpdateExpression();
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
            typeof(ExpressionBox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnExpressionChanged));

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
                        LineHeight = 1,
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
            var parser = ExpressionParser;

            if (parser == null)
            {
                throw new NullReferenceException("Expression parser is not set");
            }
            
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
                    var parser = ExpressionParser;

                    if (parser == null)
                    {
                        throw new NullReferenceException("Expression parser is not set");
                    }

                    var expressionToken = parser.Parse(expression);

                    m_Doc.Blocks.Clear();

                    var pos = m_Doc.ContentStart;

                    AddExpressionToken(Inlines, expressionToken, ref pos);
                }
            }
        }

        private void AddExpressionToken(InlineCollection inlines, IExpressionToken expressionToken, ref TextPointer pos) 
        {
            switch (expressionToken)
            {
                case IExpressionTokenVariable variable:
                    inlines.Add(new InlineUIContainer(new ExpressionVariableTokenControl(variable)
                    {
                        Title = variable.Name
                    }, pos));
                    //pos = pos.GetPositionAtOffset(1);
                    break;

                case IExpressionTokenText text:
                    inlines.Add(new Run(text.Text, pos));
                    //pos = pos.GetPositionAtOffset(text.Text.Length);
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

        private IExpressionToken GetExpressionToken(InlineCollection inlines) 
        {
            var tokens = new List<IExpressionToken>();

            foreach (var inline in inlines) 
            {
                switch (inline) 
                {
                    case Run run:
                        tokens.Add(new ExpressionTokenText(run.Text));
                        break;

                    case InlineUIContainer cont:
                        if (cont.Child is ExpressionVariableTokenControl)
                        {
                            var varCtrl = (ExpressionVariableTokenControl)cont.Child;
                            tokens.Add(varCtrl.Variable);
                        }
                        else 
                        {
                            throw new Exception("Not supported UI token");
                        }
                        break;
                }
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
    }
}
