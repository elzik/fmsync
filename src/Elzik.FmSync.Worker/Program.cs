using Elzik.FmSync.Worker;
using Microsoft.Extensions.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        var options = configuration.GetSection("FmSyncOptions").Get<FmSyncOptions>();

        services.AddSingleton(options);
        services.AddHostedService<FmSyncWorker>();
    })
    .Build();

host.Run();
