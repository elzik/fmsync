using System.Diagnostics;
using EPS.Extensions.YamlMarkdown;
using Microsoft.Extensions.Logging;

namespace Elzik.FmSync;

public class FrontMatterFileSynchroniser : IFrontMatterFileSynchroniser
{
    private readonly ILogger<FrontMatterFileSynchroniser> _logger;

    public FrontMatterFileSynchroniser(ILogger<FrontMatterFileSynchroniser> logger)
    {
        _logger = logger;
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

        var createdDateYamlMarkdown = new YamlMarkdown<CreatedDateFrontMatter>();

        foreach (var markDownFilePath in markdownFiles)
        {
            var markdownFileInfo = new FileInfo(markDownFilePath);

            var fileCreatedDate = markdownFileInfo.CreationTimeUtc;

            _logger.LogInformation("{FileName} file created date: {FileCreatedDate}.",
                markdownFileInfo.Name, fileCreatedDate);

            var localCreatedDateFrontMatter = createdDateYamlMarkdown.Parse(markDownFilePath).CreatedDate;
            var createdDateFrontMatter =
                TimeZoneInfo.ConvertTimeToUtc(localCreatedDateFrontMatter, TimeZoneInfo.FindSystemTimeZoneById("GB"));

            _logger.LogInformation("{FileName} Front Matter created date: {FrontMatterCreatedDate}.",
                markdownFileInfo.Name, createdDateFrontMatter);

            if (fileCreatedDate.Equals(createdDateFrontMatter))
            {
                _logger.LogInformation("Dates equal.");
            }
            else
            {
                _logger.LogInformation("Dates not equal.");
                File.SetCreationTimeUtc(markDownFilePath, createdDateFrontMatter);
                editCount++;
            }
        }

        var elapsedTime = Stopwatch.GetElapsedTime(startTime);
        _logger.LogInformation("Synchronised {EditedFileCount} files out of a total {TotalFileCount} in {TimeTaken}.", 
            editCount, markdownFiles.Count, elapsedTime);
    }
}