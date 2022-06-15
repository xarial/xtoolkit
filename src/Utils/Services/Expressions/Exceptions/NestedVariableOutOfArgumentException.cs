using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class NestedVariableOutOfArgumentException : InvalidExpressionException
    {
        public NestedVariableOutOfArgumentException(char argumentStartTag, char argumentEndTag) 
            : base($"Nested variables can only be used as arguments. Enclose variable into '{argumentStartTag}{argumentEndTag}'") 
        { 
        }
    }
}
