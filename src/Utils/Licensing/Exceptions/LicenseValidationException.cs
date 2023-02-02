using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reporting;

namespace Xarial.XToolkit.Licensing.Exceptions
{
    /// <summary>
    /// Base exception when the license validation fails in <see cref="ILicenseValidator{TLicData}.ValidateLicense(byte[])"/>
    /// </summary>
    public class LicenseValidationException : Exception, IUserMessageException
    {
        public LicenseValidationException(string message) : base(message) 
        {
        }

        public LicenseValidationException(string message, Exception innerException) : base(message, innerException) 
        {
        }
    }
}
