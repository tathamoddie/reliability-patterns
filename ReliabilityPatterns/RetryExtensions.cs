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

        public static TResult ExecuteWithRetries<TResult>(this CircuitBreaker circuitBreaker, Func<TResult> operation,
                                           RetryOptions retryOptions = null)
        {
            retryOptions = retryOptions ?? new RetryOptions();
            var attempts = 0;
            var exceptions = new List<Exception>();
            var handleFailure = HandleFailure(retryOptions, exceptions, attempts);
            while (attempts < retryOptions.AllowedRetries)
            {
                try
                {
                    if (!circuitBreaker.AllowedToAttemptExecute)
                    {
                        attempts = handleFailure(null);
                        continue;
                    }
                    return circuitBreaker.Execute(operation);
                }
                catch (OperationFailedException ex)
                {
                    attempts = handleFailure(ex.InnerException);
                }
            }
            return default(TResult);
        }

        private static Func<Exception,int> HandleFailure(RetryOptions retryOptions, ICollection<Exception> exceptions, int attempts)
        {
            return ex =>
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
                           return attempts;
                       };
        }

        public static void ExecuteWithRetries(this CircuitBreaker circuitBreaker, Action operation, RetryOptions retryOptions = null)
        {
            retryOptions = retryOptions ?? new RetryOptions();

            var attempts = 0;
            var exceptions = new List<Exception>();
            var handleFailure = HandleFailure(retryOptions, exceptions, attempts);
            while (attempts < retryOptions.AllowedRetries)
            {
                try
                {
                    if (!circuitBreaker.AllowedToAttemptExecute)
                    {
                        attempts = handleFailure(null);
                        continue;
                    }
                    circuitBreaker.Execute(operation);
                    return;
                }
                catch (OperationFailedException ex)
                {
                    attempts = handleFailure(ex.InnerException);
                }
            }
        }
    }
}
