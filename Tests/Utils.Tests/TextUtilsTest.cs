//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit;

namespace Utils.Tests
{
    public class TextUtilsTest
    {
        [Test]
        public void MatchesAnyFilterTest()
        {
            var r1 = TextUtils.MatchesAnyFilter("D:\\myfile1.txt", "*");
            var r2 = TextUtils.MatchesAnyFilter("D:\\myfile1.txt", "*.txt");
            var r3 = TextUtils.MatchesAnyFilter("D:\\myfile1.bin", "*.txt");
            var r4 = TextUtils.MatchesAnyFilter("D:\\myfile1.bin", "*fil*");
            var r5 = TextUtils.MatchesAnyFilter("D:\\myfile1.bin", "*fil");
            var r6 = TextUtils.MatchesAnyFilter("D:\\myfile1.bin", "fil");
            var r7 = TextUtils.MatchesAnyFilter("D:\\myfile1.bin", "fil", "*1*");
            var r8 = TextUtils.MatchesAnyFilter("XYZ", true, default(TimeSpan?), null);
            var r9 = TextUtils.MatchesAnyFilter("XYZ");
            var r10 = TextUtils.MatchesAnyFilter("", "*");
            var r11 = TextUtils.MatchesAnyFilter("XYZ", "XYZ*");
            var r12 = TextUtils.MatchesAnyFilter("XYZ", "*XYZ");
            var r13 = TextUtils.MatchesAnyFilter("XYZ", "*XYZ*");
            var r14 = TextUtils.MatchesAnyFilter("", "");

            Assert.IsTrue(r1);
            Assert.IsTrue(r2);
            Assert.IsFalse(r3);
            Assert.IsTrue(r4);
            Assert.IsFalse(r5);
            Assert.IsFalse(r6);
            Assert.IsTrue(r7);
            Assert.IsTrue(r8);
            Assert.IsTrue(r9);
            Assert.IsFalse(r10);
            Assert.IsTrue(r11);
            Assert.IsTrue(r12);
            Assert.IsTrue(r13);
            Assert.IsTrue(r14);
        }

        [TestCase("Part1.SLDPRT", "*.sldprt", true)]
        [TestCase("Part1.SLDPRT", "Part?.SLDPRT", true)]
        [TestCase("Part12.SLDPRT", "Part?.SLDPRT", false)]
        [TestCase("Assm.SLDASM", "*.SLDPRT", false)]
        [TestCase("abc", "a*b*c", true)]
        [TestCase("ac", "a*b*c", false)]
        [TestCase("abc", "*", true)]
        public void LikeWildcardsTest(string text, string pattern, bool expected)
        {
            Assert.AreEqual(expected, TextUtils.Like(text, pattern));
        }

        [TestCase("PRT-00123", "PRT-#####", true)]
        [TestCase("PRT-0012A", "PRT-#####", false)]
        [TestCase("PRT-123", "PRT-#####", false)]
        [TestCase("Rev3", "Rev#*", true)]
        [TestCase("a#b", "a#b", false)]
        public void LikeDigitTest(string text, string pattern, bool expected)
        {
            Assert.AreEqual(expected, TextUtils.Like(text, pattern));
        }

        [Test]
        public void LikeCaseAndEmptyTest()
        {
            Assert.IsTrue(TextUtils.Like("PART1", "part1", LikeOptions_e.IgnoreCase));
            Assert.IsFalse(TextUtils.Like("PART1", "part1", LikeOptions_e.None));
            Assert.IsTrue(TextUtils.Like("PART1", "PART1", LikeOptions_e.None));
            Assert.IsFalse(TextUtils.Like("", "*", LikeOptions_e.IgnoreCase));
            Assert.IsFalse(TextUtils.Like("abc", "", LikeOptions_e.IgnoreCase));
            Assert.IsFalse(TextUtils.Like(null, "*", LikeOptions_e.IgnoreCase));
        }

        [TestCase("xyz#", "*#", false)]
        [TestCase("xyz#", "*[#]", true)]
        [TestCase("xyz1", "*[#]", false)]
        [TestCase("50%*", "##%[*]", true)]
        [TestCase("50%x", "##%[*]", false)]
        [TestCase("Why?", "*[?]", true)]
        [TestCase("Why", "*[?]", false)]
        [TestCase("[a]", "[[]a]", true)]
        [TestCase("[ab]", "[ab]", true)]
        public void LikeLiteralTest(string text, string pattern, bool expected)
        {
            Assert.AreEqual(expected, TextUtils.Like(text, pattern));
        }
    }
}
