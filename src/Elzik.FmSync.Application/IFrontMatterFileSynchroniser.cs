namespace Elzik.FmSync.Application;

public interface IFrontMatterFileSynchroniser
{
    SyncResult SyncCreationDate(string markDownFilePath);
}