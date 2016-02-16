﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;

namespace Expressive.Tests
{
    [TestClass]
    public class ExpressionTests
    {
        #region Operators

        #region Plus Operator

        [TestMethod]
        public void SimpleIntegerAddition()
        {
            Expression expression = new Expression("1+3");

            object value = expression.Evaluate();

            Assert.AreEqual(4, value);
        }

        [TestMethod]
        public void SimpleDecimalAddition()
        {
            Expression expression = new Expression("1.3+3.5");

            object value = expression.Evaluate();

            Assert.AreEqual(4.8M, value);
        }

        [TestMethod]
        public void ShouldAddDoubleAndDecimal()
        {
            var expression = new Expression("1.8 + Abs([var1])");

            object value = expression.Evaluate(new Dictionary<string, object> { { "var1", 9.2 } });

            Assert.AreEqual(11M, value);
        }

        [TestMethod]
        public void ShouldConcatenateStrings()
        {
            var expression = new Expression("'1.8' + 'suffix'");

            object value = expression.Evaluate();

            Assert.AreEqual("1.8suffix", value);
        }

        #endregion

        #region Subtract Operator

        [TestMethod]
        public void SimpleIntegerSubtraction()
        {
            Expression expression = new Expression("3-1");

            object value = expression.Evaluate();

            Assert.AreEqual(2, value);
        }

        [TestMethod]
        public void SimpleDecimalSubtraction()
        {
            Expression expression = new Expression("3.5-1.2");

            object value = expression.Evaluate();

            Assert.AreEqual(2.3M, value);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException), "Operator '-' can't be applied to operands of types 'decimal' and 'double'")]
        public void ShouldNotSubtractDoubleAndDecimal()
        {
            var expression = new Expression("1.8 - Abs([var1])");

            object value = expression.Evaluate(new Dictionary<string, object> { { "var1", 0.2 } });
        }

        [TestMethod]
        public void ShouldHandleUnarySubtraction()
        {
            var expression = new Expression("1.8--0.2");

            object value = expression.Evaluate();

            Assert.AreEqual(2.0M, value);
        }

        #endregion

        #endregion

        #region Functions

        [TestMethod, ExpectedException(typeof(ArgumentException), "Abs() takes only 1 argument(s)")]
        public void AbsShouldHandleOnlyOneArgument()
        {
            Assert.AreEqual(1, new Expression("abs(-1)").Evaluate());
            Assert.AreEqual(12, new Expression("abs(1,2,4,5)").Evaluate());
        }

        [TestMethod, ExpectedException(typeof(ArgumentException), "Acos() takes only 1 argument(s)")]
        public void AcosShouldHandleOnlyOneArgument()
        {
            Assert.AreEqual(0d, new Expression("acos(1)").Evaluate());
            Assert.AreEqual(12, new Expression("acos(1,2,4,5)").Evaluate());
        }

        [TestMethod, ExpectedException(typeof(ArgumentException), "Asin() takes only 1 argument(s)")]
        public void AsinShouldHandleOnlyOneArgument()
        {
            Assert.AreEqual(0d, new Expression("asin(0)").Evaluate());
            Assert.AreEqual(12, new Expression("asin(1,2,4,5)").Evaluate());
        }

        [TestMethod, ExpectedException(typeof(ArgumentException), "Atan() takes only 1 argument(s)")]
        public void AtanShouldHandleOnlyOneArgument()
        {
            Assert.AreEqual(0d, new Expression("atan(0)").Evaluate());
            Assert.AreEqual(12, new Expression("atan(1,2,4,5)").Evaluate());
        }

        [TestMethod, ExpectedException(typeof(ArgumentException), "Average() expects at least 1 argument(s)")]
        public void AverageShouldHandleAtLeastOneArgument()
        {
            Assert.AreEqual(3d, new Expression("average(1,2,4,5)").Evaluate());
            Assert.AreEqual(1d, new Expression("average(1)").Evaluate());
            Assert.AreEqual(12.5, new Expression("average(10, 20, 5, 15)").Evaluate());

            new Expression("average()").Evaluate();
        }

        [TestMethod, ExpectedException(typeof(ArgumentException), "Ceiling() takes only 1 argument(s)")]
        public void CeilingShouldHandleOnlyOneArgument()
        {
            Assert.AreEqual(2M, new Expression("ceiling(1.5)").Evaluate());
            Assert.AreEqual(12, new Expression("ceiling(1,2,4,5)").Evaluate());
        }

        [TestMethod, ExpectedException(typeof(ArgumentException), "Cos() takes only 1 argument(s)")]
        public void CosShouldHandleOnlyOneArgument()
        {
            Assert.AreEqual(1d, new Expression("cos(0)").Evaluate());
            Assert.AreEqual(12, new Expression("cos(1,2,4,5)").Evaluate());
        }

        [TestMethod, ExpectedException(typeof(ArgumentException), "Count() expects at least 1 argument(s)")]
        public void CountShouldHandleAtLeastOneArgument()
        {
            Assert.AreEqual(1, new Expression("count(0)").Evaluate());
            Assert.AreEqual(4, new Expression("count(1,2,4,5)").Evaluate());

            new Expression("count()").Evaluate();
        }

        [TestMethod, ExpectedException(typeof(ArgumentException), "Sum() expects at least 1 argument(s)")]
        public void SumShouldHandleAtLeastOneArgument()
        {
            Assert.AreEqual(12, new Expression("sum(1,2,4,5)").Evaluate());
            Assert.AreEqual(1, new Expression("sum(1)").Evaluate());
            Assert.AreEqual(72, new Expression("sum(1,2,4,5,10,20,30)").Evaluate());

            new Expression("sum()").Evaluate();
        }

        #endregion

        #region General

        [TestMethod]
        public void TestAsync()
        {
            Expression expression = new Expression("1+3");

            AutoResetEvent waitHandle = new AutoResetEvent(false);

            object result = null;

            expression.EvaluateASync((r) =>
            {
                result = r;
                waitHandle.Set();
            });

            waitHandle.WaitOne();
            Assert.AreEqual(4, result);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException), "There aren't enough ')' symbols. Expected 2 but there is only 1")]
        public void ShouldIdentifyParenthesisMismatch()
        {
            Expression expression = new Expression("(a + b) * (4 - 2");
            
            object value = expression.Evaluate(new Dictionary<string, object> { { "a", 2 }, { "b", 3 } });
        }

        [TestMethod]
        public void ShouldShortCircuitBooleanExpressions()
        {
            var expression = new Expression("([a] != 0) && ([b]/[a]>2)");

            Assert.AreEqual(false, expression.Evaluate(new Dictionary<string, object> { { "a", 0 } }));
        }

        [TestMethod]
        public void ShouldCompareDates()
        {
            Assert.AreEqual(true, new Expression("#1/1/2009#==#1/1/2009#").Evaluate());
            Assert.AreEqual(false, new Expression("#2/1/2009#==#1/1/2009#").Evaluate());
        }

        [TestMethod]
        public void ShouldEvaluateSubExpressions()
        {
            var volume = new Expression("[surface] * [h]");
            var surface = new Expression("[l] * [K]");

            Assert.AreEqual(6, volume.Evaluate(new Dictionary<string, object> { { "surface", surface }, { "h", 3 }, { "l", 1 }, { "K", 2 } }));
        }

        [TestMethod]
        public void ShouldParseValues()
        {
            Assert.AreEqual(123456, new Expression("123456").Evaluate());
            Assert.AreEqual(new DateTime(2001, 01, 01), new Expression("#01/01/2001#").Evaluate());
            Assert.AreEqual(123.456M, new Expression("123.456").Evaluate());
            Assert.AreEqual(true, new Expression("true").Evaluate());
            Assert.AreEqual("true", new Expression("'true'").Evaluate());
            Assert.AreEqual("qwerty", new Expression("'qwerty'").Evaluate());
        }

        [TestMethod]
        public void ShouldEscapeCharacters()
        {
            Assert.AreEqual("'hello'", new Expression(@"'\'hello\''").Evaluate());
            Assert.AreEqual(" ' hel lo ' ", new Expression(@"' \' hel lo \' '").Evaluate());
            System.Diagnostics.Debug.WriteLine("hel\nlo");
            System.Diagnostics.Debug.WriteLine(new Expression(@"'hel\nlo'").Evaluate());
            Assert.AreEqual("hel\nlo", new Expression(@"'hel\nlo'").Evaluate());
        }

        [TestMethod]
        public void ShouldHandleOperatorsPriority()
        {
            Assert.AreEqual(8, new Expression("2+2+2+2").Evaluate());
            Assert.AreEqual(16, new Expression("2*2*2*2").Evaluate());
            Assert.AreEqual(6, new Expression("2*2+2").Evaluate());
            Assert.AreEqual(6, new Expression("2+2*2").Evaluate());

            Assert.AreEqual(9d, new Expression("1 + 2 + 3 * 4 / 2").Evaluate());
            Assert.AreEqual(13.5M, new Expression("18.0/2.0/2.0*3.0").Evaluate());
        }

        [TestMethod]
        public void ShouldNotLosePrecision()
        {
            Assert.AreEqual(0.5, new Expression("3/6").Evaluate());
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException), "Unrecognised token 'blarsh'")]
        public void ShouldFailOnUnrecognisedToken()
        {
            Assert.AreEqual(0.5, new Expression("1 + blarsh + 4").Evaluate());
        }

        #endregion
    }
}