// -----------------------------------------------------------------------------
// <copyright file="ICircuitBreaker.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the ICircuitBreaker interface.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker
{
    using System;

    /// <summary>
    ///     The CircuitBreaker interface.
    /// </summary>
    public interface ICircuitBreaker
    {
        /// <summary>
        ///     The state changed.
        /// </summary>
        event EventHandler StateChanged;

        /// <summary>
        ///     The service level changed.
        /// </summary>
        event EventHandler ServiceLevelChanged;

        /// <summary>
        ///     Gets or sets the number of failures allowed before the circuit trips.
        /// </summary>
        uint Threshold { get; set; }

        /// <summary>
        ///     Gets or sets the time before the circuit attempts to close after being tripped.
        /// </summary>
        TimeSpan ResetTimeout { get; set; }

        /// <summary>
        ///     Gets the current service level of the circuit represented as as percentage.
        ///     0 means it's offline. 100 means everything is ok.
        /// </summary>
        double ServiceLevel { get; }

        /// <summary>
        ///     Gets the current state of the circuit.
        /// </summary>
        CircuitBreakerState State { get; }

        /// <summary>
        ///     Gets a value indicating whether or not the call through the circuit is allowed to
        ///     attempt to execute.
        /// </summary>
        bool AllowedToAttemptExecute { get; }

        /// <summary>
        ///     Attempts to execute the requested operation through the circuit.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The type of the requested operation.
        /// </typeparam>
        /// <param name="operation">
        ///     The operation to attempt to execute.
        /// </param>
        /// <returns>
        ///     The result of the requested operation.
        /// </returns>
        TResult Execute<TResult>(Func<TResult> operation);

        /// <summary>
        ///     Attempts to execute the requested operation through the circuit.
        /// </summary>
        /// <param name="operation">
        ///     The operation to attempt to execute.
        /// </param>
        void Execute(Action operation);

        /// <summary>
        ///     Trips the circuit breaker. If the circuit breaker is already open,
        ///     this method has no effect.
        /// </summary>
        void Trip();

        /// <summary>
        ///     Resets the circuit breaker. If the circuit breaker is already closed,
        ///     this method has no effect.
        /// </summary>
        void Reset();
    }
}
