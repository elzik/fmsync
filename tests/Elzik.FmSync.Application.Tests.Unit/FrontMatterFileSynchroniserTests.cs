using AutoFixture;
using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Thinktecture.IO;
using Xunit;

namespace Elzik.FmSync.Application.Tests.Unit;

public class FrontMatterFileSynchroniserTests
{
    private readonly Fixture _fixture;
    private readonly MockLogger<FrontMatterFileSynchroniser> _mockLogger;
    private readonly IMarkdownFrontMatter _mockMarkDownFrontMatter;
    private readonly IFile _mockFile;
    private readonly FrontMatterFileSynchroniser _frontMatterFileSynchroniser;

    public FrontMatterFileSynchroniserTests()
    {
        _mockMarkDownFrontMatter = Substitute.For<IMarkdownFrontMatter>();
        _mockFile = Substitute.For<IFile>();
        _mockLogger = Substitute.For<MockLogger<FrontMatterFileSynchroniser>>();

        _fixture = new Fixture();
        _fixture.Register<ILogger<FrontMatterFileSynchroniser>>(() => _mockLogger);
        _fixture.Register(() => _mockMarkDownFrontMatter);
        _fixture.Register(() => _mockFile);
        _frontMatterFileSynchroniser = _fixture.Create<FrontMatterFileSynchroniser>();
    }

    [Fact]
    public void SyncCreationDates_NoMarkDownCreationDate_OnlyLogs()
    {
        // Arrange
        var testFilePath = _fixture.Create<string>();
        _mockMarkDownFrontMatter.GetCreatedDateUtc(testFilePath).ReturnsNull();

        // Act
        _frontMatterFileSynchroniser.SyncCreationDate(testFilePath);

            // Assert
            _mockLogger.Received(1).Log(LogLevel.Debug, 
                $"{testFilePath} has no Front Matter created date.");
            _mockFile.DidNotReceiveWithAnyArgs().SetCreationTimeUtc(default!, default);
        }

    [Fact]
    public void SyncCreationDates_MarkDownAndFileDateEqual_OnlyLogs()
    {
        // Arrange
        var testFilePath = _fixture.Create<string>();
        var testDate = _fixture.Create<DateTime>();
        _mockFile.GetCreationTimeUtc(testFilePath).Returns(testDate);
        _mockMarkDownFrontMatter.GetCreatedDateUtc(testFilePath).Returns(testDate);

        // Act
        _frontMatterFileSynchroniser.SyncCreationDate(testFilePath);

            // Assert
            _mockLogger.Received(1).Log(
                LogLevel.Debug,
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
        var testFilePath = _fixture.Create<string>();
        var testMarkDownDate = _fixture.Create<DateTime>();
        var testFileDate = testMarkDownDate.AddTicks(_fixture.Create<long>());
        _mockFile.GetCreationTimeUtc(testFilePath).Returns(testFileDate);
        _mockMarkDownFrontMatter.GetCreatedDateUtc(testFilePath).Returns(testMarkDownDate);

        // Act
        _frontMatterFileSynchroniser.SyncCreationDate(testFilePath);

            // Assert
            _mockLogger.Received(1).Log(
                LogLevel.Debug,
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
        var testFilePath = _fixture.Create<string>();
        var testMarkDownDate = _fixture.Create<DateTime>();
        var testFileDate = testMarkDownDate.Subtract(_fixture.Create<TimeSpan>());
        _mockFile.GetCreationTimeUtc(testFilePath).Returns(testFileDate);
        _mockMarkDownFrontMatter.GetCreatedDateUtc(testFilePath).Returns(testMarkDownDate);

        // Act
        _frontMatterFileSynchroniser.SyncCreationDate(testFilePath);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Debug,
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
}