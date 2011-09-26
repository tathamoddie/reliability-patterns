using System;
using NUnit.Framework;
using ReliabilityPatterns;

namespace Tests
{
    [TestFixture]
    public class CircuitBreakerTests
    {
        [Test]
        public void ShouldInitializeWithServiceLevel100()
        {
            var circuitBreaker = new CircuitBreaker();
            Assert.AreEqual(100, circuitBreaker.ServiceLevel);
        }

        [Test]
        public void ShouldReduceServiceLevelWithFailure()
        {
            var circuitBreaker = new CircuitBreaker();

            try { circuitBreaker.Execute(() => { throw new ApplicationException(); }); }
            catch (OperationFailedException) { }

            Assert.AreEqual(80, circuitBreaker.ServiceLevel);
        }

        [Test]
        public void ShouldReduceServiceLevelWithConsecutiveFailures()
        {
            var circuitBreaker = new CircuitBreaker();

            try { circuitBreaker.Execute(() => { throw new ApplicationException(); }); }
            catch (OperationFailedException) { }

            try { circuitBreaker.Execute(() => { throw new ApplicationException(); }); }
            catch (OperationFailedException) { }

            Assert.AreEqual(60, circuitBreaker.ServiceLevel);
        }
    }
}
