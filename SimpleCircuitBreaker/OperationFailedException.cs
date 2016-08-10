// -----------------------------------------------------------------------------
// <copyright file="OperationFailedException.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the OperationFailedException class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker
{
    using System;

    /// <summary>
    /// Exception thrown when an attempted operation has failed.
    /// </summary>
    public class OperationFailedException : ApplicationException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperationFailedException"/> class.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        public OperationFailedException(string message) 
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperationFailedException"/> class.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="innerException">
        ///     The inner exception.
        /// </param>
        public OperationFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
