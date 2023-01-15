using AutoFixture;
using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Thinktecture.IO;
using Xunit;

namespace Elzik.FmSync.Application.Tests.Unit
{
    public class FrontMatterFileSynchroniserTests
    {
        private readonly Fixture _fixture;
        private readonly MockLogger<FrontMatterFileSynchroniser> _mockLogger;
        private readonly IMarkdownFrontMatter _mockMarkDownFrontMatter;
        private readonly IFile _mockFile;
        private readonly IDirectory _mockDirectory;

        public FrontMatterFileSynchroniserTests()
        {
            _mockLogger = Substitute.For<MockLogger<FrontMatterFileSynchroniser>>();
            _mockMarkDownFrontMatter = Substitute.For<IMarkdownFrontMatter>();
            _mockFile = Substitute.For<IFile>();
            _mockDirectory = Substitute.For<IDirectory>();

            _fixture = new Fixture();
            _fixture.Register<ILogger<FrontMatterFileSynchroniser>>(() => _mockLogger);
            _fixture.Register(() => _mockMarkDownFrontMatter);
            _fixture.Register(() => _mockFile);
            _fixture.Register(() => _mockDirectory);
        }

        [Fact]
        public void SyncCreationDates_DirectoryPathSupplied_LogsDirectoryPath()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            
            // Act
            var frontMatterFileSynchroniser = _fixture.Create<FrontMatterFileSynchroniser>();
            frontMatterFileSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(Arg.Is(LogLevel.Information), Arg.Is($"Synchronising files in {testDirectoryPath}"));
        }

        [Fact]
        public void SyncCreationDates_NoMarkDownFiles_LogsExpectedSummary()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            //_mockDirectory.EnumerateFiles(Arg.Is(testDirectoryPath)).Returns(Array.Empty<string>());

            // Act
            var frontMatterFileSynchroniser = _fixture.Create<FrontMatterFileSynchroniser>();
            frontMatterFileSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(Arg.Is(LogLevel.Information), Arg.Is<string>(s =>
                s.StartsWith("Synchronised 0 files out of a total 0 in")));
        }
    }
}