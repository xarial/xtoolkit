using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class MissingArgumentOpeningTagException : Exception
    {
        public MissingArgumentOpeningTagException(char argumentStartTag) : base($"Argument is missing the opening tag '{argumentStartTag}'") 
        {
        }
    }
}
