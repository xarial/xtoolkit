//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Xarial.XToolkit.Reflection
{
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
}