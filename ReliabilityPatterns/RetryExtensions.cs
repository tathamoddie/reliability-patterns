using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ReliabilityPatterns
{
    public static class RetryExtensions
    {
        public static void ExecuteWithRetries(this CircuitBreaker circuitBreaker, Action operation, ushort allowedRetries, TimeSpan retryInterval)
        {
            var attempts = 0;
            var exceptions = new List<Exception>();
            Action<Exception> handleFailure = ex =>
            {
                attempts++;

                if (ex != null) exceptions.Add(ex);

                if (attempts >= allowedRetries)
                {
                    if (exceptions.Any())
                        throw new AggregateException("The operation exhausted all possible retry opportunities.", exceptions);

                    throw new OpenCircuitException("The operation exhausted all possible retry opportunities while waiting for the circuit breaker to close (it was in the open state for every attempt).");
                }

                Thread.Sleep(retryInterval);
            };
            while (attempts < allowedRetries)
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
