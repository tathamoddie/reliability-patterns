using System;

namespace ReliabilityPatterns
{
    /// <summary>
    /// Exception thrown when an operation is being called on an open circuit.
    /// </summary>
    public class OpenCircuitException : ApplicationException
    {
        public OpenCircuitException()
        {
        }

        public OpenCircuitException(string message) 
            : base(message)
        {
        }

        public OpenCircuitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
