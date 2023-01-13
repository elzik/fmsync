using Elzik.FmSync.Domain;
using EPS.Extensions.YamlMarkdown;
using YamlDotNet.Serialization;

namespace Elzik.FmSync.Infrastructure;

public class MarkdownFrontMatter : IMarkdownFrontMatter
{
    private readonly YamlMarkdown<CreatedDateFrontMatter> _createdDateYamlMarkdown;

    public MarkdownFrontMatter()
    {
        var deserializerBuilder = new DeserializerBuilder();
        deserializerBuilder.IgnoreUnmatchedProperties();

        _createdDateYamlMarkdown = new YamlMarkdown<CreatedDateFrontMatter>(deserializerBuilder.Build());
    }

    public DateTime? GetCreatedDateUtc(string markDownFilePath)
    {
        var frontMatter = _createdDateYamlMarkdown.Parse(markDownFilePath);

        if (frontMatter?.CreatedDate == null)
        {
            return null;
        }

        var createdDateFrontMatter =
            TimeZoneInfo.ConvertTimeToUtc(frontMatter.CreatedDate.Value, TimeZoneInfo.FindSystemTimeZoneById("GB"));

        return createdDateFrontMatter;
    }
}