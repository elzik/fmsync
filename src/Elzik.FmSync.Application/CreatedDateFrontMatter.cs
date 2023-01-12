using YamlDotNet.Serialization;

namespace Elzik.FmSync;

public class CreatedDateFrontMatter
{
    [YamlMember(Alias = "created")]
    public DateTime CreatedDate { get; set; }
}