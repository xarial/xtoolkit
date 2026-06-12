using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reporting;

namespace Utils.Tests
{
    public class LoggerTests
    {
        private class LoggerMock : TraceLogger
        {
            private readonly List<string> m_Msgs;

            public LoggerMock(List<string> msgs, string category, bool singleLine = true) : base(category, singleLine)
            {
                m_Msgs = msgs;
            }

            protected override void WriteLine(string line) => m_Msgs.Add(line);
        }

        [Test]
        public void TestLogSingleLineNoStackTrace() 
        {
            var ex = new Exception("Test1", new Exception("Test2", new Exception("Test3")));
            
            var msgs = new List<string>();

            var logger = new LoggerMock(msgs, "Test");

            logger.Log(ex, false);

            Assert.AreEqual(3, msgs.Count);
            Assert.AreEqual("Test1", msgs[0]);
            Assert.AreEqual("Test2", msgs[1]);
            Assert.AreEqual("Test3", msgs[2]);
        }

        [Test]
        public void TestLogSingleLineStackTrace()
        {
            Exception ex;

            try
            {
                throw new Exception("Test1", new Exception("Test2", new Exception("Test3")));
            }
            catch (Exception e)
            {
                ex = e;
            }

            var msgs = new List<string>();

            var logger = new LoggerMock(msgs, "Test");

            logger.Log(ex, true);

            var frame = new StackFrame(0, true);
            var method = frame.GetMethod();

            var stackTrace = $"   at {method.DeclaringType.FullName}.{method.Name}() in {frame.GetFileName()}";

            Assert.AreEqual(4, msgs.Count);
            Assert.AreEqual("Test1", msgs[0]);
            Assert.That(msgs[1].StartsWith(stackTrace));
            Assert.AreEqual("Test2", msgs[2]);
            Assert.AreEqual("Test3", msgs[3]);
        }

        [Test]
        public void TestLogMultiLineStackTrace()
        {
            Exception ex;

            try
            {
                throw new Exception("Test1", new Exception("Test2", new Exception("Test3")));
            }
            catch (Exception e)
            {
                ex = e;
            }

            var msgs = new List<string>();

            var logger = new LoggerMock(msgs, "Test", false);

            logger.Log(ex, true);

            var frame = new StackFrame(0, true);
            var method = frame.GetMethod();

            var stackTrace = $"   at {method.DeclaringType.FullName}.{method.Name}() in {frame.GetFileName()}";

            Assert.AreEqual(1, msgs.Count);
            Assert.That(msgs[0].Contains("Test1"));
            Assert.That(msgs[0].Contains("Test2"));
            Assert.That(msgs[0].Contains("Test3"));
            Assert.That(msgs[0].Contains(stackTrace));
        }
    }
}
