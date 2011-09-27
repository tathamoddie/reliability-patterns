using System;
using System.Threading;

namespace ReliabilityPatterns
{
    public static class CircuitBreakerExtensions
    {
        public static void ExecuteWithRetries(this CircuitBreaker circuitBreaker, Action operation, ushort allowedRetries, TimeSpan retryInterval)
        {
            var attempts = 0;
            Action<Exception> handleFailure = ex =>
            {
                attempts++;
                if (attempts >= allowedRetries)
                {
                    throw ex ??
                        new OpenCircuitException("The operation exhausted all possible retry opportunities while waiting for the circuit breaker to close.");
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
