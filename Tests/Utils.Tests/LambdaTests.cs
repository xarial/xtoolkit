using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reflection;

namespace Utils.Tests
{
    public class LambdaTests
    {
        public class Class1 
        {
            public string Test1<T, X>(Action<T> action1, Func<T> func1, Action<T, X> action2, Func<X, T> func2) 
            {
                var res = "";

                action1.Invoke((T)(object)"X");
                res += (string)(object)func1.Invoke();
                action2.Invoke((T)(object)"Y", (X)(object)1);
                res += (string)(object)func2.Invoke((X)(object)2);

                return "A" + res;
            }
        }

        [Test]
        public void InvokeGenericMethodTest() 
        {
            var c1 = new Class1();

            var calls = new List<object>();

            Delegate a1 = new Action<string>(x => { calls.Add(x); });
            Delegate f1 = new Func<string>(() => "B");
            Delegate a2 = new Action<string, int>((x, y) => { calls.Add(x); calls.Add(y); });
            Delegate f2 = new Func<int, string>(x => { calls.Add(x); return "C"; });

            var r = Lambda.InvokeGenericMethod(() => c1.Test1<object, object>((Action<object>)a1, (Func<object>)f1, (Action<object, object>)a2, (Func<object, object>)f2), typeof(string), typeof(int));

            Assert.AreEqual("ABC", r);
            Assert.AreEqual(4, calls.Count);
            Assert.AreEqual("X", calls[0]);
            Assert.AreEqual("Y", calls[1]);
            Assert.AreEqual(1, calls[2]);
            Assert.AreEqual(2, calls[3]);
        }
    }
}
