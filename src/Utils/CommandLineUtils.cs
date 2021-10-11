using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Xarial.XToolkit
{
    public static class CommandLineUtils
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] ParseCommandLine(string cmdLineArgs)
        {
            int count;
            var argsPtr = CommandLineToArgvW(cmdLineArgs, out count);

            if (argsPtr != IntPtr.Zero)
            {
                try
                {
                    var args = new string[count];

                    for (var i = 0; i < args.Length; i++)
                    {
                        var ptr = Marshal.ReadIntPtr(argsPtr, i * IntPtr.Size);
                        args[i] = Marshal.PtrToStringUni(ptr);
                    }

                    return args;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to parse arguments", ex);
                }
                finally
                {
                    Marshal.FreeHGlobal(argsPtr);
                }
            }
            else
            {
                throw new Exception("Failed to parse arguments, pointer is null");
            }
        }
    }
}
