//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf.Dialogs;
using WinForms = System.Windows.Forms;

namespace Xarial.XToolkit.Wpf.Services
{
    /// <summary>
    /// Represents the instance of the <see cref="IMessageService"/> based on the message box
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly Type[] m_UserErrors;
        private readonly string m_Title;
        private readonly IParentWindow m_Parent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the message box</param>
        public MessageService(string title)
            : this(title, Type.EmptyTypes)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the message box</param>
        /// <param name="userErrors">Additional user errors</param>
        public MessageService(string title, Type[] userErrors)
            : this(title, default(IParentWindow), userErrors)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the message box</param>
        /// <param name="parent">Parent window of the message box or null</param>
        /// <param name="userErrors">Additional user errors</param>
        public MessageService(string title, IParentWindow parent, Type[] userErrors) : this(title, parent)
        {
            m_UserErrors = userErrors;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the message box</param>
        /// <param name="parent">Parent window of the message box or null</param>
        public MessageService(string title, IParentWindow parent)
        {
            m_Title = title;
            m_Parent = parent;
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
            MessageBoxResult Show()
            {
                try
                {
                    switch (m_Parent)
                    {
                        case WpfParentWindow wpfParent when wpfParent.IsValidWindow():
                            return MessageBox.Show(wpfParent.Window, msg, title, btn, img);

                        case IParentWindow parent when parent.IsValidWindow():
                            return (MessageBoxResult)WinForms.MessageBox.Show(new Win32Window(parent.Handle), msg, title,
                                (WinForms.MessageBoxButtons)btn, (WinForms.MessageBoxIcon)img);

                    }
                }
                catch
                {
                }

                return MessageBox.Show(msg, title, btn, img);
            }

            return m_Parent.InvokeOnWindowThread(Show);
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