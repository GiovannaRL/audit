using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using reportsWebJob.Models;
using System.Globalization;
using xPlannerCommon.App_Data;
using System.Data.SqlClient;
using OfficeOpenXml;

namespace reportsWebJob.Services
{
    class BudgetSummaryRepository : GenericReportRepository
    {
        private FileStreamRepository fileRepository;

        public BudgetSummaryRepository()
        {
            fileRepository = new FileStreamRepository();
        }

        public void Build(project_report item)
        {
            InitiateReport(item);

            string filename = GetFilename(item);

            bool ok = BuildHtml(item, filename, 1);
            if (ok)
            {
                UpdateReportStatus(item, 58);
                string excelFile = BuildExcel(item, filename, 58);
                UpdateReportStatus(item, 96);

                using (FileStreamRepository fileRepository = new FileStreamRepository())
                {

                    if (UploadToCloud(item, excelFile, "xlsx"))
                        CompleteReport(item);
                    else
                        CompleteReportError(item);

                    fileRepository.DeleteFile(excelFile);
                }
            }
            else
                CompleteReportError(item);
        }

        public bool BuildHtml(project_report item, string filename, Decimal totalPercentage)
        {
            StringBuilder select = new StringBuilder();
            var culture = CultureInfo.GetCultureInfo("en");

            /* Mount data report */
            StringWriter stringWriter = new StringWriter();
            stringWriter.Write("<html><head><meta charset='UTF-8'>");
            stringWriter.Write("<link type='text/css' href='" + Path.Combine(Domain.GetRoot(), "css", "BudgetSummary.css") + "' rel='stylesheet' />");
            stringWriter.Write("<link type='text/css' href='" + Path.Combine(Domain.GetRoot(), "..\\xPlannerUI\\ThirdParty\\KendoUI\\Q32015\\styles", "kendo.common.min.css") + "' rel='stylesheet' />");
            stringWriter.Write("<link type='text/css' href='" + Path.Combine(Domain.GetRoot(), "..\\xPlannerUI\\ThirdParty\\KendoUI\\Q32015\\styles", "kendo.default.min.css") + "' rel='stylesheet' />");
            stringWriter.Write("<link type='text/css' href='" + Path.Combine(Domain.GetRoot(), "..\\xPlannerUI\\ThirdParty\\KendoUI\\Q32015\\styles", "kendo.default.mobile.min.css") + "' rel='stylesheet' />");
            stringWriter.Write("<script src='" + Path.Combine(Domain.GetRoot(), "..\\xPlannerUI\\ThirdParty\\KendoUI\\Q32015\\js", "jquery.min.js") + "'></script>");
            stringWriter.Write("<script src='" + Path.Combine(Domain.GetRoot(), "..\\xPlannerUI\\ThirdParty\\KendoUI\\Q32015\\js", "kendo.all.min.js") + "'></script>");
            stringWriter.Write("</head><body style=font-family:Arial; font-size:8pt;>");
            stringWriter.Write("<table width=\"100%\" border=\"0px\">");

            /* Get header values */
            var project_info = ProjectInfo(item);

            /* Header (begin) */
            stringWriter.Write("<tr><td><table width=\"100%\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td>");
            stringWriter.Write("<table width=\"350px\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td colspan=\"2\"><h2>Medical Asset Report - Budget Summary</h2></td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td colspan=\"2\">&nbsp;</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td>Client:</td>");
            stringWriter.Write("<td>" + project_info.client.name + "</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td>Client Project No:</td>");
            stringWriter.Write("<td>" + project_info.client_project_number + "</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td>Project Name:</td>");
            stringWriter.Write("<td>" + project_info.project_description + "</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td>Planner Project:</td>");
            stringWriter.Write("<td>" + project_info.hsg_project_number + "</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td>Cost Center:</td>");
            stringWriter.Write("<td>" + GetCostCenterInfo(item) + "</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table>");
            stringWriter.Write("</td>");
            stringWriter.Write("<td>");
            stringWriter.Write("<img height=\"40\" src='" + Path.Combine(Domain.GetRoot(), "images", "logo_aw.png") + "'/>");
            stringWriter.Write("</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table></td></tr>");
            /* Header (end) */

            stringWriter.Write("<tr>");
            stringWriter.Write("<td>");
            stringWriter.Write("<table width=\"100%\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td width=\"550px\" valign=\"top\">");

            UpdateReportStatus(item, 5);

            /* First table */
            stringWriter.Write("<table>");
            stringWriter.Write("<tr><td>");
            stringWriter.Write("<table border=\"1\"  width=\"550px\" class=\"bordertab\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\"><font color=\"white\">Phase</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\"><font color=\"white\">Budget</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\"><font color=\"white\">Projected</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" colspan=\"3\" align=\"center\"><font color=\"white\">Progress</font></th>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"125px\"><font color=\"white\">&nbsp;</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">Planned</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">Cost</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">% Committed</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">PO's</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">PO Delta</font></th>");
            stringWriter.Write("</tr>");

            /* Get phases information */
            List<BudgetSummaryPhase> phases = PhaseInfo(item);
            IncrementReportStatus(item, 1);

            decimal percentageByItem;
            if (phases.Count() > 0)
            {
                percentageByItem = ((decimal)9) / phases.Count();
                /* For each phase result */
                foreach (BudgetSummaryPhase phase in phases)
                {
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td class=\"datatab\">" + phase.description + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"right\">" + phase.total_budget_amt.ToString("C", culture) + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"right\">" + phase.projected_budget.ToString("C", culture) + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + phase.pct_committed + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"right\">" + phase.total_po_amt.ToString("C", culture) + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"right\">" + phase.buyout_delta.ToString("C", culture) + "</td>");
                    stringWriter.Write("</tr>");
                    IncrementReportStatus(item, percentageByItem);
                }
            }
            else
            {
                IncrementReportStatus(item, 9);
            }
            /* For each phase result (end) */
            stringWriter.Write("</table>");
            /* First table (end) */

            /* total phases result */
            BudgetSummaryPhasesTotal phasesTotal = PhaseInfoTotal(item);

            stringWriter.Write("<table border=\"0\"  width=\"550px\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td width=\"125px\"><b>Phase Subtotals</b></td>");
            stringWriter.Write("<td width=\"85\" align=\"right\"><b>" + (phasesTotal.total_budget_amt ?? 0).ToString("C", culture) + "</b></td>");
            stringWriter.Write("<td width=\"85\" align=\"right\"><b>" + (phasesTotal.projected_budget ?? 0).ToString("C", culture) + "</b></td>");
            stringWriter.Write("<td width=\"85\" align=\"center\"><b>" + phasesTotal.pct_committed + "</b></td>");
            stringWriter.Write("<td width=\"85\" align=\"right\"><b>" + (phasesTotal.total_po_amt ?? 0).ToString("C", culture) + "</b></td>");
            stringWriter.Write("<td width=\"85\" align=\"right\"><b>" + (phasesTotal.buyout_delta ?? 0).ToString("C", culture) + "</b></td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table><br/>");
            IncrementReportStatus(item, 1);
            /* total phases result (end) */
            stringWriter.Write("</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr><td></td></tr>");
            stringWriter.Write("<tr><td>");
            /* Second table */
            stringWriter.Write("<table border=\"1\"  width=\"550px\" class=\"bordertab\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\"><font color=\"white\">Department</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\"><font color=\"white\">Budget</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\"align=\"center\"><font color=\"white\">Projected</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" colspan=\"3\" align=\"center\"><font color=\"white\">Progress</font></th>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"125px\"><font color=\"white\">&nbsp;</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">Planned</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">Cost</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">% Committed</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">PO's</font></th>");
            stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">PO Delta</font></th>");
            stringWriter.Write("</tr>");

            /* Get departments information */
            List<BudgetSummaryDepartment> departments = DepartmentInfo(item);
            IncrementReportStatus(item, 1);

            if (departments.Count() > 0)
            {
                percentageByItem = ((decimal)9) / departments.Count();
                /* For each department result */
                foreach (BudgetSummaryDepartment department in departments)
                {
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td class=\"datatab\">" + department.description + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (department.total_budget_amt ?? 0).ToString("C", culture) + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (department.projected_budget ?? 0).ToString("C", culture) + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + department.pct_committed + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (department.total_po_amt ?? 0).ToString("C", culture) + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (department.buyout_delta ?? 0).ToString("C", culture) + "</td>");
                    stringWriter.Write("</tr>");
                    IncrementReportStatus(item, percentageByItem);
                }
            }
            else
            {
                IncrementReportStatus(item, 9);
            }
            /* For each department result (end) */
            stringWriter.Write("</table>");

            /* total departments result */
            stringWriter.Write("<table  border=\"0\"  width=\"550px\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td width=\"125px\">Dept Subtotals</td>");
            stringWriter.Write("<td width=\"85\" align=\"right\"><b>" + (phasesTotal.total_budget_amt ?? 0).ToString("C", culture) + "</b></td>");
            stringWriter.Write("<td width=\"85\" align=\"right\"><b>" + (phasesTotal.projected_budget ?? 0).ToString("C", culture) + "</b></td>");
            stringWriter.Write("<td width=\"85\" align=\"center\"><b>" + phasesTotal.pct_committed + "</b></td>");
            stringWriter.Write("<td width=\"85\" align=\"right\"><b>" + (phasesTotal.total_po_amt ?? 0).ToString("C", culture) + "</b></td>");
            stringWriter.Write("<td width=\"85\" align=\"right\"><b>" + (phasesTotal.buyout_delta ?? 0).ToString("C", culture) + "</b></td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table><br/>");
            /* total departments result (end) */
            IncrementReportStatus(item, 1);
            /* Second table (end) */

            stringWriter.Write("</td></tr>");
            stringWriter.Write("<tr><td>");

            int qty_rooms = QtyRooms(item);

            if (item.cost_center1 == null && item.report_location.Count() == qty_rooms)
            {
                /* Thirdy table */

                ancillary_v ancillary = AncillaryCosts(item);

                stringWriter.Write("<table width=\"550px\" id=\"ancillary_tab\">");
                stringWriter.Write("<tr><td align=\"right\">");
                stringWriter.Write("<table border=\"1\"  width=\"450px\" class=\"bordertab\">");
                stringWriter.Write("<tr>");
                stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"200px\"><font color=\"white\">Ancillary Costs</font></th>");
                stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">Budget</font></th>");
                stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">Charges</font></th>");
                stringWriter.Write("<th bgcolor=\"#404040\" align=\"center\" width=\"85px\"><font color=\"white\">Projected</font></th>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\">Warehouse</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.warehouse_budget ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.warehouse_charges ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.warehouse_projected ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\">Freight</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.freight_budget ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.freight_charges ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.freight_projected ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\">Sales Tax</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.tax_budget ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.tax_charges ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.tax_projected ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\">Warranty</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.warranty_budget ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.warranty_charges ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.warranty_projected ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\">Misc</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.misc_budget ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.misc_charges ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.misc_projected ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\">Install</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.install_budget ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.install_charges ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\">" + (ancillary.install_projected ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("</table>");
                /* ancillary total result */
                stringWriter.Write("<table>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td width=\"200px\">&nbsp;</td>");
                stringWriter.Write("<td width=\"85px\">&nbsp;</td>");
                stringWriter.Write("<td width=\"85px\" align=\"left\">SUBTOTAL</td>");
                stringWriter.Write("<td width=\"85px\" align=\"right\">" + ((ancillary.freight_projected + ancillary.warehouse_projected + ancillary.tax_projected + ancillary.misc_projected + ancillary.warranty_projected + ancillary.install_projected) ?? 0).ToString("C", culture) + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("</table>");
                /* ancillary total result (end) */
                stringWriter.Write("</td></tr>");
                stringWriter.Write("</table><br/>");
                IncrementReportStatus(item, 5);
                /* Thirdy table (end) */

                stringWriter.Write("</td></tr>");
                stringWriter.Write("<tr><td>");

                /* Fourth table */
                matching_values ancilary_totals = AncillaryTotals(item);

                var totalsString = new
                {
                    projected_cost = phasesTotal != null ? ((decimal)phasesTotal.projected_budget).ToString("C", culture) : "",
                    planned_budget = phasesTotal != null ? ((decimal)phasesTotal.total_budget_amt).ToString("C", culture) : "",
                    projected_budget_delta = ancilary_totals != null ? ancilary_totals.projected_delta : "",
                    client_budget = ancilary_totals != null ? ancilary_totals.project_budget : "",
                    client_budget_delta = ancilary_totals != null ? ancilary_totals.cliente_budget_delta : ""
                };

                stringWriter.Write("<table width=\"550px\" id=\"totals_tab\">");
                stringWriter.Write("<tr><td align=\"right\">");
                stringWriter.Write("<table border=\"1\" width=\"250px\" class=\"bordertab\">");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\" width=\"165px\"><b>Projected Cost</b></td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\" width=\"85px\"><b>" + totalsString.projected_cost + "</b></td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\" bgcolor=\"#404040\" align=\"right\" width=\"165px\"><font color=\"white\"><b>Planned Budget</b></font></td>");
                stringWriter.Write("<td class=\"datatab\" bgcolor=\"#404040\" align=\"right\" width=\"85px\"><font color=\"white\"><b>" + totalsString.planned_budget + "</b></FONT></td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\"><b>Projected Budget Delta</b></td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\"><b>" + totalsString.projected_budget_delta + "</b></td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\" bgcolor=\"#404040\" align=\"right\"><font color=\"white\"><b>Client Budget</b></font></td>");
                stringWriter.Write("<td class=\"datatab\" bgcolor=\"#404040\" align=\"right\"><font color=\"white\"><b>" + totalsString.client_budget + "</b></font></td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\"><b>Client Budget Delta</b></td>");
                stringWriter.Write("<td class=\"datatab\" align=\"right\"><b>" + totalsString.client_budget_delta + "</b></td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("</table>");
                /* Fourth table (end) */
                stringWriter.Write("</td></tr>");
                stringWriter.Write("</table>");
                IncrementReportStatus(item, 5);
            }
            stringWriter.Write("</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table>");
            stringWriter.Write("</td>");
            stringWriter.Write("<td valign=\"top\" align=\"left\">");
            stringWriter.Write("<table width=\"100%\">");
            stringWriter.Write("<tr><td>");
            UpdateReportStatus(item, 30);

            /* charts - Projected Budget */

            //--------------phase chart
            var domain1 = 1;
            if (!Helper.ShowAudaxWareInfo(item.project_domain_id))
                domain1 = item.project_domain_id;

            select.Clear();
            select.Append("select c.phase_id, c.description, round(sum(a.total_budget) - sum(a.buyout_delta),2) as projected_budget ");
            select.Append(" from inventory_po_qty_v a, project_department b, project_phase c, project_room_inventory d ");
            select.Append(" where a.project_id = b.project_id and c.project_id = a.project_id and a.department_id = b.department_id and b.phase_id = c.phase_id and d.project_id = a.project_id and d.asset_id = a.asset_id and d.asset_domain_id = a.asset_domain_id and d.room_id = a.room_id and d.department_id = a.department_id and a.project_id=" + item.project_id + " and a.domain_id in(" + domain1 + "," + item.project_domain_id + ")");
            if (item.phase_id > 0)
                select.Append(" and b.phase_id = " + item.phase_id);
            if (item.department_id > 0)
                select.Append(" and b.department_id = " + item.department_id);
            if (item.cost_center > 0)
                select.Append(" and d.cost_center_id = " + item.cost_center);
            select.Append("group by c.phase_id, c.description having (sum(a.total_budget) - sum(a.buyout_delta)) > 0 order by 3 asc");

            List<BudgetSummaryProjectedBudget> projected_budget = _db.Database.SqlQuery<BudgetSummaryProjectedBudget>(select.ToString()).ToList();

            var chart_colors = new string[] { "#9de219", "#90cc38", "#068c35", "#006634", "#004d38", "#033939", "#FF7F15", "#FF7815", "#FF7109", "#FF6C00", "#532300", "#3A1F0B", "#382314" };

            stringWriter.Write("<div id=\"example\">");
            stringWriter.Write("<div class=\"demo-section k-content wide\">");
            stringWriter.Write("<div id = \"chart\" style=\"width:100%\"></div>");
            stringWriter.Write("</div>");

            stringWriter.Write("<script>");
            stringWriter.Write("function createChart() {");
            stringWriter.Write("$(\"#chart\").kendoChart({");
            stringWriter.Write("title: {");
            stringWriter.Write("position: \"bottom\", ");
            stringWriter.Write("text: \"Projected Budget\"");
            stringWriter.Write("},");
            stringWriter.Write("legend: {");
            stringWriter.Write("visible: false");
            stringWriter.Write("},");
            stringWriter.Write("chartArea: {");
            stringWriter.Write("background: \"\"");
            stringWriter.Write("},");
            stringWriter.Write("seriesDefaults: {");
            stringWriter.Write("labels: {");
            stringWriter.Write("visible: true,");
            stringWriter.Write("background: \"transparent\", ");
            stringWriter.Write("template: \"#= category #: #= value#%\"");
            stringWriter.Write("}");
            stringWriter.Write("},");
            stringWriter.Write("series: [{");
            stringWriter.Write("type: \"pie\", ");
            stringWriter.Write("startAngle: 150,");
            stringWriter.Write("data: [");

            decimal total = 0;
            foreach (var data in projected_budget)
                total += data.projected_budget;

            int cont = 0;
            foreach (var data in projected_budget)
            {
                stringWriter.Write("{category: \"" + data.description + "\", ");
                stringWriter.Write("value: " + Math.Round((data.projected_budget * 100) / total, 0) + ",");
                stringWriter.Write("color: \"" + chart_colors[cont] + "\"");
                stringWriter.Write("},");
                cont++;
            }

            stringWriter.Write("]");
            stringWriter.Write("}],");
            stringWriter.Write("tooltip: {");
            stringWriter.Write("visible: true,");
            stringWriter.Write("format: \"{ 0}% \"");
            stringWriter.Write("}");
            stringWriter.Write("});");
            stringWriter.Write("}");
            stringWriter.Write("");
            stringWriter.Write("$(document).ready(createChart);");
            stringWriter.Write("$(document).bind(\"kendo: skinChange\", createChart);");
            stringWriter.Write("</script>");
            stringWriter.Write("</div>");

            /* end charts */
            stringWriter.Write("</td></tr>");

            /* charts - Projected Budget 2 */

            stringWriter.Write("<tr><td>");
            UpdateReportStatus(item, 30);

            //--------------phase chart
            select.Clear();
            select.Append("select b.department_id, b.description, round(sum(a.total_budget) - sum(a.buyout_delta),2) as projected_budget");
            select.Append(" from inventory_po_qty_v a, project_department b, project_room_inventory d");
            select.Append(" where a.project_id = b.project_id and a.department_id = b.department_id and d.project_id = a.project_id and d.department_id = a.department_id and d.room_id = a.room_id and d.asset_id = a.asset_id and a.asset_domain_id = d.asset_domain_id");
            select.Append(" and a.project_id = " + item.project_id + " and a.domain_id in(" + domain1 + "," + item.project_domain_id + ")");
            if (item.phase_id > 0)
                select.Append(" and b.phase_id = " + item.phase_id);
            if (item.department_id > 0)
                select.Append(" and b.department_id = " + item.department_id);
            if (item.cost_center > 0)
                select.Append(" and d.cost_center_id = " + item.cost_center);
            select.Append("group by b.department_id, b.description having (sum(a.total_budget) - sum(a.buyout_delta)) > 0 order by 3 desc");

            List<BudgetSummaryProjectedBudget> projected_budget2 = _db.Database.SqlQuery<BudgetSummaryProjectedBudget>(select.ToString()).ToList();

            stringWriter.Write("<div >");
            stringWriter.Write("<div class=\"demo-section k-content wide\">");
            stringWriter.Write("<div id = \"chart2\" style=\"width:100%\"></div>");
            stringWriter.Write("</div>");

            stringWriter.Write("<script>");
            stringWriter.Write("function createChart2() {");
            stringWriter.Write("$(\"#chart2\").kendoChart({");
            stringWriter.Write("title: {");
            stringWriter.Write("position: \"bottom\", ");
            stringWriter.Write("text: \"Projected Budget\"");
            stringWriter.Write("},");
            stringWriter.Write("legend: {");
            stringWriter.Write("visible: false");
            stringWriter.Write("},");
            stringWriter.Write("chartArea: {");
            stringWriter.Write("background: \"\"");
            stringWriter.Write("},");
            stringWriter.Write("seriesDefaults: {");
            stringWriter.Write("labels: {");
            stringWriter.Write("visible: true,");
            stringWriter.Write("background: \"transparent\", ");
            stringWriter.Write("template: \"#= category #: #= value#%\"");
            stringWriter.Write("}");
            stringWriter.Write("},");
            stringWriter.Write("series: [{");
            stringWriter.Write("type: \"pie\", ");
            stringWriter.Write("startAngle: 150,");
            stringWriter.Write("data: [");

            total = 0;
            foreach (var data in projected_budget2)
                total += data.projected_budget;

            cont = 0;
            decimal others_values = 0;
            foreach (var data in projected_budget2)
            {
                if (cont < 11)
                {
                    stringWriter.Write("{category: \"" + data.description + "\", ");
                    stringWriter.Write("value: " + Math.Round((data.projected_budget * 100) / total, 0) + ",");
                    stringWriter.Write("color: \"" + chart_colors[cont] + "\"");
                    stringWriter.Write("},");
                    cont++;
                }
                else
                {
                    others_values += data.projected_budget;
                    cont++;
                }
            }

            //if there is more than 12 values, we are adding the values and show as others
            if (others_values > 0)
            {
                stringWriter.Write("{category: \"Other\", ");
                stringWriter.Write("value: " + Math.Round((others_values * 100) / total, 0) + ",");
                stringWriter.Write("color: \"" + chart_colors[11] + "\"");
                stringWriter.Write("},");
            }

            stringWriter.Write("]");
            stringWriter.Write("}],");
            stringWriter.Write("tooltip: {");
            stringWriter.Write("visible: true,");
            stringWriter.Write("format: \"{ 0}% \"");
            stringWriter.Write("}");
            stringWriter.Write("});");
            stringWriter.Write("}");
            stringWriter.Write("");
            stringWriter.Write("$(document).ready(createChart2);");
            stringWriter.Write("$(document).bind(\"kendo: skinChange\", createChart2);");
            stringWriter.Write("</script>");
            stringWriter.Write("</div>");
            stringWriter.Write("</td></tr>");

            /* end charts */

            stringWriter.Write("</table>");
            stringWriter.Write("</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table>");
            stringWriter.Write("</td></tr>");
            stringWriter.Write("<tr><td>");
            stringWriter.Write("<table width=\"100%\">");
            stringWriter.Write("<tr>");

            //CHART 3
            List<procurement_chart1_Result> chart1 = this._db.procurement_chart1(item.project_id, item.phase_id, item.department_id, item.cost_center, domain1, item.project_domain_id).ToList();

            stringWriter.Write("<td width=\"50%\">");

            stringWriter.Write("<div id=\"procurement_progress1\">");
            stringWriter.Write("<div class=\"demo-section k-content wide\">");
            stringWriter.Write("<div id=\"chart3\" style=\"background: center no-repeat url('../content/shared/styles/world-map.png');\"></div>");
            stringWriter.Write("</div>");
            stringWriter.Write("<script>");
            stringWriter.Write("function createChart3() {");
            stringWriter.Write("$(\"#chart3\").kendoChart({");
            stringWriter.Write("title: {");
            stringWriter.Write("text: \"Procurement Progress (%)\"");
            stringWriter.Write("},");
            stringWriter.Write("legend: {");
            stringWriter.Write("position: \"bottom\"");
            stringWriter.Write("},");
            stringWriter.Write("chartArea: {");
            stringWriter.Write("background: \"\"");
            stringWriter.Write("},");
            stringWriter.Write("seriesDefaults: {");
            stringWriter.Write("type: \"line\",");
            stringWriter.Write("style: \"smooth\"");
            stringWriter.Write("},");
            stringWriter.Write("series: [{");
            stringWriter.Write("name: \"% of Projected Budget Commited\",");
            stringWriter.Write("data: [");
            cont = 0;
            foreach (var chart in chart1)
            {
                if (cont == 0)
                    stringWriter.Write(chart.pct_committed);
                else
                    stringWriter.Write("," + chart.pct_committed);

                cont++;

            }
            stringWriter.Write("]");
            stringWriter.Write("},{");
            stringWriter.Write("name: \"% of Assets Items Commited\",");
            stringWriter.Write("data: [");
            cont = 0;
            foreach (var chart in chart1)
            {
                if (cont == 0)
                    stringWriter.Write(chart.pct_purchased);
                else
                    stringWriter.Write("," + chart.pct_purchased);

                cont++;
            }
            stringWriter.Write("]");
            stringWriter.Write("}],");
            stringWriter.Write("valueAxis: {");
            stringWriter.Write("labels: {");
            stringWriter.Write("format: \"{0}%\"");
            stringWriter.Write("},");
            stringWriter.Write("line: {");
            stringWriter.Write("visible: false");
            stringWriter.Write("},");
            stringWriter.Write("axisCrossingValue: -10");
            stringWriter.Write("},");
            stringWriter.Write("categoryAxis: {");
            stringWriter.Write("categories: [");
            cont = 0;
            foreach (var chart in chart1)
            {
                if (cont == 0)
                    stringWriter.Write("'" + chart.period_end_date.Value.ToShortDateString() + "'");
                else
                    stringWriter.Write(",'" + chart.period_end_date.Value.ToShortDateString() + "'");

                cont++;
            }
            stringWriter.Write("],");
            stringWriter.Write("majorGridLines: {");
            stringWriter.Write("visible: false");
            stringWriter.Write("},");
            stringWriter.Write("labels: {");
            stringWriter.Write("rotation: \"auto\"");
            stringWriter.Write("}");
            stringWriter.Write("},");
            stringWriter.Write("tooltip: {");
            stringWriter.Write("visible: true,");
            stringWriter.Write("format: \"{0}%\",");
            stringWriter.Write("template: \"#= series.name #: #= value #\"");
            stringWriter.Write("}");
            stringWriter.Write("});");
            stringWriter.Write("}");
            stringWriter.Write("");
            stringWriter.Write("$(document).ready(createChart3);");
            stringWriter.Write("$(document).bind(\"kendo:skinChange\", createChart3);");
            stringWriter.Write("</script>");
            stringWriter.Write("</div>");


            stringWriter.Write("</td>");
            //END CHART 3

            //CHART 4
            List<procurement_chart2_Result> chart2 = this._db.procurement_chart2(item.project_id, item.phase_id, item.department_id, item.cost_center, domain1, item.project_domain_id).ToList();

            stringWriter.Write("<td>");

            stringWriter.Write("<div id=\"procurement_progress1\">");
            stringWriter.Write("<div class=\"demo-section k-content wide\">");
            stringWriter.Write("<div id=\"chart4\" style=\"background: center no-repeat url('../content/shared/styles/world-map.png');\"></div>");
            stringWriter.Write("</div>");
            stringWriter.Write("<script>");
            stringWriter.Write("function createChart4() {");
            stringWriter.Write("$(\"#chart4\").kendoChart({");
            stringWriter.Write("title: {");
            stringWriter.Write("text: \"Procurement Progress ($)\"");
            stringWriter.Write("},");
            stringWriter.Write("legend: {");
            stringWriter.Write("position: \"bottom\"");
            stringWriter.Write("},");
            stringWriter.Write("chartArea: {");
            stringWriter.Write("background: \"\"");
            stringWriter.Write("},");
            stringWriter.Write("seriesDefaults: {");
            stringWriter.Write("type: \"line\",");
            stringWriter.Write("style: \"smooth\"");
            stringWriter.Write("},");
            stringWriter.Write("series: [{");
            stringWriter.Write("name: \"Projected Budget\",");
            stringWriter.Write("data: [");
            cont = 0;
            foreach (var chart in chart2)
            {
                if (cont == 0)
                    stringWriter.Write(chart.projected_budget);
                else
                    stringWriter.Write("," + chart.projected_budget);

                cont++;

            }
            stringWriter.Write("]");
            stringWriter.Write("},{");
            stringWriter.Write("name: \"PO Amount\",");
            stringWriter.Write("data: [");
            cont = 0;
            foreach (var chart in chart2)
            {
                if (cont == 0)
                    stringWriter.Write(chart.po_amount);
                else
                    stringWriter.Write("," + chart.po_amount);

                cont++;
            }
            stringWriter.Write("]");
            stringWriter.Write("}],");
            stringWriter.Write("valueAxis: {");
            stringWriter.Write("labels: {");
            stringWriter.Write("format: \"${0}\"");
            stringWriter.Write("},");
            stringWriter.Write("line: {");
            stringWriter.Write("visible: false");
            stringWriter.Write("},");
            stringWriter.Write("axisCrossingValue: -10");
            stringWriter.Write("},");
            stringWriter.Write("categoryAxis: {");
            stringWriter.Write("categories: [");
            cont = 0;
            foreach (var chart in chart2)
            {
                if (cont == 0)
                    stringWriter.Write("'" + chart.period_end_date.Value.ToShortDateString() + "'");
                else
                    stringWriter.Write(",'" + chart.period_end_date.Value.ToShortDateString() + "'");

                cont++;
            }
            stringWriter.Write("],");
            stringWriter.Write("majorGridLines: {");
            stringWriter.Write("visible: false");
            stringWriter.Write("},");
            stringWriter.Write("labels: {");
            stringWriter.Write("rotation: \"auto\"");
            stringWriter.Write("}");
            stringWriter.Write("},");
            stringWriter.Write("tooltip: {");
            stringWriter.Write("visible: true,");
            stringWriter.Write("format: \"${0}\",");
            stringWriter.Write("template: \"#= series.name #: #= value #\"");
            stringWriter.Write("}");
            stringWriter.Write("});");
            stringWriter.Write("}");
            stringWriter.Write("");
            stringWriter.Write("$(document).ready(createChart4);");
            stringWriter.Write("$(document).bind(\"kendo:skinChange\", createChart4);");
            stringWriter.Write("</script>");
            stringWriter.Write("</div>");


            stringWriter.Write("</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table>");
            stringWriter.Write("</td></tr>");
            stringWriter.Write("</table>");
            stringWriter.Write("</body></html>");
            stringWriter.Close();

            /* Crete local directory if not exists */
            string reportsDirectory = Path.Combine(Domain.GetRoot(), "reports");
            this.fileRepository.CreateLocalDirectory(reportsDirectory);

            /* define filename */
            string htmlFilename = (item.name + "_" + item.report_type.name + "_" + item.id + ".html").Replace(" ", "_");
            string pdfFileName = htmlFilename.Replace("html", "pdf");
            string filename2 = htmlFilename.Replace(".html", "");
            string temporaryFilePath = Path.Combine(reportsDirectory, pdfFileName);
            string temporaryFileHtmlPath = Path.Combine(reportsDirectory, htmlFilename);

            /* save html file temporarily */
            this.fileRepository.SaveDocumentTemporarily(stringWriter.ToString(), reportsDirectory, htmlFilename);

            /* Convert html file to pdf*/
            //string htmlFilePath = Path.Combine(reportsDirectory, htmlFilename);
            //WkhtmltopdfRepository.ConvertToPDF(htmlFilePath, temporaryFilePath);

            ///* Upload file in the azure cloud */
            //if (UploadToCloud(item, temporaryFilePath, "pdf"))
            //{
            //    item.file_name = filename;
            //    CompleteReport(item);
            //}


            var ok = true;
            if (UploadToCloud(item, temporaryFileHtmlPath, "html"))
            {
                this.fileRepository.DeleteFile(temporaryFilePath);
                item.file_name = filename2;
            }
            else
                ok = false;

            this.fileRepository.DeleteFile(temporaryFilePath);


            return ok;
        }



        public string BuildExcel(project_report item, string filename, Decimal totalPercentage)
        {
            var culture = CultureInfo.GetCultureInfo("en");

            string reportsDirectory = Path.Combine(Domain.GetRoot(), "reports");
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "budgetSummaryTemplate.xlsx"));
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {

                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Budget Summary"];

                /* Get header values */
                var project_info = ProjectInfo(item);


                /* Report Header */
                worksheet.Cells[4, 2].Value = project_info.client.name;
                worksheet.Cells[5, 2].Value = project_info.client_project_number;
                worksheet.Cells[6, 2].Value = project_info.project_description;
                worksheet.Cells[7, 2].Value = project_info.hsg_project_number;
                worksheet.Cells[8, 2].Value = GetCostCenterInfo(item);


                /* First table */
                /* Get phases information */
                List<BudgetSummaryPhase> phases = PhaseInfo(item);
                IncrementReportStatus(item, 1);

                decimal percentageByItem;
                var row = 15;
                if (phases.Count() > 0)
                {
                    percentageByItem = ((decimal)9) / phases.Count();
                    /* For each phase result */
                    foreach (BudgetSummaryPhase phase in phases)
                    {
                        if (row > 15) {
                            worksheet.InsertRow(row, 1);
                            worksheet.SelectedRange[row, 2, row, 3].Style.Numberformat.Format = worksheet.Cells[15, 2].Style.Numberformat.Format;
                            worksheet.SelectedRange[row, 5, row, 6].Style.Numberformat.Format = worksheet.Cells[15, 2].Style.Numberformat.Format;
                        }

                        worksheet.Cells[row, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.SelectedRange[row, 2, row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        worksheet.SelectedRange[row, 5, row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 1].Value = phase.description;
                        worksheet.Cells[row, 2].Value = phase.total_budget_amt;
                        worksheet.Cells[row, 3].Value = phase.projected_budget;
                        worksheet.Cells[row, 4].Value = phase.pct_committed;
                        worksheet.Cells[row, 5].Value = phase.total_po_amt;
                        worksheet.Cells[row, 6].Value = phase.buyout_delta;
                        IncrementReportStatus(item, percentageByItem);
                        row++;
                    }
                }
                else
                {
                    row++;
                    IncrementReportStatus(item, 9);
                }

                var phasesTotal = PhaseInfoTotal(item);

                worksheet.Cells[row, 2].Value = (phasesTotal.total_budget_amt ?? 0);
                worksheet.Cells[row, 3].Value = (phasesTotal.projected_budget ?? 0);
                worksheet.Cells[row, 4].Value = phasesTotal.pct_committed;
                worksheet.Cells[row, 5].Value = (phasesTotal.total_po_amt ?? 0);
                worksheet.Cells[row, 6].Value = (phasesTotal.buyout_delta ?? 0);
                IncrementReportStatus(item, 1);

                var departments = DepartmentInfo(item);

                row += 4;
                if (departments.Count() > 0)
                {
                    percentageByItem = ((decimal)9) / departments.Count();
                    /* For each department result */
                    foreach (BudgetSummaryDepartment department in departments)
                    {
                        if (row > 20) {
                            worksheet.InsertRow(row, 1);
                            worksheet.SelectedRange[row, 2, row, 3].Style.Numberformat.Format = worksheet.Cells[20, 2].Style.Numberformat.Format;
                            worksheet.SelectedRange[row, 5, row, 6].Style.Numberformat.Format = worksheet.Cells[20, 2].Style.Numberformat.Format;
                        }

                        worksheet.Cells[row, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.SelectedRange[row, 2, row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        worksheet.SelectedRange[row, 5, row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 1].Value = department.description;
                        worksheet.Cells[row, 2].Value = (department.total_budget_amt ?? 0);
                        worksheet.Cells[row, 3].Value = (department.projected_budget ?? 0);
                        worksheet.Cells[row, 4].Value = department.pct_committed;
                        worksheet.Cells[row, 5].Value = (department.total_po_amt ?? 0);
                        worksheet.Cells[row, 6].Value = (department.buyout_delta ?? 0);
                        row++;
                        IncrementReportStatus(item, percentageByItem);
                    }
                }
                else
                {
                    row++;
                    IncrementReportStatus(item, 9);
                }
                if (worksheet.Cells[row, 1].Value == null)
                    row++;

                worksheet.Cells[row, 2].Value = (phasesTotal.total_budget_amt ?? 0);
                worksheet.Cells[row, 3].Value = (phasesTotal.projected_budget ?? 0);
                worksheet.Cells[row, 4].Value = phasesTotal.pct_committed;
                worksheet.Cells[row, 5].Value = (phasesTotal.total_po_amt ?? 0);
                worksheet.Cells[row, 6].Value = (phasesTotal.buyout_delta ?? 0);
                IncrementReportStatus(item, 1);


                var qty_rooms = QtyRooms(item);

                if (item.cost_center1 == null && item.report_location.Count() == qty_rooms)
                {
                    /* Thirdy table */

                    var ancillary = AncillaryCosts(item);

                    row += 3;
                    worksheet.Cells[row, 4].Value = (ancillary.warehouse_budget ?? 0);
                    worksheet.Cells[row, 5].Value = (ancillary.warehouse_charges ?? 0);
                    worksheet.Cells[row, 6].Value = (ancillary.warehouse_projected ?? 0);
                    row++;
                    worksheet.Cells[row, 4].Value = (ancillary.freight_budget ?? 0);
                    worksheet.Cells[row, 5].Value = (ancillary.freight_charges ?? 0);
                    worksheet.Cells[row, 6].Value = (ancillary.freight_projected ?? 0);
                    row++;
                    worksheet.Cells[row, 4].Value = (ancillary.tax_budget ?? 0);
                    worksheet.Cells[row, 5].Value = (ancillary.tax_charges ?? 0);
                    worksheet.Cells[row, 6].Value = (ancillary.tax_projected ?? 0);
                    row++;
                    worksheet.Cells[row, 4].Value = (ancillary.warranty_budget ?? 0);
                    worksheet.Cells[row, 5].Value = (ancillary.warranty_charges ?? 0);
                    worksheet.Cells[row, 6].Value = (ancillary.warranty_projected ?? 0);
                    row++;
                    worksheet.Cells[row, 4].Value = (ancillary.misc_budget ?? 0);
                    worksheet.Cells[row, 5].Value = (ancillary.misc_charges ?? 0);
                    worksheet.Cells[row, 6].Value = (ancillary.misc_projected ?? 0);
                    row++;
                    worksheet.Cells[row, 4].Value = (ancillary.install_budget ?? 0);
                    worksheet.Cells[row, 5].Value = (ancillary.install_charges ?? 0);
                    worksheet.Cells[row, 6].Value = (ancillary.install_projected ?? 0);
                    row++;
                    worksheet.Cells[row, 6].Value = ((ancillary.freight_projected + ancillary.warehouse_projected + ancillary.tax_projected + ancillary.misc_projected + ancillary.warranty_projected + ancillary.install_projected) ?? 0);
                    IncrementReportStatus(item, 5);
                    /* Thirdy table (end) */

                    /* Fourth table */
                    var ancilary_totals = AncillaryTotals(item);

                    var totalsString = new
                    {
                        projected_cost = phasesTotal != null ? ((decimal)phasesTotal.projected_budget).ToString("C", culture) : "",
                        planned_budget = phasesTotal != null ? ((decimal)phasesTotal.total_budget_amt).ToString("C", culture) : "",
                        projected_budget_delta = ancilary_totals != null ? ancilary_totals.projected_delta : "",
                        client_budget = ancilary_totals != null ? ancilary_totals.project_budget : "",
                        client_budget_delta = ancilary_totals != null ? ancilary_totals.cliente_budget_delta : ""
                    };

                    row += 2;
                    worksheet.Cells[row, 6].Value = totalsString.projected_cost;
                    row++;
                    worksheet.Cells[row, 6].Value = totalsString.planned_budget;
                    row++;
                    worksheet.Cells[row, 6].Value = totalsString.projected_budget_delta;
                    row++;
                    worksheet.Cells[row, 6].Value = totalsString.client_budget;
                    row++;
                    worksheet.Cells[row, 6].Value = totalsString.client_budget_delta;
                    /* Fourth table (end) */
                    IncrementReportStatus(item, 5);

                }
                else {
                    var row_before = row += 2;
                    for (int i = row_before; i < row_before + 15; i++)
                    {
                        worksheet.Cells[i, 2, i, 3].Merge = false;
                        worksheet.Cells[i, 10].Copy(worksheet.Cells[i, 2]);
                        worksheet.Cells[i, 10].Copy(worksheet.Cells[i, 3]);
                        worksheet.Cells[i, 10].Copy(worksheet.Cells[i, 4]);
                        worksheet.Cells[i, 10].Copy(worksheet.Cells[i, 5]);
                        worksheet.Cells[i, 10].Copy(worksheet.Cells[i, 6]);
                    }

                }

                UpdateReportStatus(item, 90);

                xlPackage.Save();
                IncrementReportStatus(item, 2);

                return excelPath;

            }
        }

        private project ProjectInfo(project_report item)
        {
            return this._db.projects.Include("client").Where(p => p.domain_id == item.project_domain_id && p.project_id == item.project_id).FirstOrDefault();
        }

        private List<BudgetSummaryPhase> PhaseInfo(project_report item)
        {
            StringBuilder select = new StringBuilder("SELECT c.domain_id, c.project_id, c.phase_id, c.description, coalesce(SUM(a.total_budget),0) AS total_budget_amt, coalesce(SUM(a.total_po_amt),0) AS total_po_amt, SUM(-a.buyout_delta) AS buyout_delta, SUM(a.total_budget) - SUM(a.buyout_delta) AS projected_budget, CASE WHEN (SUM(a.total_budget_amt) - SUM(a.buyout_delta)) = 0 THEN null ELSE cast(round(100*(SUM(a.total_po_amt)/(SUM(a.total_budget_amt) - SUM(a.buyout_delta))),2) as numeric(36,2)) END AS pct_committed");
            select.Append(" FROM inventory_po_qty_v a, project_department b, project_phase c ");
            select.Append(" WHERE a.project_id = b.project_id AND c.project_id = a.project_id AND a.department_id = b.department_id AND b.phase_id = c.phase_id AND ");
            string reportWhere = GetWhereClause(item, "a", "a");
            select.Append(reportWhere.Replace("WHERE", ""));
            select.Append(" GROUP BY c.domain_id, c.project_id, c.phase_id, c.description ORDER BY c.description;");

            _db.Database.CommandTimeout = 6000000;
            return _db.Database.SqlQuery<BudgetSummaryPhase>(select.ToString()).ToList();
        }

        private BudgetSummaryPhasesTotal PhaseInfoTotal(project_report item)
        {
            string reportWhere = GetWhereClause(item, "a", "a");
            StringBuilder select = new StringBuilder("SELECT SUM(a.total_budget) AS total_budget_amt, SUM(a.total_po_amt) AS total_po_amt, SUM(-a.buyout_delta) AS buyout_delta, SUM(a.total_budget) - SUM(a.buyout_delta) AS projected_budget, CASE WHEN (SUM(a.total_budget_amt) - SUM(a.buyout_delta)) = 0 THEN null ELSE cast(round(100*(SUM(a.total_po_amt)/(SUM(a.total_budget_amt) - SUM(a.buyout_delta))), 2) as numeric(36,2)) END AS pct_committed ");
            select.Append("FROM inventory_po_qty_v a INNER JOIN project_department b ON b.project_id = a.project_id AND a.department_id = b.department_id ");
            select.Append(reportWhere);

            _db.Database.CommandTimeout = 6000000;
            return _db.Database.SqlQuery<BudgetSummaryPhasesTotal>(select.ToString()).FirstOrDefault();
        }

        private List<BudgetSummaryDepartment> DepartmentInfo(project_report item)
        {
            string reportWhere = GetWhereClause(item, "a", "a");
            StringBuilder select = new StringBuilder("SELECT b.domain_id, b.project_id, b.phASe_id, b.department_id, b.description, SUM(a.total_budget) AS total_budget_amt, SUM(a.total_po_amt) AS total_po_amt, SUM(-a.buyout_delta) AS buyout_delta, SUM(a.total_budget) - SUM(a.buyout_delta) AS projected_budget,CASE WHEN (SUM(a.total_budget_amt) - SUM(a.buyout_delta)) = 0 THEN null ELSE cast(round(100*(SUM(a.total_po_amt)/(SUM(a.total_budget_amt) - SUM(a.buyout_delta))), 2) as numeric(36,2)) end AS pct_committed ");
            select.Append("FROM inventory_po_qty_v a, project_department b ");
            select.Append("WHERE a.project_id = b.project_id AND a.department_id = b.department_id AND");
            select.Append(reportWhere.Replace("WHERE", ""));
            select.Append(" GROUP BY b.domain_id, b.project_id, b.phase_id, b.department_id, b.description ORDER BY b.description;");

            _db.Database.CommandTimeout = 6000000;
            return _db.Database.SqlQuery<BudgetSummaryDepartment>(select.ToString()).ToList();

        }

        private int QtyRooms(project_report item)
        {
            return this._db.project_room.Where(pr => pr.domain_id == item.project_domain_id && pr.project_id == item.project_id).Count();
        }
        private ancillary_v AncillaryCosts(project_report item)
        {
            return this._db.ancillary_v.Where(a => a.domain_id == item.project_domain_id && a.project_id == item.project_id).FirstOrDefault();
        }

        private matching_values AncillaryTotals(project_report item)
        {
            return this._db.matching_values.Where(mv => mv.domain_id == item.project_domain_id && mv.project_id == item.project_id).FirstOrDefault();
        }
    }
}
