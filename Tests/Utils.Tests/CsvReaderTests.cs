//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using NUnit.Framework;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Services;

namespace Utils.Tests
{
    public class CsvReaderTests
    {
        [Test]
        public void ReadSimple() 
        {
            var lines = ReadAll("a,b,c\r\n1,2,3");

            Assert.AreEqual(2, lines.Count);
            CollectionAssert.AreEqual(new string[] { "a", "b", "c" }, lines[0]);
            CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, lines[1]);
        }


        [Test]
        public void ReadSpecialSymbols()
        {
            var lines = ReadAll("a,\"b\r\ncd\",e,\r\n\"1\",\"2,3\",4,5\r\n6,7,8,");

            Assert.AreEqual(3, lines.Count);
            CollectionAssert.AreEqual(new string[] { "a", "b\r\ncd", "e", "" }, lines[0]);
            CollectionAssert.AreEqual(new string[] { "1", "2,3", "4", "5" }, lines[1]);
            CollectionAssert.AreEqual(new string[] { "6", "7", "8", "" }, lines[2]);
        }

        [Test]
        public void ReadQuote() 
        {
            var lines = ReadAll("\"\"\"a\"\"\",\"\"\"b,c\"\"\",\"d\"\"e\"\"f\",\"g\"\"h\"");

            Assert.AreEqual(1, lines.Count);
            CollectionAssert.AreEqual(new string[] { "\"a\"", "\"b,c\"", "d\"e\"f", "g\"h" }, lines[0]);
        }

        [Test]
        public void ReadQuoteInUnquotedCell()
        {
            var lines = ReadAll("a\"b\",\"c\"\r\n\"d\",e\"f\"g");

            Assert.AreEqual(2, lines.Count);
            CollectionAssert.AreEqual(new string[] { "a\"b\"", "c" }, lines[0]);
            CollectionAssert.AreEqual(new string[] { "d", "e\"f\"g" }, lines[1]);
        }

        [Test]
        public void ReadCrLineEnding()
        {
            var lines = ReadAll("a,b\rc,d\r");

            Assert.AreEqual(2, lines.Count);
            CollectionAssert.AreEqual(new string[] { "a", "b" }, lines[0]);
            CollectionAssert.AreEqual(new string[] { "c", "d" }, lines[1]);
        }

        [Test]
        public void ReadEofInUnclosedQuotedCell()
        {
            var lines = ReadAll("\"a\"\"");

            Assert.AreEqual(1, lines.Count);
            CollectionAssert.AreEqual(new string[] { "a\"" }, lines[0]);
        }

        [Test]
        public void ReadEmptyCells()
        {
            var lines = ReadAll("a,\r\nb,\"\"\r\nc,\r\nd,\"\"\r\n");

            Assert.AreEqual(4, lines.Count);
            CollectionAssert.AreEqual(new string[] { "a", "" }, lines[0]);
            CollectionAssert.AreEqual(new string[] { "b", "" }, lines[1]);
            CollectionAssert.AreEqual(new string[] { "c", "" }, lines[2]);
            CollectionAssert.AreEqual(new string[] { "d", "" }, lines[3]);
        }

        [Test]
        public void EmptyHasContent()
        {
            using (var r = new CsvReader(new StringReader("")))
            {
                Assert.IsFalse(r.HasContent);
            }
        }

        [Test]
        public void EmptyReadLineException()
        {
            using (var r = new CsvReader(new StringReader(""))) 
            {
                Assert.Throws<Exception>(() => r.ReadLine().ToArray());
            }
        }

        [Test]
        public void SingleField()
        {
            var rows = ReadAll("hello");
            Assert.AreEqual(1, rows.Count);
            CollectionAssert.AreEqual(new[] { "hello" }, rows[0]);
        }

        [Test]
        public void MultipleFields()
        {
            var rows = ReadAll("a,b,c");

            Assert.AreEqual(1, rows.Count);
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, rows[0]);
        }

        [Test]
        public void EmptyField()
        {
            var rows = ReadAll("a,,c");
            CollectionAssert.AreEqual(new[] { "a", "", "c" }, rows[0]);
        }

        [Test]
        public void EmptyQuotedField()
        {
            var rows = ReadAll("a,\"\",c");
            CollectionAssert.AreEqual(new[] { "a", "", "c" }, rows[0]);
        }

        [Test]
        public void LastRecordNoTrailingCRLF()
        {
            var rows = ReadAll("a,b\r\nc,d");
            Assert.AreEqual(2, rows.Count);
            CollectionAssert.AreEqual(new[] { "c", "d" }, rows[1]);
        }

        [Test]
        public void LastRecordWithTrailingCRLF()
        {
            var rows = ReadAll("a,b\r\nc,d\r\n");
            Assert.AreEqual(2, rows.Count);
        }

        [Test]
        public void HeaderPlusDataRows()
        {
            var rows = ReadAll("name,age,city\r\nAlice,30,NYC\r\nBob,25,LA");
            Assert.AreEqual(3, rows.Count);
            CollectionAssert.AreEqual(new[] { "name", "age", "city" }, rows[0]);
            CollectionAssert.AreEqual(new[] { "Alice", "30", "NYC" }, rows[1]);
            CollectionAssert.AreEqual(new[] { "Bob", "25", "LA" }, rows[2]);
        }

        [Test]
        public void BareLFRecordSeparator()
        {
            var rows = ReadAll("a,b\nc,d");
            Assert.AreEqual(2, rows.Count);
            CollectionAssert.AreEqual(new[] { "c", "d" }, rows[1]);
        }

        [Test]
        public void WhitespacePreserved()
        {
            var rows = ReadAll(" a , b ");
            CollectionAssert.AreEqual(new[] { " a ", " b " }, rows[0]);
        }

        [Test]
        public void HasContentWithData()
        {
            using (var r = new CsvReader(new StringReader("a,b")))
            {
                Assert.IsTrue(r.HasContent);
            }
        }

        [Test]
        public void HasContentNoContent()
        {
            using (var r = new CsvReader(new StringReader("a,b")))
            {
                var l = r.ReadLine().ToArray();
                Assert.IsFalse(r.HasContent);
            }
        }

        private IReadOnlyList<string[]> ReadAll(string csv)
        {
            var lines = new List<string[]>();

            using (var reader = new CsvReader(new StringReader(csv)))
            {
                while (reader.HasContent)
                {
                    lines.Add(reader.ReadLine().ToArray());
                }
            }

            return lines;
        }
    }
}
