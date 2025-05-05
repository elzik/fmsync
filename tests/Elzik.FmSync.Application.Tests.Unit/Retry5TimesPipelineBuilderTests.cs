using Shouldly;
using Polly;
using Polly.Retry;
using Polly.Testing;
using Xunit;

namespace Elzik.FmSync.Application.Tests.Unit
{
    public class Retry5TimesPipelineBuilderTests
    {
        [Fact]
        public void AddRetry5Times_Builds_ExpectedPipeline()
        {
            var testPipeline = new ResiliencePipelineBuilder()
                .AddRetry5Times()
                .Build();

            var descriptor = testPipeline.GetPipelineDescriptor();

            descriptor.Strategies.Count.ShouldBe(1);

            var options = descriptor.Strategies[0].Options;
            options.ShouldNotBeNull();
            options.ShouldBeOfType<RetryStrategyOptions>();
            var retryOptions = (RetryStrategyOptions)options!;
            retryOptions.Delay.ShouldBe(TimeSpan.FromSeconds(2));
            retryOptions.BackoffType.ShouldBe(DelayBackoffType.Exponential);
            retryOptions.MaxRetryAttempts.ShouldBe(5);
        }
    }
}
