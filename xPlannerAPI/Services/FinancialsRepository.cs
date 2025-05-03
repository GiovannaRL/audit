using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerCommon.Models;
using xPlannerAPI.Interfaces;

namespace xPlannerAPI.Services
{
    public class FinancialsRepository : IFinancialsRepository
    {

        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public FinancialsRepository()
        {
            _db = new audaxwareEntities();
        }

        public FinancialStructure GetAll(int domainId, int projectId, int? phaseId, int? departmentId, int? roomId)
        {
            var structure = new FinancialStructure();
            var financials = _db.get_financials(domainId, projectId, phaseId, departmentId, roomId).FirstOrDefault();
            if (financials != null)
            {
                structure = GetTotalBudget(financials);
                structure.warehouse = GetWarehouse(financials);
                structure.tax = GetTax(financials);
                structure.contingency = GetContigency(financials);
                structure.freight = GetFreight(financials);
                structure.warranty = GetWarranty(financials);
                structure.markup = GetMarkup(financials);
                structure.escalation = GetEscalation(financials);
                structure.install = GetInstall(financials);
                structure.budget_qty = financials.budget_qty ?? 0;
                structure.dnp_qty = financials.dnp_qty ?? 0;
                structure.po_qty = financials.po_qty ?? 0;
            }
            return structure;

        }

        private FinancialStructure GetTotalBudget(get_financials_Result financials)
        {
            var financialsData = new FinancialStructure();

            financialsData.actual = financials.po_total;
            financialsData.budget = financials.total_budget;
            financialsData.delta = financials.planned_budget_delta;
            financialsData.planned = financials.planned_budget;
            financialsData.projected = financials.projected_cost;

            return financialsData;

        }

        private FinancialValues GetWarehouse(get_financials_Result financials)
        {
            var financialsData = new FinancialValues();

            financialsData.actual = financials.warehouse_actual;
            financialsData.budget = financials.warehouse_budget;
            financialsData.delta = financials.warehouse_delta;
            financialsData.planned = financials.warehouse_planned;
            financialsData.projected = financials.warehouse_projected;
            return financialsData;

        }
        private FinancialValues GetTax(get_financials_Result financials)
        {
            var financialsData = new FinancialValues();

            financialsData.actual = financials.tax_actual;
            financialsData.budget = financials.tax_budget;
            financialsData.delta = financials.tax_delta;
            financialsData.planned = financials.tax_planned;
            financialsData.projected = financials.tax_projected;
            return financialsData;

        }
        private FinancialValues GetContigency(get_financials_Result financials)
        {
            var financialsData = new FinancialValues();

            financialsData.actual = financials.contingency_actual;
            financialsData.budget = financials.contingency_budget;
            financialsData.delta = financials.contingency_delta;
            financialsData.planned = financials.contingency_planned;
            financialsData.projected = financials.contingency_projected;
            return financialsData;

        }
        private FinancialValues GetFreight(get_financials_Result financials)
        {
            var financialsData = new FinancialValues();

            financialsData.actual = financials.freight_actual;
            financialsData.budget = financials.freight_budget;
            financialsData.delta = financials.freight_delta;
            financialsData.planned = financials.freight_planned;
            financialsData.projected = financials.freight_projected;
            return financialsData;

        }
        private FinancialValues GetWarranty(get_financials_Result financials)
        {
            var financialsData = new FinancialValues();

            financialsData.actual = financials.warranty_actual;
            financialsData.budget = financials.warranty_budget;
            financialsData.delta = financials.warranty_delta;
            financialsData.planned = financials.warranty_planned;
            financialsData.projected = financials.warranty_projected;
            return financialsData;

        }
        private FinancialValues GetMarkup(get_financials_Result financials)
        {
            var financialsData = new FinancialValues();

            financialsData.actual = financials.markup_actual;
            financialsData.budget = financials.markup_budget;
            financialsData.delta = financials.markup_delta;
            financialsData.planned = financials.markup_planned;
            financialsData.projected = financials.markup_projected;
            return financialsData;

        }
        private FinancialValues GetEscalation(get_financials_Result financials)
        {
            var financialsData = new FinancialValues();

            financialsData.actual = financials.escalation_actual;
            financialsData.budget = financials.escalation_budget;
            financialsData.delta = financials.escalation_delta;
            financialsData.planned = financials.escalation_planned;
            financialsData.projected = financials.escalation_projected;
            return financialsData;

        }
        private FinancialValues GetInstall(get_financials_Result financials)
        {
            var financialsData = new FinancialValues();

            financialsData.actual = financials.install_actual;
            financialsData.budget = financials.install_budget;
            financialsData.delta = financials.install_delta;
            financialsData.planned = financials.install_planned;
            financialsData.projected = financials.install_projected;
            return financialsData;

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