namespace Elzik.FmSync.Worker
{
    public class FmSyncWorker : BackgroundService
    {
        private readonly ILogger<FmSyncWorker> _logger;
        private readonly FmSyncOptions _fmSyncOptions;
        private readonly IFrontMatterFileSynchroniser _fileSynchroniser;
        private readonly List<FileSystemWatcher> _folderWatchers;

        public FmSyncWorker(ILogger<FmSyncWorker> logger, 
            FmSyncOptions fmSyncOptions, IFrontMatterFileSynchroniser fileSynchroniser)
        {
            _logger = logger;
            _fmSyncOptions = fmSyncOptions;
            _fileSynchroniser = fileSynchroniser;
            _folderWatchers = new List<FileSystemWatcher>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FmSyncWorker running at: {Time}", DateTimeOffset.Now);

            foreach (var directoryPaths in _fmSyncOptions.WatchedDirectoryPaths)
            {
                _logger.LogInformation("Configuring watcher on {DirectoryPath} for new and changed " +
                                       "MarkDown files.", directoryPaths);
                
                var folderWatcher = new FileSystemWatcher(directoryPaths, "*.md")
                {
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime
                };

                _folderWatchers.Add(folderWatcher);

                folderWatcher.Changed += OnChanged;
                folderWatcher.Created += OnCreated;
                folderWatcher.Error += OnError;

                folderWatcher.EnableRaisingEvents = true;

                _logger.LogInformation("Watcher on {DirectoryPath} has started.", directoryPaths);
            }

            _logger.LogInformation("A total of {WatcherCount} folder watchers are running.", _folderWatchers.Count);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            ProcessFile(sender, e);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            ProcessFile(sender, e);
        }

        private void ProcessFile(object sender, FileSystemEventArgs e)
        {
            try
            {
                _fileSynchroniser.SyncCreationDate(e.FullPath);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred whilst processing {FilePath}.", 
                    e.FullPath);
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            var sourceDirectoryPath = ((FileSystemWatcher)sender).Path;

            _logger.LogError(e.GetException(), 
                "FmSync is unable to continue monitoring changes in {FolderPath}.", 
                sourceDirectoryPath);
        }
    }
}