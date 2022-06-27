using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reflection;

namespace Utils.Tests
{
    public class TypeExtensionsTests
    {
        public class Class1
        {
            public string Method1(string param) => "A";
            public string Method1<T>(List<T> param) => "B";
        }

        [Test]
        public void GetMethodWithGenericParametersTest() 
        {
            var m = typeof(Class1).GetMethodWithGenericParameters("Method1", new Type[] { typeof(List<>) });
            m = m.MakeGenericMethod(typeof(string));

            var r = m.Invoke(new Class1(), new object[] { new List<string>() });

            Assert.AreEqual("B", r);
        }
    }
}
