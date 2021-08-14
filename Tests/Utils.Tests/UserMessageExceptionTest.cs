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

        public interface ICustomUserException1 
        {
        }

        public interface ICustomUserException2
        {
        }

        public class MyException1 : Exception, ICustomUserException1 
        {
            public MyException1() : base("Message1") 
            {
            }
        }

        public class MyException2 : Exception, ICustomUserException2
        {
            public MyException2() : base("Message2")
            {
            }
        }

        [Test]
        public void TestAdditionalTypeException()
        {
            ExceptionExtension.GlobalUserExceptionTypes.Add(typeof(ICustomUserException2));

            var ex1 = new MyException1();
            var err1 = ex1.ParseUserError(out _, "" , typeof(ICustomUserException1));

            var ex2 = new MyException2();
            var err2 = ex2.ParseUserError(out _);

            Assert.AreEqual("Message1", err1);
            Assert.AreEqual("Message2", err2);
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
