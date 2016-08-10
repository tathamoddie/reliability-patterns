// -----------------------------------------------------------------------------
// <copyright file="CircuitBreakerState.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//   This file contains the CircuitBreakerState enum.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker
{
    /// <summary>
    ///     The circuit breaker state.
    /// </summary>
    public enum CircuitBreakerState
    {
        /// <summary>
        ///     The circuit is closed state.
        /// </summary>
        Closed,

        /// <summary>
        ///     The circuit is open state.
        /// </summary>
        Open,

        /// <summary>
        ///     The circuit is half open state.
        /// </summary>
        HalfOpen
    }
}
