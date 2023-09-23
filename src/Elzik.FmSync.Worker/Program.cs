using Elzik.FmSync;
using Elzik.FmSync.Domain;
using Elzik.FmSync.Infrastructure;
using Elzik.FmSync.Worker;
using Serilog;
using Thinktecture.IO.Adapters;
using Thinktecture.IO;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration))
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IMarkdownFrontMatter, MarkdownFrontMatter>();
        services.AddSingleton<IFile, FileAdapter>();
        services.AddSingleton<IFrontMatterFileSynchroniser, FrontMatterFileSynchroniser>();
        services.Configure<FmSyncOptions>(hostContext.Configuration.GetSection("FmSyncOptions"));
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
        config.AddJsonFile("appSettings.json", true, true);
    })
    .Build();

host.Run();
