using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reporting;

namespace Utils.Tests
{
    public class UserMessageExceptionTest
    {
        public class MyException : Exception, IUserMessageException
        {
            public MyException() : base("Sample Message") 
            {
            }
        }

        [Test]
        public void TestUserException() 
        {
            var ex = new MyException();
            var err = ex.ParseUserError(out _);

            Assert.AreEqual("Sample Message", err);
        }

        [Test]
        public void TestInnerUserException()
        {
            var ex = new Exception("ABC", new MyException());
            var err = ex.ParseUserError(out _);

            Assert.AreEqual("Sample Message", err);
        }

        [Test]
        public void TestStandardException()
        {
            var ex = new Exception("ABC");
            var err = ex.ParseUserError(out _);

            Assert.AreEqual("Generic error", err);
        }
    }
}
