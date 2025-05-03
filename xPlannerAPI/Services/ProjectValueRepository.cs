using System;
using System.Linq;
using xPlannerAPI.Models;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class ProjectValueRepository : IProjectValueRepository
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;


        public ProjectValueRepository()
        {
            _db = new audaxwareEntities();
        }

        public object Get(int domainId, int projectId)
        {
            var view = _db.matching_values.Find(projectId);

            return view;
        }

        public object Get(int domainId, int projectId, int? phaseId = null, int? departmentId = null, int? roomId = null)
        {
            var view = _db.inventory_po_qty_v.Where(x => x.domain_id == domainId &&  x.project_id == projectId);
            if (roomId != null)
                view = view.Where(x => x.room_id == roomId && x.department_id == departmentId);
            else if (departmentId != null)
                view = view.Where(x => x.department_id == departmentId);
            else if (phaseId != null)
                view = view.Where(x => x.phase_id == phaseId);

            var view2 = view.GroupBy(g => g.project_id)
                        .Select(s => new InventoryPoQtyVTotals
                        {
                            total_budget_amt = s.Sum(x => x.total_budget_amt),
                            total_po_amt = s.Sum(x => x.total_po_amt),
                            buyout_delta = s.Sum(x => x.buyout_delta),
                            projected_cost = s.Sum(x => x.total_budget_amt) - s.Sum(x => x.buyout_delta)
                        }).FirstOrDefault();

            return view2;
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