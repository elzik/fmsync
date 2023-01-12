using EPS.Extensions.YamlMarkdown;

namespace Elzik.FmSync.Infrastructure;

public class MarkdownFrontMatter : IMarkdownFrontMatter
{
    private readonly YamlMarkdown<CreatedDateFrontMatter> _createdDateYamlMarkdown;

    public MarkdownFrontMatter()
    {
        _createdDateYamlMarkdown = new YamlMarkdown<CreatedDateFrontMatter>();
    }

    public DateTime GetCreatedDate(string markDownFilePath)
    {
        var localCreatedDateFrontMatter = _createdDateYamlMarkdown.Parse(markDownFilePath).CreatedDate;
        var createdDateFrontMatter =
            TimeZoneInfo.ConvertTimeToUtc(localCreatedDateFrontMatter, TimeZoneInfo.FindSystemTimeZoneById("GB"));

        return createdDateFrontMatter;
    }
}