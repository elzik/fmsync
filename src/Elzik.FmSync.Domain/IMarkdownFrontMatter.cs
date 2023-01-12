namespace Elzik.FmSync;

public interface IMarkdownFrontMatter
{
    DateTime? GetCreatedDateUtc(string markDownFilePath);
}