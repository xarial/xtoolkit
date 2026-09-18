//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Windows;
using System.Windows.Interop;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.XToolkit.Wpf.Services
{
    /// <summary>
    /// Represents the instance of the <see cref="IUserInputService"/> based on the <see cref="InputBoxDialog"/>
    /// </summary>
    public class UserInputService : IUserInputService
    {
        private readonly string m_Title;
        private readonly IParentWindow m_Parent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the input box</param>
        public UserInputService(string title)
            : this(title, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Title of the input box</param>
        /// <param name="parent">Parent window of the input box or null</param>
        public UserInputService(string title, IParentWindow parent)
        {
            m_Title = title;
            m_Parent = parent;
        }

        /// <summary>
        /// Requests the input value from the user
        /// </summary>
        /// <inheritdoc/>
        public bool TryGetInput(string prompt, ref string value)
            => ShowInputBox(prompt, null, ref value);

        /// <summary>
        /// Requests the input value from the user, input box is displayed at the specified position
        /// </summary>
        /// <param name="prompt">User prompt</param>
        /// <param name="pos">Position of the input box in device independent units</param>
        /// <param name="value">Initial value of the input and the value entered by the user. Unchanged if cancelled</param>
        public bool TryGetInput(string prompt, Point pos, ref string value)
            => ShowInputBox(prompt, pos, ref value);

        /// <summary>
        /// Requests the input value from the user, input box is displayed at the mouse cursor
        /// </summary>
        /// <param name="prompt">User prompt</param>
        /// <param name="value">Initial value of the input and the value entered by the user. Unchanged if cancelled</param>
        public bool TryGetInputAtCursor(string prompt, ref string value)
            => ShowInputBox(prompt, GetCursorPosition(), ref value);

        /// <summary>
        /// Display the input box
        /// </summary>
        /// <param name="title">Title of the input box</param>
        /// <param name="prompt">User prompt</param>
        /// <param name="pos">Position of the input box or null to center</param>
        /// <param name="value">Initial value of the input and the value entered by the user</param>
        /// <returns>True if user accepted the input</returns>
        /// <remarks>This method is called on the thread of the parent window</remarks>
        protected virtual bool DisplayInputBox(string title, string prompt, Point? pos, ref string value)
        {
            var dlg = new InputBoxDialog()
            {
                Title = title,
                Prompt = prompt,
                Value = value
            };

            bool hasOwner;

            switch (m_Parent)
            {
                case WpfParentWindow wpfParent when wpfParent.IsValidWindow():
                    dlg.Owner = wpfParent.Window;
                    hasOwner = true;
                    break;

                case IParentWindow parent when parent.IsValidWindow():
                    new WindowInteropHelper(dlg).Owner = parent.Handle;
                    hasOwner = true;
                    break;

                default:
                    hasOwner = false;
                    break;
            }

            if (pos.HasValue)
            {
                dlg.WindowStartupLocation = WindowStartupLocation.Manual;
                dlg.Left = pos.Value.X;
                dlg.Top = pos.Value.Y;
            }
            else
            {
                dlg.WindowStartupLocation = hasOwner ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
            }

            if (dlg.ShowDialog() == true)
            {
                value = dlg.Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ShowInputBox(string prompt, Point? pos, ref string value)
        {
            var result = value;

            var accepted = m_Parent.InvokeOnWindowThread(
                () => DisplayInputBox(m_Title, prompt, pos, ref result));

            if (accepted)
            {
                value = result;
            }

            return accepted;
        }

        private static Point GetCursorPosition()
        {
            var cursorPos = System.Windows.Forms.Cursor.Position;

            using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                const int DPI = 96;

                var scaleX = graphics.DpiX / DPI;
                var scaleY = graphics.DpiY / DPI;

                return new Point(cursorPos.X / scaleX, cursorPos.Y / scaleY);
            }
        }
    }
}