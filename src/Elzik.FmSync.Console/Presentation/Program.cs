using Elzik.FmSync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<IFrontMatterFileSynchroniser, FrontMatterFileSynchroniser>();
    })
    .Build();

var searchPath = args.Length == 0
    ? Directory.GetCurrentDirectory()
    : args[0];

var frontMatterFileSynchroniser = host.Services.GetRequiredService<IFrontMatterFileSynchroniser>();
frontMatterFileSynchroniser.SyncCreationDates(searchPath);