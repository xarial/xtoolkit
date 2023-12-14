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
    public class LicenseFileMalformedException : LicenseValidationException
    {
        public LicenseFileMalformedException(Exception inner)
            : base("The license file is malformed. Make sure that the content of the license file has not been modified by AntiVirus or Internet Security software", inner)
        {
        }
    }
}
