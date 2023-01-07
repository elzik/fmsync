namespace Elzik.FmSync.Worker
{
    public class FmSyncWorker : BackgroundService
    {
        private readonly ILogger<FmSyncWorker> _logger;
        private readonly IEnumerable<FileSystemWatcher> _folderWatchers;
        private readonly FmSyncOptions _fmSyncOptions;

        public FmSyncWorker(ILogger<FmSyncWorker> logger, FmSyncOptions fmSyncOptions)
        {
            _logger = logger;
            _fmSyncOptions = fmSyncOptions;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FmSyncWorker running at: {time}", DateTimeOffset.Now);
        }
    }
}