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
    [Flags]
    public enum RequestigAssemblyScope_e
    {
        Requesting = 1,
        Calling = 2,
        Executing = 4
    }

    public static class AppDomainExtension
    {
        private static readonly Dictionary<int, RequestigAssemblyScope_e> m_DomainsScopes;

        static AppDomainExtension()
        {
            m_DomainsScopes = new Dictionary<int, RequestigAssemblyScope_e>();
        }

        public static void ResolveBindingRedirects(this AppDomain appDomain,
            RequestigAssemblyScope_e reqScope = RequestigAssemblyScope_e.Requesting | RequestigAssemblyScope_e.Calling | RequestigAssemblyScope_e.Executing)
        {
            m_DomainsScopes[appDomain.Id] = reqScope;

            appDomain.AssemblyResolve -= OnAssemblyResolve;
            appDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var appDomain = sender as AppDomain;

            var scope = m_DomainsScopes[appDomain.Id];
            var srcAssms = GetSourceAssemblies(scope, args);

            foreach (var requestingAssm in srcAssms)
            {
                var appConfigPath = requestingAssm.Location + ".config";

                if (File.Exists(appConfigPath))
                {
                    using (var xmlStream = File.OpenRead(appConfigPath))
                    {
                        using (var reader = new XmlTextReader(xmlStream))
                        {
                            var doc = XDocument.Load(reader);

                            var namespaceManager = new XmlNamespaceManager(reader.NameTable);
                            namespaceManager.AddNamespace("b", "urn:schemas-microsoft-com:asm.v1");

                            var assmName = new AssemblyName(args.Name);

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

                                    var replacementAssm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                                        a => string.Equals(a.GetName().FullName, searchAssmName.FullName));

                                    if (replacementAssm == null)
                                    {
                                        var assmDir = Path.GetDirectoryName(requestingAssm.Location);

                                        if (TryFindAssemblyByName(assmDir, searchAssmName, out string probeAssmFile))
                                        {
                                            replacementAssm = Assembly.LoadFrom(probeAssmFile);
                                        }
                                    }

                                    if (replacementAssm != null)
                                    {
                                        return replacementAssm;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static Assembly[] GetSourceAssemblies(RequestigAssemblyScope_e scope, ResolveEventArgs args)
        {
            var srcAssms = new List<Assembly>();

            if (scope.HasFlag(RequestigAssemblyScope_e.Requesting))
            {
                if (args.RequestingAssembly != null)
                {
                    srcAssms.Add(args.RequestingAssembly);
                }
            }

            if (scope.HasFlag(RequestigAssemblyScope_e.Calling))
            {
                var callingAssm = Assembly.GetCallingAssembly();
                if (callingAssm != null && !srcAssms.Contains(callingAssm))
                {
                    srcAssms.Add(callingAssm);
                }
            }

            if (scope.HasFlag(RequestigAssemblyScope_e.Executing))
            {
                var executingAssm = Assembly.GetExecutingAssembly();

                if (executingAssm != null && !srcAssms.Contains(executingAssm))
                {
                    srcAssms.Add(executingAssm);
                }
            }

            return srcAssms.ToArray();
        }

        private static string GetPublicKeyToken(AssemblyName assmName)
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

        private static string GetCulture(AssemblyName assmName)
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

        private static bool TryFindAssemblyByName(string dir, AssemblyName searchAssmName, out string filePath)
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

            foreach (var subDir in Directory.EnumerateDirectories(dir, "*.*", SearchOption.TopDirectoryOnly))
            {
                if (TryFindAssemblyByName(subDir, searchAssmName, out filePath))
                {
                    return true;
                }
            }

            filePath = "";
            return false;
        }
    }
}