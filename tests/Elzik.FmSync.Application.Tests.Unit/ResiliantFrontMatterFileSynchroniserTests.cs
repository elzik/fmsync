using AutoFixture;
using NSubstitute;
using Polly;
using Polly.Registry;
using Xunit;

namespace Elzik.FmSync.Application.Tests.Unit
{
    public class ResiliantFrontMatterFileSynchroniserTests
 {
        private readonly Fixture _fixture;
        private readonly IFrontMatterFileSynchroniser _mockFrontMatterFileSynchroniser;
        private readonly ResiliencePipelineProvider<string> _emptyResiliencePipelineProvider;

        public ResiliantFrontMatterFileSynchroniserTests()
        {
            _fixture = new Fixture();

            _mockFrontMatterFileSynchroniser = Substitute.For<IFrontMatterFileSynchroniser>();

            _emptyResiliencePipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
            _emptyResiliencePipelineProvider
                .GetPipeline(Retry5TimesPipelineBuilder.StrategyName)
                .Returns(ResiliencePipeline.Empty);
        }

        [Fact]
        public void SyncCreationDate_IsCalled_Resiliently()
        {
            // Arrange
            var testFilePath = _fixture.Create<string>();

            // Act
            var resiliantFrontMatterFileSynchroniser = new ResiliantFrontMatterFileSynchroniser(
                _mockFrontMatterFileSynchroniser,
                _emptyResiliencePipelineProvider);
            resiliantFrontMatterFileSynchroniser.SyncCreationDate(testFilePath);

            // Assert
            _mockFrontMatterFileSynchroniser.Received(1).SyncCreationDate(testFilePath);
        }
    }
}
