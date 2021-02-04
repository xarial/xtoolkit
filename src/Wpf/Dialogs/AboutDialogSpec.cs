//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
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

        public string Title { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public Version Version { get; set; }
        public Image Logo { get; set; }
        public LicenseInfo[] Licenses { get; set; }

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
            Version = assm.GetName().Version;
            Licenses = licenses;
            Logo = logo;
        }
    }
}
