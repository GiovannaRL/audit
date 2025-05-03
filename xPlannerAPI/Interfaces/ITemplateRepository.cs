using System;
using System.Collections.Generic;
using xPlannerAPI.Models;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface ITemplateRepository : IDisposable
    {
        IEnumerable<get_templates_Result> GetAll(int domainId, bool showAudaxware, int? projectId = null, int? templateType = null);
        IEnumerable<LinkedRooms> GetLinkedRooms(int? id = null);
        project_room CloneTemplate(project_room newTemplate, short newDomainId, string addedBy, short oldDomainId, int oldProjectId,
            int oldPhaseId, int oldDepartmentId, int oldRoomId, bool fromRoom = false, string comment = null);
        List<string> GetUsedNames(int domainId, int? roomId = null, int? departmentTypeId = null);
        bool ApplyTemplate(ApplyTemplateData template, short domainId, int projectId, int phaseId, int departmentId, int roomId, string addedBy);
        bool UnlinkTemplate(ApplyTemplateData template, short domainId, int projectId, int phaseId, int departmentId, int roomId);
    }
}
