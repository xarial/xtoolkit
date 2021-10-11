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

        [Test]
        public void CombinePathsTest() 
        {
            var workDir1 = @"C:\folder1\folder2";
            var workDir2 = @"\\server1\folder1\folder2";
            var workDir3 = @"folder1\folder2";

            var p1 = FileSystemUtils.CombinePaths(workDir1, @"abc\xyz.txt");
            var p2 = FileSystemUtils.CombinePaths(workDir1, @"\abc\xyz.txt");
            var p3 = FileSystemUtils.CombinePaths(workDir1, @"..\abc\xyz.txt");
            var p4 = FileSystemUtils.CombinePaths(workDir1, @"\..\abc\xyz.txt");
            var p5 = FileSystemUtils.CombinePaths(workDir1, @"..\..\abc\xyz.txt");
            var p6 = FileSystemUtils.CombinePaths(workDir1, @"\..\..\abc\xyz.txt");
            
            var p7 = FileSystemUtils.CombinePaths(workDir2, @"abc\xyz.txt");
            var p8 = FileSystemUtils.CombinePaths(workDir2, @"\abc\xyz.txt");
            var p9 = FileSystemUtils.CombinePaths(workDir2, @"..\abc\xyz.txt");
            var p10 = FileSystemUtils.CombinePaths(workDir2, @"\..\abc\xyz.txt");
            var p11 = FileSystemUtils.CombinePaths(workDir2, @"..\..\abc\xyz.txt");
            var p12 = FileSystemUtils.CombinePaths(workDir2, @"\..\..\abc\xyz.txt");
            
            var p13 = FileSystemUtils.CombinePaths(workDir3, @"abc\xyz.txt");
            var p14 = FileSystemUtils.CombinePaths(workDir3, @"\abc\xyz.txt");
            var p15 = FileSystemUtils.CombinePaths(workDir3, @"..\abc\xyz.txt");
            var p16 = FileSystemUtils.CombinePaths(workDir3, @"\..\abc\xyz.txt");
            var p17 = FileSystemUtils.CombinePaths(workDir3, @"..\..\abc\xyz.txt");
            var p18 = FileSystemUtils.CombinePaths(workDir3, @"\..\..\abc\xyz.txt");

            Assert.AreEqual(p1, @"C:\folder1\folder2\abc\xyz.txt");
            Assert.AreEqual(p2, @"C:\folder1\folder2\abc\xyz.txt");
            Assert.AreEqual(p3, @"C:\folder1\abc\xyz.txt");
            Assert.AreEqual(p4, @"C:\folder1\abc\xyz.txt");
            Assert.AreEqual(p5, @"C:\abc\xyz.txt");
            Assert.AreEqual(p6, @"C:\abc\xyz.txt");
            
            Assert.AreEqual(p7, @"\\server1\folder1\folder2\abc\xyz.txt");
            Assert.AreEqual(p8, @"\\server1\folder1\folder2\abc\xyz.txt");
            Assert.AreEqual(p9, @"\\server1\folder1\abc\xyz.txt");
            Assert.AreEqual(p10, @"\\server1\folder1\abc\xyz.txt");
            Assert.AreEqual(p11, @"\\server1\abc\xyz.txt");
            Assert.AreEqual(p12, @"\\server1\abc\xyz.txt");
            
            Assert.AreEqual(p13, @"folder1\folder2\abc\xyz.txt");
            Assert.AreEqual(p14, @"folder1\folder2\abc\xyz.txt");
            Assert.AreEqual(p15, @"folder1\abc\xyz.txt");
            Assert.AreEqual(p16, @"folder1\abc\xyz.txt");
            Assert.AreEqual(p17, @"abc\xyz.txt");
            Assert.AreEqual(p18, @"abc\xyz.txt");
        }
    }
}
