using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Thinktecture.IO;

namespace Elzik.FmSync;

public class FrontMatterFileSynchroniser : IFrontMatterFileSynchroniser
{
    private readonly ILogger<FrontMatterFileSynchroniser> _logger;
    private readonly IMarkdownFrontMatter _markdownFrontMatter;
    private readonly IFile _file;

    public FrontMatterFileSynchroniser(ILogger<FrontMatterFileSynchroniser> logger,
        IMarkdownFrontMatter markdownFrontMatter, IFile file)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _markdownFrontMatter = markdownFrontMatter ?? throw new ArgumentNullException(nameof(markdownFrontMatter));
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }

    public virtual SyncResult SyncCreationDate(string markDownFilePath)
    {
        var fileCreatedDateUpdated = false;
        var frontMatterCreatedDate = _markdownFrontMatter.GetCreatedDateUtc(markDownFilePath);

        if (frontMatterCreatedDate.HasValue)
        {
            var fileCreatedDate = _file.GetCreationTimeUtc(markDownFilePath);

            var comparisonResult = fileCreatedDate.CompareTo(frontMatterCreatedDate);

            if (comparisonResult == 0)
            {
                _logger.LogDebug("{FilePath} has a file created date ({FileCreatedDate}) the same as the created " +
                                       "date specified in its Front Matter.", markDownFilePath, fileCreatedDate);
            }
            else
            {
                var relativeDescription = comparisonResult < 0 ? "earlier" : "later";
                _logger.LogDebug("{FilePath} has a file created date ({FileCreatedDate}) {RelativeDescription} " +
                                       "than the created date specified in its Front Matter ({FrontMatterCreatedDate})",
                markDownFilePath, fileCreatedDate, relativeDescription, frontMatterCreatedDate);

                _file.SetCreationTimeUtc(markDownFilePath, frontMatterCreatedDate.Value);
                fileCreatedDateUpdated = true;

                _logger.LogInformation("{FilePath} file created date updated to match that of its Front Matter.",
                    markDownFilePath);
            }
        }
        else
        {
            _logger.LogDebug("{FilePath} has no Front Matter created date.", markDownFilePath);
        }

        return new SyncResult
        {
            FileCreatedDateUpdated = fileCreatedDateUpdated
        };
    }
}