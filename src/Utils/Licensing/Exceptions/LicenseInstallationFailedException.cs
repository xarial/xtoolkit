//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XToolkit.Reporting;

namespace Xarial.XToolkit.Licensing.Exceptions
{
    public class LicenseInstallationFailedException : Exception, IUserMessageException
    {
        public LicenseInstallationFailedException(Exception inner)
            : base("Failed to install the license", inner)
        {
        }
    }
}
