// -----------------------------------------------------------------------------
// <copyright file="RetryExtensionsTests.cs" company="none">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the RetryExtensionsTests class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    /// <summary>
    ///     The retry extension tests.
    /// </summary>
    [TestFixture]
    public class RetryExtensionsTests
    {
        /// <summary>
        ///     The should throw aggregate exception when multiple retries fail with exceptions 
        ///     test.
        /// </summary>
        /// <exception cref="Exception">
        ///     Aggregate exception.
        /// </exception>
        [Test]
        public void ShouldThrowAggregateExceptionWhenMultipleRetriesFailWithExceptions()
        {
            var circuitBreaker = new CircuitBreaker(100, TimeSpan.FromSeconds(1));
            try
            {
                circuitBreaker.ExecuteWithRetries<int>(
                    () =>
                        {
                            throw new Exception("foo");
                        },
                    new RetryOptions { AllowedRetries = 5, RetryInterval = TimeSpan.Zero });

                Assert.Fail("No exception was thrown");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(5, ex.InnerExceptions.Count);
                Assert.IsTrue(ex.InnerExceptions.All(e => e.Message == "foo"));
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<AggregateException>(ex);
            }
        }

        /// <summary>
        ///     The should throw open circuit exception when circuit breaker is open the entire 
        ///     time test.
        /// </summary>
        [Test]
        public void ShouldThrowOpenCircuitExceptionWhenCircuitBreakerIsOpenTheEntireTime()
        {
            var circuitBreaker = new CircuitBreaker(100, TimeSpan.FromHours(1));
            circuitBreaker.Trip();
            try
            {
                circuitBreaker.ExecuteWithRetries(
                    () => 1,
                    new RetryOptions { AllowedRetries = 5, RetryInterval = TimeSpan.Zero });
                Assert.Fail("No exception was thrown");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<OpenCircuitException>(ex);
            }           
        }

        /// <summary>
        ///     The should not call function when circuit breaker is open test.
        /// </summary>
        [Test]
        public void ShouldNotCallFuncWhenCircuitBreakerIsOpen()
        {
            var circuitBreaker = new CircuitBreaker(100, TimeSpan.FromHours(1));
            circuitBreaker.Trip();
            var result = 0;

            try
            {
                result = circuitBreaker.ExecuteWithRetries(
                    () => 999,
                    new RetryOptions
                        {
                            AllowedRetries = 5, RetryInterval = TimeSpan.Zero
                        });
            }
            catch (OpenCircuitException)
            {
                // Swallow exception for testing purposes.
            }

            Assert.AreNotEqual(result, 999);
        }

        /// <summary>
        ///     The should not call action when circuit breaker is open test.
        /// </summary>
        [Test]
        public void ShouldNotCallActionWhenCircuitBreakerIsOpen()
        {
            var circuitBreaker = new CircuitBreaker(100, TimeSpan.FromHours(1));
            circuitBreaker.Trip();
            var actionWasCalled = false;
            try
            {
                circuitBreaker.ExecuteWithRetries(
                    () => { actionWasCalled = true; },
                    new RetryOptions
                        {
                            AllowedRetries = 5, RetryInterval = TimeSpan.Zero
                        });
            }
            catch (OpenCircuitException)
            {
                // Swallow exception for testing purposes.
            }

            Assert.IsFalse(actionWasCalled);
        }
    }
}
