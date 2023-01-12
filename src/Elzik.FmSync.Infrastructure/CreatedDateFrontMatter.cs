using YamlDotNet.Serialization;

namespace Elzik.FmSync.Infrastructure;

public class CreatedDateFrontMatter
{
    [YamlMember(Alias = "created")]
    public DateTime CreatedDate { get; set; }
}