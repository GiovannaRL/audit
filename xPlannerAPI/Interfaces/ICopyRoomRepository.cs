using System;
using System.Net.Http;
using xPlannerAPI.Models;

namespace xPlannerAPI.Interfaces
{
    interface ICopyRoomRepository : IDisposable
    {
        HttpResponseMessage Add(int fromDomainId, int fromProjectId, CopyRoom data);
    }
}