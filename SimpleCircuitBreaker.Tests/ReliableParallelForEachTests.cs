// -----------------------------------------------------------------------------
// <copyright file="ReliableParallelForEachTests.cs" company="none">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the ReliableParallelForEachTests class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    /// <summary>
    ///     The reliable parallel for each tests.
    /// </summary>
    [TestFixture]
    public class ReliableParallelForEachTests
    {
        /// <summary>
        ///     The should process on multiple threads test.
        /// </summary>
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
            {
                Assert.Inconclusive("This test relies on multiple threads, however only one was ever spawned.");
            }
        }
    }
}
