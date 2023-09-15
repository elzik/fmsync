using Elzik.FmSync;
using Elzik.FmSync.Domain;
using Elzik.FmSync.Infrastructure;
using Elzik.FmSync.Worker;
using Thinktecture.IO.Adapters;
using Thinktecture.IO;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IMarkdownFrontMatter, MarkdownFrontMatter>();
        services.AddSingleton<IFile, FileAdapter>();
        services.AddSingleton<IFrontMatterFileSynchroniser, FrontMatterFileSynchroniser>();
        services.Configure<FmSyncOptions>(hostContext.Configuration.GetSection("FmSyncOptions"));
        services.Configure<FileSystemOptions>(hostContext.Configuration.GetSection("FileSystemOptions"));
        services.Configure<FrontMatterOptions>(hostContext.Configuration.GetSection("FrontMatterOptions"));
        services.AddHostedService<FmSyncWorker>();
    })
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile("appSettings.json", true, true);
    })
    .ConfigureLogging((context, config) => 
        config.AddConfiguration(context.Configuration.GetSection("Logging")))
    .Build();

host.Run();
