using System;
using System.Data;
using System.IO;
using System.Linq;
using xPlannerCommon.Models;
using xPlannerCommon.Models.Enum;
using xPlannerCommon.Services;

namespace reportsWebJob.Services
{
    class GenericReportRepository : IDisposable
    {

        public audaxwareEntities _db;
        private bool _disposed = false;

        public GenericReportRepository()
        {
            this._db = new audaxwareEntities();
            using (var cmd = this._db.Database.Connection.CreateCommand())
            {
                this._db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '1';";
                cmd.CommandType = CommandType.Text;
                int x = cmd.ExecuteNonQuery();
            }
        }

        public void InitiateReport(project_report item)
        {
            UpdateReportStatus(item, ReportStatusCategory.Initializing.PercentageStart);
        }

        public void CompleteReport(project_report item)
        {
            try
            {
                this.UpdateReportStatus(item, ReportStatusCategory.Completed.PercentageStart);

                using (NotificationRepository notRepository = new NotificationRepository())
                {
                    user_notification notification = new user_notification();
                    notification.domain_id = item.project_domain_id;
                    notification.userId = item.generated_by;
                    notification.message = String.Format("Report named '{0}' in project {0} was successfuly generated", item.name, item.project.project_description);

                    notRepository.Add(notification);
                }
            }
            catch (Exception)
            {
                using (FileStreamRepository fileRepository = new FileStreamRepository())
                {
                    fileRepository.DeleteBlob("reports", item.file_name);
                    throw;
                }
            }
        }

        public void CompleteReportError(project_report item, Exception error = null)
        {
            try
            {
                this.UpdateReportStatus(item, ReportStatusCategory.Error.PercentageStart);

                using (NotificationRepository notRepository = new NotificationRepository())
                {
                    user_notification notification = new user_notification();
                    notification.domain_id = item.project_domain_id;
                    notification.userId = item.generated_by;
                    notification.message = "Report named '" + item.name + "' was not generated! "; //+ get_error;

                    notRepository.Add(notification);
                }
            }
            catch (Exception)
            {
                using (FileStreamRepository fileRepository = new FileStreamRepository())
                {
                    fileRepository.DeleteBlob("reports", item.file_name);
                    throw;
                }
            }
        }

        public string GetCostCenterInfo(project_report item)
        {
            /* Cost Center */
            string cost_center_description = "All Cost Centers";
            if (item.cost_center1 != null && item.cost_center1.id > 0)
            {
                if (item.cost_center1.code != null && item.cost_center1.description != null)
                {
                    cost_center_description = String.Format("Cost Center: {0} - {1}", item.cost_center1.code, item.cost_center1.description);
                }
                else
                {
                    cost_center cost = this._db.cost_center.Find(item.cost_center1.id);
                    if (cost != null)
                    {
                        cost_center_description = String.Format("Cost Center: {0} - {1}", cost.code, cost.description);
                        item.cost_center1 = cost;
                    }
                }
            }

            return cost_center_description;
        }

        public cost_center GetCostCenter(project_report item)
        {
            /* Cost Center */
            if (item.cost_center1 != null && item.cost_center1.id > 0)
            {
                if (item.cost_center1.code != null && item.cost_center1.description != null)
                {
                    return item.cost_center1;
                }
                else
                {
                    return this._db.cost_center.Find(item.cost_center1.id);
                }
            }

            return null;
        }

        public string GetWhereClause(project_report item, string roomsTable_letter, string cost_centerTable_letter, string costCenterIdColumn = "cost_center_id")
        {
            string reportWhere = " WHERE pri.domain_id = " + item.project_domain_id + " AND pri.project_id = " + item.project_id;

            if (item.cost_center1 != null && item.cost_center1.id > 0)
            {
                reportWhere += " AND " + cost_centerTable_letter + "." + costCenterIdColumn + " = " + item.cost_center1.id;
            }

            int selected_rooms_qty = item.report_location.Count();
            int project_rooms_qty = this._db.project_room.Where(pr => pr.domain_id == item.project_domain_id && pr.project_id == item.project_id).Count();

            if (item.report_location != null && selected_rooms_qty > 0 && selected_rooms_qty != project_rooms_qty)
            {
                report_location first = item.report_location.First();
                reportWhere += " AND ((pri.phase_id = " + first.phase_id + " AND pri.department_id = " + first.department_id + " AND pri.room_id = " + first.room_id + ")";
                foreach (var room in item.report_location.ToList().GetRange(1, item.report_location.Count() - 1))
                {
                    reportWhere += " OR (pri.phase_id = " + room.phase_id + " AND pri.department_id = " + room.department_id + " AND pri.room_id = " + room.room_id + ")";
                }
                reportWhere += ")";
            }

            return reportWhere.Replace("pri.", roomsTable_letter + ".");
        }

        public string GetFilename(project_report report)
        {
            return (report.name + "_" + report.report_type.name + "_" + report.id).Replace(" ", "_");
        }

        public string GetReportsDirectory()
        {
            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                string reportDirectory = Path.Combine(Domain.GetRoot(), "reports");

                fileRepository.CreateLocalDirectory(reportDirectory);

                return reportDirectory;
            }
        }

        public bool UploadToCloud(project_report report, string savedIn, string extension)
        {
            using (FileStreamRepository fileRep = new FileStreamRepository())
            {
                try
                {
                    UpdateReportStatus(report, ReportStatusCategory.Uploading.PercentageStart);

                    fileRep.UploadToCloud(savedIn, "reports", GetFilename(report) + "." + extension);
                    fileRep.DeleteFile(savedIn);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public int CalcNetNew(string resp, int budget_qty, int dnp_qty)
        {
            if (resp.Equals("EXOI") || resp.Equals("EXCI") || resp.Equals("EXVI") || resp.Equals("EXEX"))
            {
                return 0;
            }

            return budget_qty - dnp_qty;
        }

        public project_report UpdateReportStatus(project_report report, decimal newValue)
        {
            if (report == null)
                return null;

            // Sets the category according the percentage
            report.status_category = ReportStatusCategory.GetByValue(newValue);
            report.status = newValue;
            this._db.Entry(report).State = System.Data.Entity.EntityState.Modified;
            this._db.SaveChanges();
            return report;
        }

        public project_report IncrementReportStatus(project_report report, Decimal value)
        {
            return this.UpdateReportStatus(report, report.status + value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
