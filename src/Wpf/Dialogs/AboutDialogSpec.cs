//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using Xarial.XToolkit.Wpf.Attributes;

namespace Xarial.XToolkit.Wpf.Dialogs
{
    public class PackageEditionSpec
    {
        public string EditionName { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public PackageEditionSpec(string packageName, DateTime? expiryDate) 
        {
            EditionName = packageName;
            ExpiryDate = expiryDate;
        }
    }

    /// <summary>
    /// Specification for <see cref="AboutDialog"/>
    /// </summary>
    public class AboutDialogSpec
    {
        private static Image TryFindAssemblyLogo(Assembly assm)
            => assm.GetCustomAttribute<AssemblyLogoAttribute>()?.Logo;

        private static LicenseInfo[] TryFindAssemblyLicenses(Assembly assm)
        {
            var atts = assm.GetCustomAttributes(typeof(AssemblyLicenseAttribute));

            var lics = new List<LicenseInfo>();

            if (atts?.Any() == true)
            {
                foreach (AssemblyLicenseAttribute att in atts)
                {
                    lics.AddRange(att.Licenses);
                }
            }

            return lics.ToArray();
        }

        /// <summary>
        /// Product title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Product description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Company
        /// </summary>
        public string Company { get; set; }
        
        /// <summary>
        /// Copyright
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// Product version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Product logo
        /// </summary>
        public Image Logo { get; set; }

        /// <summary>
        /// Product 3rd party license information
        /// </summary>
        public LicenseInfo[] Licenses { get; set; }

        /// <summary>
        /// Product edition
        /// </summary>
        public PackageEditionSpec Edition { get; set; }

        /// <summary>
        /// RTF encoded text of the end-user license agreement to display in the About dialog
        /// </summary>
        public string Eula { get; set; }

        public AboutDialogSpec() 
        {
        }

        public AboutDialogSpec(Assembly assm) : this(assm, TryFindAssemblyLogo(assm))
        {
        }

        public AboutDialogSpec(Assembly assm, Image logo) : this(assm, logo, TryFindAssemblyLicenses(assm))
        {
        }

        public AboutDialogSpec(Assembly assm, LicenseInfo[] licenses) 
            : this(assm, TryFindAssemblyLogo(assm), licenses)
        {
        }

        public AboutDialogSpec(Assembly assm, Image logo, LicenseInfo[] licenses)
        {
            Title = assm.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            Description = assm.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
            Copyright = assm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            Company = assm.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            Version = assm.GetName().Version;
            Licenses = licenses;
            Logo = logo;
            Eula = assm.GetCustomAttribute<AssemblyEulaAttribute>()?.Eula;
        }
    }
}
