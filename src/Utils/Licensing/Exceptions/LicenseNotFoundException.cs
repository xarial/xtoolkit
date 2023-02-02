using System.IO;
using Xarial.XToolkit.Reporting;

namespace Xarial.XToolkit.Licensing.Exceptions
{
    public class LicenseNotFoundException : LicenseValidationException
    {
        public LicenseNotFoundException(string licFilePath) : base($"License file not found: '{licFilePath}'")
        {
        }
    }
}
