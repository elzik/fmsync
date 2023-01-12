using EPS.Extensions.YamlMarkdown;

namespace Elzik.FmSync.Infrastructure;

public class MarkdownFrontMatter : IMarkdownFrontMatter
{
    private readonly YamlMarkdown<CreatedDateFrontMatter> _createdDateYamlMarkdown;

    public MarkdownFrontMatter()
    {
        _createdDateYamlMarkdown = new YamlMarkdown<CreatedDateFrontMatter>();
    }

    public DateTime? GetCreatedDateUtc(string markDownFilePath)
    {
        var frontMatter = _createdDateYamlMarkdown.Parse(markDownFilePath);

        if (frontMatter == null)
        {
            return null;
        }

        var createdDateFrontMatter =
            TimeZoneInfo.ConvertTimeToUtc(frontMatter.CreatedDate, TimeZoneInfo.FindSystemTimeZoneById("GB"));

        return createdDateFrontMatter;
    }
}