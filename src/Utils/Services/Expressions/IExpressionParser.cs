using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xarial.XToolkit.Services.Expressions.Exceptions;

namespace Xarial.XToolkit.Services.Expressions
{
    public interface IExpressionParser
    {
        IExpressionElement Parse(string expression);
        string CreateExpression(IExpressionElement element);
    }

    public class ExpressionParser : IExpressionParser
    {
        private enum ExpressionElementType_e
        {
            Variable,
            FreeText
        }

        private readonly char m_VariableStartTag;
        private readonly char m_VariableEndTag;
        private readonly char m_ArgumentStartTag;
        private readonly char m_ArgumentEndTag;
        private readonly char m_EscapeSymbol;

        private readonly char[] m_SpecialSymbols;

        public ExpressionParser(char variableStartTag, char variableEndTag, char argumentStartTag, char argumentEndTag, char escapeSymbol)
        {
            m_VariableStartTag = variableStartTag;
            m_VariableEndTag = variableEndTag;
            m_ArgumentStartTag = argumentStartTag;
            m_ArgumentEndTag = argumentEndTag;
            m_EscapeSymbol = escapeSymbol;

            m_SpecialSymbols = new char[]
            {
                m_VariableStartTag,
                m_VariableEndTag,
                m_ArgumentStartTag,
                m_ArgumentEndTag,
                m_EscapeSymbol
            };
        }

        public ExpressionParser() : this('{', '}', '[', ']', '\\')
        {
        }

        public IExpressionElement Parse(string expression)
        {
            var startPos = 0;
            return GroupElements(EnumerateElements(expression, ref startPos, false));
        }

        public string CreateExpression(IExpressionElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var expression = new StringBuilder();

            AppendExpressionElement(element, expression);

            return expression.ToString();
        }

        private void AppendExpressionElement(IExpressionElement element, StringBuilder expression)
        {
            switch (element)
            {
                case IExpressionElementGroup group:
                    foreach (var child in group.Children) 
                    {
                        AppendExpressionElement(child, expression);
                    }
                    break;

                case IExpressionFreeTextElement freeText:
                    AppendText(freeText.Text, expression);
                    break;

                case IExpressionVariableElement variable:
                    
                    expression.Append(m_VariableStartTag);
                    expression.Append(" ");
                    AppendText(variable.Name, expression);
                                        
                    if (variable.Arguments != null)
                    {
                        foreach (var arg in variable.Arguments)
                        {
                            expression.Append(" ");
                            expression.Append(m_ArgumentStartTag);
                            AppendExpressionElement(arg, expression);
                            expression.Append(m_ArgumentEndTag);
                        }
                    }
                    expression.Append(" ");
                    expression.Append(m_VariableEndTag);

                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void AppendText(string text, StringBuilder expression) 
        {
            foreach (var symb in text) 
            {
                if (m_SpecialSymbols.Contains(symb)) 
                {
                    expression.Append(m_EscapeSymbol);
                }

                expression.Append(symb);
            }
        }

        private IReadOnlyList<IExpressionElement> EnumerateElements(string expression, ref int position, bool isArgumentParsing) 
        {
            var elements = new List<IExpressionElement>();
            var nested = new List<IExpressionElement>();

            var currentElementType = default(ExpressionElementType_e?);
            var currentElementContent = new StringBuilder();

            bool varHasSpace = false;

            bool isProtected = false;

            void CloseCurrentElement()
            {
                if (currentElementType.HasValue)
                {
                    IExpressionElement newElem;

                    switch (currentElementType.Value)
                    {
                        case ExpressionElementType_e.Variable:
                            newElem = new ExpressionVariableElement(currentElementContent.ToString().Trim(), nested?.ToArray());
                            break;

                        case ExpressionElementType_e.FreeText:
                            newElem = new ExpressionFreeTextElement(currentElementContent.ToString());
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    currentElementType = null;
                    currentElementContent.Clear();
                    nested.Clear();
                    varHasSpace = false;
                    isProtected = false;

                    elements.Add(newElem);
                }
            }

            for (; position < expression.Length; position++) 
            {
                var thisChar = expression[position];

                if (!isProtected && thisChar == m_VariableStartTag)
                {
                    if (currentElementType.HasValue)
                    {
                        if (currentElementType == ExpressionElementType_e.Variable)
                        {
                            throw new NestedVariableOutOfArgumentException(m_ArgumentStartTag, m_ArgumentEndTag);
                        }

                        CloseCurrentElement();
                    }

                    currentElementType = ExpressionElementType_e.Variable;
                }
                else if (!isProtected && thisChar == m_VariableEndTag)
                {
                    CloseCurrentElement();
                }
                else if (!isProtected && thisChar == m_ArgumentStartTag)
                {
                    if (currentElementType == ExpressionElementType_e.Variable)
                    {
                        position++;

                        nested.Add(GroupElements(EnumerateElements(expression, ref position, true)));
                    }
                    else
                    {
                        throw new ArgumentOutOfVariableException();
                    }
                }
                else if (!isProtected && thisChar == m_ArgumentEndTag)
                {
                    if (isArgumentParsing) 
                    {
                        CloseCurrentElement();
                        return elements;
                    }
                    else
                    {
                        throw new MissingArgumentOpeningTagException(m_ArgumentStartTag);
                    }
                }
                else if (!isProtected && thisChar == m_EscapeSymbol)
                {
                    isProtected = true;
                }
                else 
                {
                    isProtected = false;

                    if (!currentElementType.HasValue)
                    {
                        currentElementType = ExpressionElementType_e.FreeText;
                    }

                    if (currentElementType == ExpressionElementType_e.Variable) 
                    {
                        if (thisChar == ' ') 
                        {
                            if (currentElementContent.Length != 0)
                            {
                                varHasSpace = true;
                            }

                            continue;
                        }
                        else
                        {
                            if (varHasSpace) 
                            {
                                throw new VariableNameSpaceNotSupportedException();
                            }
                        }
                    }

                    currentElementContent.Append(thisChar);
                }
            }

            if (currentElementType.HasValue)
            {
                if (currentElementType == ExpressionElementType_e.FreeText)
                {
                    CloseCurrentElement();
                }
                else
                {
                    throw new NotClosedVariableOrParameterException();
                }
            }

            return elements;
        }

        private IExpressionElement GroupElements(IReadOnlyList<IExpressionElement> elements)
        {
            if (elements.Count == 1)
            {
                return elements.First();
            }
            else if (elements.Count > 1)
            {
                return new ExpressionElementGroup(elements.ToArray());
            }
            else
            {
                return new ExpressionFreeTextElement("");
            }
        }
    }
}
