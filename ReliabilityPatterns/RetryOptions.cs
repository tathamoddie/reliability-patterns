using System;

namespace ReliabilityPatterns
{
    public class RetryOptions
    {
        public RetryOptions()
        {
            AllowedRetries = 12;
            RetryInterval = TimeSpan.FromSeconds(5);
        }

        public ushort AllowedRetries { get; set; }
        public TimeSpan RetryInterval { get; set; }
    }
}
