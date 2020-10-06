//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
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

    public abstract class AssemblyNameReferenceResolver : IReferenceResolver
    {
        public virtual Assembly Resolve(AppDomain appDomain, AssemblyName assmName, Assembly requestingAssembly)
        {
            var searchAssmName = GetReplacementAssemblyName(assmName, requestingAssembly, out string searchDir, out bool recursiveSearch);

            if (searchAssmName != null) 
            {
                var replacementAssm = appDomain.GetAssemblies().FirstOrDefault(
                                        a => string.Equals(a.GetName().FullName, searchAssmName.FullName));

                if (replacementAssm == null)
                {
                    if (TryFindAssemblyByName(searchDir, recursiveSearch, searchAssmName, out string probeAssmFile))
                    {
                        replacementAssm = Assembly.LoadFrom(probeAssmFile);
                    }
                }

                if (replacementAssm != null)
                {
                    return replacementAssm;
                }
            }

            return null;
        }

        private bool TryFindAssemblyByName(string dir, bool recurse, AssemblyName searchAssmName, out string filePath)
        {
            var probeAssmFilePath = Path.Combine(dir, searchAssmName.Name + ".dll");

            if (File.Exists(probeAssmFilePath))
            {
                var probeAssmName = AssemblyName.GetAssemblyName(probeAssmFilePath);

                if (string.Equals(probeAssmName.FullName, searchAssmName.FullName))
                {
                    filePath = probeAssmFilePath;
                    return true;
                }
            }

            if (recurse)
            {
                foreach (var subDir in Directory.EnumerateDirectories(dir, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if (TryFindAssemblyByName(subDir, recurse, searchAssmName, out filePath))
                    {
                        return true;
                    }
                }
            }

            filePath = "";
            return false;
        }

        protected abstract AssemblyName GetReplacementAssemblyName(AssemblyName assmName, Assembly requestingAssembly, 
            out string searchDir, out bool recursiveSearch);
    }

    public class AppConfigBindingRedirectReferenceResolver : AssemblyNameReferenceResolver
    {   
        protected virtual Assembly[] GetRequestingAssemblies(Assembly requestingAssembly) 
        {
            if (requestingAssembly != null)
            {
                return new Assembly[] { requestingAssembly };
            }
            else 
            {
                return new Assembly[0];
            }
        }

        private string GetPublicKeyToken(AssemblyName assmName)
        {
            var bytes = assmName.GetPublicKeyToken();

            if (bytes == null || bytes.Length == 0)
            {
                return "null";
            }

            var publicKeyToken = "";

            for (int i = 0; i < bytes.GetLength(0); i++)
            {
                publicKeyToken += string.Format("{0:x2}", bytes[i]);
            }

            return publicKeyToken;
        }

        private string GetCulture(AssemblyName assmName)
        {
            if (string.IsNullOrEmpty(assmName.CultureName))
            {
                return "neutral";
            }
            else
            {
                return assmName.CultureName;
            }
        }

        protected override AssemblyName GetReplacementAssemblyName(AssemblyName assmName, Assembly requestingAssembly, 
            out string searchDir, out bool recursiveSearch)
        {
            var reqAssms = GetRequestingAssemblies(requestingAssembly);

            foreach (var lookupAssm in reqAssms)
            {
                var appConfigPath = lookupAssm.Location + ".config";

                if (File.Exists(appConfigPath))
                {
                    using (var xmlStream = File.OpenRead(appConfigPath))
                    {
                        using (var reader = new XmlTextReader(xmlStream))
                        {
                            var doc = XDocument.Load(reader);

                            var namespaceManager = new XmlNamespaceManager(reader.NameTable);
                            namespaceManager.AddNamespace("b", "urn:schemas-microsoft-com:asm.v1");

                            var name = assmName.Name;
                            var publicKeyToken = GetPublicKeyToken(assmName);
                            var culture = GetCulture(assmName);

                            var bindRedirect = doc.XPathSelectElement(
                                $"//configuration/runtime/b:assemblyBinding/b:dependentAssembly[b:assemblyIdentity[@name = '{name}'][@publicKeyToken = '{publicKeyToken}'][@culture = '{culture}']]/b:bindingRedirect", namespaceManager);

                            if (bindRedirect != null)
                            {
                                var oldVersionVal = bindRedirect.Attribute("oldVersion").Value;
                                var newVersionVal = bindRedirect.Attribute("newVersion").Value;

                                Version oldVersionMin = null;
                                Version oldVersionMax = null;

                                if (oldVersionVal.Contains("-"))
                                {
                                    var oldVersRange = oldVersionVal.Split('-');
                                    oldVersionMin = Version.Parse(oldVersRange[0]);
                                    oldVersionMax = Version.Parse(oldVersRange[1]);
                                }
                                else
                                {
                                    oldVersionMin = Version.Parse(oldVersionVal);
                                    oldVersionMax = Version.Parse(oldVersionVal);
                                }

                                var newVersion = Version.Parse(newVersionVal);

                                if (assmName.Version >= oldVersionMin && assmName.Version <= oldVersionMax)
                                {
                                    var searchAssmName = new AssemblyName($"{name}, Version={newVersion}, Culture={culture}, PublicKeyToken={publicKeyToken}");

                                    searchDir = Path.GetDirectoryName(lookupAssm.Location);
                                    recursiveSearch = true;
                                    return searchAssmName;
                                }
                            }
                        }
                    }
                }
            }

            searchDir = "";
            recursiveSearch = false;
            return null;
        }
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
            
            return resolver.Resolve(appDomain, new AssemblyName(args.Name), args.RequestingAssembly);
        }
    }
}