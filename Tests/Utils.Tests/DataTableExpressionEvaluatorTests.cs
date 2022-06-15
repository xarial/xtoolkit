using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Services.Expressions.Exceptions;

namespace Utils.Tests
{
    public class DataTableExpressionEvaluatorTests
    {
        [Test]
        public void EvaluateTest() 
        {
            var eval = new DataTableExpressionEvaluator();

            var r1 = eval.Evaluate<int>("1 + 1");
            var r2 = eval.Evaluate<double>("2.5 + 2 * 2");
            var r3 = eval.Evaluate<string>("'1' + '2'");
            var r4 = eval.Evaluate<bool>("2 > 1");
            var r5 = eval.Evaluate<int>("IIF('A' = 'A', 10, 15)");

            Assert.AreEqual(2, r1);
            Assert.AreEqual(6.5d, r2);
            Assert.AreEqual("12", r3);
            Assert.AreEqual(true, r4);
            Assert.AreEqual(10, r5);
        }

        [Test]
        public void EvaluateErrorTest() 
        {
            var eval = new DataTableExpressionEvaluator();

            Assert.Throws<ExpressionSyntaxErrorException>(() => eval.Evaluate<int>("in > 0"));
            Assert.Throws<ExpressionEvaluateErrorException>(() => eval.Evaluate<int>("IIF(true, 1, 2, 3)"));
            Assert.Throws<ExpressionEvaluateErrorException>(() => eval.Evaluate<int>("IIFx(true, 1, 2)"));
            Assert.Throws<ExpressionEvaluateErrorException>(() => eval.Evaluate<bool>("1 = 'A'"));
            Assert.Throws<ExpressionResultInvalidCastException>(() => eval.Evaluate<int>("'A' + 'B'"));
        }
    }
}
