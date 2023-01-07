namespace Elzik.FmSync.Console;

using YamlDotNet.Serialization;

internal class CreatedDateFrontMatter
{
    [YamlMember(Alias = "created")]
    public DateTime CreatedDate { get; set; }
}