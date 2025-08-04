using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Xarial.XToolkit.Services
{
    /// <summary>
    /// Isolates an instance of the specified type in the specified assembly in new application domain
    /// </summary>
    /// <remarks>Only supported in .NET Framework</remarks>
    public class IsolatedInstance : IDisposable
    {
        private class RemoteInvoker : MarshalByRefObject
        {
            private object m_Instance;
            private readonly Dictionary<string, MethodInfo> m_Methods;

            public RemoteInvoker()
            {
                m_Methods = new Dictionary<string, MethodInfo>();
            }

            public void Init(string assemblyPath, string typeName)
            {
                var assm = Assembly.LoadFrom(assemblyPath);
                var type = assm.GetType(typeName);

                if (type != null)
                {
                    m_Instance = Activator.CreateInstance(type);
                }
                else
                {
                    throw new NullReferenceException($"Failed to find type '{typeName}'");
                }
            }

            public object InvokeMethod(string methodName, params object[] parameters)
            {
                if (!m_Methods.TryGetValue(methodName, out var method))
                {
                    method = m_Instance.GetType().GetMethod(methodName);

                    if (method != null)
                    {
                        m_Methods.Add(methodName, method);
                    }
                    else
                    {
                        throw new ArgumentNullException($"Method is not found: '{methodName}'");
                    }
                }

                return method.Invoke(m_Instance, parameters);
            }
        }

#if NETFRAMEWORK
        private readonly AppDomain m_Domain;
#endif

        private readonly RemoteInvoker m_Proxy;

        /// <summary>
        /// Construcotr
        /// </summary>
        /// <param name="assemblyPath">Assembly path which contains instance to create</param>
        /// <param name="typeName">Type name of the object to create</param>
        /// <exception cref="NotSupportedException">Target framwork is not supported</exception>
        public IsolatedInstance(string assemblyPath, string typeName) 
        {
#if NETFRAMEWORK
            
            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(typeof(RemoteInvoker).Assembly.Location)
            };

            m_Domain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null, setup);
            
            m_Proxy = (RemoteInvoker)m_Domain.CreateInstanceAndUnwrap(
                    typeof(RemoteInvoker).Assembly.FullName,
                    typeof(RemoteInvoker).FullName);

            m_Proxy.Init(assemblyPath, typeName);
#else
            throw new NotSupportedException();
#endif
        }

        /// <summary>
        /// Calls the specific method via interface
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="expression">Expression</param>
        public void Call<T>(Expression<Action<T>> expression)
        {
            ParseExpression(expression.Body, out var methodName, out var args);
            
            InvokeMethod(methodName, args);
        }

        /// <summary>
        /// Calls the specific function via interface
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="TResult">Result</typeparam>
        /// <param name="expression">Expression</param>
        /// <returns>Function result</returns>
        public TResult Call<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            ParseExpression(expression.Body, out var methodName, out var args);

            return (TResult)InvokeMethod(methodName, args);
        }

        /// <summary>
        /// Invokes specified method with parameters
        /// </summary>
        /// <param name="methodName">Name of the method</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Result</returns>
        public object InvokeMethod(string methodName, params object[] parameters)
            => m_Proxy.InvokeMethod(methodName, parameters);

        private void ParseExpression(Expression expressionBody, out string methodName, out object[] args)
        {
            if (expressionBody is MethodCallExpression methodCall)
            {
                var argsList = new List<object>();

                var parameters = methodCall.Method.GetParameters();

                for (int i = 0; i < methodCall.Arguments.Count; i++)
                {
                    var argExpr = methodCall.Arguments[i];
                    var argValue = Expression.Lambda(argExpr).Compile().DynamicInvoke();
                    argsList.Add(argValue);
                }

                methodName = methodCall.Method.Name;
                args = argsList.ToArray();
            }
            else
            {
                throw new Exception("Expression is not a method call");
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
#if NETFRAMEWORK
            if (m_Domain != null)
            {
                AppDomain.Unload(m_Domain);
            }
#endif
        }
    }
}