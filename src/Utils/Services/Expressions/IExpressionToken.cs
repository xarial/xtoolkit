//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;

namespace Xarial.XToolkit.Services.Expressions
{
    /// <summary>
    /// Represents the base token for <see cref="IExpressionParser"/>
    /// </summary>
    public interface IExpressionToken
    { 
    }

    /// <summary>
    /// Additional methods of <see cref="IExpressionToken"/>
    /// </summary>
    public static class ExpressionTokenExtension 
    {
        /// <summary>
        /// Compares the content of 2 expression tokens
        /// </summary>
        /// <param name="token">Source token</param>
        /// <param name="other">Token to compare to</param>
        /// <param name="comparison">Comparison type</param>
        /// <returns>True if content is the same</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsSame(this IExpressionToken token, IExpressionToken other, StringComparison comparison = StringComparison.CurrentCulture) 
        {
            bool IsSameGroup(IExpressionToken[] firstGroup, IExpressionToken[] secondGroup) 
            {
                if (firstGroup?.Length == secondGroup?.Length)
                {
                    if (firstGroup != null && secondGroup != null)
                    {
                        for (int i = 0; i < firstGroup.Length; i++)
                        {
                            if (!IsSame(firstGroup[i], secondGroup[i], comparison))
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (token == null) 
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (token is IExpressionTokenGroup && other is IExpressionTokenGroup)
            {
                var thisGroup = (IExpressionTokenGroup)token;
                var otherGroup = (IExpressionTokenGroup)other;

                return IsSameGroup(thisGroup.Children, otherGroup.Children);
            }
            else if (token is IExpressionTokenVariable && other is IExpressionTokenVariable)
            {
                var thisVar = (IExpressionTokenVariable)token;
                var otherVar = (IExpressionTokenVariable)other;

                if (string.Equals(thisVar.Name, otherVar.Name, comparison))
                {
                    return IsSameGroup(thisVar.Arguments, otherVar.Arguments);
                }
                else
                {
                    return false;
                }
            }
            else if (token is IExpressionTokenText && other is IExpressionTokenText)
            {
                return string.Equals(((IExpressionTokenText)token).Text, ((IExpressionTokenText)other).Text, comparison);
            }
            else 
            {
                return false;
            }
        }
    }
}
