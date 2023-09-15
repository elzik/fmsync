using Elzik.FmSync;
using Elzik.FmSync.Domain;
using Elzik.FmSync.Infrastructure;
using Elzik.FmSync.Worker;
using Thinktecture.IO.Adapters;
using Thinktecture.IO;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        const string optionsKey = "FmSyncOptions";
        var options = configuration.GetSection(optionsKey).Get<FmSyncOptions>() ?? throw new InvalidOperationException(
                $"{optionsKey} configuration could not be found.");
        services.AddSingleton(options);
        services.AddSingleton<IMarkdownFrontMatter, MarkdownFrontMatter>();
        services.AddSingleton<IFile, FileAdapter>();
        services.AddSingleton<IFrontMatterFileSynchroniser, FrontMatterFileSynchroniser>();
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
