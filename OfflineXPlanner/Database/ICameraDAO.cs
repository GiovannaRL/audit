using System;
namespace OfflineXPlanner.Database
{
    public interface ICameraDAO
    {
        string GetLastSelectedCamera();
        bool UpdateLastSelectedCamera(string camera);
    }
}
