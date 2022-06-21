//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions.Exceptions
{
    public class MissingArgumentOpeningTagException : InvalidExpressionException
    {
        public MissingArgumentOpeningTagException(char argumentStartTag) : base($"Argument is missing the opening tag '{argumentStartTag}'") 
        {
        }
    }
}
