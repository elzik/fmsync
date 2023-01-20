using Elzik.FmSync;
using Elzik.FmSync.Domain;
using Elzik.FmSync.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thinktecture.IO;
using Thinktecture.IO.Adapters;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<IMarkdownFrontMatter, MarkdownFrontMatter>();
        services.AddTransient<IFile, FileAdapter>();
        services.AddTransient<IDirectory, DirectoryAdapter>();
        services.AddTransient<IFrontMatterFileSynchroniser, FrontMatterFileSynchroniser>();
        services.AddTransient<IFrontMatterFolderSynchroniser, FrontMatterFolderSynchroniser>();
    })
    .Build();

var searchPath = args.Length == 0
    ? Directory.GetCurrentDirectory()
    : args[0];

var frontMatterFileSynchroniser = host.Services.GetRequiredService<IFrontMatterFolderSynchroniser>();
frontMatterFileSynchroniser.SyncCreationDates(searchPath);