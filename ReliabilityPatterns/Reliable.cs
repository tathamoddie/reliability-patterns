using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReliabilityPatterns
{
    public static class Reliable
    {
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
