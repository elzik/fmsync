
string searchPath;
if (args.Length == 0)
{
    searchPath = Directory.GetCurrentDirectory();
}
else
{
    searchPath = args[0];
}

Console.WriteLine($"Syncing Markdown files in {searchPath}");

var files = Directory.EnumerateFiles(searchPath, "*.md", new EnumerationOptions()
{
    MatchCasing = MatchCasing.CaseInsensitive,
    RecurseSubdirectories = true
});

foreach (var file in files)
{
    var markdownFileInfo = new FileInfo(file);

    var fileCreatedDate = markdownFileInfo.CreationTimeUtc;

    Console.WriteLine(file);
    Console.WriteLine($"\tFile created date: {fileCreatedDate}");
}