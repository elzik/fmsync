using Elzik.FmSync.Application;
using Elzik.FmSync.Infrastructure;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;

namespace Elzik.FmSync.Worker
{
    public class FmSyncWorker : BackgroundService
    {
        private readonly ILogger<FmSyncWorker> _logger;
        private readonly WatcherOptions _watcherOptions;
        private readonly FileSystemOptions _fileSystemOptions;
        private readonly IFrontMatterFileSynchroniser _fileSynchroniser;
        private readonly List<FileSystemWatcher> _folderWatchers;

        public FmSyncWorker(ILogger<FmSyncWorker> logger, IOptions<WatcherOptions> watcherOptions, 
            IResilientFrontMatterFileSynchroniser fileSynchroniser, IOptions<FileSystemOptions> fileSystemOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ArgumentNullException.ThrowIfNull(watcherOptions);
            ArgumentNullException.ThrowIfNull(fileSystemOptions);
            _fileSystemOptions = fileSystemOptions.Value;
            _watcherOptions = watcherOptions.Value;
            _fileSynchroniser = fileSynchroniser ?? throw new ArgumentNullException(nameof(fileSynchroniser));
            _folderWatchers = [];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("fmsync {Version} has started.", GetProductVersion());
            _logger.LogDebug("File synchroniation is implemented by {SyncName}", _fileSynchroniser.GetType().Name);

            foreach (var directoryPaths in _watcherOptions.WatchedDirectoryPaths)
            {
                _logger.LogInformation("Configuring watcher on {DirectoryPath} for new and changed " +
                                       "{FilenamePattern} files.", directoryPaths, _fileSystemOptions.FilenamePattern);

                var folderWatcher = new FileSystemWatcher(directoryPaths,
                    _fileSystemOptions.FilenamePattern ?? string.Empty)
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

            _logger.LogInformation("A total of {WatcherCount} directory watchers are running.", _folderWatchers.Count);
            if (_folderWatchers.Count < 1)
            {
                _logger.LogWarning("No directories are being watched. Add at least one directory to watch to " +
                    "the {ConfigSection}:{ConfigItem} configuration.",
                    nameof(WatcherOptions), nameof(WatcherOptions.WatchedDirectoryPaths));
            }

            await Task.Yield();
        }

        private static string GetProductVersion()
        {
            var assemblyAttributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);

            if(assemblyAttributes.Length == 0)
            {
                throw new InvalidOperationException("No custom assembly attributes found; " +
                    "unable to get informational version.");
            }

            return ((AssemblyInformationalVersionAttribute)assemblyAttributes[0]).InformationalVersion;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            ProcessFile(e.FullPath);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            ProcessFile(e.FullPath);
        }

        private void ProcessFile(string markDownFilePath)
        {
            try
            {
                _fileSynchroniser.SyncCreationDate(markDownFilePath);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred whilst processing {FilePath}.", 
                    markDownFilePath);
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