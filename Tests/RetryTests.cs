using System;
using System.Linq;
using NUnit.Framework;
using ReliabilityPatterns;

namespace Tests
{
    [TestFixture]
    public class RetryTests
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
    }
}
