﻿using System.Diagnostics;
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
            var markdownCreatedDate = GetFileCreatedDate(markDownFilePath);
            var createdDateFrontMatter = GetFrontMatterCreatedDate(createdDateYamlMarkdown, markDownFilePath);

            var comparisonResult = markdownCreatedDate.CompareTo(createdDateFrontMatter);

            if (comparisonResult == 0)
            {
                _logger.LogInformation("{FilePath} has a file created date ({FileCreatedDate}) the same as the created " +
                                       "date specified in its Front Matter.", markDownFilePath, markdownCreatedDate);
            }
            else
            {
                var relativeDescription = comparisonResult < 0 ? "earlier" : "later";
                _logger.LogInformation("{FilePath} has a file created date ({FileCreatedDate}) {RelativeDescription} " +
                                       "than the created date specified in its Front Matter ({FrontMatterCreatedDate})",
                    markDownFilePath, markdownCreatedDate, relativeDescription, createdDateFrontMatter);

                File.SetCreationTimeUtc(markDownFilePath, createdDateFrontMatter);
                editCount++;

                _logger.LogInformation("{FilePath} file created date updated to match that of its Front Matter.", markDownFilePath);
            }
        }

        var elapsedTime = Stopwatch.GetElapsedTime(startTime);
        _logger.LogInformation("Synchronised {EditedFileCount} files out of a total {TotalFileCount} in {TimeTaken}.", 
            editCount, markdownFiles.Count, elapsedTime);
    }

    private static DateTime GetFrontMatterCreatedDate(YamlMarkdown<CreatedDateFrontMatter> createdDateYamlMarkdown, string markDownFilePath)
    {
        var localCreatedDateFrontMatter = createdDateYamlMarkdown.Parse(markDownFilePath).CreatedDate;
        var createdDateFrontMatter =
            TimeZoneInfo.ConvertTimeToUtc(localCreatedDateFrontMatter, TimeZoneInfo.FindSystemTimeZoneById("GB"));

        return createdDateFrontMatter;
    }

    private static DateTime GetFileCreatedDate(string markDownFilePath)
    {
        var markdownFileInfo = new FileInfo(markDownFilePath);

        return markdownFileInfo.CreationTimeUtc;
    }
}