using System;

namespace ReliabilityPatterns
{
    /// <summary>
    /// Exception thrown when an attempted operation has failed.
    /// </summary>
    public class OperationFailedException : ApplicationException
    {
        public OperationFailedException()
        {
        }

        public OperationFailedException(string message) 
            : base(message)
        {
        }

        public OperationFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
