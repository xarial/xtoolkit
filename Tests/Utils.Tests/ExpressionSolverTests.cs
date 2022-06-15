using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Services.Expressions;

namespace Utils.Tests
{
    public class ExpressionSolverTests
    {
        [Test]
        public void SolveTextTest() 
        {
            int solverCalled = 0;

            var solver = new ExpressionSolver((v, a) => 
            {
                solverCalled++;
                return "xyz"; 
            });

            var t1 = new ExpressionTokenText("abc");

            var r1 = solver.Solve(t1);

            Assert.AreEqual("abc", r1);
            Assert.AreEqual(0, solverCalled);
        }

        [Test]
        public void SolveVariableTest()
        {
            var calls = new List<Tuple<string, string[]>>();

            var solver = new ExpressionSolver((v, a) =>
            {
                calls.Add(new Tuple<string, string[]>(v, a));

                if (v == "v")
                {
                    return "abc";
                }
                else 
                {
                    throw new Exception();
                }
            });

            var t1 = new ExpressionTokenVariable("v", null);

            var r1 = solver.Solve(t1);

            Assert.AreEqual("abc", r1);
            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual("v", calls[0].Item1);
            Assert.AreEqual(0, calls[0].Item2.Length);
        }

        [Test]
        public void SolveGroupTest()
        {
            var calls = new List<Tuple<string, string[]>>();

            var solver = new ExpressionSolver((v, a) =>
            {
                calls.Add(new Tuple<string, string[]>(v, a));

                if (v == "v")
                {
                    return "abc";
                }
                else
                {
                    throw new Exception();
                }
            });

            var t1 = new ExpressionTokenGroup(new IExpressionToken[]
            {
                new ExpressionTokenText("1 "),
                new ExpressionTokenVariable("v", null),
                new ExpressionTokenText(" 2")
            });

            var r1 = solver.Solve(t1);

            Assert.AreEqual("1 abc 2", r1);
            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual("v", calls[0].Item1);
            Assert.AreEqual(0, calls[0].Item2.Length);
        }

        [Test]
        public void SolveVariablesCachedTest()
        {
            var calls = new List<Tuple<string, string[]>>();

            var solver = new ExpressionSolver((v, a) =>
            {
                calls.Add(new Tuple<string, string[]>(v, a));

                if (v == "v")
                {
                    return "abc";
                }
                else if (v == "V")
                {
                    return "ABC";
                }
                else if (v == "x" && a.Length == 1 && a[0] == "y")
                {
                    return "XYZ";
                }
                else
                {
                    throw new Exception();
                }
            });

            var t1 = new ExpressionTokenGroup(new IExpressionToken[]
            {
                new ExpressionTokenVariable("v", null),
                new ExpressionTokenText("_"),
                new ExpressionTokenVariable("V", null),
                new ExpressionTokenText("_"),
                new ExpressionTokenVariable("v", null),
                new ExpressionTokenText("_"),
                new ExpressionTokenVariable("x", new IExpressionToken[] { new ExpressionTokenText("y") }),
                new ExpressionTokenVariable("x", new IExpressionToken[] { new ExpressionTokenText("y") }),
                new ExpressionTokenText("_"),
                new ExpressionTokenVariable("x", new IExpressionToken[] { new ExpressionTokenText("y") }),
            });

            var r1 = solver.Solve(t1);

            var c1 = calls.FirstOrDefault(c => c.Item1 == "v");
            var c2 = calls.FirstOrDefault(c => c.Item1 == "V");
            var c3 = calls.FirstOrDefault(c => c.Item1 == "x");

            Assert.AreEqual("abc_ABC_abc_XYZXYZ_XYZ", r1);
            Assert.AreEqual(3, calls.Count);
            Assert.IsNotNull(c1);
            Assert.IsNotNull(c2);
            Assert.IsNotNull(c3);
            Assert.AreEqual(0, c1.Item2.Length);
            Assert.AreEqual(0, c2.Item2.Length);
            Assert.AreEqual(1, c3.Item2.Length);
            Assert.AreEqual("y", c3.Item2[0]);
        }

        [Test]
        public void SolveVariablesNestedTest()
        {
            var calls = new List<Tuple<string, string[]>>();

            var solver = new ExpressionSolver((v, a) =>
            {
                calls.Add(new Tuple<string, string[]>(v, a));

                if (v == "f1" && a.Length == 0)
                {
                    return "f1_0";
                }
                else if (v == "f2" && a.Length == 3)
                {
                    return "f2_" + String.Join("+", a);
                }
                else if (v == "f1" && a.Length == 1 && a[0] == "bf3_0")
                {
                    return "f1_bf3_0";
                }
                else if (v == "f1" && a.Length == 1 && a[0] == "a")
                {
                    return "f1_1_a";
                }
                else if (v == "f3" && a.Length == 0) 
                {
                    return "f3_0";
                }
                else
                {
                    throw new Exception();
                }
            });

            var t1 = new ExpressionTokenGroup(new IExpressionToken[]
            {
                new ExpressionTokenVariable("f1", null),
                new ExpressionTokenVariable("f2", new IExpressionToken[] 
                {
                    new ExpressionTokenText("y"),
                    new ExpressionTokenVariable("f1", new IExpressionToken[] 
                    {
                        new ExpressionTokenGroup(new IExpressionToken[] 
                        {
                            new ExpressionTokenText("b"),
                            new ExpressionTokenVariable("f3", null)
                        }) 
                    }),
                    new ExpressionTokenVariable("f1", new IExpressionToken[]{ new ExpressionTokenText("a")})
                }),
                new ExpressionTokenText("~"),
                new ExpressionTokenVariable("f1", new IExpressionToken[]{ new ExpressionTokenText("a")})
            });

            var r1 = solver.Solve(t1);

            var c1 = calls.FirstOrDefault(c => c.Item1 == "f1" && c.Item2.SequenceEqual(new string[0]));
            var c2 = calls.FirstOrDefault(c => c.Item1 == "f1" && c.Item2.SequenceEqual(new string[] { "a" }));
            var c3 = calls.FirstOrDefault(c => c.Item1 == "f1" && c.Item2.SequenceEqual(new string[] { "bf3_0" }));
            var c4 = calls.FirstOrDefault(c => c.Item1 == "f2" && c.Item2.SequenceEqual(new string[] { "y", "f1_bf3_0", "f1_1_a" }));
            var c5 = calls.FirstOrDefault(c => c.Item1 == "f3" && c.Item2.SequenceEqual(new string[0]));

            Assert.AreEqual("f1_0f2_y+f1_bf3_0+f1_1_a~f1_1_a", r1);
            Assert.AreEqual(5, calls.Count);
            Assert.IsNotNull(c1);
            Assert.IsNotNull(c2);
            Assert.IsNotNull(c3);
            Assert.IsNotNull(c4);
            Assert.IsNotNull(c5);
        }
    }
}
