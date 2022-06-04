using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Services.Expressions.Exceptions;

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

            Assert.IsInstanceOf<IExpressionTokenText>(r1);
            Assert.AreEqual("", ((IExpressionTokenText)r1).Text);

            Assert.IsInstanceOf<IExpressionTokenText>(r2);
            Assert.AreEqual("a", ((IExpressionTokenText)r2).Text);
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

            Assert.IsInstanceOf<IExpressionTokenVariable>(r1);
            Assert.AreEqual("x", ((IExpressionTokenVariable)r1).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)r1).Arguments.Length);

            Assert.IsInstanceOf<IExpressionTokenGroup>(r2);
            Assert.AreEqual(2, ((IExpressionTokenGroup)r2).Children.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r2).Children[0]);
            Assert.AreEqual("A ", ((IExpressionTokenText)((IExpressionTokenGroup)r2).Children[0]).Text);
            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r2).Children[1]);
            Assert.AreEqual("x", ((IExpressionTokenVariable)((IExpressionTokenGroup)r2).Children[1]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenGroup)r2).Children[1]).Arguments.Length);

            Assert.IsInstanceOf<IExpressionTokenGroup>(r3);
            Assert.AreEqual(3, ((IExpressionTokenGroup)r3).Children.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r3).Children[0]);
            Assert.AreEqual("A ", ((IExpressionTokenText)((IExpressionTokenGroup)r3).Children[0]).Text);
            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r3).Children[1]);
            Assert.AreEqual("x", ((IExpressionTokenVariable)((IExpressionTokenGroup)r3).Children[1]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenGroup)r3).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r3).Children[2]);
            Assert.AreEqual(" B", ((IExpressionTokenText)((IExpressionTokenGroup)r3).Children[2]).Text);

            Assert.IsInstanceOf<IExpressionTokenGroup>(r4);
            Assert.AreEqual(5, ((IExpressionTokenGroup)r4).Children.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r4).Children[0]);
            Assert.AreEqual("A ", ((IExpressionTokenText)((IExpressionTokenGroup)r4).Children[0]).Text);
            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r4).Children[1]);
            Assert.AreEqual("x", ((IExpressionTokenVariable)((IExpressionTokenGroup)r4).Children[1]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenGroup)r4).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r4).Children[2]);
            Assert.AreEqual(" ", ((IExpressionTokenText)((IExpressionTokenGroup)r4).Children[2]).Text);
            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r4).Children[3]);
            Assert.AreEqual("y", ((IExpressionTokenVariable)((IExpressionTokenGroup)r4).Children[3]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenGroup)r4).Children[3]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r4).Children[4]);
            Assert.AreEqual(" C", ((IExpressionTokenText)((IExpressionTokenGroup)r4).Children[4]).Text);

            Assert.IsInstanceOf<IExpressionTokenGroup>(r5);
            Assert.AreEqual(4, ((IExpressionTokenGroup)r5).Children.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r5).Children[0]);
            Assert.AreEqual("A", ((IExpressionTokenText)((IExpressionTokenGroup)r5).Children[0]).Text);
            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r5).Children[1]);
            Assert.AreEqual("x", ((IExpressionTokenVariable)((IExpressionTokenGroup)r5).Children[1]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenGroup)r5).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r5).Children[2]);
            Assert.AreEqual("y", ((IExpressionTokenVariable)((IExpressionTokenGroup)r5).Children[2]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenGroup)r5).Children[2]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r5).Children[3]);
            Assert.AreEqual("C", ((IExpressionTokenText)((IExpressionTokenGroup)r5).Children[3]).Text);

            Assert.IsInstanceOf<IExpressionTokenGroup>(r6);
            Assert.AreEqual(5, ((IExpressionTokenGroup)r6).Children.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r6).Children[0]);
            Assert.AreEqual("A ", ((IExpressionTokenText)((IExpressionTokenGroup)r6).Children[0]).Text);
            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r6).Children[1]);
            Assert.AreEqual("x", ((IExpressionTokenVariable)((IExpressionTokenGroup)r6).Children[1]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenGroup)r6).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r6).Children[2]);
            Assert.AreEqual(" ", ((IExpressionTokenText)((IExpressionTokenGroup)r6).Children[2]).Text);
            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r6).Children[3]);
            Assert.AreEqual("yz", ((IExpressionTokenVariable)((IExpressionTokenGroup)r6).Children[3]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenGroup)r6).Children[3]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r6).Children[4]);
            Assert.AreEqual(" C", ((IExpressionTokenText)((IExpressionTokenGroup)r6).Children[4]).Text);
        }

        [Test]
        public void ParseProtectedSymbolTest()
        {
            var parser = new ExpressionParser();

            var r1 = parser.Parse(@"\{\}\[\]\\");
            var r2 = parser.Parse(@"a { x\}[\{\}\[\]\\\\] }");
            var r3 = parser.Parse(@"{\ b c }");

            Assert.IsInstanceOf<IExpressionTokenText>(r1);
            Assert.AreEqual("{}[]\\", ((IExpressionTokenText)r1).Text);

            Assert.IsInstanceOf<IExpressionTokenGroup>(r2);
            Assert.AreEqual(2, ((IExpressionTokenGroup)r2).Children.Length);

            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)r2).Children[0]);
            Assert.AreEqual("a ", ((IExpressionTokenText)((IExpressionTokenGroup)r2).Children[0]).Text);

            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r2).Children[1]);
            Assert.AreEqual("x}", ((IExpressionTokenVariable)((IExpressionTokenGroup)r2).Children[1]).Name);
            Assert.AreEqual(1, ((IExpressionTokenVariable)((IExpressionTokenGroup)r2).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenVariable)((IExpressionTokenGroup)r2).Children[1]).Arguments[0]);
            Assert.AreEqual("{}[]\\\\", ((IExpressionTokenText)((IExpressionTokenVariable)((IExpressionTokenGroup)r2).Children[1]).Arguments[0]).Text);

            Assert.IsInstanceOf<IExpressionTokenVariable>(r3);
            Assert.AreEqual(" b c", ((IExpressionTokenVariable)r3).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)r3).Arguments.Length);
        }

        [Test]
        public void ParseVariableArgumentsTest()
        {
            var parser = new ExpressionParser();

            var r1 = parser.Parse("{ x [1][2] }");
            var r2 = parser.Parse("{  xyz  [1]  [2]  }");
            var r3 = parser.Parse("{ b c }");

            Assert.IsInstanceOf<IExpressionTokenVariable>(r1);
            Assert.AreEqual("x", ((IExpressionTokenVariable)r1).Name);
            Assert.AreEqual(2, ((IExpressionTokenVariable)r1).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenVariable)r1).Arguments[0]);
            Assert.AreEqual("1", ((IExpressionTokenText)((IExpressionTokenVariable)r1).Arguments[0]).Text);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenVariable)r1).Arguments[1]);
            Assert.AreEqual("2", ((IExpressionTokenText)((IExpressionTokenVariable)r1).Arguments[1]).Text);

            Assert.IsInstanceOf<IExpressionTokenVariable>(r2);
            Assert.AreEqual("xyz", ((IExpressionTokenVariable)r2).Name);
            Assert.AreEqual(2, ((IExpressionTokenVariable)r2).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenVariable)r2).Arguments[0]);
            Assert.AreEqual("1", ((IExpressionTokenText)((IExpressionTokenVariable)r2).Arguments[0]).Text);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenVariable)r2).Arguments[1]);
            Assert.AreEqual("2", ((IExpressionTokenText)((IExpressionTokenVariable)r2).Arguments[1]).Text);

            Assert.IsInstanceOf<IExpressionTokenVariable>(r3);
            Assert.AreEqual("b c", ((IExpressionTokenVariable)r3).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)r3).Arguments.Length);
        }

        [Test]
        public void ParseNestedVariableTest()
        {
            var parser = new ExpressionParser();

            var r1 = parser.Parse("{x[{z}]}{y[1]}");
            var r2 = parser.Parse("{ x [1] [{y} { z[2] }] }");
            var r3 = parser.Parse("{ x [] [{y} ] [{y}]}");

            Assert.IsInstanceOf<IExpressionTokenGroup>(r1);
            Assert.AreEqual(2, ((IExpressionTokenGroup)r1).Children.Length);

            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r1).Children[0]);
            Assert.AreEqual("x", ((IExpressionTokenVariable)((IExpressionTokenGroup)r1).Children[0]).Name);
            Assert.AreEqual(1, ((IExpressionTokenVariable)((IExpressionTokenGroup)r1).Children[0]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenVariable)((IExpressionTokenGroup)r1).Children[0]).Arguments[0]);
            Assert.AreEqual("z", ((IExpressionTokenVariable)((IExpressionTokenVariable)((IExpressionTokenGroup)r1).Children[0]).Arguments[0]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenVariable)((IExpressionTokenGroup)r1).Children[0]).Arguments[0]).Arguments.Length);

            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)r1).Children[1]);
            Assert.AreEqual("y", ((IExpressionTokenVariable)((IExpressionTokenGroup)r1).Children[1]).Name);
            Assert.AreEqual(1, ((IExpressionTokenVariable)((IExpressionTokenGroup)r1).Children[1]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenVariable)((IExpressionTokenGroup)r1).Children[1]).Arguments[0]);
            Assert.AreEqual("1", ((IExpressionTokenText)((IExpressionTokenVariable)((IExpressionTokenGroup)r1).Children[1]).Arguments[0]).Text);

            Assert.IsInstanceOf<IExpressionTokenVariable>(r2);
            Assert.AreEqual("x", ((IExpressionTokenVariable)r2).Name);
            Assert.AreEqual(2, ((IExpressionTokenVariable)r2).Arguments.Length);

            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenVariable)r2).Arguments[0]);
            Assert.AreEqual("1", ((IExpressionTokenText)((IExpressionTokenVariable)r2).Arguments[0]).Text);

            Assert.IsInstanceOf<IExpressionTokenGroup>(((IExpressionTokenVariable)r2).Arguments[1]);
            Assert.AreEqual(3, ((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children.Length);

            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children[0]);
            Assert.AreEqual("y", ((IExpressionTokenVariable)((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children[0]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children[0]).Arguments.Length);

            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children[1]);
            Assert.AreEqual(" ", ((IExpressionTokenText)((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children[1]).Text);

            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children[2]);
            Assert.AreEqual("z", ((IExpressionTokenVariable)((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children[2]).Name);
            Assert.AreEqual(1, ((IExpressionTokenVariable)((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children[2]).Arguments.Length);
            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenVariable)((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children[2]).Arguments[0]);
            Assert.AreEqual("2", ((IExpressionTokenText)((IExpressionTokenVariable)((IExpressionTokenGroup)((IExpressionTokenVariable)r2).Arguments[1]).Children[2]).Arguments[0]).Text);
            
            Assert.IsInstanceOf<IExpressionTokenVariable>(r3);
            Assert.AreEqual("x", ((IExpressionTokenVariable)r3).Name);
            Assert.AreEqual(3, ((IExpressionTokenVariable)r3).Arguments.Length);

            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenVariable)r3).Arguments[0]);
            Assert.AreEqual("", ((IExpressionTokenText)((IExpressionTokenVariable)r3).Arguments[0]).Text);

            Assert.IsInstanceOf<IExpressionTokenGroup>(((IExpressionTokenVariable)r3).Arguments[1]);
            Assert.AreEqual(2, ((IExpressionTokenGroup)((IExpressionTokenVariable)r3).Arguments[1]).Children.Length);

            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenGroup)((IExpressionTokenVariable)r3).Arguments[1]).Children[0]);
            Assert.AreEqual("y", ((IExpressionTokenVariable)((IExpressionTokenGroup)((IExpressionTokenVariable)r3).Arguments[1]).Children[0]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenGroup)((IExpressionTokenVariable)r3).Arguments[1]).Children[0]).Arguments.Length);

            Assert.IsInstanceOf<IExpressionTokenText>(((IExpressionTokenGroup)((IExpressionTokenVariable)r3).Arguments[1]).Children[1]);
            Assert.AreEqual(" ", ((IExpressionTokenText)((IExpressionTokenGroup)((IExpressionTokenVariable)r3).Arguments[1]).Children[1]).Text);

            Assert.IsInstanceOf<IExpressionTokenVariable>(((IExpressionTokenVariable)r3).Arguments[2]);
            Assert.AreEqual("y", ((IExpressionTokenVariable)((IExpressionTokenVariable)r3).Arguments[2]).Name);
            Assert.AreEqual(0, ((IExpressionTokenVariable)((IExpressionTokenVariable)r3).Arguments[2]).Arguments.Length);
        }

        [Test]
        public void ParseInvalidTest()
        {
            var parser = new ExpressionParser();
            
            Assert.Throws<ArgumentOutOfVariableException>(() => parser.Parse("a {x [y]} [z] z"));
            Assert.Throws<MissingArgumentOpeningTagException>(() => parser.Parse("a b {x ]}"));
            Assert.Throws<NestedVariableOutOfArgumentException>(() => parser.Parse("a {x {y}}"));
            Assert.Throws<NotClosedVariableOrParameterException>(() => parser.Parse("a {b} {c"));
            Assert.Throws<NotClosedVariableOrParameterException>(() => parser.Parse("a {b} {c [x] [y}"));
            Assert.Throws<VariableNameInvalidException>(() => parser.Parse("{b [x] c}"));
        }

        [Test]
        public void CreateExpressionTest() 
        {
            var parser = new ExpressionParser();

            var t1 = new ExpressionTokenGroup(new IExpressionToken[]
            {
                new ExpressionTokenText("AB"),
                new ExpressionTokenVariable("x", new IExpressionToken[0]),
                new ExpressionTokenText("CD"),
                new ExpressionTokenVariable("y", new IExpressionToken[0]),
            });

            var exp1 = parser.CreateExpression(t1);

            Assert.AreEqual("AB{ x }CD{ y }", exp1);
        }

        [Test]
        public void CreateExpressionNestedTest()
        {
            var parser = new ExpressionParser();

            var t1 = new ExpressionTokenGroup(new IExpressionToken[]
            {
                new ExpressionTokenText("AB"),
                new ExpressionTokenVariable("1", new IExpressionToken[]
                {
                    new ExpressionTokenText("CD"),
                    new ExpressionTokenVariable("2", new IExpressionToken[]
                    {
                        new ExpressionTokenVariable("3", new IExpressionToken[]
                        {
                            new ExpressionTokenText("EF"),
                            new ExpressionTokenVariable("4", new IExpressionToken[0]),
                            new ExpressionTokenText("GH"),
                        })
                    })
                }),
                new ExpressionTokenVariable("5", new IExpressionToken[0])
            });

            var exp1 = parser.CreateExpression(t1);

            Assert.AreEqual("AB{ 1 [CD] [{ 2 [{ 3 [EF] [{ 4 }] [GH] }] }] }{ 5 }", exp1);
        }

        [Test]
        public void CreateExpressionProtectedSymbolsTest()
        {
            var parser = new ExpressionParser();

            var t1 = new ExpressionTokenGroup(new IExpressionToken[]
            {
                new ExpressionTokenText("AB"),
                new ExpressionTokenVariable("x{}", new IExpressionToken[]
                {
                    new ExpressionTokenText("CD[]_{}_\\_A"),
                    new ExpressionTokenVariable("[]_{}_\\_A", new IExpressionToken[0]),
                }),
            });

            var t2 = new ExpressionTokenVariable(" a b", new IExpressionToken[] 
            {
                new ExpressionTokenText("x")
            });

            var exp1 = parser.CreateExpression(t1);
            var exp2 = parser.CreateExpression(t2);

            Assert.AreEqual(@"AB{ x\{\} [CD\[\]_\{\}_\\_A] [{ \[\]_\{\}_\\_A }] }", exp1);
            Assert.AreEqual(@"{ \ a b [x] }", exp2);
        }
    }
}
