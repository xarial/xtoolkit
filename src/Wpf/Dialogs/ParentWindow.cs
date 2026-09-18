using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Xarial.XToolkit.Wpf.Dialogs
{
    /// <summary>
    /// Represents the parent window
    /// </summary>
    public interface IParentWindow
    {
        /// <summary>
        /// Handle of the window
        /// </summary>
        /// <remarks>Must be accessed from the thread of the <see cref="Dispatcher"/></remarks>
        IntPtr Handle { get; }

        /// <summary>
        /// Dispatcher of the thread which owns this window or null to use the calling thread
        /// </summary>
        Dispatcher Dispatcher { get; }
    }

    internal class Win32ParentWindow : IParentWindow
    {
        public IntPtr Handle { get; }
        public Dispatcher Dispatcher { get; }

        internal Win32ParentWindow(IntPtr handle, Dispatcher disp)
        {
            Handle = handle;
            Dispatcher = disp;
        }
    }

    internal class WpfParentWindow : IParentWindow
    {
        internal Window Window { get; }

        public IntPtr Handle => new WindowInteropHelper(Window).Handle;

        public Dispatcher Dispatcher => Window.Dispatcher;

        internal WpfParentWindow(Window wnd)
        {
            if (wnd == null)
            {
                throw new ArgumentNullException(nameof(wnd));
            }

            Window = wnd;
        }
    }

    /// <summary>
    /// Factory of the <see cref="IParentWindow"/>
    /// </summary>
    public static class ParentWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        /// <summary>
        /// Creates parent window from the native window handle
        /// </summary>
        /// <param name="handle">Window handle</param>
        /// <param name="disp">Dispatcher of the thread which owns this window or null to use the calling thread</param>
        /// <returns>Parent window</returns>
        public static IParentWindow FromHandle(IntPtr handle, Dispatcher disp = null)
            => new Win32ParentWindow(handle, disp);

        /// <summary>
        /// Creates parent window from the WPF window
        /// </summary>
        /// <param name="wnd">WPF window</param>
        /// <returns>Parent window</returns>
        public static IParentWindow FromWindow(Window wnd)
            => new WpfParentWindow(wnd);

        internal static bool IsValidWindow(this IParentWindow parentWnd)
        {
            if (parentWnd != null)
            {
                try
                {
                    return IsWindow(parentWnd.Handle);
                }
                catch
                {
                }
            }

            return false;
        }

        internal static TResult InvokeOnWindowThread<TResult>(this IParentWindow parentWnd, Func<TResult> callback)
        {
            var disp = parentWnd?.Dispatcher;

            if (disp != null && !disp.HasShutdownStarted && !disp.CheckAccess())
            {
                return disp.Invoke(callback);
            }
            else
            {
                return callback.Invoke();
            }
        }
    }
}
