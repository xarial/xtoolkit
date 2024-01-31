//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

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
