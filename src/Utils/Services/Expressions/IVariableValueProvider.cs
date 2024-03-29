﻿//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XToolkit.Services.Expressions
{
    /// <summary>
    /// Service to provide the value of the specific variable
    /// </summary>
    public interface IVariableValueProvider 
    {
        /// <summary>
        /// Solves the variable
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="args">Variable arguments</param>
        /// <param name="context">Variable context</param>
        /// <returns>Resolved value</returns>
        object Provide(string name, object[] args, object context);
    }

    public delegate object VariableValueProviderDelegate<TContext>(string name, object[] args, TContext context);

    public interface IVariableValueProvider<TContext> : IVariableValueProvider
    {
        object Provide(string name, object[] args, TContext context);
    }

    public class VariableValueProvider<TContext> : IVariableValueProvider<TContext>
    {
        object IVariableValueProvider.Provide(string name, object[] args, object context) => Provide(name, args, (TContext)context);

        private readonly VariableValueProviderDelegate<TContext> m_Provider;

        public VariableValueProvider(VariableValueProviderDelegate<TContext> provider)
        {
            m_Provider = provider;
        }

        public VariableValueProvider()
        {
        }

        public virtual object Provide(string name, object[] args, TContext context)
        {
            if (m_Provider != null)
            {
                return m_Provider.Invoke(name, args, context);
            }
            else 
            {
                throw new Exception($"Provider is not specified. Either provide the provider delegate in the constructor overload or override '{nameof(Provide)}' method");
            }
        }
    }
}
