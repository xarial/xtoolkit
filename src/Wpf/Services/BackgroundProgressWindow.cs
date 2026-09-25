//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Xarial.XToolkit.Wpf.Dialogs;
using static Xarial.XToolkit.Wpf.Services.BackgroundProgressWindowNative;

namespace Xarial.XToolkit.Wpf.Services
{
    /// <summary>
    /// Service to display progress message in background thread
    /// </summary>
    /// <typeparam name="T">Type of the progress window</typeparam>
    public interface IBackgroundProgressWindow<T> : IDisposable
        where T : Window
    {
        /// <summary>
        /// Window
        /// </summary>
        /// <remarks>Window is owned by the background thread and must only be accessed within the <see cref="BeginInvoke(Action{T})"/></remarks>
        T Window { get; }

        /// <summary>
        /// Invoke the window update status
        /// </summary>
        /// <param name="action">Action to invoke</param>
        /// <returns>Dispatcher operation</returns>
        /// <remarks>Action is not invoked if window is already closed</remarks>
        DispatcherOperation BeginInvoke(Action<T> action);
    }

    internal static class BackgroundProgressWindowNative
    {
        internal const uint SWP_NOSIZE = 0x0001;
        internal const uint SWP_NOZORDER = 0x0004;
        internal const uint SWP_NOACTIVATE = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    }

    /// <inheritdoc/>
    public class BackgroundProgressWindow<T> : IBackgroundProgressWindow<T>
        where T : Window
    {
        private const int SHUTDOWN_TIMEOUT = 1000;
        private const double OFF_SCREEN_POS = -32000;

        /// <inheritdoc/>
        public T Window { get; private set; }

        private readonly Thread m_Thread;
        private readonly Dispatcher m_Dispatcher;

        private volatile bool m_IsClosed;
        private bool m_IsDisposed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="wndFact">Window creation factory</param>
        public BackgroundProgressWindow(Func<T> wndFact) : this(wndFact, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="wndFact">Window creation factory</param>
        /// <param name="parent">Parent window of the progress window or null</param>
        /// <remarks>Window factory is called on the background thread.
        /// Window is not owned by the parent (to keep it responsive while the parent's thread is blocked); instead it is displayed topmost and centered over the parent</remarks>
        public BackgroundProgressWindow(Func<T> wndFact, IParentWindow parent = null)
        {
            if (wndFact == null)
            {
                throw new ArgumentNullException(nameof(wndFact));
            }

            var parentHandle = parent.InvokeOnWindowThread(() => parent.IsValidWindow() ? parent.Handle : IntPtr.Zero);

            Exception initErr = null;
            Dispatcher disp = null;

            using (var ready = new ManualResetEventSlim())
            {
                m_Thread = new Thread(() =>
                {
                    try
                    {
                        disp = Dispatcher.CurrentDispatcher;
                        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(disp));
                        disp.UnhandledException += OnDispatcherUnhandledException;

                        Window = wndFact.Invoke();

                        if (Window != null)
                        {
                            Window.Closed += OnWindowClosed;

                            Show(parentHandle);
                        }
                        else 
                        {
                            throw new NullReferenceException("Window factory returned null");
                        }
                    }
                    catch (Exception ex)
                    {
                        initErr = ex;
                    }
                    finally
                    {
                        ready.Set();
                    }

                    if (initErr == null)
                    {
                        Dispatcher.Run();
                    }
                    else
                    {
                        disp?.InvokeShutdown();
                    }
                });

                m_Thread.SetApartmentState(ApartmentState.STA);
                m_Thread.IsBackground = true;
                m_Thread.Start();

                ready.Wait();
            }

            if (initErr != null)
            {
                ExceptionDispatchInfo.Capture(initErr).Throw();
            }

            m_Dispatcher = disp;
        }

        /// <summary>
        /// Show window
        /// </summary>
        /// <param name="parentHandle"></param>
        protected virtual void Show(IntPtr parentHandle)
        {
            if (parentHandle != IntPtr.Zero)
            {
                var parentRect = default(RECT);

                var center = Window.WindowStartupLocation == WindowStartupLocation.Manual
                    && double.IsNaN(Window.Left) && double.IsNaN(Window.Top)
                    && !IsIconic(parentHandle)
                    && GetWindowRect(parentHandle, out parentRect);

                if (center)
                {
                    //NOTE: displaying off-screen first as the size of the window is only known once shown (e.g. SizeToContent)
                    Window.Left = OFF_SCREEN_POS;
                    Window.Top = OFF_SCREEN_POS;
                }

                Window.Show();

                if (center)
                {
                    var hWnd = new WindowInteropHelper(Window).Handle;

                    if (GetWindowRect(hWnd, out var wndRect))
                    {
                        var x = parentRect.Left + (parentRect.Right - parentRect.Left - (wndRect.Right - wndRect.Left)) / 2;
                        var y = parentRect.Top + (parentRect.Bottom - parentRect.Top - (wndRect.Bottom - wndRect.Top)) / 2;

                        SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
                    }
                }
            }
            else 
            {
                Window.Show();
            }
        }

        /// <inheritdoc/>
        public DispatcherOperation BeginInvoke(Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return m_Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!m_IsClosed)
                {
                    action.Invoke(Window);
                }
            }));
        }

        /// <summary>
        /// Called when unhandled exception is thrown on the thread of the progress window
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <remarks>Exceptions are handled by default to prevent the process termination</remarks>
        protected virtual void OnUnhandledException(Exception ex)
        {
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            try
            {
                OnUnhandledException(e.Exception);
            }
            catch
            {
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            m_IsClosed = true;
        }

        /// <summary>
        /// Disposing the background window service
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;

            if (!m_Dispatcher.HasShutdownStarted)
            {
                m_Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!m_IsClosed)
                    {
                        Window.Close();
                    }
                }));

                m_Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            }

            if (Thread.CurrentThread != m_Thread)
            {
                m_Thread.Join(SHUTDOWN_TIMEOUT);
            }
        }
    }
}
