using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Utils.Tests
{
    internal static class ShortPathHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);

        /// <summary>
        /// Returns the 8.3 short path or ignores the test if short names are not generated on this volume
        /// </summary>
        internal static string GetShortPathOrIgnore(string longPath)
        {
            var buffer = new StringBuilder(1024);

            var len = GetShortPathName(longPath, buffer, (uint)buffer.Capacity);

            if (len == 0 || len > buffer.Capacity)
            {
                Assert.Ignore($"Failed to get short path for '{longPath}'");
            }

            var shortPath = buffer.ToString();

            if (string.Equals(Path.GetFileName(shortPath), Path.GetFileName(longPath), StringComparison.OrdinalIgnoreCase))
            {
                Assert.Ignore("8.3 short names are not generated on this volume");
            }

            return shortPath;
        }
    }
}
