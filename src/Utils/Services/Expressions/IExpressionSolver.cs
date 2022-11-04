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
    /// Solves the expression token with context
    /// </summary>
    public interface IExpressionSolver<TContext>
    {
        /// <summary>
        /// Replaces the expression token with values
        /// </summary>
        /// <param name="token">Expression token to solve</param>
        /// <param name="context">Context</param>
        /// <returns>Expression with replaced variables</returns>
        string Solve(IExpressionToken token, TContext context);
    }

    //Solver without the context
    public interface IExpressionSolver : IExpressionSolver<object>
    {
        /// <inheritdoc/>
        string Solve(IExpressionToken token);
    }

    public delegate object VariableValueProviderDelegate<TContext>(string name, object[] args, TContext context);
    public delegate object VariableValueProviderDelegate(string name, object[] args);

    public class ExpressionSolver<TContext> : IExpressionSolver<TContext>
    {
        private class VariableCacheKey
        {
            internal string VariableName { get; }
            internal object[] Arguments { get; }

            internal VariableCacheKey(string varName, object[] args)
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
                                if (!object.Equals(x.Arguments[i], y.Arguments[i]))
                                {
                                    if (x.Arguments[i] is string && y.Arguments[i] is string)
                                    {
                                        if (!string.Equals((string)x.Arguments[i], (string)y.Arguments[i], m_Comparison)) 
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        return false;
                                    }
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

        private readonly StringComparison m_Comparison;
        private readonly VariableValueProviderDelegate<TContext> m_Solver;

        public ExpressionSolver(VariableValueProviderDelegate<TContext> solver, StringComparison comparison = StringComparison.CurrentCulture)
        {
            if (solver == null)
            {
                throw new ArgumentNullException(nameof(solver));
            }

            m_Solver = solver;

            m_Comparison = comparison;
        }

        public string Solve(IExpressionToken token, TContext context)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Resolve(token, context, new Dictionary<VariableCacheKey, object>(new VariableCacheKeyEqualityComparer(m_Comparison)))?.ToString();
        }

        private object Resolve(IExpressionToken token, TContext context, Dictionary<VariableCacheKey, object> variableCache)
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
                        value.Append(Resolve(child, context, variableCache));
                    }
                    break;

                case IExpressionTokenText text:
                    value.Append(text.Text);
                    break;

                case IExpressionTokenVariable variable:

                    object[] arguments;

                    if (variable.Arguments?.Any() == true)
                    {
                        arguments = new object[variable.Arguments.Length];

                        for (int i = 0; i < variable.Arguments.Length; i++)
                        {
                            arguments[i] = Resolve(variable.Arguments[i], context, variableCache);
                        }
                    }
                    else
                    {
                        arguments = new object[0];
                    }

                    var cacheKey = new VariableCacheKey(variable.Name, arguments);

                    if (!variableCache.TryGetValue(cacheKey, out object varVal))
                    {
                        varVal = m_Solver.Invoke(variable.Name, arguments, context);

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

    public class ExpressionSolver : ExpressionSolver<object>, IExpressionSolver
    {
        public ExpressionSolver(VariableValueProviderDelegate solver, StringComparison comparison = StringComparison.CurrentCulture)
            : base((n, a, c) => solver.Invoke(n, a), comparison)
        {
        }

        public string Solve(IExpressionToken token) => base.Solve(token, null);
    }
}
