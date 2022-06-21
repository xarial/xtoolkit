//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions
{
    /// <summary>
    /// Solves the expression token
    /// </summary>
    public interface IExpressionSolver
    {
        /// <summary>
        /// Replaces the expression token with values
        /// </summary>
        /// <param name="token">Expression token to solve</param>
        /// <returns>Expression with replaced variables</returns>
        string Solve(IExpressionToken token);
    }

    public delegate string VariableValueProviderDelegate(string name, string[] args);

    public class ExpressionSolver : IExpressionSolver
    {
        private class VariableCacheKey
        {
            internal string VariableName { get; }
            internal string[] Arguments { get; }
            
            internal VariableCacheKey(string varName, string[] args) 
            {
                VariableName = varName;
                Arguments = args;
            }
        }

        private class VariableCacheKeyEqualityComparer : IEqualityComparer<VariableCacheKey>
        {
            private readonly StringComparison m_Comparison;

            internal VariableCacheKeyEqualityComparer(StringComparison comparison) 
            {
                m_Comparison = comparison;
            }

            public bool Equals(VariableCacheKey x, VariableCacheKey y)
            {
                if (object.ReferenceEquals(x, y)) 
                {
                    return true;
                }

                if (x == null || y == null) 
                {
                    return false;
                }

                if (string.Equals(x.VariableName, y.VariableName, m_Comparison))
                {
                    if (x.Arguments == null && y.Arguments == null)
                    {
                        return true;
                    }

                    if (x.Arguments != null && y.Arguments != null)
                    {
                        if (x.Arguments.Length == y.Arguments.Length)
                        {
                            for (int i = 0; i < x.Arguments.Length; i++)
                            {
                                if (!string.Equals(x.Arguments[i], y.Arguments[i], m_Comparison))
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    }
                }

                return false;
            }

            public int GetHashCode(VariableCacheKey obj) => 0;
        }

        private readonly VariableValueProviderDelegate m_Solver;

        private readonly StringComparison m_Comparison;

        public ExpressionSolver(VariableValueProviderDelegate solver, StringComparison comparison = StringComparison.CurrentCulture) 
        {
            if (solver == null) 
            {
                throw new ArgumentNullException(nameof(solver));
            }

            m_Comparison = comparison;

            m_Solver = solver;
        }

        public string Solve(IExpressionToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Resolve(token, new Dictionary<VariableCacheKey, string>(new VariableCacheKeyEqualityComparer(m_Comparison)));
        }

        private string Resolve(IExpressionToken token, Dictionary<VariableCacheKey, string> variableCache)
        {
            if (token == null) 
            {
                throw new ArgumentNullException(nameof(token));
            }

            var value = new StringBuilder();

            switch (token)
            {
                case IExpressionTokenGroup group:
                    foreach (var child in group.Children)
                    {
                        value.Append(Resolve(child, variableCache));
                    }
                    break;

                case IExpressionTokenText text:
                    value.Append(text.Text);
                    break;

                case IExpressionTokenVariable variable:

                    string[] arguments;

                    if (variable.Arguments?.Any() == true)
                    {
                        arguments = new string[variable.Arguments.Length];

                        for (int i = 0; i < variable.Arguments.Length; i++)
                        {
                            arguments[i] = Resolve(variable.Arguments[i], variableCache);
                        }
                    }
                    else
                    {
                        arguments = new string[0];
                    }

                    var cacheKey = new VariableCacheKey(variable.Name, arguments);

                    if (!variableCache.TryGetValue(cacheKey, out string varVal)) 
                    {
                        varVal = m_Solver.Invoke(variable.Name, arguments);

                        variableCache.Add(cacheKey, varVal);
                    }

                    value.Append(varVal);

                    break;

                default:
                    throw new NotSupportedException();
            }

            return value.ToString();
        }
    }
}
