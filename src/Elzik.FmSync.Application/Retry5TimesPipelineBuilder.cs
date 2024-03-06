using Polly;
using Polly.Retry;
using YamlDotNet.Core;

namespace Elzik.FmSync.Application
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
                ShouldHandle = new PredicateBuilder()
                    .Handle<IOException>()
                    .Handle<YamlException>(WithInnerFormatException)
            });

            return builder;
        }

        private static bool WithInnerFormatException(YamlException arg)
        {
            return arg.InnerException?.GetType() == typeof(FormatException);
        }
    }
}
