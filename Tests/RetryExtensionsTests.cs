using System;
using System.Linq;
using NUnit.Framework;
using ReliabilityPatterns;

namespace Tests
{
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
                    5,
                    TimeSpan.Zero);

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
                circuitBreaker.ExecuteWithRetries(() => {}, 5, TimeSpan.Zero);
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
                circuitBreaker.ExecuteWithRetries(() => { actionWasCalled = true; }, 5, TimeSpan.Zero);
            }
            catch (OpenCircuitException ex)
            {
            }
            Assert.IsFalse(actionWasCalled);
        }
    }
}
