using OfflineXPlanner.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text;
using xPlannerCommon.Models;

namespace OfflineXPlanner.Domain
{
    public class Inventory
    {
        private static Dictionary<int, string> _classes = new Dictionary<int, string>() {
            {0, "N/A"},
            {1, "AW"},
            {2, "CC"},
            {3, "CM"},
            {4, "FF"},
            {5, "ME"},
            {6, "RP"},
            {7, "SW"}
        };

        public ExportItemStatusEnum Status { get; set; }
        public string StatusComment { get; set; }
        public int? Id { get; set; }
        public string Code { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string ModelNumber { get; set; }
        public string ModelName { get; set; }
        public string JSN { get; set; }
        public string JSNNomenclature { get; set; }
        public int PlannedQty { get; set; }
        public string Class { get; set; }
        public string Clin { get; set; }
        public decimal? UnitBudget { get; set; }
        public string Phase { get; set; }
        public string Department { get; set; }
        public string RoomNumber { get; set; }
        public string RoomName { get; set; }
        public string Resp { get; set; }
        public string U1 { get; set; }
        public string U2 { get; set; }
        public string U3 { get; set; }
        public string U4 { get; set; }
        public string U5 { get; set; }
        public string U6 { get; set; }
        public decimal? UnitMarkup { get; set; }
        public decimal? UnitEscalation { get; set; }
        public decimal? UnitTax { get; set; }
        public decimal? UnitInstallNet { get; set; }
        public decimal? UnitInstallMarkup { get; set; }
        public decimal? UnitFreightNet { get; set; }
        public decimal? UnitFreightMarkup { get; set; }

        public string UnitOfMeasure { get; set; }
        public string ECN { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public string Depth { get; set; }
        public string MountingHeight { get; set; }
        public string Placement { get; set; }
        public string Comment { get; set; }
        public string CADID { get; set; }

        public int inventory_id { get; set; }

        /* Only to internal user */
        public int project_id { get; set; }
        public int department_id { get; set; }
        public int room_id { get; set; }

        public string PhotoFile { get; set; }
        public string TagPhotoFile { get; set; }
        public DateTime? DateAdded { get; set; }

        public Inventory() {
            PlannedQty = 1;
        }

        public Inventory(OleDbDataReader inventoryRow)
        {
            Id = Convert.ToInt32(inventoryRow["Id"]);
            inventory_id = Convert.ToInt32(inventoryRow["inventory_id"]);
            Code = inventoryRow["Code"].ToString();
            Manufacturer = inventoryRow["Manufacturer"].ToString();
            Description = inventoryRow["Description"].ToString();
            ModelNumber = inventoryRow["ModelNumber"].ToString();
            ModelName = inventoryRow["ModelName"].ToString();
            JSN = inventoryRow["JSN"].ToString();
            JSNNomenclature = inventoryRow["JSNNomenclature"].ToString();
            PlannedQty = Convert.ToInt32(inventoryRow["PlannedQty"]);
            if (PlannedQty <= 0) {
                PlannedQty = 1;
            }
            Class = inventoryRow["Class"].ToString();
            Clin = inventoryRow["Clin"].ToString();
            UnitBudget = Convert.ToDecimal(inventoryRow["UnitBudget"]);
            Phase = inventoryRow["Phase"].ToString();
            Department = inventoryRow["Department"].ToString();
            RoomNumber = inventoryRow["RoomNumber"].ToString();
            RoomName = inventoryRow["RoomName"].ToString();
            Resp = inventoryRow["Resp"].ToString();
            U1 = inventoryRow["U1"].ToString();
            U2 = inventoryRow["U2"].ToString();
            U3 = inventoryRow["U3"].ToString();
            U4 = inventoryRow["U4"].ToString();
            U5 = inventoryRow["U5"].ToString();
            U6 = inventoryRow["U6"].ToString();
            UnitMarkup = Convert.ToDecimal(inventoryRow["UnitMarkup"]);
            UnitEscalation = Convert.ToDecimal(inventoryRow["UnitEscalation"]);
            UnitTax = Convert.ToDecimal(inventoryRow["UnitTax"]);
            UnitInstallNet = Convert.ToDecimal(inventoryRow["UnitInstallNet"]);
            UnitInstallMarkup = Convert.ToDecimal(inventoryRow["UnitInstallMarkup"]);
            UnitFreightNet = Convert.ToDecimal(inventoryRow["UnitFreightNet"]);
            UnitFreightMarkup = Convert.ToDecimal(inventoryRow["UnitFreightMarkup"]);
            UnitOfMeasure = inventoryRow["UnitOfMeasure"].ToString();
            project_id = Convert.ToInt32(inventoryRow["project_id"]);
            department_id = Convert.ToInt32(inventoryRow["department_id"]);
            room_id = Convert.ToInt32(inventoryRow["room_id"]);
            ECN = inventoryRow["ECN"].ToString();
            Height = inventoryRow["Height"].ToString();
            Width = inventoryRow["Width"].ToString();
            Depth = inventoryRow["Depth"].ToString();
            MountingHeight = inventoryRow["MountingHeight"].ToString();
            Placement = inventoryRow["InstallMethod"].ToString();
            Comment = inventoryRow["Comments"].ToString();
            CADID = inventoryRow["CADID"].ToString();
            PhotoFile = inventoryRow["PhotoFile"].ToString();
            TagPhotoFile = inventoryRow["TagPhotoFile"].ToString();
            DateAdded = inventoryRow["DateAdded"] != DBNull.Value ? Convert.ToDateTime(inventoryRow["DateAdded"]) : DateTime.Now;

        }

        public Inventory(asset_inventory assetInventory)
        {
            UnitOfMeasure = "EA";
            inventory_id = Convert.ToInt32(assetInventory.inventory_id);
            Code = assetInventory.asset_code;
            CADID = assetInventory.cad_id;
            Manufacturer = assetInventory.manufacturer_description;
            Description = assetInventory.asset_description;
            ModelNumber = assetInventory.serial_number;
            ModelName = assetInventory.serial_name;
            JSN = assetInventory.jsn_code;
            JSNNomenclature = assetInventory.jsn_nomenclature;
            PlannedQty = assetInventory.budget_qty.GetValueOrDefault();
            if (PlannedQty <= 0) {
                PlannedQty = 1;
            }
            if (_classes.ContainsKey(assetInventory.@class.GetValueOrDefault()))
            {
                Class = _classes[assetInventory.@class.GetValueOrDefault()];
            }
            Clin = assetInventory.clin;
            UnitBudget = assetInventory.unit_budget;
            Phase = assetInventory.phase_description;
            Department = assetInventory.department_description;
            RoomNumber = assetInventory.room_number;
            RoomName = assetInventory.room_name;
            Resp = assetInventory.resp;
            U1 = assetInventory.jsn_utility1.ToLower() != "n/a" ? assetInventory.jsn_utility1 : null;
            U2 = assetInventory.jsn_utility2.ToLower() != "n/a" ? assetInventory.jsn_utility2 : null;
            U3 = assetInventory.jsn_utility3.ToLower() != "n/a" ? assetInventory.jsn_utility3 : null;
            U4 = assetInventory.jsn_utility4.ToLower() != "n/a" ? assetInventory.jsn_utility4 : null;
            U5 = assetInventory.jsn_utility5.ToLower() != "n/a" ? assetInventory.jsn_utility5 : null;
            U6 = assetInventory.jsn_utility6.ToLower() != "n/a" ? assetInventory.jsn_utility6 : null;
            UnitMarkup = Convert.ToDecimal(assetInventory.unit_markup);
            UnitEscalation = Convert.ToDecimal(assetInventory.unit_escalation);
            UnitTax = Convert.ToDecimal(assetInventory.unit_tax);
            UnitInstallNet = Convert.ToDecimal(assetInventory.unit_install_net);
            UnitInstallMarkup = Convert.ToDecimal(assetInventory.unit_install_markup);
            UnitFreightNet = Convert.ToDecimal(assetInventory.unit_freight_net);
            UnitFreightMarkup = Convert.ToDecimal(assetInventory.unit_freight_markup);
            ECN = assetInventory.ECN;
            Height = assetInventory.height;
            Width = assetInventory.width;
            Depth = assetInventory.depth;
            MountingHeight = assetInventory.mounting_height;
            Placement = assetInventory.placement;
            Comment = assetInventory.comment;
            DateAdded = assetInventory.date_modified;
        }

        public override string ToString()
        {
            StringBuilder toStringData = new StringBuilder("{");
            toStringData.Append($"Status: {this.Status.ToString()},");
            toStringData.Append($"StatusComment: {this.StatusComment ?? "null"},");
            toStringData.Append($"Id: {this.inventory_id},");
            toStringData.Append($"Code: {this.Code ?? "null"},");
            toStringData.Append($"CADID: {this.CADID ?? "null"},");
            toStringData.Append($"Manufacturer: {this.Manufacturer ?? "null"},");
            toStringData.Append($"Description: {this.Description ?? "null"},");
            toStringData.Append($"ModelNumber: {this.ModelNumber ?? "null"},");
            toStringData.Append($"ModelName: {this.ModelName ?? "null"},");
            toStringData.Append($"ModelJSN: {this.JSN ?? "null"},");
            toStringData.Append($"JSNNomenclature: {this.JSNNomenclature ?? "null"},");
            toStringData.Append($"PlannedQty: {this.PlannedQty},");
            toStringData.Append($"Class: {this.Class ?? "null"},");
            toStringData.Append($"Clin: {this.Clin ?? "null"},");
            toStringData.Append($"UnitBudget: {this.UnitBudget},");
            toStringData.Append($"Phase: {this.Phase ?? "null"},");
            toStringData.Append($"Department: {this.Department ?? "null"},");
            toStringData.Append($"RoomNumber: {this.RoomNumber ?? "null"},");
            toStringData.Append($"RoomName: {this.RoomName ?? "null"},");
            toStringData.Append($"Resp: {this.Resp ?? "null"},");
            toStringData.Append($"U1: {this.U1 ?? "null"},");
            toStringData.Append($"U2: {this.U2 ?? "null"},");
            toStringData.Append($"U3: {this.U3 ?? "null"},");
            toStringData.Append($"U4: {this.U4 ?? "null"},");
            toStringData.Append($"U5: {this.U5 ?? "null"},");
            toStringData.Append($"U6: {this.U6 ?? "null"},");
            toStringData.Append($"UnitMarkup: {this.UnitMarkup},");
            toStringData.Append($"UnitEscalation: {this.UnitEscalation},");
            toStringData.Append($"UnitTax: {this.UnitTax},");
            toStringData.Append($"UnitInstallNet: {this.UnitInstallNet},");
            toStringData.Append($"UnitInstallMarkup: {this.UnitInstallMarkup},");
            toStringData.Append($"UnitFreightNet: {this.UnitFreightNet},");
            toStringData.Append($"UnitFreightMarkup: {this.UnitFreightMarkup},");
            toStringData.Append($"UnitOfMeasure: {this.UnitOfMeasure ?? "null"},");
            toStringData.Append($"inventory_id: {this.inventory_id}");
            toStringData.Append($"ECN: {this.ECN}");
            toStringData.Append($"Height: {this.Height}");
            toStringData.Append($"Depth: {this.Depth}");
            toStringData.Append($"Width: {this.Width}");
            toStringData.Append($"MountingHeight: {this.MountingHeight}");
            toStringData.Append($"InstallMethod: {this.Placement}");
            toStringData.Append($"Comments: {this.Comment}");
            toStringData.Append("}");

            return toStringData.ToString();
        }
    }
}
