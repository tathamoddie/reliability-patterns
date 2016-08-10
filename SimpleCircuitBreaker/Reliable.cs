// -----------------------------------------------------------------------------
// <copyright file="Reliable.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the Reliable class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    ///     The reliable class.
    /// </summary>
    public static class Reliable
    {
        /// <summary>
        ///     The for each overload.
        /// </summary>
        /// <param name="circuitBreaker">
        ///     The circuit breaker.
        /// </param>
        /// <param name="source">
        ///     The source.
        /// </param>
        /// <param name="body">
        ///     The body of the request.
        /// </param>
        /// <param name="retryOptions">
        ///     The retry options.
        /// </param>
        /// <typeparam name="TSource">
        ///     The type of request.
        /// </typeparam>
        public static void ForEach<TSource>(
            CircuitBreaker circuitBreaker,
            IEnumerable<TSource> source,
            Action<TSource> body,
            RetryOptions retryOptions = null)
        {
            foreach (var element in source)
            {
                var element1 = element;
                circuitBreaker.ExecuteWithRetries(
                    () => body(element1),
                    retryOptions);
            }
        }

        /// <summary>
        /// The parallel foreach overload.
        /// </summary>
        /// <param name="circuitBreaker">
        ///     The circuit breaker.
        /// </param>
        /// <param name="source">
        ///     The source.
        /// </param>
        /// <param name="body">
        ///     The body.
        /// </param>
        /// <param name="retryOptions">
        ///     The retry options.
        /// </param>
        /// <param name="parallelOptions">
        ///     The parallel options.
        /// </param>
        /// <typeparam name="TSource">
        ///     The type for the request.
        /// </typeparam>
        public static void ParallelForEach<TSource>(
            CircuitBreaker circuitBreaker,
            IEnumerable<TSource> source,
            Action<TSource> body,
            RetryOptions retryOptions = null,
            ParallelOptions parallelOptions = null)
        {
            parallelOptions = parallelOptions ?? new ParallelOptions();

            Parallel.ForEach(
                source,
                parallelOptions,
                element =>
                {
                    var element1 = element;
                    circuitBreaker.ExecuteWithRetries(
                        () => body(element1),
                        retryOptions);
                });
        }
    }
}
