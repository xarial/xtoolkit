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
