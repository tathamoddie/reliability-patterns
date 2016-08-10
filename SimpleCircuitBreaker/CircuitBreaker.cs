// -----------------------------------------------------------------------------
// <copyright file="CircuitBreaker.cs" company="None">
//     Copyright (c) 2016 Microsoft Public License.
// </copyright>
// <summary>
//     This file contains the CircuitBreaker class.
// </summary>
// -----------------------------------------------------------------------------

namespace SimpleCircuitBreaker
{
    using System;
    using System.Threading;
    using System.Timers;

    using Properties;

    using Timer = System.Timers.Timer;

    /// <summary>
    ///     The circuit breaker class.
    /// </summary>
    public class CircuitBreaker : ICircuitBreaker
    {
        /// <summary>
        ///     The reset timer.
        /// </summary>
        private readonly Timer resetTimer;

        /// <summary>
        ///     The latency timer.
        /// </summary>
        private readonly Timer latencyTimer;

        /// <summary>
        ///     The failure count.
        /// </summary>
        private int failureCount;

        /// <summary>
        ///     The error threshold before the circuit trips.
        /// </summary>
        private uint threshold;

        /// <summary>
        ///     The most recent time it took to make a call through the circuit breaker.
        /// </summary>
        private TimeSpan latency;

        /// <summary>
        ///     The average amount of time it takes to make a call through the circuit breaker.
        /// </summary>
        private TimeSpan averageLatency;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CircuitBreaker"/> class.
        /// </summary>
        public CircuitBreaker()
            : this(5, TimeSpan.FromSeconds(60))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CircuitBreaker"/> class.
        /// </summary>
        /// <param name="threshold">
        ///     The threshold.
        /// </param>
        /// <param name="resetTimeout">
        ///     The reset timeout.
        /// </param>
        public CircuitBreaker(uint threshold, TimeSpan resetTimeout)
        {
            this.threshold = threshold;
            this.failureCount = 0;
            this.State = CircuitBreakerState.Closed;

            this.resetTimer = new Timer(resetTimeout.TotalMilliseconds);
            this.resetTimer.Elapsed += this.ResetTimerElapsed;
        }

        public event EventHandler StateChanged;

        public event EventHandler ServiceLevelChanged;

        /// <summary>
        ///     Gets or sets the number of failures allowed before the circuit trips.
        /// </summary>
        public uint Threshold
        {
            get
            {
                return this.threshold;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(Resource.CircuitBreakerThresholdExceptionMessage);
                }

                this.threshold = value;
            }
        }

        /// <summary>
        ///     Gets or sets the time before the circuit attempts to close after being tripped.
        /// </summary>
        public TimeSpan ResetTimeout
        {
            get
            {
                return TimeSpan.FromMilliseconds(this.resetTimer.Interval);
            }

            set
            {
                this.resetTimer.Interval = value.TotalMilliseconds;
            }
        }

        /// <summary>
        ///     Gets the current service level of the circuit represented as as percentage.
        ///     0 means it's offline. 100 means everything is ok.
        /// </summary>
        public double ServiceLevel => ((this.threshold - (double)this.failureCount) / this.threshold) * 100;

        /// <summary>
        ///     Gets the current state of the circuit.
        /// </summary>
        public CircuitBreakerState State { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether or not the call through the circuit is allowed to
        ///     attempt to execute.
        /// </summary>
        public bool AllowedToAttemptExecute => 
            this.State == CircuitBreakerState.Closed 
            || this.State == CircuitBreakerState.HalfOpen;

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
        public TResult Execute<TResult>(Func<TResult> operation)
        {
            // Start the latency timer.


            // Check if the circuit is already open.
            if (this.State == CircuitBreakerState.Open)
            {
                throw new OpenCircuitException(Resource.CircuitBreakerCurrentlyOpenExceptionMessage);
            }

            // Result to eventually return.
            TResult result;

            try
            {
                // Execute operation
                result = operation();
            }
            catch (Exception ex)
            {
                if (this.State == CircuitBreakerState.HalfOpen)
                {
                    // Operation failed in a half-open state, so reopen circuit
                    this.Trip();
                }
                else if (this.failureCount < this.threshold)
                {
                    // Operation failed in an open state, so increment failure count and throw exception
                    Interlocked.Increment(ref this.failureCount);

                    this.OnServiceLevelChanged(new EventArgs());
                }
                else if (this.failureCount >= this.threshold)
                {
                    // Failure count has reached threshold, so trip circuit breaker
                    this.Trip();
                }

                throw new OperationFailedException(Resource.CircuitBreakerOperationFailedExceptionMessage, ex);
            }

            if (this.State == CircuitBreakerState.HalfOpen)
            {
                // If operation succeeded without error and circuit breaker 
                // is in a half-open state, then reset
                this.Reset();
            }

            if (this.failureCount > 0)
            {
                // Decrement failure count to improve service level
                Interlocked.Decrement(ref this.failureCount);

                this.OnServiceLevelChanged(new EventArgs());
            }

            return result;
        }

        /// <summary>
        ///     Attempts to execute the requested operation through the circuit.
        /// </summary>
        /// <param name="operation">
        ///     The operation to attempt to execute.
        /// </param>
        public void Execute(Action operation)
        {
            this.Execute<object>(() =>
            {
                operation();
                return null;
            });
        }

        /// <summary>
        ///     Trips the circuit breaker. If the circuit breaker is already open,
        ///     this method has no effect.
        /// </summary>
        public void Trip()
        {
            if (this.State == CircuitBreakerState.Open)
            {
                return;
            }

            this.ChangeState(CircuitBreakerState.Open);

            this.resetTimer.Start();
        }

        /// <summary>
        ///     Resets the circuit breaker. If the circuit breaker is already closed,
        ///     this method has no effect.
        /// </summary>
        public void Reset()
        {
            if (this.State == CircuitBreakerState.Closed)
            {
                return;
            }

            this.ChangeState(CircuitBreakerState.Closed);

            this.resetTimer.Stop();
        }

        private void ChangeState(CircuitBreakerState newState)
        {
            this.State = newState;
            this.OnCircuitBreakerStateChanged(new EventArgs());
        }

        private void ResetTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (this.State != CircuitBreakerState.Open)
            {
                return;
            }

            this.ChangeState(CircuitBreakerState.HalfOpen);

            this.resetTimer.Stop();
        }

        private void OnCircuitBreakerStateChanged(EventArgs e)
        {
            this.StateChanged?.Invoke(this, e);
        }

        private void OnServiceLevelChanged(EventArgs e)
        {
            this.ServiceLevelChanged?.Invoke(this, e);
        }
    }
}
