using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class ProjectFilesRepository : IDisposable
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public ProjectFilesRepository()
        {
            this._db = new audaxwareEntities();
        }

        public static List<string> GetReportsName(int domainId, int projectId, int? phaseId = null, int? departmentId = null, int? roomId = null)
        {
            using (var repository = new TableRepository<project_report>())
            {
                var keys = new List<string>(new [] { "project_domain_id", "project_id" });
                if (phaseId != null)
                    keys.Add("phase_id");
                if (departmentId != null)
                    keys.Add("department_id");
                if (roomId != null)
                    keys.Add("room_id");

                return repository.GetAll(keys.ToArray(),
                    new [] { domainId, projectId, phaseId, departmentId, roomId }, new [] { "report_type" })
                    .Select(pr => string.Format("{0}_{1}_{2}.xlsx", pr.name, pr.report_type.name.Replace(" ", "_"), pr.id)).ToList();
            }
        }

        public static Dictionary<string, List<string>> GetQuotePONames(int domainId, int projectId, int? phaseId = null, int? departmentId = null, int? roomId = null)
        {
            var allFiles = new Dictionary<string, List<string>>();
            var quotes = new List<string>();
            var poFiles = new List<string>();

            using (var repository = new TableRepository<purchase_order>())
            {
                var keys = new List<string>(new [] { "domain_id", "project_id" });
                if (phaseId != null)
                    keys.Add("phase_id");
                if (departmentId != null)
                    keys.Add("department_id");
                if (roomId != null)
                    keys.Add("room_id");

                repository.GetAll(keys.ToArray(),
                    new [] { domainId, projectId, phaseId, departmentId, roomId }, null)
                    .ForEach(delegate (purchase_order po)
                    {
                        if (!string.IsNullOrEmpty(po.quote_file))
                            quotes.Add(po.quote_file);
                        if (!string.IsNullOrEmpty(po.po_file))
                            poFiles.Add(po.po_file);
                    });
            }

            allFiles.Add("quotes", quotes);
            allFiles.Add("pos", poFiles);
            return allFiles;
        }

        public static List<string> GetPhaseDocumentsName(int domainId, int projectId, int? phaseId = null)
        {
            var names = new List<string>();

            using (var repository = new TableRepository<phase_documents>())
            {
                var keys = new List<string>(new [] { "domain_id", "project_id" });
                if (phaseId != null)
                    keys.Add("phase_id");
                var phaseDocuments = repository.GetAll(keys.ToArray(), new [] { domainId, projectId, phaseId }, null);

                foreach (var document in phaseDocuments)
                {
                    names.Add($"{document.drawing_id}{document.filename}");
                }
            }

            return names;
        }

        public void UpdateLinkedAssets(project_documents document)
        {
            if (document.project_room_inventory == null)
                return;

            var inventoriesId = string.Join(", ", document.project_room_inventory.Select(d => d.inventory_id));

            var query = new StringBuilder("UPDATE project_room_inventory SET linked_document = ");
            query.Append(document.id);
            query.Append(" WHERE inventory_id IN (");
            query.Append(inventoriesId);
            query.Append(");");

            _db.Database.ExecuteSqlCommand(query.ToString());

            query.Clear();
            query.Append("UPDATE project_room_inventory SET linked_document = null WHERE linked_document = ");
            query.Append(document.id);
            query.Append(" AND inventory_id NOT IN (");
            query.Append(inventoriesId);
            query.Append(");");

            _db.Database.ExecuteSqlCommand(query.ToString());
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