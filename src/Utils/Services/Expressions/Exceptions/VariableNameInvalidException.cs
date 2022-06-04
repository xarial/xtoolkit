using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class VariableNameInvalidException : Exception
    {
        public VariableNameInvalidException() : base("Variable name cannot appear after arguments")
        {
        }
    }
}
