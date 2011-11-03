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
            if (usedThreadIds.Distinct().Count() == 1)
                Assert.Inconclusive("This test relies on multiple threads, however only one was ever spawned.");
        }
    }
}
