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
    public class CsvWriterTests
    {
        [Test]
        public void WriteSimple() 
        {
            string csv;

            using (var memStr = new MemoryStream())
            {
                using (var writer = new StreamWriter(memStr))
                {
                    using (var csvWriter = new CsvWriter(writer))
                    {
                        csvWriter.WriteLine(new string[] { "a", "b", "c" });
                        csvWriter.WriteLine(new string[] { "1", "2", "3" });
                    }
                }

                csv = Encoding.UTF8.GetString(memStr.ToArray());
            }

            Assert.AreEqual("a,b,c\r\n1,2,3", csv);
        }


        [Test]
        public void WriteSpecialSymbols()
        {
            string csv;

            using (var memStr = new MemoryStream())
            {
                using (var writer = new StreamWriter(memStr))
                {
                    using (var csvWriter = new CsvWriter(writer))
                    {
                        csvWriter.WriteLine(new string[] { "a", "b\r\ncd", "e", "" });
                        csvWriter.WriteLine(new string[] { "1", "2,3", "4", "5" });
                        csvWriter.WriteLine(new string[] { "6", "7", "8", "" });
                    }
                }

                csv = Encoding.UTF8.GetString(memStr.ToArray());
            }

            Assert.AreEqual("a,\"b\r\ncd\",e,\r\n1,\"2,3\",4,5\r\n6,7,8,", csv);
        }
    }
}
