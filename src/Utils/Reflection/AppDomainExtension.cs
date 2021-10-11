//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Xarial.XToolkit.Reflection
{
    public interface IReferenceResolver 
    {
        Assembly Resolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly);
    }

    public static class AppDomainExtension
    {
        private static readonly Dictionary<int, List<IReferenceResolver>> m_DomainsReferenceResolvers;
        private static readonly object m_Lock;

        static AppDomainExtension()
        {
            m_DomainsReferenceResolvers = new Dictionary<int, List<IReferenceResolver>>();
            m_Lock = new object();
        }

        public static void ResolveBindingRedirects(this AppDomain appDomain)
            => ResolveBindingRedirects(appDomain, new AppConfigBindingRedirectReferenceResolver());

        public static void ResolveBindingRedirects(this AppDomain appDomain,
            IReferenceResolver resolver)
        {
            lock (m_Lock)
            {
                if (!m_DomainsReferenceResolvers.TryGetValue(appDomain.Id, out List<IReferenceResolver> resolvers))
                {
                    resolvers = new List<IReferenceResolver>();
                    m_DomainsReferenceResolvers.Add(appDomain.Id, resolvers);
                }

                resolvers.Add(resolver);

                appDomain.AssemblyResolve -= OnAssemblyResolve;
                appDomain.AssemblyResolve += OnAssemblyResolve;
            }
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var appDomain = sender as AppDomain;
            
            var resolvers = m_DomainsReferenceResolvers[appDomain.Id];

            var assmName = new AssemblyName(args.Name);

            if (!assmName.Name.EndsWith(".resources"))
            {
                foreach (var resolver in resolvers)
                {
                    var assm = resolver.Resolve(appDomain, assmName, args.RequestingAssembly);

                    if (assm != null)
                    {
                        Trace.WriteLine($"Assembly '{args.Name}' is resolved to '{assm.Location}' via '{resolver.GetType().FullName}' resolver", "Xarial.xToolkit");
                        return assm;
                    }
                    else
                    {
                        Trace.WriteLine($"Assembly '{args.Name}' is not resolved via '{resolver.GetType().FullName}' resolver", "Xarial.xToolkit");
                    }
                }
            }
            
            return null;
        }
    }
}