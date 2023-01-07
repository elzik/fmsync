using Elzik.FmSync.Console;
using EPS.Extensions.YamlMarkdown;

var searchPath = args.Length == 0 
    ? Directory.GetCurrentDirectory() 
    : args[0];

Console.WriteLine($"Syncing Markdown files in {searchPath}");

var files = Directory.EnumerateFiles(searchPath, "*.md", new EnumerationOptions()
{
    MatchCasing = MatchCasing.CaseInsensitive,
    RecurseSubdirectories = true
});

var createdDateYamlMarkdown = new YamlMarkdown<CreatedDateFrontMatter>();

foreach (var file in files)
{
    var markdownFileInfo = new FileInfo(file);

    var fileCreatedDate = markdownFileInfo.CreationTimeUtc;

    Console.WriteLine(file);
    Console.WriteLine($"\tFile created date: {fileCreatedDate}");

    var createdDateFrontMatter = createdDateYamlMarkdown.Parse(file);

    Console.WriteLine($"\tFront Matter created date: {createdDateFrontMatter.CreatedDate}");
}