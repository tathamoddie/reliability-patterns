using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ReliabilityPatterns;

namespace Tests
{
    [TestFixture]
    public class CircuitBreakerTests
    {
        [Test]
        public void ExecuteShouldExecuteActionOnce()
        {
            var circuitBreaker = new CircuitBreaker();
            var actionCallCount = 0;

            circuitBreaker.Execute(() => { actionCallCount++; });

            Assert.AreEqual(1, actionCallCount);
        }

        [Test]
        public void ExecuteWithResultShouldReturnResult()
        {
            var circuitBreaker = new CircuitBreaker();
            var expectedResult = new object();

            var result = circuitBreaker.Execute(() => expectedResult);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase("", Result = 100d)]
        [TestCase("bad", Result = 80d)]
        [TestCase("bad good", Result = 100d)]
        [TestCase("bad bad", Result = 60d)]
        [TestCase("bad bad good", Result = 80d)]
        [TestCase("bad bad good good", Result = 100d)]
        [TestCase("bad good bad good", Result = 100d)]
        public double ServiceLevel(string callPattern)
        {
            var circuitBreaker = new CircuitBreaker();

            foreach (var call in callPattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (call)
                {
                    case "good":
                        circuitBreaker.Execute(() => { });
                        break;
                    case "bad":
                        try { circuitBreaker.Execute(() => { throw new Exception(); }); }
                        catch (OperationFailedException) { }
                        break;
                    default:
                        Assert.Fail("Unknown call sequence");
                        break;
                }
            }

            return circuitBreaker.ServiceLevel;
        }

		[Test]
		[TestCase("", Result = 100d)]
		[TestCase("bad", Result = 80d)]
		[TestCase("bad good", Result = 100d)]
		[TestCase("bad bad", Result = 60d)]
		[TestCase("bad bad good", Result = 80d)]
		[TestCase("bad bad good good", Result = 100d)]
		[TestCase("bad good bad good", Result = 100d)]
		public double ServiceLevelAsync(string callPattern)
		{
			var task = Task.Run<double>(async () => {

				var circuitBreaker = new CircuitBreaker ();

				foreach (var call in callPattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) {

					switch (call) {
					case "good":
						await circuitBreaker.ExecuteAsync (async () => {
							await Task.FromResult (0);
						});
						break;
					case "bad":
						try { 
							await circuitBreaker.ExecuteAsync (async () => {
								await Task.FromResult (0);
								throw new Exception ();
							}); 
						} catch (OperationFailedException) {
						}
						break;
					default:
						Assert.Fail ("Unknown call sequence");
						break;
					}
				}
				return circuitBreaker.ServiceLevel;
			});

			return task.Result;
		}
			
		[Test]
		public void WhenExecutingAsyncTresholdMayBeExceeded()
		{
			int totalTasks = 10;
			uint threshold = 2;

			var circuitBreaker = new CircuitBreaker (threshold, TimeSpan.FromHours(1));

			int operationFailed = 0;
			int operationFailedExceptions = 0;
			int openCircuitExceptions = 0;

			Task[] tasks = new Task[totalTasks];

			for (int i = 0; i < totalTasks; i++) 
			{
				tasks[i] = Task.Run(async () => {
					await circuitBreaker.ExecuteAsync (async () => {
						await Task.FromResult(0);
						await Task.Delay(1);
						Interlocked.Increment(ref operationFailed);
						throw new Exception();
					});
				});
			}

			// wait all tasks not throwing errors
			try
			{
				Task.WaitAll(tasks);
			}
			catch (AggregateException ae)
			{
				openCircuitExceptions = ae.InnerExceptions.Count(c => c is OpenCircuitException);
				operationFailedExceptions = ae.InnerExceptions.Count(c => c is OperationFailedException);
			}

			int openCircuitCount = totalTasks - operationFailed;

			// pretty sure circuit is open at this stage
			Assert.IsFalse(circuitBreaker.AllowedToAttemptExecute);

			Assert.AreEqual(openCircuitCount, openCircuitExceptions);
			Assert.AreEqual(operationFailed, operationFailedExceptions);

			// however several arriving requests might have entered the curcuit
			// before the previous one's failures were tracked!
			Assert.GreaterOrEqual(operationFailed, threshold);
		}
    }
}
