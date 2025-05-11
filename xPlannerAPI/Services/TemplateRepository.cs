using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Models;
using xPlannerCommon.Models;
using System.Diagnostics;

namespace xPlannerAPI.Services
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public TemplateRepository()
        {
            _db = new audaxwareEntities();
        }

        public IEnumerable<get_templates_Result> GetAll(int domainId, bool showAudaxware, int? projectId = null, int? templateType = null)
        {
            if (projectId == 0)
                projectId = null;

            return _db.get_templates((short)domainId, showAudaxware, projectId, templateType).ToList();
        }

        public IEnumerable<LinkedRooms> GetLinkedRooms(int? id = null)
        {
            return _db.project_room.Include("project_department.project_phase.project").Where(tr => tr.linked_template == true && tr.applied_id_template == id)
                .Select(tr => new LinkedRooms
                {
                    project = tr.project_department.project_phase.project.project_description,
                    phase = tr.project_department.project_phase.description,
                    department = tr.project_department.description,
                    room_number = tr.drawing_room_number,
                    room_name = tr.drawing_room_name
                }).ToList();
        }

        public project_room CloneTemplate(project_room newTemplate, short newDomainId, string addedBy, short oldDomainId, int oldProjectId,
            int oldPhaseId, int oldDepartmentId, int oldRoomId, bool fromRoom = false, string comment = null)
        {

            short? projectDomainId = null;
            int? projectId = null;

            if (newTemplate.project_id > 0)
            {
                projectDomainId = newTemplate.domain_id;
                projectId = newTemplate.project_id;
            }

            if (newTemplate.project_id_template == 0 || newTemplate.project_id_template == null)
            {
                projectDomainId = null;
                projectId = null;
            }

            return _db.clone_template_ex(oldDomainId, oldProjectId, oldPhaseId, oldDepartmentId, oldRoomId, newDomainId,
                 newTemplate.drawing_room_name, addedBy, projectDomainId, projectId, fromRoom, comment);

        }

        public bool ApplyTemplate(ApplyTemplateData template, short domainId, int projectId, int phaseId, int departmentId, int roomId, string addedBy)
        {
            try
            {
                _db.apply_template(domainId, projectId, phaseId, departmentId, roomId, template.template_id, template.cost_field ?? "source", addedBy, template.delete_assets, template.link_template);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error to apply template. Exception {0}", ex);
                return false;
            }

        }

        public bool UnlinkTemplate(ApplyTemplateData template, short domainId, int projectId, int phaseId, int departmentId, int roomId)
        {
            try
            {
                var room = _db.project_room.FirstOrDefault(pr => pr.applied_id_template == template.template_id && pr.domain_id == domainId && pr.linked_template == true && pr.project_id == projectId && pr.phase_id == phaseId && pr.department_id == departmentId && pr.room_id == roomId);
                if (room != null)
                {
                    room.linked_template = false;
                    _db.Entry(room).State = EntityState.Modified;
                    _db.SaveChanges();
                }

                var inventory = _db.project_room_inventory.Where(pr => pr.linked_id_template == template.template_id && pr.domain_id == domainId && pr.project_id == projectId && pr.phase_id == phaseId && pr.department_id == departmentId && pr.room_id == roomId).ToList();
                //inventory.Select(i => { i.linked_id_template = null; return i; }).ToList();
                inventory.ForEach(i => i.linked_id_template = null);
                inventory.ForEach(p => _db.Entry(p).State = EntityState.Modified);
                //this.db.Entry(inventory).State = EntityState.Modified;

                _db.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public List<string> GetUsedNames(int domainId, int? roomId = null, int? departmentTypeId = null)
        {
                return _db.project_room.Where(tr => tr.domain_id == domainId && tr.is_template == true
                    && (roomId == null || tr.room_id != roomId)
                    && (departmentTypeId == null || tr.department_type_id_template == departmentTypeId ))
                    .Select(tr => tr.drawing_room_name.ToLower()).ToList();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}