using Elzik.FmSync;
using Elzik.FmSync.Domain;
using Elzik.FmSync.Infrastructure;
using Elzik.FmSync.Worker;
using Serilog;
using Thinktecture.IO.Adapters;
using Thinktecture.IO;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, config) => config
        .ReadFrom.Configuration(context.Configuration))
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IMarkdownFrontMatter, MarkdownFrontMatter>();
        services.AddSingleton<IFile, FileAdapter>();
        services.AddSingleton<IFrontMatterFileSynchroniser, ResiliantFrontMatterFileSynchroniser>();
        services.Configure<WatcherOptions>(hostContext.Configuration.GetSection("WatcherOptions"));
        services.Configure<FileSystemOptions>(hostContext.Configuration.GetSection("FileSystemOptions"));
        services.Configure<FrontMatterOptions>(hostContext.Configuration.GetSection("FrontMatterOptions"));
#if IS_WINDOWS_OS
        services.AddWindowsService(options =>
        {
            options.ServiceName = "fmsync";
        });
#endif
        services.AddHostedService<FmSyncWorker>();
    })
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile("appSettings.json", false);
    })
    .Build();

host.Run();
