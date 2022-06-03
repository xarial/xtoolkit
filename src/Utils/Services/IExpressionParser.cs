using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Xarial.XToolkit.Services
{
    public interface IExpressionElement
    { 
    }

    public interface IExpressionFreeTextElement : IExpressionElement
    {
        string Text { get; }
    }

    [DebuggerDisplay("Free Text: '{" + nameof(Text) + "}'")]
    public class ExpressionFreeTextElement : IExpressionFreeTextElement
    {
        public string Text { get; }

        public ExpressionFreeTextElement(string text) 
        {
            Text = text;
        }
    }

    public interface IExpressionVariableElement : IExpressionElement
    {
        string Name { get; }
        IExpressionElement[] Arguments { get; }
    }

    [DebuggerDisplay("Variable: '{" + nameof(Name) + "}'")]
    public class ExpressionVariableElement : IExpressionVariableElement
    {
        public string Name { get; }
        public IExpressionElement[] Arguments { get; }

        public ExpressionVariableElement(string name, IExpressionElement[] args) 
        {
            Name = name;
            Arguments = args;
        }
    }

    public interface IExpressionElementGroup : IExpressionElement 
    {
        IExpressionElement[] Children { get; }
    }

    public class ExpressionElementGroup : IExpressionElementGroup
    {
        public IExpressionElement[] Children { get; }

        public ExpressionElementGroup(IExpressionElement[] children) 
        {
            Children = children;
        }
    }

    public interface IExpressionParser
    {
        IExpressionElement Parse(string expression);
    }

    public class ExpressionParser : IExpressionParser
    {
        private enum ExpressionElementType_e
        {
            Variable,
            FreeText
        }

        private readonly char m_VaribleStartTag;
        private readonly char m_VariableEndTag;
        private readonly char m_ArgumentStartTag;
        private readonly char m_ArgumentEndTag;
        private readonly char m_ProtectionSymbol;

        public ExpressionParser(char varibleStartTag, char variableEndTag, char argumentStartTag, char argumentEndTag, char protectionSymbol)
        {
            m_VaribleStartTag = varibleStartTag;
            m_VariableEndTag = variableEndTag;
            m_ArgumentStartTag = argumentStartTag;
            m_ArgumentEndTag = argumentEndTag;
            m_ProtectionSymbol = protectionSymbol;
        }

        public ExpressionParser() : this('{', '}', '[', ']', '\\')
        {
        }

        public IExpressionElement Parse(string expression)
        {
            var startPos = 0;
            return GroupElements(EnumerateElements(expression, ref startPos, false));
        }

        private IReadOnlyList<IExpressionElement> EnumerateElements(string expression, ref int position, bool isArgumentParsing) 
        {
            var elements = new List<IExpressionElement>();
            var nested = new List<IExpressionElement>();

            var currentElementType = default(ExpressionElementType_e?);
            var currentElementContent = new StringBuilder();

            bool varHasSpace = false;

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

                    elements.Add(newElem);
                }
            }

            for (; position < expression.Length; position++) 
            {
                var thisChar = expression[position];

                if (thisChar == m_VaribleStartTag)
                {
                    if (currentElementType.HasValue)
                    {
                        if (currentElementType == ExpressionElementType_e.Variable)
                        {
                            throw new Exception($"Nested variables can only be used as arguments. Enclose variable into '{m_ArgumentStartTag}{m_ArgumentEndTag}'");
                        }

                        CloseCurrentElement();
                    }

                    currentElementType = ExpressionElementType_e.Variable;
                }
                else if (thisChar == m_VariableEndTag)
                {
                    CloseCurrentElement();
                }
                else if (thisChar == m_ArgumentStartTag)
                {
                    if (currentElementType == ExpressionElementType_e.Variable)
                    {
                        position++;

                        nested.Add(GroupElements(EnumerateElements(expression, ref position, true)));
                    }
                    else
                    {
                        throw new Exception("Argument can only appear within the variable");
                    }
                }
                else if (thisChar == m_ArgumentEndTag)
                {
                    if (isArgumentParsing) 
                    {
                        CloseCurrentElement();
                        return elements;
                    }
                    else
                    {
                        throw new Exception($"Argument is missing the opening tag '{m_ArgumentStartTag}'");
                    }
                }
                else if (thisChar == m_ProtectionSymbol)
                {
                    //TODO: take next symbol litelarly
                }
                else 
                {
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
                                throw new Exception("Variables cannot have space");
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
                    throw new Exception("Variable or parameter is not closed");
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
