using Elzik.FmSync.Worker;
using Microsoft.Extensions.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        const string optionsKey = "FmSyncOptions";
        var options = configuration.GetSection(optionsKey).Get<FmSyncOptions>();
        if (options == null)
        {
            throw new InvalidOperationException(
                $"{optionsKey} configuration could not be found.");
        }

        services.AddSingleton(options);
        services.AddHostedService<FmSyncWorker>();
    })
    .Build();

host.Run();
