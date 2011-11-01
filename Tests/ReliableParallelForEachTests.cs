using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ReliabilityPatterns;

namespace Tests
{
    [TestFixture]
    public class ReliableParallelForEachTests
    {
        [Test]
        public void ShouldProcessOnMultipleThreads()
        {
            if (Environment.ProcessorCount == 1)
                Assert.Inconclusive("This test requires multiple logical processors.");

            // Arrange
            var breaker = new CircuitBreaker();
            var source = Enumerable.Range(0, 100).ToArray();

            // Act
            var usedThreadIds = new List<int>();
            Reliable.ParallelForEach(
                breaker,
                source,
                elements => usedThreadIds.Add(Thread.CurrentThread.ManagedThreadId),
                new RetryOptions
                {
                    AllowedRetries = 5,
                    RetryInterval = TimeSpan.Zero
                },
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 100
                });

            // Assert
            var distinctThreadIdCount = usedThreadIds.Distinct().Count();
            Assert.IsTrue(distinctThreadIdCount > 1);
        }
    }
}
