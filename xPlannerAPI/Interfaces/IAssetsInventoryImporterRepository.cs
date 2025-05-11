using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using xPlannerAPI.Models;
using xPlannerCommon.Models;
using xPlannerCommon.Services;

namespace xPlannerAPI.Interfaces
{
    public enum ImportColumnsFormat { AudaxWare, MillCreek }
    interface IAssetsInventoryImporterRepository : IDisposable
    {
        List<ImportAnalysisResult> Analyze(int domainId, int projectId, string xlsFilePath, ImportColumnsFormat columnsFormat);

        ImportAnalysisResult Import(ImportItem[] items, int domainId, int projectId, string email, NotificationRepository notificationRepo);
    }
}
