using System;
using System.Collections.Generic;

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
    }
}
