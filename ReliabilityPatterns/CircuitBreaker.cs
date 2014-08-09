﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ReliabilityPatterns
{
    public class CircuitBreaker
    {
        readonly Timer resetTimer;
        int failureCount;
        CircuitBreakerState state;
        uint threshold;

        public CircuitBreaker()
            : this(5, TimeSpan.FromSeconds(60))
        {
        }

        public CircuitBreaker(uint threshold, TimeSpan resetTimeout)
        {
            this.threshold = threshold;
            failureCount = 0;
            state = CircuitBreakerState.Closed;

            resetTimer = new Timer(resetTimeout.TotalMilliseconds);
            resetTimer.Elapsed += ResetTimerElapsed;
        }

        /// <summary>
        /// Number of failures allowed before the circuit trips.
        /// </summary>
        public uint Threshold
        {
            get { return threshold; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Threshold must be greater than zero");
                
                threshold = value;
            }
        }

        /// <summary>
        /// The time before the circuit attempts to close after being tripped.
        /// </summary>
        public TimeSpan ResetTimeout
        {
            get { return TimeSpan.FromMilliseconds(resetTimer.Interval); }
            set { resetTimer.Interval = value.TotalMilliseconds; }
        }

        /// <summary>
        /// The current service level of the circuit represented as as percentage.
        /// 0 means it's offline. 100 means everything is ok.
        /// </summary>
        public double ServiceLevel
        {
            get { return ((threshold - (double) failureCount)/threshold)*100; }
        }
			
        public CircuitBreakerState State
        {
            get { return state; }
        }

        public bool AllowedToAttemptExecute
        {
            get { return State == CircuitBreakerState.Closed || State == CircuitBreakerState.HalfOpen; }
        }

        public event EventHandler StateChanged;
        public event EventHandler ServiceLevelChanged;

		/// <summary>
		/// Execute the operation "protected" by the circuit breaker.
		/// </summary>
		/// <returns>The operation result.</returns>
		/// <param name="operation">Operation to execute.</param>
		/// <typeparam name="TResult">The underlying operation return type.</typeparam>
        public TResult Execute<TResult>(Func<TResult> operation)
		{
			EnsureNotOpen ();
      
            TResult result;

            try
            {
                // Execute operation
                result = operation();

				OperationSucceeded ();

				return result;
            }
            catch (Exception ex)
            {
				OperationFailed (ex);
				throw new OperationFailedException("Operation failed", ex);
            }
        }

		/// <summary>
		/// Execute the operation "protected" by the circuit breaker.
		/// </summary>
		/// <param name="operation">Operation to execute.</param>
		public void Execute(Action operation)
		{
			Execute<object>(() =>
			{
				operation();
				return null;
			});
		}


		/// <summary>
		/// Execute an async operation "protected" by the circuit breaker and return an awaitable task.
		/// </summary>
		/// <returns>An awaitable task with the operation result.</returns>
		/// <param name="operation">Operation to execute.</param>
		/// <typeparam name="TResult">The underlying operation return type.</typeparam>
		public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation)
		{
			EnsureNotOpen();

			TResult result;

			try
			{
				// Execute operation
				result = await operation();

				OperationSucceeded();

				return result;
			}
			catch (Exception ex)
			{
				OperationFailed (ex);
				throw new OperationFailedException("Operation failed", ex);
			}
		}

		/// <summary>
		/// Execute an async operation "protected" by the circuit breaker and return an awaitable task.
		/// </summary>
		/// <returns>Awaitable task.</returns>
		/// <param name="operation">Operation to execute.</param>
		public async Task ExecuteAsync(Func<Task> operation)
		{
			await ExecuteAsync<Task<object>> (async () => {
				await operation();
				return null;
			});
		}

        /// <summary>
        /// Trips the circuit breaker. If the circuit breaker is already open,
        /// this method has no effect.
        /// </summary>
        public void Trip()
        {
            if (state == CircuitBreakerState.Open) return;
            ChangeState(CircuitBreakerState.Open);
            resetTimer.Start();
        }

        /// <summary>
        /// Resets the circuit breaker. If the circuit breaker is already closed,
        /// this method has no effect.
        /// </summary>
        public void Reset()
        {
            if (state == CircuitBreakerState.Closed) return;
            ChangeState(CircuitBreakerState.Closed);
            resetTimer.Stop();
        }

        void ChangeState(CircuitBreakerState newState)
        {
            state = newState;
            OnCircuitBreakerStateChanged(new EventArgs());
        }

        void ResetTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (State != CircuitBreakerState.Open) return;
            ChangeState(CircuitBreakerState.HalfOpen);
            resetTimer.Stop();
        }

        void OnCircuitBreakerStateChanged(EventArgs e)
        {
            if (StateChanged != null)
                StateChanged(this, e);
        }

        void OnServiceLevelChanged(EventArgs e)
        {
            if (ServiceLevelChanged != null)
                ServiceLevelChanged(this, e);
        }

		void EnsureNotOpen()
		{
			if (state == CircuitBreakerState.Open)
				throw new OpenCircuitException("Circuit breaker is currently open");
		}

		void OperationFailed(Exception ex)
		{
			if (state == CircuitBreakerState.HalfOpen)
			{
				// Operation failed in a half-open state, so reopen circuit
				Trip();
			}
			else if (failureCount < threshold)
			{
				// Operation failed in an open state, so increment failure count and throw exception
				Interlocked.Increment(ref failureCount);

				OnServiceLevelChanged(new EventArgs());
			}
			else if (failureCount >= threshold)
			{
				// Failure count has reached threshold, so trip circuit breaker
				Trip();
			}
		}

		void OperationSucceeded()
		{
			if (state == CircuitBreakerState.HalfOpen)
			{
				// If operation succeeded without error and circuit breaker 
				// is in a half-open state, then reset
				Reset();
			}

			if (failureCount > 0)
			{
				// Decrement failure count to improve service level
				Interlocked.Decrement(ref failureCount);

				OnServiceLevelChanged(new EventArgs());
			}
		}
    }
}
