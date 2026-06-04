//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Xarial.XToolkit.Services;

namespace Xarial.XToolkit.Wpf.Services
{
    /// <summary>
    /// Represents the instance of the <see cref="IMessageService"/> based on WPF message box
    /// </summary>
    public class WindowsMessageService : IMessageService
    {
        private readonly Type[] m_UserErrors;

        private readonly string m_Title;

        private readonly Dispatcher m_Dispatcher;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the message box</param>
        /// <param name="userErrors">Additional user errors</param>
        public WindowsMessageService(string title, Type[] userErrors) : this(title)
        {
            m_UserErrors = userErrors;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the mesage box</param>
        public WindowsMessageService(string title) : this(title, Dispatcher.CurrentDispatcher)
        {
            m_Title = title;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the mesage box</param>
        /// <param name="dispatcher">dispatcher for thread safety</param>
        protected WindowsMessageService(string title, Dispatcher dispatcher)
        {
            m_Title = title;
            m_Dispatcher = dispatcher;
        }

        /// <summary>
        /// Display the message box
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="title">Title of the message box</param>
        /// <param name="img">Image</param>
        /// <param name="btn">Buttons</param>
        /// <returns>Message box result</returns>
        protected virtual MessageBoxResult DisplayMessageBox(string msg, string title, MessageBoxImage img, MessageBoxButton btn)
        {
            MessageBoxResult Show() => MessageBox.Show(msg, title, btn, img);

            if (m_Dispatcher != null && m_Dispatcher.Thread != Thread.CurrentThread)
            {
                return m_Dispatcher.Invoke(Show);
            }
            else
            {
                return Show();
            }
        }

        /// <inheritdoc/>
        public virtual bool? ShowMessage(string msg, MessageServiceIcon_e icon, MessageServiceButtons_e btns)
        {
            MessageBoxImage msgBoxImg;
            MessageBoxButton msgBoxBtns;

            switch (icon)
            {
                case MessageServiceIcon_e.None:
                    msgBoxImg = MessageBoxImage.None;
                    break;

                case MessageServiceIcon_e.Information:
                    msgBoxImg = MessageBoxImage.Information;
                    break;

                case MessageServiceIcon_e.Warning:
                    msgBoxImg = MessageBoxImage.Warning;
                    break;

                case MessageServiceIcon_e.Error:
                    msgBoxImg = MessageBoxImage.Error;
                    break;

                case MessageServiceIcon_e.Question:
                    msgBoxImg = MessageBoxImage.Question;
                    break;

                default:
                    throw new NotSupportedException();
            }

            switch (btns)
            {
                case MessageServiceButtons_e.Ok:
                    msgBoxBtns = MessageBoxButton.OK;
                    break;

                case MessageServiceButtons_e.OkCancel:
                    msgBoxBtns = MessageBoxButton.OKCancel;
                    break;

                case MessageServiceButtons_e.YesNo:
                    msgBoxBtns = MessageBoxButton.YesNo;
                    break;

                case MessageServiceButtons_e.YesNoCancel:
                    msgBoxBtns = MessageBoxButton.YesNoCancel;
                    break;

                default:
                    throw new NotSupportedException();
            }

            switch (DisplayMessageBox(msg, m_Title, msgBoxImg, msgBoxBtns))
            {
                case MessageBoxResult.Yes:
                case MessageBoxResult.OK:
                    return true;

                case MessageBoxResult.No:
                    return false;

                case MessageBoxResult.Cancel:
                    return null;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public virtual string ParseError(Exception ex, string genericErrorMsg) => this.ParseExceptionError(ex, m_UserErrors, genericErrorMsg);
    }
}
