//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Xarial.XToolkit.Services.Expressions.Exceptions;

namespace Xarial.XToolkit.Services.Expressions
{
    /// <summary>
    /// Evaluates the expression and casts to the specified type
    /// </summary>
    public interface IExpressionEvaluator
    {
        /// <summary>
        /// Evaluates the given expression
        /// </summary>
        /// <typeparam name="T">Type of the return value</typeparam>
        /// <param name="expression">Expression to evaluate</param>
        /// <returns>Evaluated value</returns>
        T Evaluate<T>(string expression);
    }

    public class DataTableExpressionEvaluator : IExpressionEvaluator
    {
        private readonly DataTable m_Table;

        public DataTableExpressionEvaluator()
        {
            m_Table = new DataTable();
        }

        public T Evaluate<T>(string expression)
        {
            try
            {
                var res = m_Table.Compute(expression, "");

                if (res is T)
                {
                    return (T)res;
                }
                else
                {
                    return (T)Convert.ChangeType(res, typeof(T));
                }
            }
            catch (SyntaxErrorException syntEx)
            {
                throw new ExpressionSyntaxErrorException(syntEx.Message, syntEx);
            }
            catch (EvaluateException evalEx)
            {
                throw new ExpressionEvaluateErrorException(evalEx.Message, evalEx);
            }
            catch (FormatException formEx)
            {
                throw new ExpressionResultInvalidCastException(typeof(T), formEx);
            }
            catch (Exception ex) 
            {
                throw new ExpressionFailedException("Unknown expression evaluation error", ex);
            }
        }
    }
}
