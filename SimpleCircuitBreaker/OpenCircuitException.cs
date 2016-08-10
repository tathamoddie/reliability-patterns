// -----------------------------------------------------------------------------
// <copyright file="OpenCircuitException.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the OpenCircuitException class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker
{
    using System;

    /// <summary>
    ///     Exception thrown when an operation is being called on an open circuit.
    /// </summary>
    public class OpenCircuitException : ApplicationException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OpenCircuitException"/> class.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        public OpenCircuitException(string message) 
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OpenCircuitException"/> class.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="innerException">
        ///     The inner exception.
        /// </param>
        public OpenCircuitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
