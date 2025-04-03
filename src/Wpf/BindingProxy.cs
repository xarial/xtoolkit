//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System.Windows;

namespace Xarial.XToolkit.Wpf
{
    /// <summary>
    /// Data container to store the data in the WPF resources
    /// </summary>
    /// <example>
    /// xmlns:xtlk="clr-namespace:Xarial.XToolkit.Wpf;assembly=Xarial.XToolkit.Wpf"
    /// ...
    /// xtlk:BindingProxy x:Key="proxy" Data={ Binding Path=.}
    /// ...
    /// Property={Binding Path=Data, Source={StaticResource proxy}}
    /// </example>
    public class BindingProxy : Freezable
    {
        /// <summary>
        /// Creates new instance
        /// </summary>
        /// <returns>Proxy instance</returns>
        protected override Freezable CreateInstanceCore()
            => new BindingProxy();

        /// <summary>
        /// Data of proxy
        /// </summary>
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object),
            typeof(BindingProxy));
    }
}
