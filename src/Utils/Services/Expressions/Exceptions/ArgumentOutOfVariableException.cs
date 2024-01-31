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
    public class ArgumentOutOfVariableException : InvalidExpressionException
    {
        public ArgumentOutOfVariableException() : base("Argument can only appear within the variable") 
        {
        }
    }
}
