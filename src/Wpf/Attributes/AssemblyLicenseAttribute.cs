using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xarial.XToolkit.Reflection;

namespace Xarial.XToolkit.Wpf.Attributes
{
    public class LicenseInfo 
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class AssemblyLicenseAttribute : Attribute
    {
        public LicenseInfo[] Licenses { get; }

        public AssemblyLicenseAttribute(Type resType, string resName)
        {
            const string MARKDOWN_URL_REGEX = @"\[(.*?)\]\((.*?)\)";

            var licsTxt = ResourceHelper.GetResource<string>(resType, resName);

            var lics = new List<LicenseInfo>();

            foreach (var lic in licsTxt.Split(new[] { '\r', '\n' }).Where(l => !string.IsNullOrEmpty(l)))
            {
                var match = Regex.Match(lic, MARKDOWN_URL_REGEX);

                if (match.Success)
                {
                    lics.Add(new LicenseInfo()
                    {
                        Title = match.Groups[1].Value,
                        Url = match.Groups[2].Value
                    });
                }
                else
                {
                    lics.Add(new LicenseInfo()
                    {
                        Title = lic,
                        Url = ""
                    });
                }
            }

            Licenses = lics.ToArray();
        }

        public AssemblyLicenseAttribute(string title, string url)
        {
            Licenses = new LicenseInfo[]
            {
                new LicenseInfo()
                {
                    Title = title,
                    Url = url
                } 
            };
        }
    }
}
