using FluentAssertions;
using Xunit;

namespace Elzik.FmSync.Infrastructure.Tests.Integration
{
    public class MarkdownFrontMatterTests
    {
        [Theory]
        [InlineData("./TestFiles/YamlContainsOnlyGBCreatedDate.md")]
        [InlineData("./TestFiles/YamlContainsGBCreatedDateAndOtherValue.md")]
        [InlineData("./TestFiles/YamlContainsOnlyGBCreatedDateWithPrecedingNewLines.md")]
        public void GetCreatedDate_YamlContainsCreatedDate_ReturnsCreatedDate(string testFilePath)
        {
            // Arrange
            var expectedDateUtc = DateTime.Parse("2023-01-07 14:28:22");

            // Act
            var markdownFrontMatter = new MarkdownFrontMatter();
            var createdDate = markdownFrontMatter.GetCreatedDateUtc(testFilePath);

            // Assert
            createdDate.Should().Be(expectedDateUtc);
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
            // Act
            var markdownFrontMatter = new MarkdownFrontMatter();
            var createdDate = markdownFrontMatter.GetCreatedDateUtc(testFilePath);

            // Assert
            createdDate.Should().BeNull();
        }

        [Theory]
        [InlineData("./TestFiles/YamlContainsOnlyMinCreatedDate.md")]
        public void GetCreatedDate_YamlContainsMinDate_ReturnsCreatedDate(string testFilePath)
        {
            // Arrange
            var expectedDateUtc = DateTime.MinValue;

            // Act
            var markdownFrontMatter = new MarkdownFrontMatter();
            var createdDate = markdownFrontMatter.GetCreatedDateUtc(testFilePath);

            // Assert
            createdDate.Should().Be(expectedDateUtc);
        }

        [Theory]
        [InlineData("./TestFiles/YamlContainsOnlyMaxCreatedDate.md")]
        public void GetCreatedDate_YamlContainsMaxDate_ReturnsCreatedDate(string testFilePath)
        {
            // Arrange
            var expectedDateUtc = DateTime.MaxValue;

            // Act
            var markdownFrontMatter = new MarkdownFrontMatter();
            var createdDate = markdownFrontMatter.GetCreatedDateUtc(testFilePath);

            // Assert
            createdDate.Should().Be(expectedDateUtc);
        }
    }
}