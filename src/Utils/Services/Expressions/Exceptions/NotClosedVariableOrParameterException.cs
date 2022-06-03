using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class NotClosedVariableOrParameterException : Exception
    {
        public NotClosedVariableOrParameterException() : base("Variable or parameter is not closed") 
        {
        }
    }
}
