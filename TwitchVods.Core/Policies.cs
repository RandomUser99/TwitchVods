using Polly;
using System;

namespace TwitchVods.Core
{
    internal class Policies
    {
        internal static IAsyncPolicy RetryPolicy()
        {
            const int maxRetries = 2;

            return Policy.Handle<Exception>()
                .WaitAndRetryAsync(
                    maxRetries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
