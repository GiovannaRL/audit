using System;
using xPlannerCommon.Models;
using System.Threading.Tasks;
using System.Linq;
using xPlannerCommon.Models.Enum;

namespace xPlannerAPI.Services
{
    public class ReportRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public ReportRepository()
        {
            this._db = new audaxwareEntities();
        }

        /* Add report data in the database and call the function to build the report */
        public async Task<project_report> Add(project_report item)
        {
            if (item.cost_center1 != null)
            {
                this._db.cost_center.Attach(item.cost_center1);
            }

            this._db.report_type.Attach(item.report_type);

            //foreach (var loc in item.report_location)
            //{
            //    //var room = loc.GetRoom(_db);
            //    this._db.report_location.Attach(loc);
            //}

            _db.project_report.Add(item);
            if (_db.SaveChanges() <= 0)
                return null;

            //gambiarra forte. Só consegui resolver assim.
            var total = item.report_location.Count;
            for (int i = 0; i < total; i++)
            {
                var loc = item.report_location.ToArray()[i];
                var report = new report_location();
                report.phase_id = loc.phase_id;
                report.department_id = loc.department_id;
                report.project_domain_id = item.project_domain_id;
                report.project_id = loc.project_id;
                report.report = item.id;
                report.room_id = loc.room_id;
                _db.report_location.Add(report);
                if (_db.SaveChanges() <= 0)
                    return null;

            }

            WebjobRepository<string> webJobRepository = new WebjobRepository<string> ();
                
            string queueName = item.report_type.name.ToLower().Replace(" ", "-").Replace("(", "").Replace(")", "");
            await webJobRepository.SendMessage(queueName, item.id.ToString());

            return item;
        }

        /* Add report data in the database and call the function to build the report */
        public async Task<project_report> Regenerate(project_report item)
        {
            item.status = ReportStatusCategory.Waiting.PercentageStart;
            item.status_category = ReportStatusCategory.Waiting.Category;
            this._db.Entry(item).State = System.Data.Entity.EntityState.Modified;
            this._db.SaveChanges();

            WebjobRepository<int> webJobRepository = new WebjobRepository<int>();

            await webJobRepository.SendMessage(item.report_type.name.ToLower().Replace(" ", "-").Replace("(", "").Replace(")", ""), item.id); 

            return item;
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