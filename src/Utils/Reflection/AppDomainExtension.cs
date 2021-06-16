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
        private static readonly Dictionary<int, IReferenceResolver> m_DomainsReferenceResolvers;

        static AppDomainExtension()
        {
            m_DomainsReferenceResolvers = new Dictionary<int, IReferenceResolver>();
        }

        public static void ResolveBindingRedirects(this AppDomain appDomain)
            => ResolveBindingRedirects(appDomain, new AppConfigBindingRedirectReferenceResolver());

        public static void ResolveBindingRedirects(this AppDomain appDomain,
            IReferenceResolver resolver)
        {
            m_DomainsReferenceResolvers[appDomain.Id] = resolver;

            appDomain.AssemblyResolve -= OnAssemblyResolve;
            appDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var appDomain = sender as AppDomain;
            
            var resolver = m_DomainsReferenceResolvers[appDomain.Id];
            
            var assm = resolver.Resolve(appDomain, new AssemblyName(args.Name), args.RequestingAssembly);

            Trace.WriteLine($"Assembly '{args.Name}' is resolved to '{assm?.Location}'", "Xarial.xToolkit");

            return assm;
        }
    }
}