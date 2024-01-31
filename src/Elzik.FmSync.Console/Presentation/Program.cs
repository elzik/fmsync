using Elzik.FmSync.Application;
using Elzik.FmSync.Domain;
using Elzik.FmSync.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Thinktecture.IO;
using Thinktecture.IO.Adapters;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, config) => config
        .ReadFrom.Configuration(context.Configuration))
    .ConfigureServices((context,services) =>
    {
        services.AddTransient<IMarkdownFrontMatter, MarkdownFrontMatter>();
        services.AddTransient<IFile, FileAdapter>();
        services.AddTransient<IDirectory, DirectoryAdapter>();
        services.AddTransient<IFrontMatterFileSynchroniser, FrontMatterFileSynchroniser>();
        services.AddTransient<IFrontMatterFolderSynchroniser, FrontMatterFolderSynchroniser>();
        services.Configure<FrontMatterOptions>(context.Configuration.GetSection("FrontMatterOptions"));
        services.Configure<FileSystemOptions>(context.Configuration.GetSection("FileSystemOptions"));
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));
            loggingBuilder.AddConsole();
        });
    })
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile(Path.Join(AppContext.BaseDirectory, "appSettings.json"), false);
    })
    .Build();

var searchPath = args.Length == 0
    ? Directory.GetCurrentDirectory()
    : args[0];

var frontMatterFolderSynchroniser = host.Services.GetRequiredService<IFrontMatterFolderSynchroniser>();
frontMatterFolderSynchroniser.SyncCreationDates(searchPath);