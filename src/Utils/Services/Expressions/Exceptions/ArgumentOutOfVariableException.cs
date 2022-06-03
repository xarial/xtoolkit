using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class ArgumentOutOfVariableException : Exception
    {
        public ArgumentOutOfVariableException() : base("Argument can only appear within the variable") 
        {
        }
    }
}
