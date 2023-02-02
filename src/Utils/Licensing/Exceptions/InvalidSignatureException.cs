using System;
using Xarial.XToolkit.Reporting;

namespace Xarial.XToolkit.Licensing.Exceptions
{
    public class InvalidSignatureException : LicenseValidationException
    {
        public InvalidSignatureException() : base("Invalid signature. Content of this license was modifed")
        {
        }
    }
}
