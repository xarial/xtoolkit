//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Reporting
{
    public static class ExceptionExtension
    {
        /// <summary>
        /// Parses the exception error and extract user visible error of <see cref="IUserMessageException"/>
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="fullLog">Full log (including non user errors)</param>
        /// <param name="genericError">Text of generic error if no user error found</param>
        /// <returns>user friendly error</returns>
        public static string ParseUserError(this Exception ex, out string fullLog, string genericError = "Generic error")
        {
            var res = new StringBuilder();
            var fullLogBuilder = new StringBuilder();

            void ProcessException(Exception curEx)
            {
                fullLogBuilder.AppendLine(curEx.Message);
                fullLogBuilder.AppendLine(curEx.StackTrace);

                if (curEx is IUserMessageException)
                {
                    res.AppendLine(curEx.Message);
                }

                if (curEx.InnerException != null)
                {
                    ProcessException(curEx.InnerException);
                }
            }

            ProcessException(ex);

            if (res.Length == 0)
            {
                res.Append(genericError);
            }

            fullLog = fullLogBuilder.ToString();

            return res.ToString();
        }
    }
}
