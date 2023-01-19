using System.Diagnostics;
using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using Thinktecture.IO;

namespace Elzik.FmSync;

public class FrontMatterFolderSynchroniser : IFrontMatterFolderSynchroniser
{
    private readonly ILogger<FrontMatterFolderSynchroniser> _logger;
    private readonly IDirectory _directory;
    private readonly IFrontMatterFileSynchroniser _frontMatterFileSynchroniser;

    public FrontMatterFolderSynchroniser(ILogger<FrontMatterFolderSynchroniser> logger, 
        IDirectory directory, IFrontMatterFileSynchroniser frontMatterFileSynchroniser)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        _frontMatterFileSynchroniser = frontMatterFileSynchroniser ?? throw new ArgumentNullException(nameof(frontMatterFileSynchroniser));
    }

    public void SyncCreationDates(string directoryPath)
    {
        var loggingInfo = (StartTime: Stopwatch.GetTimestamp(), EditedCount: 0, TotalCount: 0);

        _logger.LogInformation("Synchronising files in {DirectoryPath}", directoryPath);

        var markdownFiles = _directory.EnumerateFiles(directoryPath, "*.md", new EnumerationOptions()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = true
        });

        foreach (var markDownFilePath in markdownFiles)
        {
            loggingInfo.TotalCount++;

            if (_frontMatterFileSynchroniser.SyncCreationDate(markDownFilePath).FileCreatedDateUpdated)
            {
                loggingInfo.EditedCount++;
            }
        }

        _logger.LogInformation("Synchronised {EditedFileCount} files out of a total {TotalFileCount} in {TimeTaken}.",
            loggingInfo.EditedCount, loggingInfo.TotalCount, Stopwatch.GetElapsedTime(loggingInfo.StartTime));
    }
}