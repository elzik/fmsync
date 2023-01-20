using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Thinktecture.IO;
using Xunit;

namespace Elzik.FmSync.Application.Tests.Unit
{
    public class FrontMatterFolderSynchroniserTests
    {
        private readonly Fixture _fixture;
        private readonly MockLogger<FrontMatterFolderSynchroniser> _mockLogger;
        private readonly IDirectory _mockDirectory;
        private readonly IFrontMatterFileSynchroniser _mockFileSynchroniser;
        private readonly FrontMatterFolderSynchroniser _frontMatterFolderSynchroniser;


        public FrontMatterFolderSynchroniserTests()
        {
            _mockLogger = Substitute.For<MockLogger<FrontMatterFolderSynchroniser>>();
            _mockDirectory = Substitute.For<IDirectory>();
            _mockFileSynchroniser = Substitute.For<IFrontMatterFileSynchroniser>();

            _fixture = new Fixture();
            _fixture.Register<ILogger<FrontMatterFolderSynchroniser>>(() => _mockLogger);
            _fixture.Register(() => _mockDirectory);
            _fixture.Register(() => _mockFileSynchroniser);
            _frontMatterFolderSynchroniser = _fixture.Create<FrontMatterFolderSynchroniser>();
        }

        [Fact]
        public void SyncCreationDates_DirectoryPathSupplied_OnlyLogs()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            
            // Act
            _frontMatterFolderSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(LogLevel.Information, $"Synchronising files in {testDirectoryPath}");

            _mockFileSynchroniser.DidNotReceiveWithAnyArgs().SyncCreationDate(default!);
        }

        [Fact]
        public void SyncCreationDates_NoMarkDownFiles_OnlyLogs()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();

            // Act
            _frontMatterFolderSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(LogLevel.Information, Arg.Is<string>(s =>
                s.StartsWith("Synchronised 0 files out of a total 0 in")));
            _mockFileSynchroniser.DidNotReceiveWithAnyArgs().SyncCreationDate(default!);
        }

        [Fact]
        public void SyncCreationDates_WithMarkDownFiles_SyncsThoseFiles()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            var testFiles = _fixture.CreateMany<KeyValuePair<string, bool>>().ToList();
            SetMockDirectoryFilePaths(testDirectoryPath, testFiles);

            // Act
            _frontMatterFolderSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockFileSynchroniser.ReceivedWithAnyArgs(testFiles.Count).SyncCreationDate(default!);
            foreach (var testFilesPath in testFiles)
            {
                _mockFileSynchroniser.Received(1).SyncCreationDate(testFilesPath.Key);
            }
        }

        [Fact]
        public void SyncCreationDates_WithMarkDownFiles_LogsSummary()
        {
            // Arrange
            var testDirectoryPath = _fixture.Create<string>();
            var testFiles = _fixture.CreateMany<KeyValuePair<string, bool>>().ToList();
            SetMockDirectoryFilePaths(testDirectoryPath, testFiles);

            // Act
            _frontMatterFolderSynchroniser.SyncCreationDates(testDirectoryPath);

            // Assert
            _mockLogger.Received(1).Log(LogLevel.Information, Arg.Is<string>(s =>
                s.StartsWith($"Synchronised {testFiles.Count(pair => pair.Value)} files out of a total {testFiles.Count} in")));
        }

        private void SetMockDirectoryFilePaths(string testDirectoryPath, IEnumerable<KeyValuePair<string, bool>> testFiles)
        {
            var testFileList = testFiles.ToList();

            _mockDirectory.EnumerateFiles(testDirectoryPath, "*.md",
                    Arg.Is<EnumerationOptions>(options =>
                        options.MatchCasing == MatchCasing.CaseInsensitive && options.RecurseSubdirectories))
                .Returns(testFileList.Select(pair => pair.Key));

            foreach (var testFilePath in testFileList)
            {
                _mockFileSynchroniser.SyncCreationDate(testFilePath.Key).Returns(new SyncResult()
                {
                    FileCreatedDateUpdated = testFilePath.Value
                });
            }
        }
    }
}