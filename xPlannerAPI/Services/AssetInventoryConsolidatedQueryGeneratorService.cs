using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using xPlannerCommon.Models;
using xPlannerCommon.Enumerators;
using System.Net.Http;
using System.Net;
using System.Data;
using System.Web.Http;

namespace xPlannerAPI.Services
{
    public class AssetInventoryConsolidatedQueryGeneratorService 
    {
        //all the columns with specialTreatment used on AddSpecialColumns needs to be added here 
        static readonly string[] _columnsToIgnore = { "inventory_ids", "inventory_id", "consolidated_view", "option_ids", "unit_budget", "phase_id", "department_id", "room_id", "phases_qty", "quantity", "departments_qty", "rooms_qty", "total_assets_options" };
        static readonly PropertyInfo[] _inventoryColumns = typeof(asset_inventory).GetProperties();
        static readonly string[] _columnsToMin = { "cost_center_id", "jsn_description", "jsn_comments", "jsn_nomenclature", "project_description", "phase_description", "department_description", "room_number", "room_name", "final_room_number", "final_room_name", "room_count", "date_modified", "photo_domain_id", "discontinued", "clearance_top", "clearance_bottom", "clearance_right", "clearance_left", "clearance_front", "clearance_back", "weight", "loaded_weight", "ship_weight", "asset_comment", "eq_unit_desc"};
        static readonly string[] _columnsToMax = { "delivered_date", "received_date", "date_added", "date_modified" };
        static readonly string[] _columnsToMinAsBit = { "detailed_budget", "has_shop_drawing", "none_option" };
        static readonly string[] _columnsToRemoveDuplicate = { "po_number", "resp", "type_resp", "cad_id", "manufacturer_description", "cut_sheet", "model_number", "model_name", "photo", "cost_center", "current_location", "height", "width", "depth", "weight", "jsn_utility1", "jsn_utility2", "jsn_utility3", "jsn_utility4", "jsn_utility5", "jsn_utility6", "placement", "tag", "option_codes", "option_descriptions", "cfm", "btus", "electrical", "volts", "hertz", "watts", "data_desc", "vendor", "comment", "medgas", "room_code", "blueprint", "staff", "functional_area" };
        static readonly string[] _stringColumnsToSum = { "options_price" };
        static readonly string[] _columnsToAgregateWithSemicolon = { "source_location", "target_location", "asset_description", "unit_budget_total", "total_unit_budget", "unit_budget_adjusted", "unit_escalation", "unit_escalation_calc", "unit_freight", "unit_freight_net", "unit_freight_markup", "unit_install", "unit_install_net", "unit_install_markup", "unit_markup", "unit_markup_calc", "unit_tax", "unit_tax_calc", "po_status" };
        static readonly string[] _columnsRequiredInGroupBy = { "asset_id", "asset_domain_id", "asset_code", "domain_id", "jsn_code" };

        public AssetInventoryConsolidatedQueryGeneratorService()
        {
        }


        //HttpResponseMessage
        public string CreateQuery(int domainId, int projectId, int? phaseId, int? departmentId, int? roomId, string[] groupBy, bool? filterPoQty = false, bool? showOnlyApprovedAssets = false)
        {
            int projectLevel = ProjectLevelEnum.Project;
            
            if ((roomId??0) > 0)
                projectLevel = ProjectLevelEnum.Room;
            else if ((departmentId??0) > 0)
                projectLevel = ProjectLevelEnum.Department;
            else if ((phaseId??0) > 0)
                projectLevel = ProjectLevelEnum.Phase;

            //add required columns to group by if they don´t exist
            foreach (var item in _columnsRequiredInGroupBy)
            {
                if (!groupBy.Contains(item)) 
                    groupBy = groupBy.Concat(new string[] { item }).ToArray();
            }

            //remove columns that needs to be ignored in case they are in the group by
            groupBy = groupBy.Where(g => !_columnsToIgnore.Contains(g)).ToArray();


            if (!ValidateGroupBy(groupBy))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            
            StringBuilder sb = new StringBuilder("SELECT ");
            sb = AddSelect(sb, groupBy, projectLevel);
            sb.Append(" FROM asset_inventory a ");
            sb = AddWhere(sb, domainId, projectId, phaseId, departmentId, roomId, filterPoQty, showOnlyApprovedAssets);
            sb = AddGroupBy(sb, projectLevel, groupBy);

            return sb.ToString(); 

        }


        public List<CompareInventoriesResult> CompareInventories(string[] groupBy, asset_inventory inventory1, asset_inventory inventory2) {


            if (!ValidateGroupBy(groupBy))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var data = new List<CompareInventoriesResult>();
            groupBy = groupBy.Where(g => !_columnsToIgnore.Contains(g)).ToArray();

            foreach (var item in _inventoryColumns)
            {
                string content1 = (item.GetValue(inventory1) ?? "").ToString();
                string content2 = (item.GetValue(inventory2) ?? "").ToString();

                if (groupBy.Contains(item.Name) && content1 != content2)
                {
                    var comparedItem = new CompareInventoriesResult();
                    comparedItem.columnName = item.Name;
                    comparedItem.contentInventory1 = content1;
                    comparedItem.contentInventory2 = content2;

                    data.Add(comparedItem);
                }
            }

            return data;
        }

        private bool ValidateGroupBy(string[] groupBy)
        {
            if (groupBy.Count() == 0)
                return false;

            return groupBy.Where(groupColumn => !_inventoryColumns.Any(inventoryColumn => inventoryColumn.Name == groupColumn.Trim())).Count() <= 0;
        }

        private StringBuilder AddWhere(StringBuilder sb, int domainId, int projectId, int? phaseId, int? departmentId, int? roomId, bool? filterPoQty, bool? showOnlyApprovedAssets)
        {
            sb.Append(" WHERE project_id = " + projectId.ToString() + " AND domain_id = " + domainId.ToString());
            if ((phaseId ?? 0) > 0)
                sb.Append(" AND phase_id = " + phaseId.ToString());
            if ((departmentId ?? 0) > 0)
                sb.Append(" AND department_id = " + departmentId.ToString());
            if ((roomId ?? 0) > 0)
                sb.Append(" AND room_id = " + roomId.ToString());
            if (filterPoQty??false)
                sb.Append(" AND po_qty = 0");
            if (showOnlyApprovedAssets??false)
                sb.Append(" AND current_location = 'Approved'");

            return sb;

        }

        private StringBuilder AddGroupBy(StringBuilder sb, int projectLevel, string[] groupBy)
        {
            sb.Append(" GROUP BY " + String.Join(", ", groupBy));
            if (projectLevel >= ProjectLevelEnum.Phase)
                sb.Append(", phase_description, phase_id ");
            if (projectLevel >= ProjectLevelEnum.Department)
                sb.Append(", department_description, department_id ");
            if (projectLevel == ProjectLevelEnum.Room)
                sb.Append(", room_number, room_name, final_room_number, final_room_name, room_count, room_id ");


            return sb;
        }

        //add here all columns that need diferent treatment
        private string AddSelectSpecialColumns(int projectLevel) {
            
            string exceptions = $", STRING_AGG(CAST(inventory_id AS VARCHAR(MAX)), ',') as inventory_ids, 0 as inventory_id, 1 as consolidated_view,'' as option_ids, 0.00 as unit_budget, " + 
             $" CASE WHEN {projectLevel} >= {ProjectLevelEnum.Phase} THEN phase_id ELSE 0 END AS phase_id," +
             $" CASE WHEN {projectLevel} >= {ProjectLevelEnum.Department} THEN department_id ELSE 0 END AS department_id," +
             $" CASE WHEN {projectLevel} = {ProjectLevelEnum.Room} THEN room_id ELSE 0 END AS room_id, " +
             $" COUNT(*) AS quantity, STRING_AGG(CAST(phase_id AS VARCHAR(MAX)), ',') as phases_qty, " +
             $" STRING_AGG(CAST(department_id AS VARCHAR(MAX)), ',') as departments_qty, STRING_AGG(CAST(room_id AS VARCHAR(MAX)), ',') as rooms_qty, " +
             $" (SELECT COUNT(asset_option_id) FROM assets_options ao WHERE ao.asset_id = a.asset_id AND ao.domain_id = a.asset_domain_id) as total_assets_options ";

            return exceptions;
        }

        private StringBuilder AddSelect(StringBuilder sb,  string[] groupBy, int projectLevel)
        {
            var remainingColumns = "";

            sb.Append(String.Join(", ", groupBy));
            sb.Append(AddSelectSpecialColumns(projectLevel));

            //obs: ver questao de equivalencia de nomes, exemplo budget_qty no banco vira planned_qty no front. Como tratar isso.

            foreach (var item in _inventoryColumns)
            {
                var columnType = item.PropertyType.FullName;
                var columnName = item.Name;

                if (_columnsToIgnore.Contains(columnName) || groupBy.Contains(columnName))
                    continue;

                //these are bool columns that needs to continue as bit
                if (columnName.Contains("_ow") || _columnsToMinAsBit.Contains(columnName))
                    remainingColumns += ", CAST(MIN(" + columnName + "+0) AS BIT) as " + columnName;
                else if (_columnsToMin.Contains(columnName))
                    remainingColumns += ", MIN(" + columnName + ") as " + columnName;
                else if (_columnsToMax.Contains(columnName))
                    remainingColumns += ", MAX(" + columnName + ") as " + columnName;
                else if (_stringColumnsToSum.Contains(columnName))
                    remainingColumns += ", CAST(SUM(CAST(" + columnName + " AS FLOAT)) AS VARCHAR(15)) as " + columnName;
                else if (_columnsToAgregateWithSemicolon.Contains(columnName))
                    remainingColumns += ", STRING_AGG(CAST(" + columnName + " AS VARCHAR(MAX)), ';') as " + columnName;
                else
                {
                    if ((columnType.ToUpper().Contains("DECIMAL") || columnType.ToUpper().Contains("INT")))
                        remainingColumns += ", SUM(COALESCE(" + columnName + ", 0)) AS " + columnName;
                    else if(_columnsToRemoveDuplicate.Contains(columnName))
                        remainingColumns += ", (SELECT DBO.FN_REMOVE_DUPLICATED_ITEMS(STRING_AGG(CAST(" + columnName + " AS VARCHAR(MAX)), ','), ',')) as " + columnName;
                    else
                        remainingColumns += ", STRING_AGG(CAST(" + columnName + " AS VARCHAR(MAX)), ',') as " + columnName;

                }
            }

            sb.Append(remainingColumns);
            return sb;
        }

    }
}