using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.Shared.Lib.Messaging.Exceptions
{
    public class MaxRetriesExceededException : Exception
    {
        public int AttemptsCount { get; }
        public TimeSpan TotalRetryDuration { get; }

        public MaxRetriesExceededException(
            string message,
            Exception innerException,
            int attemptsCount,
            TimeSpan totalRetryDuration)
            : base(message, innerException)
        {
            AttemptsCount = attemptsCount;
            TotalRetryDuration = totalRetryDuration;
        }
    }
}
