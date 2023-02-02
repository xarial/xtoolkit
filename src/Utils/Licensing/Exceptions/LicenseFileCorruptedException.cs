﻿using System;
using Xarial.XToolkit.Reporting;

namespace Xarial.XToolkit.Licensing.Exceptions
{
    public class LicenseFileCorruptedException : LicenseValidationException
    {
        public LicenseFileCorruptedException(Exception inner)
            : base("The license file is corrupted. Make sure that the content of the license file has not been modified by AntiVirus or Internet Security software", inner)
        {
        }
    }
}
