using System;
using System.Linq;
using xPlannerAPI.Models;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using System.Collections.Generic;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Services
{
    public class BudgetCopilotRepository : IBudgetCopilotRepository, IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public BudgetCopilotRepository()
        {
            this._db = new audaxwareEntities();
        }

    
        public BudgetCopilot GetBudgetCopilot(int domainId, int? projectId, string search, int? period)
        {
            var budgetCopilot = new BudgetCopilot();
            var dateToSubtract = DateTime.Today.AddMonths(-1 * period??12);

            budgetCopilot.assets = _db.get_budget_data_from_inventories(domainId, period, projectId, search).ToList();

            var budgets = _db.get_budget_summary_values(domainId, period, projectId, search).FirstOrDefault();

            var listBudgetTypes = new List<BudgetAggregate>();
            budgetCopilot.budgets = new List<BudgetAggregate>();

            if (budgets != null)
            {
                var props = budgets.GetType();

                for (int i = 0; i < Enum.GetNames(typeof(BudgetValueTypes)).Length; i++)
                {
                    var budgetAgg = new BudgetAggregate();
                    var descLower = ((BudgetValueTypes)i).ToString().ToLower();
                    budgetAgg.aggregate_type = (BudgetValueTypes)i;
                    budgetAgg.unit_budget = (decimal)props.GetProperty(descLower + "_unit_budget").GetValue(budgets);
                    budgetAgg.unit_freight = (decimal)props.GetProperty(descLower + "_unit_freight").GetValue(budgets);
                    budgetAgg.unit_install = (decimal)props.GetProperty(descLower + "_unit_install").GetValue(budgets);
                    budgetAgg.unit_tax = (decimal)props.GetProperty(descLower + "_unit_tax").GetValue(budgets);
                    budgetAgg.unit_markup = (decimal)props.GetProperty(descLower + "_unit_markup").GetValue(budgets);
                    budgetAgg.unit_escalation = (decimal)props.GetProperty(descLower + "_unit_escalation").GetValue(budgets);

                    budgetCopilot.budgets.Add(budgetAgg);

                }
            }


            return budgetCopilot;
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