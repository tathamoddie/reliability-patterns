using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ReliabilityPatterns
{
    public static class RetryExtensions
    {
        [Obsolete("Use the overload which accepts a RetryOptions instance instead. This overload will be removed in a future build.")]
        public static void ExecuteWithRetries(this CircuitBreaker circuitBreaker, Action operation, ushort allowedRetries, TimeSpan retryInterval)
        {
            ExecuteWithRetries(
                circuitBreaker,
                operation,
                new RetryOptions
                    {
                        AllowedRetries = allowedRetries,
                        RetryInterval = retryInterval
                    });
        }

        public static void ExecuteWithRetries(this CircuitBreaker circuitBreaker, Action operation, RetryOptions retryOptions = null)
        {
            retryOptions = retryOptions ?? new RetryOptions();

            var attempts = 0;
            var exceptions = new List<Exception>();
            Action<Exception> handleFailure = ex =>
            {
                attempts++;

                if (ex != null) exceptions.Add(ex);

                if (attempts >= retryOptions.AllowedRetries)
                {
                    if (exceptions.Any())
                        throw new AggregateException("The operation exhausted all possible retry opportunities.", exceptions);

                    throw new OpenCircuitException("The operation exhausted all possible retry opportunities while waiting for the circuit breaker to close (it was in the open state for every attempt).");
                }

                Thread.Sleep(retryOptions.RetryInterval);
            };
            while (attempts < retryOptions.AllowedRetries)
            {
                try
                {
                    if (!circuitBreaker.AllowedToAttemptExecute)
                    {
                        handleFailure(null);
                        continue;
                    }
                    circuitBreaker.Execute(operation);
                    return;
                }
                catch (OperationFailedException ex)
                {
                    handleFailure(ex.InnerException);
                }
            }
        }
    }
}
