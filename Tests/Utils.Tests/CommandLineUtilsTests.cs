using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Xarial.XToolkit;

namespace Utils.Tests
{
    [TestFixture]
    public class CommandLineUtilsTests
    {
        [TestCase("abc", "abc")]
        [TestCase(@"C:\path\file.txt", @"C:\path\file.txt")]
        [TestCase("a b", @"""a b""")]
        [TestCase("", @"""""")]
        [TestCase(@"C:\My Files\", @"""C:\My Files\\""")]
        [TestCase(@"say ""hi""", @"""say \""hi\""""")]
        [TestCase(@"a\""b", @"""a\\\""b""")]
        [TestCase("tab\there", "\"tab\there\"")]
        public void EncodeArgumentProducesExpectedString(string input, string expected)
        {
            Assert.That(CommandLineUtils.Encode(input), Is.EqualTo(expected));
        }

        [Test]
        public void EncodeArgumentNullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => CommandLineUtils.Encode(null));
        }

        [Test]
        public void EncodeJoinsWithSpaces()
        {
            var result = CommandLineUtils.Encode(new[] { "a", "b c", "d" });
            Assert.That(result, Is.EqualTo(@"a ""b c"" d"));
        }

        [TestCase("simple", new[] { "simple" })]
        [TestCase(@"a ""b c"" """"", new[] { "a", "b c", "" })]
        [TestCase(@"""C:\My Files\\"" D:\out\", new[] { @"C:\My Files\", @"D:\out\" })]
        [TestCase(@"""say \""hi\"""" ""a\\\""b"" \\server\share\", new[] { @"say ""hi""", @"a\""b", @"\\server\share\" })]
        [TestCase(@"""\\\\\\\"""" x\\ ""\"""" """ + "\t" + @""" ""multi" + "\n" + @"line""", new[] { @"\\\""", @"x\\", "\"", "\t", "multi\nline" })]
        public void ParseExpectedArray(string input, string[] expected)
        {
            var args = CommandLineUtils.ParseCommandLine(input);

            Assert.That(args, Is.EqualTo(expected));
        }
    }
}