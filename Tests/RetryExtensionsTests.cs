using System;
using System.Linq;
using NUnit.Framework;
using ReliabilityPatterns;

namespace Tests
{
    [TestFixture]
    public class FuncRetryExtensionTests
    {
        [Test]
        public void ShouldThrowAggregateExceptionWhenMultipleRetriesFailWithExceptions()
        {
            var circuitBreaker = new CircuitBreaker(100, TimeSpan.FromSeconds(1));
            try
            {
                circuitBreaker.ExecuteWithRetries<int>(
                     () => {throw new Exception("foo"); },
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
                    new RetryOptions { AllowedRetries = 5, RetryInterval = TimeSpan.Zero });
            }
            catch (OpenCircuitException)
            {
            }
            Assert.AreNotEqual(result, 999);
        }
    }

    [TestFixture]
    public class RetryExtensionsTests
    {
        [Test]
        public void ShouldThrowAggregateExceptionWhenMultipleRetriesFailWithExceptions()
        {
            var circuitBreaker = new CircuitBreaker(100, TimeSpan.FromSeconds(1));
            try
            {
                circuitBreaker.ExecuteWithRetries(
                    () => { throw new Exception("foo"); },
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

        [Test]
        public void ShouldThrowOpenCircuitExceptionWhenCircuitBreakerIsOpenTheEntireTime()
        {
            var circuitBreaker = new CircuitBreaker(100, TimeSpan.FromHours(1));
            circuitBreaker.Trip();
            try
            {
                circuitBreaker.ExecuteWithRetries(
                    () => { },
                    new RetryOptions { AllowedRetries = 5, RetryInterval = TimeSpan.Zero });
                Assert.Fail("No exception was thrown");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<OpenCircuitException>(ex);
            }
        }

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
                    new RetryOptions { AllowedRetries = 5, RetryInterval = TimeSpan.Zero });
            }
            catch (OpenCircuitException)
            {
            }
            Assert.IsFalse(actionWasCalled);
        }
    }
}
