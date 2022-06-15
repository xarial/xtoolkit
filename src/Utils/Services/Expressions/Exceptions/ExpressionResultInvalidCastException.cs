using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class ExpressionResultInvalidCastException : ExpressionFailedException
    {
        public ExpressionResultInvalidCastException(Type targetType, Exception innerException) 
            : base($"Failed to cast the expression evaluated value to {targetType.FullName}", innerException)
        {
        }
    }
}
