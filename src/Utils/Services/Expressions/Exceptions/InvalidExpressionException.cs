using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class InvalidExpressionException : Exception
    {
        public InvalidExpressionException(string message) : base(message)
        {
        }
    }
}
