//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xarial.XToolkit.Reflection;

namespace Xarial.XToolkit.Helpers
{
    public interface IReferenceResolver
    {
        string Name { get; }
        Assembly Resolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly);
    }

    /// <summary>
    /// This is a helper class allowing to specify strategies for resolving the missing dlls
    /// </summary>
    public class AssemblyResolver
    {
        private readonly List<IReferenceResolver> m_AssemblyResolvers;
        private readonly AppDomain m_AppDomain;
        private readonly string m_LogName;

        public AssemblyResolver(AppDomain appDomain) : this(appDomain, "Xarial.xToolkit")
        {
        }

        public AssemblyResolver(AppDomain appDomain, string logName)
        {
            m_LogName = logName;
            m_AppDomain = appDomain;
            m_AssemblyResolvers = new List<IReferenceResolver>();

            m_AppDomain.AssemblyResolve += OnResolveMissingAssembly;
        }

        public void RegisterAssemblyReferenceResolver(IReferenceResolver resolver)
        {
            m_AssemblyResolvers.Add(resolver);
        }

        private Assembly OnResolveMissingAssembly(object sender, ResolveEventArgs args)
        {
            var assmName = new AssemblyName(args.Name);

            if (!assmName.Name.EndsWith(".resources"))
            {
                foreach (var resolver in m_AssemblyResolvers)
                {
                    var assm = resolver.Resolve(m_AppDomain, assmName, args.RequestingAssembly);

                    if (assm != null)
                    {
                        Trace.WriteLine($"Assembly '{args.Name}' is resolved to '{assm.Location}' via '{resolver.Name}' resolver", m_LogName);
                        return assm;
                    }
                    else
                    {
                        Trace.WriteLine($"Assembly '{args.Name}' is not resolved via '{resolver.Name}' resolver", m_LogName);
                    }
                }
            }

            return null;
        }
    }
}
