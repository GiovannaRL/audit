namespace OfflineXPlanner.Database
{
    public interface IVersionDAO
    {
        int GetCurrentInstalledVersion();
        int GetNewVersion();
        bool UpdateVersion(int newVersion);
    }
}
