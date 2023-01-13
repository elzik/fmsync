using System.Diagnostics;
using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using Thinktecture.IO;

namespace Elzik.FmSync;

public class FrontMatterFileSynchroniser : IFrontMatterFileSynchroniser
{
    private readonly ILogger<FrontMatterFileSynchroniser> _logger;
    private readonly IMarkdownFrontMatter _markdownFrontMatter;
    private readonly IFile _file;

    public FrontMatterFileSynchroniser(ILogger<FrontMatterFileSynchroniser> logger, IMarkdownFrontMatter markdownFrontMatter, IFile file)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _markdownFrontMatter = markdownFrontMatter ?? throw new ArgumentNullException(nameof(markdownFrontMatter));
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }

    public void SyncCreationDates(string directoryPath)
    {
        var startTime = Stopwatch.GetTimestamp();
        var editCount = 0;

        _logger.LogInformation("Synchronising files in {directoryPath}", directoryPath);

        var markdownFiles = Directory.EnumerateFiles(directoryPath, "*.md", new EnumerationOptions()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = true
        }).ToList();

        foreach (var markDownFilePath in markdownFiles)
        {
            var frontMatterCreatedDate = _markdownFrontMatter.GetCreatedDateUtc(markDownFilePath);

            if (frontMatterCreatedDate.HasValue)
            {
                var fileCreatedDate = _file.GetCreationTimeUtc(markDownFilePath);

                var comparisonResult = fileCreatedDate.CompareTo(frontMatterCreatedDate);

                if (comparisonResult == 0)
                {
                    _logger.LogInformation("{FilePath} has a file created date ({FileCreatedDate}) the same as the created " +
                                           "date specified in its Front Matter.", markDownFilePath, fileCreatedDate);
                }
                else
                {
                    var relativeDescription = comparisonResult < 0 ? "earlier" : "later";
                    _logger.LogInformation("{FilePath} has a file created date ({FileCreatedDate}) {RelativeDescription} " +
                                           "than the created date specified in its Front Matter ({FrontMatterCreatedDate})",
                        markDownFilePath, fileCreatedDate, relativeDescription, frontMatterCreatedDate);

                    _file.SetCreationTimeUtc(markDownFilePath, frontMatterCreatedDate.Value);
                    editCount++;

                    _logger.LogInformation("{FilePath} file created date updated to match that of its Front Matter.", markDownFilePath);
                }
            }
            else
            {
                _logger.LogInformation("{FilePath} has no Front Matter created date.", markDownFilePath);
            }
        }

        var elapsedTime = Stopwatch.GetElapsedTime(startTime);
        _logger.LogInformation("Synchronised {EditedFileCount} files out of a total {TotalFileCount} in {TimeTaken}.", 
            editCount, markdownFiles.Count, elapsedTime);
    }
}