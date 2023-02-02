//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    /// <summary>
    /// Exceptions of <see cref="IExpressionEvaluator"/>
    /// </summary>
    public class ExpressionFailedException : Exception
    {
        public ExpressionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
