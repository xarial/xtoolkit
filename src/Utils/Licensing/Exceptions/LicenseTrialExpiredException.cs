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
    public class LicenseTrialExpiredException : LicenseValidationException
    {
        public LicenseTrialExpiredException(DateTime expiryDate) : base($"Trial license has expired on {expiryDate}")
        {
        }
    }
}
