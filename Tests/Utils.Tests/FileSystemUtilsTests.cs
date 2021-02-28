using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit;

namespace Utils.Tests
{
    public class FileSystemUtilsTests
    {
        [Test]
        public void IsInDirectoryTest() 
        {
            var r1 = FileSystemUtils.IsInDirectory(@"D:\1\2", @"D:\1\2\3");
            var r2 = FileSystemUtils.IsInDirectory(@"D:\1\2\3", @"D:\1\2");
            
            var r3 = FileSystemUtils.IsInDirectory(@"D:\1\2\", @"D:\1\2\3");
            var r4 = FileSystemUtils.IsInDirectory(@"D:\1\2\3\", @"D:\1\2");
            
            var r5 = FileSystemUtils.IsInDirectory(@"D:\1\2", @"D:\1\2\3\");
            var r6 = FileSystemUtils.IsInDirectory(@"D:\1\2\3", @"D:\1\2\");

            var r7 = FileSystemUtils.IsInDirectory(@"D:\1\2\", @"D:\1\2\3\");
            var r8 = FileSystemUtils.IsInDirectory(@"D:\1\2\3\", @"D:\1\2\");

            var r9 = FileSystemUtils.IsInDirectory(@"D:\a\bc", @"D:\a\bcd");
            var r10 = FileSystemUtils.IsInDirectory(@"D:\a\bcd", @"D:\a\bc");

            var r11 = FileSystemUtils.IsInDirectory(@"D:\b\c", @"D:\x\y");

            Assert.IsFalse(r1);
            Assert.IsTrue(r2);
            Assert.IsFalse(r3);
            Assert.IsTrue(r4);
            Assert.IsFalse(r5);
            Assert.IsTrue(r6);
            Assert.IsFalse(r7);
            Assert.IsTrue(r8);
            Assert.IsFalse(r9);
            Assert.IsFalse(r10);
            Assert.IsFalse(r11);
        }

        [Test]
        public void GetTopFoldersTest() 
        {
            var i1 = new string[]
            {
                @"D:\x\y\z",
                @"D:\x",
                @"D:\a\b",
                @"D:\a\b\c",
                @"D:\a\b\d\",
                @"D:\z\ab",
            };

            var r1 = FileSystemUtils.GetTopFolders(i1);

            CollectionAssert.AreEquivalent(new string[] { @"D:\x", @"D:\a\b", @"D:\z\ab" }, r1);
        }
    }
}
