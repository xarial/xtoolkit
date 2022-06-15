using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class ExpressionSyntaxErrorException : ExpressionFailedException
    {
        public ExpressionSyntaxErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
