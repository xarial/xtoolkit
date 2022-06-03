using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Services;

namespace Utils.Tests
{
    public class ExpressionParserTests
    {
        [Test]
        public void ParseFreeTextTest() 
        {
            var parser = new ExpressionParser();

            var r1 = parser.Parse("");
            var r2 = parser.Parse("a");

            Assert.IsInstanceOf<IExpressionFreeTextElement>(r1);
            Assert.AreEqual("", ((IExpressionFreeTextElement)r1).Text);

            Assert.IsInstanceOf<IExpressionFreeTextElement>(r2);
            Assert.AreEqual("a", ((IExpressionFreeTextElement)r2).Text);
        }

        [Test]
        public void ParseVariableTest()
        {
            var parser = new ExpressionParser();

            var r1 = parser.Parse("{ x }");
            var r2 = parser.Parse("A { x }");
            var r3 = parser.Parse("A { x } B");
            var r4 = parser.Parse("A { x } { y } C");
            var r5 = parser.Parse("A{x}{y}C");
            var r6 = parser.Parse("A {   x} {  yz } C");

            Assert.IsInstanceOf<IExpressionVariableElement>(r1);
            Assert.AreEqual("x", ((IExpressionVariableElement)r1).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)r1).Arguments.Length);

            Assert.IsInstanceOf<IExpressionElementGroup>(r2);
            Assert.AreEqual(2, ((IExpressionElementGroup)r2).Children.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r2).Children[0]);
            Assert.AreEqual("A ", ((IExpressionFreeTextElement)((IExpressionElementGroup)r2).Children[0]).Text);
            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)r2).Children[1]);
            Assert.AreEqual("x", ((IExpressionVariableElement)((IExpressionElementGroup)r2).Children[1]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionElementGroup)r2).Children[1]).Arguments.Length);

            Assert.IsInstanceOf<IExpressionElementGroup>(r3);
            Assert.AreEqual(3, ((IExpressionElementGroup)r3).Children.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r3).Children[0]);
            Assert.AreEqual("A ", ((IExpressionFreeTextElement)((IExpressionElementGroup)r3).Children[0]).Text);
            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)r3).Children[1]);
            Assert.AreEqual("x", ((IExpressionVariableElement)((IExpressionElementGroup)r3).Children[1]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionElementGroup)r3).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r3).Children[2]);
            Assert.AreEqual(" B", ((IExpressionFreeTextElement)((IExpressionElementGroup)r3).Children[2]).Text);

            Assert.IsInstanceOf<IExpressionElementGroup>(r4);
            Assert.AreEqual(5, ((IExpressionElementGroup)r4).Children.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r4).Children[0]);
            Assert.AreEqual("A ", ((IExpressionFreeTextElement)((IExpressionElementGroup)r4).Children[0]).Text);
            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)r4).Children[1]);
            Assert.AreEqual("x", ((IExpressionVariableElement)((IExpressionElementGroup)r4).Children[1]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionElementGroup)r4).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r4).Children[2]);
            Assert.AreEqual(" ", ((IExpressionFreeTextElement)((IExpressionElementGroup)r4).Children[2]).Text);
            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)r4).Children[3]);
            Assert.AreEqual("y", ((IExpressionVariableElement)((IExpressionElementGroup)r4).Children[3]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionElementGroup)r4).Children[3]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r4).Children[4]);
            Assert.AreEqual(" C", ((IExpressionFreeTextElement)((IExpressionElementGroup)r4).Children[4]).Text);

            Assert.IsInstanceOf<IExpressionElementGroup>(r5);
            Assert.AreEqual(4, ((IExpressionElementGroup)r5).Children.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r5).Children[0]);
            Assert.AreEqual("A", ((IExpressionFreeTextElement)((IExpressionElementGroup)r5).Children[0]).Text);
            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)r5).Children[1]);
            Assert.AreEqual("x", ((IExpressionVariableElement)((IExpressionElementGroup)r5).Children[1]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionElementGroup)r5).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)r5).Children[2]);
            Assert.AreEqual("y", ((IExpressionVariableElement)((IExpressionElementGroup)r5).Children[2]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionElementGroup)r5).Children[2]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r5).Children[3]);
            Assert.AreEqual("C", ((IExpressionFreeTextElement)((IExpressionElementGroup)r5).Children[3]).Text);

            Assert.IsInstanceOf<IExpressionElementGroup>(r6);
            Assert.AreEqual(5, ((IExpressionElementGroup)r6).Children.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r6).Children[0]);
            Assert.AreEqual("A ", ((IExpressionFreeTextElement)((IExpressionElementGroup)r6).Children[0]).Text);
            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)r6).Children[1]);
            Assert.AreEqual("x", ((IExpressionVariableElement)((IExpressionElementGroup)r6).Children[1]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionElementGroup)r6).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r6).Children[2]);
            Assert.AreEqual(" ", ((IExpressionFreeTextElement)((IExpressionElementGroup)r6).Children[2]).Text);
            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)r6).Children[3]);
            Assert.AreEqual("yz", ((IExpressionVariableElement)((IExpressionElementGroup)r6).Children[3]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionElementGroup)r6).Children[3]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)r6).Children[4]);
            Assert.AreEqual(" C", ((IExpressionFreeTextElement)((IExpressionElementGroup)r6).Children[4]).Text);
        }

        [Test]
        public void ParseProtectedSymbolTest()
        {
            var parser = new ExpressionParser();

            var r1 = parser.Parse("\\{\\}\\[\\]\\\\");
            var r2 = parser.Parse("a { x\\} }");
        }

        [Test]
        public void ParseVariableArgumentsTest()
        {
            var parser = new ExpressionParser();

            var r1 = parser.Parse("{ x [1][2] }");
            var r2 = parser.Parse("{  xyz  [1]  [2]  }");

            Assert.IsInstanceOf<IExpressionVariableElement>(r1);
            Assert.AreEqual("x", ((IExpressionVariableElement)r1).Name);
            Assert.AreEqual(2, ((IExpressionVariableElement)r1).Arguments.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionVariableElement)r1).Arguments[0]);
            Assert.AreEqual("1", ((IExpressionFreeTextElement)((IExpressionVariableElement)r1).Arguments[0]).Text);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionVariableElement)r1).Arguments[1]);
            Assert.AreEqual("2", ((IExpressionFreeTextElement)((IExpressionVariableElement)r1).Arguments[1]).Text);

            Assert.IsInstanceOf<IExpressionVariableElement>(r2);
            Assert.AreEqual("xyz", ((IExpressionVariableElement)r2).Name);
            Assert.AreEqual(2, ((IExpressionVariableElement)r2).Arguments.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionVariableElement)r2).Arguments[0]);
            Assert.AreEqual("1", ((IExpressionFreeTextElement)((IExpressionVariableElement)r2).Arguments[0]).Text);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionVariableElement)r2).Arguments[1]);
            Assert.AreEqual("2", ((IExpressionFreeTextElement)((IExpressionVariableElement)r2).Arguments[1]).Text);
        }

        [Test]
        public void ParseNestedVariableTest()
        {
            var parser = new ExpressionParser();

            var r1 = parser.Parse("{x[{z}]}{y[1]}");
            var r2 = parser.Parse("{ x [1] [{y} { z[2] }] }");
            var r3 = parser.Parse("{ x [] [{y} ] [{y}]}");

            Assert.IsInstanceOf<IExpressionElementGroup>(r1);
            Assert.AreEqual(2, ((IExpressionElementGroup)r1).Children.Length);

            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)r1).Children[0]);
            Assert.AreEqual("x", ((IExpressionVariableElement)((IExpressionElementGroup)r1).Children[0]).Name);
            Assert.AreEqual(1, ((IExpressionVariableElement)((IExpressionElementGroup)r1).Children[0]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionVariableElement)((IExpressionElementGroup)r1).Children[0]).Arguments[0]);
            Assert.AreEqual("z", ((IExpressionVariableElement)((IExpressionVariableElement)((IExpressionElementGroup)r1).Children[0]).Arguments[0]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionVariableElement)((IExpressionElementGroup)r1).Children[0]).Arguments[0]).Arguments.Length);

            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)r1).Children[1]);
            Assert.AreEqual("y", ((IExpressionVariableElement)((IExpressionElementGroup)r1).Children[1]).Name);
            Assert.AreEqual(1, ((IExpressionVariableElement)((IExpressionElementGroup)r1).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionVariableElement)((IExpressionElementGroup)r1).Children[1]).Arguments[0]);
            Assert.AreEqual("1", ((IExpressionFreeTextElement)((IExpressionVariableElement)((IExpressionElementGroup)r1).Children[1]).Arguments[0]).Text);

            Assert.IsInstanceOf<IExpressionVariableElement>(r2);
            Assert.AreEqual("x", ((IExpressionVariableElement)r2).Name);
            Assert.AreEqual(2, ((IExpressionVariableElement)r2).Arguments.Length);

            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionVariableElement)r2).Arguments[0]);
            Assert.AreEqual("1", ((IExpressionFreeTextElement)((IExpressionVariableElement)r2).Arguments[0]).Text);

            Assert.IsInstanceOf<IExpressionElementGroup>(((IExpressionVariableElement)r2).Arguments[1]);
            Assert.AreEqual(3, ((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children.Length);

            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children[0]);
            Assert.AreEqual("y", ((IExpressionVariableElement)((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children[0]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children[0]).Arguments.Length);

            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children[1]);
            Assert.AreEqual(" ", ((IExpressionFreeTextElement)((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children[1]).Text);

            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children[2]);
            Assert.AreEqual("z", ((IExpressionVariableElement)((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children[2]).Name);
            Assert.AreEqual(1, ((IExpressionVariableElement)((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children[2]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionVariableElement)((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children[2]).Arguments[0]);
            Assert.AreEqual("2", ((IExpressionFreeTextElement)((IExpressionVariableElement)((IExpressionElementGroup)((IExpressionVariableElement)r2).Arguments[1]).Children[2]).Arguments[0]).Text);
            
            Assert.IsInstanceOf<IExpressionVariableElement>(r3);
            Assert.AreEqual("x", ((IExpressionVariableElement)r3).Name);
            Assert.AreEqual(3, ((IExpressionVariableElement)r3).Arguments.Length);

            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionVariableElement)r3).Arguments[0]);
            Assert.AreEqual("", ((IExpressionFreeTextElement)((IExpressionVariableElement)r3).Arguments[0]).Text);

            Assert.IsInstanceOf<IExpressionElementGroup>(((IExpressionVariableElement)r3).Arguments[1]);
            Assert.AreEqual(2, ((IExpressionElementGroup)((IExpressionVariableElement)r3).Arguments[1]).Children.Length);

            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionElementGroup)((IExpressionVariableElement)r3).Arguments[1]).Children[0]);
            Assert.AreEqual("y", ((IExpressionVariableElement)((IExpressionElementGroup)((IExpressionVariableElement)r3).Arguments[1]).Children[0]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionElementGroup)((IExpressionVariableElement)r3).Arguments[1]).Children[0]).Arguments.Length);

            Assert.IsInstanceOf<IExpressionFreeTextElement>(((IExpressionElementGroup)((IExpressionVariableElement)r3).Arguments[1]).Children[1]);
            Assert.AreEqual(" ", ((IExpressionFreeTextElement)((IExpressionElementGroup)((IExpressionVariableElement)r3).Arguments[1]).Children[1]).Text);

            Assert.IsInstanceOf<IExpressionVariableElement>(((IExpressionVariableElement)r3).Arguments[2]);
            Assert.AreEqual("y", ((IExpressionVariableElement)((IExpressionVariableElement)r3).Arguments[2]).Name);
            Assert.AreEqual(0, ((IExpressionVariableElement)((IExpressionVariableElement)r3).Arguments[2]).Arguments.Length);
        }

        [Test]
        public void ParseInvalidTest()
        {
        }
    }
}
