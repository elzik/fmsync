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
    private readonly IDirectory _directory;

    public FrontMatterFileSynchroniser(ILogger<FrontMatterFileSynchroniser> logger, IMarkdownFrontMatter markdownFrontMatter, IFile file, IDirectory directory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _markdownFrontMatter = markdownFrontMatter ?? throw new ArgumentNullException(nameof(markdownFrontMatter));
        _file = file ?? throw new ArgumentNullException(nameof(file));
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    public void SyncCreationDates(string directoryPath)
    {
        var (startTime, editedCount, totalCount) = (Stopwatch.GetTimestamp(), 0, 0);

        _logger.LogInformation("Synchronising files in {DirectoryPath}", directoryPath);

        var markdownFiles = _directory.EnumerateFiles(directoryPath, "*.md", new EnumerationOptions()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = true
        });

        foreach (var markDownFilePath in markdownFiles)
        {
            totalCount++;

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

                    _logger.LogInformation("{FilePath} file created date updated to match that of its Front Matter.", markDownFilePath);
                }
            }
            else
            {
                _logger.LogInformation("{FilePath} has no Front Matter created date.", markDownFilePath);
            }
        }

        _logger.LogInformation("Synchronised {EditedFileCount} files out of a total {TotalFileCount} in {TimeTaken}.", 
            editedCount, totalCount, Stopwatch.GetElapsedTime(startTime));
    }
}