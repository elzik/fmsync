using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Shouldly;
using Microsoft.Extensions.Options;
using Xunit;

namespace Elzik.FmSync.Infrastructure.Tests.Integration;

public sealed class MarkdownFrontMatterTests : IDisposable
{
    [Theory]
    [InlineData("./TestFiles/YamlContainsOnlyCreatedDate.md", "GMT Standard Time", null, "2023-01-07 14:28:22", 
        "because the current time zone is GMT")]
    [InlineData("./TestFiles/YamlContainsOnlyCreatedDateWithPrecedingNewLines.md", "GMT Standard Time", null, "2023-01-07 14:28:22", 
        "because the current time zone is GMT")]
    [InlineData("./TestFiles/YamlContainsCreatedDateAndOtherValue.md", "GMT Standard Time", null, "2022-03-31 13:52:15", 
        "because the current time zone is GMT at BST")]
    [InlineData("./TestFiles/YamlContainsOnlyCreatedDate.md", "AUS Eastern Standard Time", null, "2023-01-07 03:28:22", 
        "because the current time zone is AUS")]
    [InlineData("./TestFiles/YamlContainsCreatedDateAndOtherValue.md", "AUS Eastern Standard Time", null, "2022-03-31 03:52:15", 
        "because the current time zone is AUS")]
    [InlineData("./TestFiles/YamlContainsOnlyCreatedDateWithPrecedingNewLines.md", "AUS Eastern Standard Time", null, "2023-01-07 03:28:22", 
        "because the current time zone is AUS")]
    [InlineData("./TestFiles/YamlContainsOnlyCreatedDate.md", "GMT Standard Time", "AUS Eastern Standard Time", "2023-01-07 03:28:22", 
        "because even though the current time zone is GMT the YAML is configured for AUS")]
    [InlineData("./TestFiles/YamlContainsCreatedDateAndOtherValue.md", "GMT Standard Time", "AUS Eastern Standard Time", "2022-03-31 03:52:15", 
        "because even though the current time zone is GMT the YAML is configured for AUS")]
    [InlineData("./TestFiles/YamlContainsOnlyCreatedDateWithPrecedingNewLines.md", "GMT Standard Time", "AUS Eastern Standard Time", "2023-01-07 03:28:22", 
        "because even though the current time zone is GMT the YAML is configured for AUS")]
    [InlineData("./TestFiles/YamlContainsOnlyDateWithOffset.md", "GMT Standard Time", null, "2023-02-14 13:20:32", 
        "because even though our current time zone is GMT it is ignored because a time offset was supplied in the Front Matter data")]
    [InlineData("./TestFiles/YamlContainsOnlyDateWithOffset.md", "GMT Standard Time", "AUS Eastern Standard Time", "2023-02-14 13:20:32", 
        "because even though our current time zone is GMT and the Front Matter is configured for AUS they are both ignored because a time offset was supplied in the Front Matter data")]
    public void GetCreatedDate_YamlContainsCreatedDate_ReturnsCreatedDate(string testFilePath, string localTimeZone, 
        string? configuredTimeZone, string expectedUtcDateString, string because)
    {
        // Arrange
        SetTimeZone(localTimeZone);
        var testOptions = Options.Create(new FrontMatterOptions
        {
            TimeZoneId = configuredTimeZone
        });
        var expectedDateUtc = DateTime.ParseExact(expectedUtcDateString, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        // Act
        var markdownFrontMatter = new MarkdownFrontMatter(testOptions);
        var createdDate = markdownFrontMatter.GetCreatedDateUtc(testFilePath);

        // Assert
        createdDate.ShouldBe(expectedDateUtc, because);
        createdDate!.Value.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Theory]
    [InlineData("./TestFiles/YamlIsEmpty.md")]
    [InlineData("./TestFiles/YamlContainsOnlyWhitespace.md")]
    [InlineData("./TestFiles/YamlContainsOnlyNonCreatedDate.md")]
    [InlineData("./TestFiles/YamlIsMissingBodyIsPresent.md")]
    [InlineData("./TestFiles/YamlSectionNeverClosed.md")]
    [InlineData("./TestFiles/YamlSectionNeverOpened.md")]
    [InlineData("./TestFiles/TextPrecedesYamlSection.md")]
    [InlineData("./TestFiles/WhitespacePrecedesYamlSection.md")]
    [InlineData("./TestFiles/YamlSectionOpenedWithExtraCharacters.md")]
    [InlineData("./TestFiles/YamlSectionClosedWithExtraCharacters.md")]
    public void GetCreatedDate_YamlContainsNoCreatedDateOrNoValidYaml_ReturnsNullDate(string testFilePath)
    {
        // Arrange
        var testOptions = Options.Create(new FrontMatterOptions());

        // Act
        var markdownFrontMatter = new MarkdownFrontMatter(testOptions);
        var createdDate = markdownFrontMatter.GetCreatedDateUtc(testFilePath);

        // Assert
        createdDate.ShouldBeNull();
    }

    [Theory]
    [InlineData("./TestFiles/YamlContainsOnlyMinCreatedDate.md")]
    public void GetCreatedDate_YamlContainsMinDate_ReturnsCreatedDate(string testFilePath)
    {
        // Arrange
        var testOptions = Options.Create(new FrontMatterOptions());
        var expectedDateUtc = DateTime.MinValue;

        // Act
        var markdownFrontMatter = new MarkdownFrontMatter(testOptions);
        var createdDate = markdownFrontMatter.GetCreatedDateUtc(testFilePath);

        // Assert
        createdDate.ShouldBe(expectedDateUtc);
    }

    [Theory]
    [InlineData("./TestFiles/YamlContainsOnlyMaxCreatedDate.md")]
    public void GetCreatedDate_YamlContainsMaxDate_ReturnsCreatedDate(string testFilePath)
    {
        // Arrange
        var testOptions = Options.Create(new FrontMatterOptions());
        var expectedDateUtc = DateTime.MaxValue;

        // Act
        var markdownFrontMatter = new MarkdownFrontMatter(testOptions);
        var createdDate = markdownFrontMatter.GetCreatedDateUtc(testFilePath);

        // Assert
        createdDate.ShouldBe(expectedDateUtc);
    }

    private static void SetTimeZone(string mockTimeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(mockTimeZoneId);
        var info = typeof(TimeZoneInfo).GetField("s_cachedData", BindingFlags.NonPublic | BindingFlags.Static);
        Debug.Assert(info != null, nameof(info) + " != null");

        var cachedData = info.GetValue(null);
        Debug.Assert(cachedData != null, nameof(cachedData) + " != null");

        var field = cachedData.GetType().GetField("_localTimeZone",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Instance);
        Debug.Assert(field != null, nameof(field) + " != null");

        field.SetValue(cachedData, timeZone);
    }

    public void Dispose()
    {
        TimeZoneInfo.ClearCachedData();
    }
}