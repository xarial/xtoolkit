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
    /// <summary>
    /// Expressions thrown by <see cref="IExpressionParser"/>
    /// </summary>
    public class InvalidExpressionException : Exception
    {
        public InvalidExpressionException(string message) : base(message)
        {
        }
    }
}
