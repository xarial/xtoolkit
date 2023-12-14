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
    public class LicenseAuthorizationException : LicenseValidationException
    {
        public LicenseAuthorizationException(string message) : base(message)
        {
        }

        public LicenseAuthorizationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
