using AutoFixture;
using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
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
        private readonly FrontMatterFileSynchroniser _frontMatterFileSynchroniser;

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
            _frontMatterFileSynchroniser = _fixture.Create<FrontMatterFileSynchroniser>();
        }

        [Fact]
        public void SyncCreationDates_DirectoryPathSupplied_OnlyLogs()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            
            // Act
            _frontMatterFileSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(Arg.Is(LogLevel.Information), Arg.Is($"Synchronising files in {testDirectoryPath}"));
            _mockFile.DidNotReceiveWithAnyArgs().SetCreationTimeUtc(default!, default);
        }

        [Fact]
        public void SyncCreationDates_NoMarkDownFiles_OnlyLogs()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();

            // Act
            _frontMatterFileSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(Arg.Is(LogLevel.Information), Arg.Is<string>(s =>
                s.StartsWith("Synchronised 0 files out of a total 0 in")));
            _mockFile.DidNotReceiveWithAnyArgs().SetCreationTimeUtc(default!, default);
        }

        [Fact]
        public void SyncCreationDates_NoMarkDownCreationDate_Only()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            var testFilePath = _fixture.Create<string>();
            _mockDirectory.EnumerateFiles(Arg.Is(testDirectoryPath), Arg.Is("*.md"), 
                Arg.Is<EnumerationOptions>(options => options.MatchCasing == MatchCasing.CaseInsensitive 
                                                      && options.RecurseSubdirectories))
                .Returns(new[] { testFilePath });
            _mockMarkDownFrontMatter.GetCreatedDateUtc(testFilePath).ReturnsNull();

            // Act
            _frontMatterFileSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(Arg.Is(LogLevel.Information), 
                Arg.Is($"{testFilePath} has no Front Matter created date."));
            _mockFile.DidNotReceiveWithAnyArgs().SetCreationTimeUtc(default!, default);
        }
    }
}