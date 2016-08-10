// -----------------------------------------------------------------------------
// <copyright file="RetryOptions.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the RetryOptions class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker
{
    using System;

    /// <summary>
    ///     The retry options.
    /// </summary>
    public class RetryOptions
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RetryOptions"/> class.
        /// </summary>
        public RetryOptions()
        {
            this.AllowedRetries = 12;

            this.RetryInterval = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        ///     Gets or sets the number of allowed retries.
        /// </summary>
        public ushort AllowedRetries { get; set; }

        /// <summary>
        ///     Gets or sets the retry interval.
        /// </summary>
        public TimeSpan RetryInterval { get; set; }
    }
}
