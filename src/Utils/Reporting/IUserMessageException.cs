//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Reporting
{
    /// <summary>
    /// Error with user friendly description
    /// </summary>
    /// <remarks>Use this interface in custom <see cref="Exception"/> to indicate that it should be displayed to the user</remarks>
    public interface IUserMessageException
    {
        /// <summary>
        /// User friendly message for the error
        /// </summary>
        string Message { get; }
    }
}
