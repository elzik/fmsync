using Elzik.FmSync.Domain;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;

namespace Elzik.FmSync.Infrastructure;

public class MarkdownFrontMatter : IMarkdownFrontMatter
{
    private readonly IValueDeserializer _deserializer;

    public MarkdownFrontMatter()
    {
        var deserializerBuilder = new DeserializerBuilder();
        deserializerBuilder.IgnoreUnmatchedProperties();
        _deserializer = deserializerBuilder.BuildValueDeserializer();
    }

    public DateTime? GetCreatedDateUtc(string markDownFilePath)
    {
        // Implementation taken from https://github.com/aaubry/YamlDotNet/issues/432#issuecomment-535249363

        var fileText = File.ReadAllText(markDownFilePath);

        if (!ContainsFrontMatter(fileText))
        {
            return null;
        }

        using var textReader = new StringReader(fileText);
        var parser = new Parser(textReader);
        
        parser.Consume<StreamStart>();
        parser.Consume<DocumentStart>();
        
        var createdDateFrontMatter = (CreatedDateFrontMatter?)_deserializer.DeserializeValue(
            parser, typeof(CreatedDateFrontMatter), new SerializerState(), _deserializer);

        return createdDateFrontMatter?.CreatedDate;
    }

    private static bool ContainsFrontMatter(string text)
    {
        using var textReader = new StringReader(text);
        
        var delimiterCount = 0;

        while (textReader.ReadLine() is { } line)
        {
            if (LineIsFrontMatterDelimiter(line))
            {
                delimiterCount++;

                if (BothDelimitersFound(delimiterCount))
                {
                    return true;
                }
            }

            if (LinePriorToFrontMatterIsNonDelimiter(line, delimiterCount))
            {
                return false;
            }
        }

        return false;
    }

    private static bool LineIsFrontMatterDelimiter(string fileLine)
    {
        return fileLine.TrimEnd().Equals("---", StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool BothDelimitersFound(int delimiterCount)
    {
        return delimiterCount >= 2;
    }

    private static bool LinePriorToFrontMatterIsNonDelimiter(string fileLine, int delimiterCount)
    {
        return delimiterCount < 1 && fileLine.Length > 0;
    }
}