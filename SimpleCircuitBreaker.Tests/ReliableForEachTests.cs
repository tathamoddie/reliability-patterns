// -----------------------------------------------------------------------------
// <copyright file="ReliableForEachTests.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the ReliableForEachTests class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    /// <summary>
    ///     The reliable for each tests.
    /// </summary>
    [TestFixture]
    public class ReliableForEachTests
    {
        /// <summary>
        ///     The should process all elements in order test.
        /// </summary>
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
