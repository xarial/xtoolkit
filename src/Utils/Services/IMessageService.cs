//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XToolkit.Reporting;

namespace Xarial.XToolkit.Services
{
    /// <summary>
    /// Icon to show on the <see cref="IMessageService"/>
    /// </summary>
    public enum MessageServiceIcon_e
    {
        /// <summary>
        /// Information icon
        /// </summary>
        Information,

        /// <summary>
        /// Warning icon
        /// </summary>
        Warning,

        /// <summary>
        /// Error icon
        /// </summary>
        Error,

        /// <summary>
        /// Question icon
        /// </summary>
        Question
    }

    /// <summary>
    /// Buttons to show on the <see cref="IMessageService"/>
    /// </summary>
    public enum MessageServiceButtons_e
    {
        /// <summary>
        /// OK button
        /// </summary>
        Ok,

        /// <summary>
        /// OK and Cancel buttons
        /// </summary>
        OkCancel,

        /// <summary>
        /// Yes and No buttons
        /// </summary>
        YesNo,

        /// <summary>
        /// Yes, No and Cancel buttons
        /// </summary>
        YesNoCancel
    }

    /// <summary>
    /// Represents the service to show messages to the user
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Shows message
        /// </summary>
        /// <param name="msg">Message to show</param>
        /// <param name="icon">Message box icon</param>
        /// <param name="btns">Message box buttons</param>
        /// <returns>True for Ok or Yes, False for No, null for Cancel</returns>
        bool? ShowMessage(string msg, MessageServiceIcon_e icon, MessageServiceButtons_e btns);

        /// <summary>
        /// Method to parse user error from the <see cref="Exception"/>
        /// </summary>
        /// <param name="ex">Exception to parse error from</param>
        /// <param name="unknownErrorMsg">Message for the unknown error</param>
        /// <returns>User error to show</returns>
        string ParseError(Exception ex, string unknownErrorMsg);
    }

    /// <summary>
    /// Additional methods for <see cref="IMessageService"/>
    /// </summary>
    public static class IMessageServiceExtension
    {
        /// <summary>
        /// Shows the error box with a custom message
        /// </summary>
        /// <param name="msgSvc">Message service</param>
        /// <param name="error">Content of the error to show</param>
        public static void ShowError(this IMessageService msgSvc, string error)
            => msgSvc.ShowMessage(error, MessageServiceIcon_e.Error, MessageServiceButtons_e.Ok);

        /// <summary>
        /// Shows the warning box with a custom message
        /// </summary>
        /// <param name="msgSvc">Message service</param>
        /// <param name="warn">Content of the warning to show</param>
        public static void ShowWarning(this IMessageService msgSvc, string warn)
            => msgSvc.ShowMessage(warn, MessageServiceIcon_e.Warning, MessageServiceButtons_e.Ok);

        /// <summary>
        /// Shows the information box with a custom message
        /// </summary>
        /// <param name="msgSvc">Message service</param>
        /// <param name="msg">Content of the message to show</param>
        public static void ShowInformation(this IMessageService msgSvc, string msg)
            => msgSvc.ShowMessage(msg, MessageServiceIcon_e.Information, MessageServiceButtons_e.Ok);

        /// <summary>
        /// Shows the question box with a custom message
        /// </summary>
        /// <param name="msgSvc">Message service</param>
        /// <param name="question">Content of the question to show</param>
        public static bool? ShowQuestion(this IMessageService msgSvc, string question)
            => msgSvc.ShowMessage(question, MessageServiceIcon_e.Question, MessageServiceButtons_e.YesNoCancel);

        /// <summary>
        /// Shows the error box base on the exception
        /// </summary>
        /// <param name="msgSvc">Message service</param>
        /// <param name="ex">Exception of the error</param>
        /// <param name="unknownErrorMsg">Message for the unknown error</param>
        public static void ShowError(this IMessageService msgSvc, Exception ex, string unknownErrorMsg = "Generic error")
        {
            var err = msgSvc.ParseError(ex, unknownErrorMsg);
            msgSvc.ShowError(err);
        }

        /// <summary>
        /// Parses the user error from the exception
        /// </summary>
        /// <param name="msgSvc">Messgae service</param>
        /// <param name="ex">Exception to parse</param>
        /// <param name="additionalUserExceptions">Type of exceptions which should be considered as user friendly errors</param>
        /// <param name="unknownErrorMsg">Message for the unknown error</param>
        /// <returns></returns>
        public static string ParseError(this IMessageService msgSvc, Exception ex, Type[] additionalUserExceptions, string unknownErrorMsg = "Generic error") 
            => ex.ParseUserError(out _, unknownErrorMsg, additionalUserExceptions ?? new Type[0]);
    }
}
