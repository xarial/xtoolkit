//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class ExpressionEvaluateErrorException : ExpressionFailedException
    {
        public ExpressionEvaluateErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
