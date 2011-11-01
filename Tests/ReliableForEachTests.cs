using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReliabilityPatterns;

namespace Tests
{
    [TestFixture]
    public class ReliableForEachTests
    {
        [Test]
        public void ShouldProcessAllElementsInOrder()
        {
            // Arrange
            var breaker = new CircuitBreaker();
            var source = Enumerable.Range(0, 10).ToArray();

            // Act
            var processedElements = new List<int>();
            Reliable.ForEach(
                breaker,
                source,
                processedElements.Add,
                new RetryOptions
                {
                    AllowedRetries = 5,
                    RetryInterval = TimeSpan.Zero
                });

            // Assert
            CollectionAssert.AreEqual(source, processedElements);
        }
    }
}
