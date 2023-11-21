using Elzik.FmSync;
using Elzik.FmSync.Domain;
using Elzik.FmSync.Infrastructure;
using Elzik.FmSync.Worker;
using Serilog;
using Thinktecture.IO.Adapters;
using Thinktecture.IO;
using Polly;
using Polly.Retry;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, config) => config
        .ReadFrom.Configuration(context.Configuration))
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IMarkdownFrontMatter, MarkdownFrontMatter>();
        services.AddSingleton<IFile, FileAdapter>();
        services.AddSingleton<IFrontMatterFileSynchroniser, FrontMatterFileSynchroniser>();
        services.AddSingleton<IResiliantFrontMatterFileSynchroniser, ResiliantFrontMatterFileSynchroniser>();
        services.Configure<WatcherOptions>(hostContext.Configuration.GetSection("WatcherOptions"));
        services.Configure<FileSystemOptions>(hostContext.Configuration.GetSection("FileSystemOptions"));
        services.Configure<FrontMatterOptions>(hostContext.Configuration.GetSection("FrontMatterOptions"));
        services.AddResiliencePipeline("retry-5-times", builder =>
            {
                builder.AddRetry(new RetryStrategyOptions()
                {
                    MaxRetryAttempts = 5,
                    BackoffType = DelayBackoffType.Exponential,
                });
            });
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
