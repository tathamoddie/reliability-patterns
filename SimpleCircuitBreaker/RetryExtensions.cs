// -----------------------------------------------------------------------------
// <copyright file="RetryExtensions.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the RetryExtensions class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Properties;

    /// <summary>
    ///     The retry extensions.
    /// </summary>
    public static class RetryExtensions
    {
        /// <summary>
        ///     Executes the requested operation with a circuit breaker and the requested number of
        ///     retries.
        /// </summary>
        /// <param name="circuitBreaker">
        ///     The circuit breaker.
        /// </param>
        /// <param name="operation">
        ///     The operation.
        /// </param>
        /// <param name="retryOptions">
        ///     The retry options.
        /// </param>
        /// <typeparam name="TResult">
        ///     The type for the operation.
        /// </typeparam>
        /// <returns>
        ///     The <see cref="TResult"/>.
        /// </returns>
        public static TResult ExecuteWithRetries<TResult>(
            this CircuitBreaker circuitBreaker,
            Func<TResult> operation,
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

        /// <summary>
        ///     Executes the requested operation with a circuit breaker and the requested number of
        ///     retries.
        /// </summary>
        /// <param name="circuitBreaker">
        ///     The circuit breaker.
        /// </param>
        /// <param name="operation">
        ///     The operation.
        /// </param>
        /// <param name="retryOptions">
        ///     The retry options.
        /// </param>
        public static void ExecuteWithRetries(
            this CircuitBreaker circuitBreaker, Action operation, RetryOptions retryOptions = null)
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

        /// <summary>
        ///     The handle failure extension method.
        /// </summary>
        /// <param name="retryOptions">
        ///     The retry options for the failure.
        /// </param>
        /// <param name="exceptions">
        ///     The exceptions.
        /// </param>
        /// <param name="attempts">
        ///     The number of attempts made.
        /// </param>
        /// <returns>
        ///     The <see cref="Func{TResult}"/>.
        /// </returns>
        /// <exception cref="AggregateException">
        ///     The aggregate exception.
        /// </exception>
        /// <exception cref="OpenCircuitException">
        ///     The open circuit exception.
        /// </exception> 
        private static Func<Exception, int> HandleFailure(
            RetryOptions retryOptions, ICollection<Exception> exceptions, int attempts)
        {
            return ex =>
                       {
                           attempts++;

                           if (ex != null)
                           {
                               exceptions.Add(ex);
                           }

                           if (attempts >= retryOptions.AllowedRetries)
                           {
                               if (exceptions.Any())
                               {
                                   throw new AggregateException(
                                       Resource.RetryExtensionsHandleFailureAggregateExceptionMessage,
                                       exceptions);
                               }

                               throw new OpenCircuitException(
                                   Resource.RetryExtensionsHandleFailureOpenCircuitExceptionMessage);
                           }

                           Thread.Sleep(retryOptions.RetryInterval);

                           return attempts;
                       };
        }
    }
}
