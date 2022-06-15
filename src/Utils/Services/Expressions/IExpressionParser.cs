using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xarial.XToolkit.Services.Expressions.Exceptions;

namespace Xarial.XToolkit.Services.Expressions
{
    /// <summary>
    /// Service allows to parse expression into the <see cref="IExpressionToken"/>
    /// </summary>
    public interface IExpressionParser
    {
        /// <summary>
        /// Parses tokens from the expression
        /// </summary>
        /// <param name="expression">Text expression</param>
        /// <returns>Token</returns>
        IExpressionToken Parse(string expression);

        /// <summary>
        /// Creates expression from the token
        /// </summary>
        /// <param name="token">Token to create expression from</param>
        /// <returns>Expression</returns>
        string CreateExpression(IExpressionToken token);
    }

    /// <summary>
    /// Default expression parses allows to specifying chars for the start and end tags of the variable, arguments and escape symbol
    /// </summary>
    public class ExpressionParser : IExpressionParser
    {
        private enum ExpressionTokenType_e
        {
            Variable,
            Text
        }

        /// <summary>
        /// Represents the information about the currently parsing token
        /// </summary>
        private class ParsingState
        {
            /// <summary>
            /// List of tokens collected for the current variables=
            /// </summary>
            internal List<IExpressionToken> VariableNestedTokens { get; }

            /// <summary>
            /// Type of current token or null if not yet set
            /// </summary>
            internal ExpressionTokenType_e? TokenType { get; set; }

            /// <summary>
            /// Content of the current token
            /// </summary>
            internal StringBuilder TokenContent { get; }

            /// <summary>
            /// Buffer of spaces from the current variable name
            /// </summary>
            /// <remarks>This is used to allow space in variable name but atuomatically trim the variable name and avoid excessive Trim operation.
            /// This buffer wil lonly be added if another non empty char is identified in the variable name, e.g. this space buffer is between words of the variable name</remarks>
            internal StringBuilder TokenNameSpaceBuffer { get; }

            /// <summary>
            /// Sets if the next char is protected and has to be considered as literal (even if it is a special symbol)
            /// </summary>
            internal bool CharProtected { get; set; }

            /// <summary>
            /// Indicates that the variable name was parsed (e.g. first argument encountered). This is used to handle the case where the text within variable appears after the arguments
            /// </summary>
            internal bool IsVariableNameParsed { get; set; }

            internal ParsingState()
            {
                VariableNestedTokens = new List<IExpressionToken>();

                TokenType = default;
                TokenContent = new StringBuilder();

                TokenNameSpaceBuffer = new StringBuilder();

                CharProtected = false;

                IsVariableNameParsed = false;
            }
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

        public IExpressionToken Parse(string expression)
        {
            var startPos = 0;
            return GroupElements(ParseTokens(expression, ref startPos, false));
        }

        public string CreateExpression(IExpressionToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            var expression = new StringBuilder();

            AppendExpressionToken(token, expression);

            return expression.ToString();
        }

        private void AppendExpressionToken(IExpressionToken token, StringBuilder expression)
        {
            switch (token)
            {
                case IExpressionTokenGroup group:
                    foreach (var child in group.Children) 
                    {
                        AppendExpressionToken(child, expression);
                    }
                    break;

                case IExpressionTokenText text:
                    AppendText(text.Text, expression);
                    break;

                case IExpressionTokenVariable variable:
                    
                    expression.Append(m_VariableStartTag);
                    expression.Append(" ");
                    if (variable.Name.StartsWith(" ")) 
                    {
                        expression.Append(m_EscapeSymbol);
                    }
                    AppendText(variable.Name, expression);
                                        
                    if (variable.Arguments != null)
                    {
                        foreach (var arg in variable.Arguments)
                        {
                            expression.Append(" ");
                            expression.Append(m_ArgumentStartTag);
                            AppendExpressionToken(arg, expression);
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
            if (!string.IsNullOrEmpty(text))
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
        }

        private IReadOnlyList<IExpressionToken> ParseTokens(string expression, ref int position, bool isArgumentParsing) 
        {
            var tokens = new List<IExpressionToken>();

            var state = new ParsingState();

            //complete current token and write to the return list
            void FlushCurrentToken()
            {
                if (state.TokenType.HasValue)
                {
                    IExpressionToken newElem;

                    switch (state.TokenType.Value)
                    {
                        case ExpressionTokenType_e.Variable:
                            newElem = new ExpressionTokenVariable(state.TokenContent.ToString(), state.VariableNestedTokens?.ToArray());
                            break;

                        case ExpressionTokenType_e.Text:
                            newElem = new ExpressionTokenText(state.TokenContent.ToString());
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    state = new ParsingState();

                    tokens.Add(newElem);
                }
            }

            if (expression != null)
            {
                for (; position < expression.Length; position++)
                {
                    var thisChar = expression[position];

                    if (!state.CharProtected && thisChar == m_VariableStartTag)
                    {
                        if (state.TokenType.HasValue)
                        {
                            if (state.TokenType == ExpressionTokenType_e.Variable)
                            {
                                throw new NestedVariableOutOfArgumentException(m_ArgumentStartTag, m_ArgumentEndTag);
                            }

                            FlushCurrentToken();
                        }

                        state.TokenType = ExpressionTokenType_e.Variable;
                    }
                    else if (!state.CharProtected && thisChar == m_VariableEndTag)
                    {
                        FlushCurrentToken();
                    }
                    else if (!state.CharProtected && thisChar == m_ArgumentStartTag)
                    {
                        if (state.TokenType == ExpressionTokenType_e.Variable)
                        {
                            state.IsVariableNameParsed = true;

                            position++;

                            state.VariableNestedTokens.Add(GroupElements(ParseTokens(expression, ref position, true)));
                        }
                        else
                        {
                            throw new ArgumentOutOfVariableException();
                        }
                    }
                    else if (!state.CharProtected && thisChar == m_ArgumentEndTag)
                    {
                        if (isArgumentParsing)
                        {
                            FlushCurrentToken();
                            return tokens;
                        }
                        else
                        {
                            throw new MissingArgumentOpeningTagException(m_ArgumentStartTag);
                        }
                    }
                    else if (!state.CharProtected && thisChar == m_EscapeSymbol)
                    {
                        state.CharProtected = true;
                    }
                    else
                    {
                        if (!state.TokenType.HasValue)
                        {
                            state.TokenType = ExpressionTokenType_e.Text;
                        }

                        if (!state.CharProtected)
                        {
                            if (state.TokenType == ExpressionTokenType_e.Variable)
                            {
                                if (thisChar == ' ')
                                {
                                    if (state.TokenContent.Length != 0)
                                    {
                                        state.TokenNameSpaceBuffer.Append(thisChar);
                                    }

                                    continue;
                                }
                                else
                                {
                                    if (state.IsVariableNameParsed)
                                    {
                                        throw new VariableNameInvalidException();
                                    }

                                    if (state.TokenNameSpaceBuffer.Length > 0)
                                    {
                                        state.TokenContent.Append(state.TokenNameSpaceBuffer);
                                        state.TokenNameSpaceBuffer.Clear();
                                    }
                                }
                            }
                        }
                        else
                        {
                            state.CharProtected = false;
                        }

                        state.TokenContent.Append(thisChar);
                    }
                }
            }

            if (state.TokenType.HasValue)
            {
                if (state.TokenType == ExpressionTokenType_e.Text)
                {
                    FlushCurrentToken();
                }
                else
                {
                    throw new NotClosedVariableOrParameterException();
                }
            }

            return tokens;
        }

        private IExpressionToken GroupElements(IReadOnlyList<IExpressionToken> elements)
        {
            if (elements.Count == 1)
            {
                return elements.First();
            }
            else if (elements.Count > 1)
            {
                return new ExpressionTokenGroup(elements.ToArray());
            }
            else
            {
                return new ExpressionTokenText("");
            }
        }
    }
}
