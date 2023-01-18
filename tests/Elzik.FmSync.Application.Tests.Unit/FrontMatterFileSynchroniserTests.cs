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
            _mockLogger.Received(1).Log(LogLevel.Information, $"Synchronising files in {testDirectoryPath}");

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
            _mockLogger.Received(1).Log(LogLevel.Information, Arg.Is<string>(s =>
                s.StartsWith("Synchronised 0 files out of a total 0 in")));
            _mockFile.DidNotReceiveWithAnyArgs().SetCreationTimeUtc(default!, default);
        }

        [Fact]
        public void SyncCreationDates_NoMarkDownCreationDate_OnlyLogs()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            var testFilePath = _fixture.Create<string>();
            SetMockDirectoryFilePaths(testDirectoryPath, testFilePath);
            _mockMarkDownFrontMatter.GetCreatedDateUtc(testFilePath).ReturnsNull();

            // Act
            _frontMatterFileSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(LogLevel.Information, 
                $"{testFilePath} has no Front Matter created date.");
            _mockFile.DidNotReceiveWithAnyArgs().SetCreationTimeUtc(default!, default);
        }

        [Fact]
        public void SyncCreationDates_MarkDownAndFileDateEqual_OnlyLogs()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            var testFilePath = _fixture.Create<string>();
            SetMockDirectoryFilePaths(testDirectoryPath, testFilePath);
            var testDate = _fixture.Create<DateTime>();
            _mockFile.GetCreationTimeUtc(testFilePath).Returns(testDate);
            _mockMarkDownFrontMatter.GetCreatedDateUtc(testFilePath).Returns(testDate);

            // Act
            _frontMatterFileSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(
                LogLevel.Information,
                Arg.Is<IDictionary<string, object>>(
                    dict =>
                        dict.Any(kv => kv.Key == "{OriginalFormat}" 
                                       && (string)kv.Value == "{FilePath} has a file created date ({FileCreatedDate}) " +
                                       "the same as the created date specified in its Front Matter.") &&
                        dict.Any(kv => kv.Key == "FilePath" 
                                       && (string)kv.Value == testFilePath) &&
                        dict.Any(kv => kv.Key == "FileCreatedDate" 
                                       && (DateTime)kv.Value == testDate)));
            _mockFile.DidNotReceiveWithAnyArgs().SetCreationTimeUtc(default!, default);
        }

        [Fact]
        public void SyncCreationDates_FileDateLaterThanMarkdownDate_LogsAndUpdates()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            var testFilePath = _fixture.Create<string>();
            SetMockDirectoryFilePaths(testDirectoryPath, testFilePath);
            var testMarkDownDate = _fixture.Create<DateTime>();
            var testFileDate = testMarkDownDate.AddTicks(_fixture.Create<long>());
            _mockFile.GetCreationTimeUtc(testFilePath).Returns(testFileDate);
            _mockMarkDownFrontMatter.GetCreatedDateUtc(testFilePath).Returns(testMarkDownDate);

            // Act
            _frontMatterFileSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(
                LogLevel.Information,
                Arg.Is<IDictionary<string, object>>(
                    dict =>
                        dict.Any(kv => kv.Key == "{OriginalFormat}"
                                       && (string)kv.Value == "{FilePath} has a file created date ({FileCreatedDate}) {RelativeDescription} " +
                                       "than the created date specified in its Front Matter ({FrontMatterCreatedDate})") &&
                        dict.Any(kv => kv.Key == "FilePath"
                                       && (string)kv.Value == testFilePath) &&
                        dict.Any(kv => kv.Key == "FileCreatedDate"
                                       && (DateTime)kv.Value == testFileDate) &&
                        dict.Any(kv => kv.Key == "RelativeDescription"
                                       && (string)kv.Value == "later") &&
                        dict.Any(kv => kv.Key == "FrontMatterCreatedDate"
                                       && (DateTime)kv.Value == testMarkDownDate)));
            _mockFile.Received(1).SetCreationTimeUtc(testFilePath, testMarkDownDate);
        }

        [Fact]
        public void SyncCreationDates_FileDateEarlierThanMarkdownDate_LogsAndUpdates()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            var testFilePath = _fixture.Create<string>();
            SetMockDirectoryFilePaths(testDirectoryPath, testFilePath);
            var testMarkDownDate = _fixture.Create<DateTime>();
            var testFileDate = testMarkDownDate.Subtract(_fixture.Create<TimeSpan>());
            _mockFile.GetCreationTimeUtc(testFilePath).Returns(testFileDate);
            _mockMarkDownFrontMatter.GetCreatedDateUtc(testFilePath).Returns(testMarkDownDate);

            // Act
            _frontMatterFileSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(
                LogLevel.Information,
                Arg.Is<IDictionary<string, object>>(
                    dict =>
                        dict.Any(kv => kv.Key == "{OriginalFormat}"
                                       && (string)kv.Value == "{FilePath} has a file created date ({FileCreatedDate}) {RelativeDescription} " +
                                       "than the created date specified in its Front Matter ({FrontMatterCreatedDate})") &&
                        dict.Any(kv => kv.Key == "FilePath"
                                       && (string)kv.Value == testFilePath) &&
                        dict.Any(kv => kv.Key == "FileCreatedDate"
                                       && (DateTime)kv.Value == testFileDate) &&
                        dict.Any(kv => kv.Key == "RelativeDescription"
                                       && (string)kv.Value == "earlier") &&
                        dict.Any(kv => kv.Key == "FrontMatterCreatedDate"
                                       && (DateTime)kv.Value == testMarkDownDate)));
            _mockFile.Received(1).SetCreationTimeUtc(testFilePath, testMarkDownDate);
        }

        private void SetMockDirectoryFilePaths(string testDirectoryPath, params string[] testFilePath)
        {
            _mockDirectory.EnumerateFiles(testDirectoryPath, "*.md",
                    Arg.Is<EnumerationOptions>(options =>
                        options.MatchCasing == MatchCasing.CaseInsensitive && options.RecurseSubdirectories))
                .Returns(testFilePath);
        }
    }
}