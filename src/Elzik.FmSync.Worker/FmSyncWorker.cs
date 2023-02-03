using System.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Elzik.FmSync.Worker
{
    public class FmSyncWorker : BackgroundService
    {
        private readonly ILogger<FmSyncWorker> _logger;
        private readonly FmSyncOptions _fmSyncOptions;
        private readonly IFrontMatterFileSynchroniser _fileSynchroniser;
        private readonly List<PhysicalFileProvider> _folderWatchers;

        public FmSyncWorker(ILogger<FmSyncWorker> logger, 
            FmSyncOptions fmSyncOptions, IFrontMatterFileSynchroniser fileSynchroniser)
        {
            _logger = logger;
            _fmSyncOptions = fmSyncOptions;
            _fileSynchroniser = fileSynchroniser;
            _folderWatchers = new List<PhysicalFileProvider>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FmSyncWorker running at: {Time}", DateTimeOffset.Now);

            foreach (var directoryPaths in _fmSyncOptions.WatchedDirectoryPaths)
            {
                _logger.LogInformation("Configuring watcher on {DirectoryPath} for new and changed " +
                                       "MarkDown files.", directoryPaths);
                
                var folderWatcher = new PhysicalFileProvider(directoryPaths)
                {
                    UsePollingFileWatcher = true
                };

                _folderWatchers.Add(folderWatcher);

                Watch(folderWatcher, directoryPaths);

                _logger.LogInformation("Watcher on {DirectoryPath} has started.", directoryPaths);
            }

            _logger.LogInformation("A total of {WatcherCount} folder watchers are running.", _folderWatchers.Count);
        }

        private void Watch(PhysicalFileProvider folderWatcher, string directoryPaths)
        {
            var changeToken = folderWatcher.Watch("*.md");

            changeToken.RegisterChangeCallback(Callback, new LastChange()
            {
                Watcher = folderWatcher,
                Path = directoryPaths,
                Filter = "*.md",
                LastChangeTime = DateTime.UtcNow
            });
        }

        private class LastChange
        {
            public PhysicalFileProvider Watcher { get; set; }
            public DateTime LastChangeTime { get; set; }
            public string Path { get; set; }
            public string Filter { get; set; }
        }

        private void Callback(object? obj)
        {
            if (obj == null)
            {
                throw new InvalidOperationException($"Can't watch for files any longer for some unknown path. " +
                                                    "This has got to be fatal!");
            }
            var lastChange = (LastChange)obj;

            Watch(lastChange.Watcher, lastChange.Path);

            var directoryInfo = new DirectoryInfo(lastChange.Path);
            var files = directoryInfo.EnumerateFiles(lastChange.Filter)
                .Where(info => info.LastWriteTime > lastChange.LastChangeTime).ToList();

            foreach (var file in files)
            {
                try
                {
                    _fileSynchroniser.SyncCreationDate(file.FullName);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "An error occurred whilst processing {FilePath}.",
                        file.FullName);
                }
            }
        }
    }
}