namespace Elzik.FmSync.Domain;

public interface IMarkdownFrontMatter
{
    DateTime? GetCreatedDateUtc(string markDownFilePath);
}