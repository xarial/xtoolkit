using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class VariableNameSpaceNotSupportedException : Exception
    {
        public VariableNameSpaceNotSupportedException() : base("Variables cannot have space") 
        {
        }
    }
}
