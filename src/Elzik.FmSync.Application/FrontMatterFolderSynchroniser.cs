using System.Diagnostics;
using Elzik.FmSync.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.IO;

namespace Elzik.FmSync.Application;

public class FrontMatterFolderSynchroniser(
    ILogger<FrontMatterFolderSynchroniser> logger, 
    IDirectory directory, 
    IFrontMatterFileSynchroniser frontMatterFileSynchroniser, 
    IOptions<FileSystemOptions> options) : IFrontMatterFolderSynchroniser
{
    private readonly ILogger<FrontMatterFolderSynchroniser> _logger = logger 
        ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDirectory _directory = directory 
        ?? throw new ArgumentNullException(nameof(directory));
    private readonly IFrontMatterFileSynchroniser _frontMatterFileSynchroniser = frontMatterFileSynchroniser
        ?? throw new ArgumentNullException(nameof(frontMatterFileSynchroniser));
    private readonly FileSystemOptions _options = options.Value;

    public void SyncCreationDates(string directoryPath)
    {
        var loggingInfo = (StartTime: Stopwatch.GetTimestamp(), EditedCount: 0, ErrorCount: 0, TotalCount: 0);

        _logger.LogDebug("Synchronising {FilenamePattern} files in {DirectoryPath}", 
            _options.FilenamePattern, directoryPath);

        var markdownFiles = _directory.EnumerateFiles(directoryPath, _options.FilenamePattern ?? string.Empty,
            new EnumerationOptions
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                RecurseSubdirectories = true
            });

        foreach (var markDownFilePath in markdownFiles)
        {
            loggingInfo.TotalCount++;

            try
            {
                if (_frontMatterFileSynchroniser.SyncCreationDate(markDownFilePath).FileCreatedDateUpdated)
                {
                    loggingInfo.EditedCount++;
                }
            }
            catch (Exception e)
            {
                loggingInfo.ErrorCount++;
                var additionalMessage = string.Empty;
                if (e.InnerException != null)
                {
                    additionalMessage = $" {e.InnerException.Message}";
                }
                _logger.LogError("{MarkdownFilePath} - {ExceptionMessage}{AdditionalMessage}",
                    markDownFilePath, e.Message, additionalMessage);
            }
        }

        var errorsMessage = string.Empty;
        if (loggingInfo.ErrorCount > 0)
        {
            errorsMessage = $" and failed {loggingInfo.ErrorCount}";
        }

        _logger.LogInformation("Synchronised {EditedFileCount}{ErrorsMessage} files out " +
            "of a total {TotalFileCount} in {TimeTaken}.", 
            loggingInfo.EditedCount, 
            errorsMessage, 
            loggingInfo.TotalCount, 
            Stopwatch.GetElapsedTime(loggingInfo.StartTime));
    }
}