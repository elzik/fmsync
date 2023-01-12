namespace Elzik.FmSync;

public interface IMarkdownFrontMatter
{
    DateTime GetCreatedDate(string markDownFilePath);
}