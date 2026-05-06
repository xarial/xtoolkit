//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Themes;

namespace Xarial.XToolkit.Wpf.Controls
{
    /// <summary>
    /// Extended PasswordBox control
    /// </summary>
    public class PasswordBoxEx : Control
    {
        static PasswordBoxEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PasswordBoxEx),
                new FrameworkPropertyMetadata(typeof(PasswordBoxEx)));

            StyleProperty.OverrideMetadata(typeof(PasswordBoxEx),
                new FrameworkPropertyMetadata(StyleLoader.LoadStyle<PasswordBoxEx>("PasswordBoxEx.xaml")));
        }

        private PasswordBox m_PasswordBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_PasswordBox = (PasswordBox)this.Template.FindName("PART_PasswordBox", this);

            SetPassword(Password);

            m_PasswordBox.PasswordChanged += OnPasswordChanged;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = m_PasswordBox.Password;
        }

        /// <summary>
        /// Password
        /// </summary>
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(
            nameof(Password), typeof(string),
            typeof(PasswordBoxEx),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

        /// <summary>
        /// Password
        /// </summary>
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PasswordBoxEx)d).SetPassword((string)e.NewValue);
        }

        private void SetPassword(string password)
        {
            if (m_PasswordBox != null)
            {
                if (m_PasswordBox.Password != password)
                {
                    m_PasswordBox.Password = password;
                }
            }
        }
    }
}
