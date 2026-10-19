//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Xarial.XToolkit
{
    /// <summary>
    /// Provides utility functions for working with command line
    /// </summary>
    public static class CommandLineUtils
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        private static readonly char[] m_SpecialChars = new[] { ' ', '\t', '\n', '\v', '"' };

        /// <summary>
        /// Extracts arguments from the command line string
        /// </summary>
        /// <param name="cmdLineArgs">Command line arguments</param>
        /// <returns>Separated arguments</returns>
        /// <exception cref="Exception"></exception>
        public static string[] ParseCommandLine(string cmdLineArgs)
        {
            //NOTE: CommandLineToArgvW parses the first token as a program path (no escaping rules),
            //so prepend a dummy one and skip it in the result
            var argsPtr = CommandLineToArgvW("app.exe " + cmdLineArgs, out var count);

            if (argsPtr == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to parse arguments");
            }

            try
            {
                var args = new string[count - 1];

                for (var i = 1; i < count; i++)
                {
                    var ptr = Marshal.ReadIntPtr(argsPtr, i * IntPtr.Size);
                    args[i - 1] = Marshal.PtrToStringUni(ptr);
                }

                return args;
            }
            finally
            {
                LocalFree(argsPtr);
            }
        }

        /// <summary>
        /// Encodes arguments into a command line
        /// </summary>
        /// <param name="args">Arguments to encode</param>
        /// <returns>Command line string</returns>
        public static string Encode(params string[] args)
            => string.Join(" ", args.Select(EncodeArgument));

        private static string EncodeArgument(string arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            if (arg.Length > 0 && arg.IndexOfAny(m_SpecialChars) == -1)
            {
                return arg;
            }

            var sb = new StringBuilder();
            sb.Append('"');

            for (int i = 0; i < arg.Length; i++)
            {
                var backslashes = 0;

                while (i < arg.Length && arg[i] == '\\')
                {
                    backslashes++;
                    i++;
                }

                if (i == arg.Length)
                {
                    sb.Append('\\', backslashes * 2);
                    break;
                }
                else if (arg[i] == '"')
                {
                    sb.Append('\\', backslashes * 2 + 1);
                    sb.Append('"');
                }
                else
                {
                    sb.Append('\\', backslashes);
                    sb.Append(arg[i]);
                }
            }

            sb.Append('"');
            return sb.ToString();
        }
    }
}
