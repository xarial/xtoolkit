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

    public class AssemblyResolver
    {
        private readonly List<IReferenceResolver> m_AssemblyResolvers;
        private readonly AppDomain m_AppDomain;

        public AssemblyResolver(AppDomain appDomain)
        {
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
                        Trace.WriteLine($"Assembly '{args.Name}' is resolved to '{assm.Location}' via '{resolver.Name}' resolver", "Xarial.xToolkit");
                        return assm;
                    }
                    else
                    {
                        Trace.WriteLine($"Assembly '{args.Name}' is not resolved via '{resolver.Name}' resolver", "Xarial.xToolkit");
                    }
                }
            }

            return null;
        }
    }
}
