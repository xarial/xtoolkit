//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.XToolkit.Wpf.Services
{
    /// <summary>
    /// Represents the instance of the <see cref="IAboutService"/> based on the <see cref="AboutDialog"/>
    /// </summary>
    public class AboutService : IAboutService
    {
        private readonly AboutDialogSpec m_Spec;
        private readonly IParentWindow m_Parent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assm">Assembly to read the product information from</param>
        public AboutService(Assembly assm)
            : this(new AboutDialogSpec(assm), null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assm">Assembly to read the product information from</param>
        /// <param name="parent">Parent window of the about dialog or null</param>
        public AboutService(Assembly assm, IParentWindow parent)
            : this(new AboutDialogSpec(assm), parent)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assm">Assembly to read the product information from</param>
        /// <param name="logo">Product logo</param>
        /// <param name="parent">Parent window of the about dialog or null</param>
        public AboutService(Assembly assm, Image logo, IParentWindow parent)
            : this(new AboutDialogSpec(assm, logo), parent)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="spec">Specification of the about dialog</param>
        /// <param name="parent">Parent window of the about dialog or null</param>
        public AboutService(AboutDialogSpec spec, IParentWindow parent)
        {
            if (spec == null)
            {
                throw new ArgumentNullException(nameof(spec));
            }

            m_Spec = spec;
            m_Parent = parent;
        }

        /// <inheritdoc/>
        public void ShowAbout()
            => m_Parent.InvokeOnWindowThread(() =>
            {
                DisplayAboutDialog(m_Spec);
                return true;
            });

        /// <summary>
        /// Display the about dialog
        /// </summary>
        /// <param name="spec">Specification of the about dialog</param>
        protected virtual void DisplayAboutDialog(AboutDialogSpec spec)
        {
            var dlg = new AboutDialog()
            {
                DataContext = spec
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

            dlg.WindowStartupLocation = hasOwner ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;

            dlg.ShowDialog();
        }
    }
}