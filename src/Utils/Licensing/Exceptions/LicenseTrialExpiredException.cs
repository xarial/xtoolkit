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
