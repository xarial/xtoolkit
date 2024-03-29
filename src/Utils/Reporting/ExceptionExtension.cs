﻿//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xarial.XToolkit.Reporting
{
    /// <summary>
    /// Addition methods for the <see cref="Exception"/>
    /// </summary>
    public static class ExceptionExtension
    {
        /// <summary>
        /// Types of <see cref="Exception"/> to be recognized as user-friendly
        /// </summary>
        public static List<Type> GlobalUserExceptionTypes { get; }

        /// <summary>
        /// Generic user error if no user specific exceptions found
        /// </summary>
        public static string GlobalGenericErrorMessage { get; }

        static ExceptionExtension() 
        {
            GlobalUserExceptionTypes = new List<Type>();
            GlobalGenericErrorMessage = "Generic error";
        }

        /// <summary>
        /// Parses the exception error and extract user visible error of <see cref="IUserMessageException"/>
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="fullLog">Full log (including non user errors)</param>
        /// <param name="genericError">Text of generic error if no user error found. If empty <see cref="GlobalGenericErrorMessage"/> is used</param>
        /// <param name="additionalUserExceptions">Additional types to treat as user exception</param>
        /// <returns>user friendly error</returns>
        public static string ParseUserError(this Exception ex,
            out string fullLog, string genericError = "", params Type[] additionalUserExceptions)
        {
            var res = new List<string>();
            var fullLogBuilder = new StringBuilder();

            additionalUserExceptions = additionalUserExceptions.Union(GlobalUserExceptionTypes).ToArray();

            void ProcessException(Exception curEx)
            {
                fullLogBuilder.AppendLine(curEx.Message);
                fullLogBuilder.AppendLine(curEx.StackTrace);

                if (curEx is IUserMessageException || additionalUserExceptions.Any(t => t.IsAssignableFrom(curEx.GetType())))
                {
                    if (!res.Contains(curEx.Message))
                    {
                        res.Add(curEx.Message);
                    }
                }

                if (curEx.InnerException != null)
                {
                    ProcessException(curEx.InnerException);
                }
            }

            ProcessException(ex);

            if (!res.Any())
            {
                if (string.IsNullOrEmpty(genericError))
                {
                    genericError = GlobalGenericErrorMessage;
                }

                res.Add(genericError);
            }

            fullLog = fullLogBuilder.ToString();

            return string.Join(Environment.NewLine, res);
        }
    }
}
