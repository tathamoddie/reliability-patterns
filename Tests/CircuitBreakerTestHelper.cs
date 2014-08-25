using System;
using ReliabilityPatterns;

namespace Tests
{
    public class CircuitBreakerTestHelper
    {
        readonly ushort _allowedRetries;
        public CircuitBreaker Breaker { get; private set; }
        public int CallCount { get; private set; }

        public CircuitBreakerTestHelper(uint threshold = 4, ushort allowedRetries = 0)
        {
            _allowedRetries = allowedRetries;
            Breaker = new CircuitBreaker(threshold, TimeSpan.FromSeconds(120));
        }

        public void ExecuteFailingAction()
        {
            Console.WriteLine("Before: " + Breaker.State);
            try
            {
                CallCount = 0;
                if (_allowedRetries > 0)
                {
                    Breaker.ExecuteWithRetries(() =>
                    {
                        CallCount++;
                        throw new Exception("foo");
                    }, new RetryOptions
                    {
                        AllowedRetries = _allowedRetries,
                        RetryInterval = TimeSpan.FromMilliseconds(10)
                    });
                }
                else
                {
                    Breaker.Execute(() =>
                    {
                        CallCount++;
                        throw new Exception("foo");
                    });
                }
            }
            catch (OpenCircuitException)
            {
                Console.WriteLine("Tripped due to open circuit");
            }
            catch (OperationFailedException)
            {
                Console.WriteLine("Tripped due to unhandled exception");
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Tripped due to unhandled exception. InnerExceptionCount: " + ex.InnerExceptions.Count);
            }

            Console.WriteLine("After {0} attempt/s: {1}", CallCount, Breaker.State + Environment.NewLine);
        }
    }
}