using System;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ReliabilityPatterns
{
    public class CircuitBreaker
    {
        readonly Timer timer;
        int failureCount;
        CircuitBreakerState state;
        uint threshold;

        public CircuitBreaker()
            : this(5, TimeSpan.FromSeconds(60))
        {
        }

        public CircuitBreaker(uint threshold, TimeSpan timeout)
        {
            this.threshold = threshold;
            failureCount = 0;
            state = CircuitBreakerState.Closed;

            timer = new Timer(timeout.TotalMilliseconds);
            timer.Elapsed += TimerElapsed;
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
        public TimeSpan Timeout
        {
            get { return TimeSpan.FromMilliseconds(timer.Interval); }
            set { timer.Interval = value.TotalMilliseconds; }
        }

        /// <summary>
        /// The current service level of the circuit.
        /// </summary>
        public double ServiceLevel
        {
            get { return ((threshold - (double) failureCount)/threshold)*100; }
        }

        /// <summary>
        /// Current state of the circuit breaker.
        /// </summary>
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

        public void Execute(Action operation)
        {
            if (state == CircuitBreakerState.Open)
                throw new OpenCircuitException("Circuit breaker is currently open");

            try
            {
                // Execute operation
                operation();
            }
            catch (Exception ex)
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

                throw new OperationFailedException("Operation failed", ex);
            }

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

        /// <summary>
        /// Trips the circuit breaker if not already open.
        /// </summary>
        public void Trip()
        {
            if (state == CircuitBreakerState.Open) return;
            ChangeState(CircuitBreakerState.Open);
            timer.Start();
        }

        /// <summary>
        /// Resets the circuit breaker.
        /// </summary>
        public void Reset()
        {
            if (state == CircuitBreakerState.Closed) return;
            ChangeState(CircuitBreakerState.Closed);
            timer.Stop();
        }

        void ChangeState(CircuitBreakerState newState)
        {
            state = newState;
            OnCircuitBreakerStateChanged(new EventArgs());
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (State != CircuitBreakerState.Open) return;
            ChangeState(CircuitBreakerState.HalfOpen);
            timer.Stop();
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
    }
}