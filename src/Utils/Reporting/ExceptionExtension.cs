//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xarial.XToolkit.Reporting
{
    public static class ExceptionExtension
    {
        public static List<Type> GlobalUserExceptionTypes { get; }

        static ExceptionExtension() 
        {
            GlobalUserExceptionTypes = new List<Type>();
        }

        /// <summary>
        /// Parses the exception error and extract user visible error of <see cref="IUserMessageException"/>
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="fullLog">Full log (including non user errors)</param>
        /// <param name="genericError">Text of generic error if no user error found</param>
        /// <param name="additionalUserExceptions">Additional types to treat as user exception</param>
        /// <returns>user friendly error</returns>
        public static string ParseUserError(this Exception ex,
            out string fullLog, string genericError = "Generic error", params Type[] additionalUserExceptions)
        {
            var res = new StringBuilder();
            var fullLogBuilder = new StringBuilder();

            additionalUserExceptions = additionalUserExceptions.Union(GlobalUserExceptionTypes).ToArray();

            void ProcessException(Exception curEx)
            {
                fullLogBuilder.AppendLine(curEx.Message);
                fullLogBuilder.AppendLine(curEx.StackTrace);

                if (curEx is IUserMessageException || additionalUserExceptions.Any(t => t.IsAssignableFrom(curEx.GetType())))
                {
                    if (res.Length != 0)
                    {
                        res.Append(Environment.NewLine);
                    }

                    res.Append(curEx.Message);
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
