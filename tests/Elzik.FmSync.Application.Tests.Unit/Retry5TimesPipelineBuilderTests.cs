using FluentAssertions;
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

            descriptor.Strategies.Count.Should().Be(1);

            var options = descriptor.Strategies[0].Options;
            options.Should().NotBeNull();
            options.Should().BeOfType<RetryStrategyOptions>();
            var retryOptions = (RetryStrategyOptions)options!;
            retryOptions.Delay.Should().Be(TimeSpan.FromSeconds(2));
            retryOptions.BackoffType.Should().Be(DelayBackoffType.Exponential);
            retryOptions.MaxRetryAttempts.Should().Be(5);
        }
    }
}
