using YamlDotNet.Serialization;

namespace Elzik.FmSync;

internal class CreatedDateFrontMatter
{
    [YamlMember(Alias = "created")]
    public DateTime CreatedDate { get; set; }
}