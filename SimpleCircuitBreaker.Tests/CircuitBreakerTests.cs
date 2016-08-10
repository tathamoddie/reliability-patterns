// -----------------------------------------------------------------------------
// <copyright file="CircuitBreakerTests.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the CircuitBreakerTests class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker.Tests
{
    using System;

    using NUnit.Framework;

    /// <summary>
    ///     The circuit breaker tests.
    /// </summary>
    [TestFixture]
    public class CircuitBreakerTests
    {
        /// <summary>
        ///     The execute method should execute action once test.
        /// </summary>
        [Test]
        public void ExecuteShouldExecuteActionOnce()
        {
            var circuitBreaker = new CircuitBreaker();
            var actionCallCount = 0;

            circuitBreaker.Execute(() => { actionCallCount++; });

            Assert.AreEqual(1, actionCallCount);
        }

        /// <summary>
        ///     The execute with result should return result test.
        /// </summary>
        [Test]
        public void ExecuteWithResultShouldReturnResult()
        {
            var circuitBreaker = new CircuitBreaker();
            var expectedResult = new object();

            var result = circuitBreaker.Execute(() => expectedResult);

            Assert.AreEqual(expectedResult, result);
        }

        /// <summary>
        ///     The service level tests.
        /// </summary>
        /// <param name="callPattern">
        ///     The call pattern.
        /// </param>
        /// <returns>
        ///     The <see cref="double"/>.
        /// </returns>
        /// <exception cref="Exception">
        ///     Operation failed exception.
        /// </exception>
        [Test]
        [TestCase("", ExpectedResult = 100d)]
        [TestCase("bad", ExpectedResult = 80d)]
        [TestCase("bad good", ExpectedResult = 100d)]
        [TestCase("bad bad", ExpectedResult = 60d)]
        [TestCase("bad bad good", ExpectedResult = 80d)]
        [TestCase("bad bad good good", ExpectedResult = 100d)]
        [TestCase("bad good bad good", ExpectedResult = 100d)]
        public double ServiceLevel(string callPattern)
        {
            var circuitBreaker = new CircuitBreaker();

            foreach (var call in callPattern.Split(
                new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (call)
                {
                    case "good":
                        circuitBreaker.Execute(() => { });
                        break;

                    case "bad":
                        try
                        {
                            circuitBreaker.Execute(() => { throw new Exception(); });
                        }
                        catch (OperationFailedException) { }
                        break;

                    default:
                        Assert.Fail("Unknown call sequence");
                        break;
                }
            }

            return circuitBreaker.ServiceLevel;
        }
    }
}
