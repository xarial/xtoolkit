using NUnit.Framework;
using System;
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
            var csv = "a,b,c\r\n1,2,3";

            var lines = new List<string[]>();

            using (var memStr = new MemoryStream(Encoding.UTF8.GetBytes(csv))) 
            {
                using (var streamReader = new StreamReader(memStr))
                {
                    using (var csvReader = new CsvReader(streamReader))
                    {
                        while (csvReader.HasContent)
                        {
                            lines.Add(csvReader.ReadLine().ToArray());
                        }
                    }
                }
            }

            Assert.AreEqual(2, lines.Count);
            CollectionAssert.AreEqual(new string[] { "a", "b", "c" }, lines[0]);
            CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, lines[1]);
        }


        [Test]
        public void ReadSpecialSymbols()
        {
            var csv = "a,\"b\r\ncd\",e,\r\n\"1\",\"2,3\",4,5\r\n6,7,8,";

            var lines = new List<string[]>();

            using (var memStr = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                using (var streamReader = new StreamReader(memStr))
                {
                    using (var csvReader = new CsvReader(streamReader))
                    {
                        while (csvReader.HasContent)
                        {
                            lines.Add(csvReader.ReadLine().ToArray());
                        }
                    }
                }
            }

            Assert.AreEqual(3, lines.Count);
            CollectionAssert.AreEqual(new string[] { "a", "b\r\ncd", "e", "" }, lines[0]);
            CollectionAssert.AreEqual(new string[] { "1", "2,3", "4", "5" }, lines[1]);
            CollectionAssert.AreEqual(new string[] { "6", "7", "8", "" }, lines[2]);
        }

        [Test]
        public void ReadNewLine()
        {
            var csv = "a,b,c\r\n1,2,3\r\n";

            var lines = new List<string[]>();

            using (var memStr = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                using (var streamReader = new StreamReader(memStr))
                {
                    using (var csvReader = new CsvReader(streamReader))
                    {
                        while (csvReader.HasContent)
                        {
                            lines.Add(csvReader.ReadLine().ToArray());
                        }
                    }
                }
            }

            Assert.AreEqual(2, lines.Count);
            CollectionAssert.AreEqual(new string[] { "a", "b", "c" }, lines[0]);
            CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, lines[1]);
        }
    }
}
