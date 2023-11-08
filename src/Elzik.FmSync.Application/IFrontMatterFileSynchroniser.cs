namespace Elzik.FmSync;

public interface IFrontMatterFileSynchroniser
{
    SyncResult SyncCreationDate(string markDownFilePath);
    
    SyncResult SyncCreationDateWithResilience(string markDownFilePath);
}