using NUnit.Framework;
using System;
using System.Globalization;
using Xarial.XToolkit.Wpf.Converters;

namespace Wpf.Tests
{
    public class BooleanUniversalConverterTest
    {
        [Test]
        public void TestSimple() 
        {
            var conv = new BooleanUniversalConverter();
            conv.TrueValue = "A";
            conv.FalseValue = "B";

            var res1 = conv.Convert(true, typeof(bool), null, CultureInfo.CurrentCulture);
            var res2 = conv.Convert(false, typeof(bool), null, CultureInfo.CurrentCulture);

            Assert.AreEqual("A", res1);
            Assert.AreEqual("B", res2);
            Assert.Throws<NotImplementedException>(() => conv.ConvertBack("", typeof(bool), null, CultureInfo.CurrentCulture));
        }
    }
}