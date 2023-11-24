using Polly;
using Polly.Retry;

namespace Elzik.FmSync
{
    public static class Retry5TimesPipelineBuilder
    {
        public static readonly string StrategyName = "Retry5Times";

        public static ResiliencePipelineBuilder AddRetry5Times(this ResiliencePipelineBuilder builder)
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 5,
                BackoffType = DelayBackoffType.Exponential,
            });

            return builder;
        }
    }
}
