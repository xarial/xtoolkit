using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class ExpressionFailedException : Exception
    {
        public ExpressionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
