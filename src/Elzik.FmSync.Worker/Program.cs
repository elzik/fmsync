using Elzik.FmSync.Domain;
using Elzik.FmSync.Infrastructure;
using Elzik.FmSync.Worker;
using Serilog;
using Thinktecture.IO.Adapters;
using Thinktecture.IO;
using Polly;
using Elzik.FmSync.Application;

var appSettingsPath = Path.Join(AppContext.BaseDirectory, "appSettings.json");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile(appSettingsPath, false);
    })
    .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IMarkdownFrontMatter, MarkdownFrontMatter>();
        services.AddSingleton<IFile, FileAdapter>();
        services.AddSingleton<IFrontMatterFileSynchroniser, FrontMatterFileSynchroniser>();
        services.AddSingleton<IResilientFrontMatterFileSynchroniser, ResilientFrontMatterFileSynchroniser>();
        services.Configure<WatcherOptions>(hostContext.Configuration.GetSection("WatcherOptions"));
        services.Configure<FileSystemOptions>(hostContext.Configuration.GetSection("FileSystemOptions"));
        services.Configure<FrontMatterOptions>(hostContext.Configuration.GetSection("FrontMatterOptions"));
        services.AddResiliencePipeline(Retry5TimesPipelineBuilder.StrategyName, builder => builder.AddRetry5Times());
#if IS_WINDOWS_OS
        services.AddWindowsService(options =>
        {
            options.ServiceName = "fmsync";
        });
#endif
        services.AddHostedService<FmSyncWorker>();
    })
    .Build();

await host.RunAsync();