namespace Elzik.FmSync.Worker
{
    public class FmSyncOptions
    {
        public IEnumerable<string> WatchedDirectoryPaths { get; set; } = new List<string>();
    }
}
