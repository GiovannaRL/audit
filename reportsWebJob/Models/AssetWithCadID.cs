using System;
using xPlannerCommon.Models;

namespace reportsWebJob.Models
{
    class AssetWithCadID
    {
        public int asset_id { get; set; }
        public string asset_code { get; set; }
        public string cad_id { get; set; }
        public int manufacturer_id { get; set; }
        public string asset_description { get; set; }
        public int subcategory_id { get; set; }
        public String height { get; set; }
        public string width { get; set; }
        public string depth { get; set; }
        public string weight { get; set; }
        public string serial_number { get; set; }
        public Nullable<decimal> min_cost { get; set; }
        public Nullable<decimal> max_cost { get; set; }
        public Nullable<decimal> avg_cost { get; set; }
        public Nullable<decimal> last_cost { get; set; }
        public string default_resp { get; set; }
        public string cut_sheet { get; set; }
        public Nullable<System.DateTime> date_added { get; set; }
        public string added_by { get; set; }
        public string comment { get; set; }
        public string cad_block { get; set; }
        public string water { get; set; }
        public string plumbing { get; set; }
        public string data { get; set; }
        public string electrical { get; set; }
        public string mobile { get; set; }
        public string blocking { get; set; }
        public string medgas { get; set; }
        public string supports { get; set; }
        public Nullable<bool> discontinued { get; set; }
        public Nullable<System.DateTime> last_budget_update { get; set; }
        public string photo { get; set; }
        public Nullable<int> eq_measurement_id { get; set; }
        public Nullable<int> water_option { get; set; }
        public Nullable<int> plumbing_option { get; set; }
        public Nullable<int> data_option { get; set; }
        public Nullable<int> electrical_option { get; set; }
        public Nullable<int> mobile_option { get; set; }
        public Nullable<int> blocking_option { get; set; }
        public Nullable<int> medgas_option { get; set; }
        public Nullable<int> supports_option { get; set; }
        public string revit { get; set; }
        public string placement { get; set; }
        public Nullable<decimal> clearance_left { get; set; }
        public Nullable<decimal> clearance_right { get; set; }
        public Nullable<decimal> clearance_front { get; set; }
        public Nullable<decimal> clearance_back { get; set; }
        public Nullable<decimal> clearance_top { get; set; }
        public Nullable<decimal> clearance_bottom { get; set; }
        public string volts { get; set; }
        public Nullable<int> phases { get; set; }
        public string hertz { get; set; }
        public string amps { get; set; }
        public Nullable<decimal> volt_amps { get; set; }
        public string watts { get; set; }
        public string cfm { get; set; }
        public string btus { get; set; }
        public Nullable<bool> misc_ase { get; set; }
        public Nullable<bool> misc_ada { get; set; }
        public Nullable<bool> misc_seismic { get; set; }
        public Nullable<bool> misc_antimicrobial { get; set; }
        public Nullable<bool> misc_ecolabel { get; set; }
        public string misc_ecolabel_desc { get; set; }
        public string mapping_code { get; set; }
        public Nullable<bool> medgas_oxygen { get; set; }
        public Nullable<bool> medgas_nitrogen { get; set; }
        public Nullable<bool> medgas_air { get; set; }
        public Nullable<bool> medgas_n2o { get; set; }
        public Nullable<bool> medgas_vacuum { get; set; }
        public Nullable<bool> medgas_wag { get; set; }
        public Nullable<bool> medgas_co2 { get; set; }
        public Nullable<bool> medgas_other { get; set; }
        public Nullable<bool> medgas_steam { get; set; }
        public Nullable<bool> medgas_natgas { get; set; }
        public Nullable<bool> plu_hot_water { get; set; }
        public Nullable<bool> plu_drain { get; set; }
        public Nullable<bool> plu_cold_water { get; set; }
        public Nullable<bool> plu_return { get; set; }
        public Nullable<bool> plu_treated_water { get; set; }
        public Nullable<bool> plu_relief { get; set; }
        public Nullable<bool> plu_chilled_water { get; set; }
        public string serial_name { get; set; }
        public Nullable<int> useful_life { get; set; }
        public Nullable<decimal> loaded_weight { get; set; }
        public Nullable<decimal> ship_weight { get; set; }
        public string alternate_asset { get; set; }
        public Nullable<System.DateTime> updated_at { get; set; }
        public short domain_id { get; set; }
        public short manufacturer_domain_id { get; set; }
        public Nullable<bool> no_options { get; set; }
        public Nullable<bool> no_colors { get; set; }
        public short subcategory_domain_id { get; set; }
        public string category_attribute { get; set; }

        public asset GetAsset()
        {
            return new asset
            {
                asset_id = this.asset_id,
                asset_code = this.asset_code,
                manufacturer_id = this.manufacturer_id,
                asset_description = this.asset_description,
                subcategory_id = this.subcategory_id,
                height = this.height,
                width = this.width,
                depth = this.depth,
                weight = this.weight,
                serial_number = this.serial_number,
                min_cost = this.min_cost,
                max_cost = this.max_cost,
                avg_cost = this.avg_cost,
                last_cost = this.last_cost,
                default_resp = this.default_resp,
                cut_sheet = this.cut_sheet,
                date_added = this.date_added,
                added_by = this.added_by,
                comment = this.comment,
                cad_block = this.cad_block,
                water = this.water,
                plumbing = this.plumbing,
                data = this.data,
                electrical = this.electrical,
                mobile = this.mobile,
                blocking = this.blocking,
                medgas = this.medgas,
                supports = this.supports,
                discontinued = this.discontinued,
                last_budget_update = this.last_budget_update,
                photo = this.photo,
                eq_measurement_id = this.eq_measurement_id,
                water_option = this.water_option,
                plumbing_option = this.plumbing_option,
                data_option = this.data_option,
                electrical_option = this.electrical_option,
                mobile_option = this.mobile_option,
                blocking_option = this.blocking_option,
                medgas_option = this.medgas_option,
                supports_option = this.supports_option,
                revit = this.revit,
                placement = this.placement,
                clearance_left = this.clearance_left,
                clearance_right = this.clearance_right,
                clearance_front = this.clearance_front,
                clearance_back = this.clearance_back,
                clearance_top = this.clearance_top,
                clearance_bottom = this.clearance_bottom,
                volts = this.volts,
                phases = this.phases,
                hertz = this.hertz,
                amps = this.amps,
                volt_amps = this.volt_amps,
                watts = this.watts,
                cfm = this.cfm,
                btus = this.btus,
                misc_ase = this.misc_ase,
                misc_ada = this.misc_ada,
                misc_seismic = this.misc_seismic,
                misc_antimicrobial = this.misc_antimicrobial,
                misc_ecolabel = this.misc_ecolabel,
                misc_ecolabel_desc = this.misc_ecolabel_desc,
                mapping_code = this.mapping_code,
                medgas_oxygen = this.medgas_oxygen,
                medgas_nitrogen = this.medgas_nitrogen,
                medgas_air = this.medgas_air,
                medgas_n2o = this.medgas_n2o,
                medgas_vacuum = this.medgas_vacuum,
                medgas_wag = this.medgas_wag,
                medgas_co2 = this.medgas_co2,
                medgas_other = this.medgas_other,
                medgas_steam = this.medgas_steam,
                medgas_natgas = this.medgas_natgas,
                plu_hot_water = this.plu_hot_water,
                plu_drain = this.plu_drain,
                plu_cold_water = this.plu_cold_water,
                plu_return = this.plu_return,
                plu_treated_water = this.plu_treated_water,
                plu_relief = this.plu_relief,
                plu_chilled_water = this.plu_chilled_water,
                serial_name = this.serial_name,
                useful_life = this.useful_life,
                loaded_weight = this.loaded_weight,
                ship_weight = this.ship_weight,
                alternate_asset = this.alternate_asset,
                updated_at = this.updated_at,
                domain_id = this.domain_id,
                manufacturer_domain_id = this.manufacturer_domain_id,
                no_options = this.no_options,
                no_colors = this.no_colors,
                subcategory_domain_id = this.subcategory_domain_id,
                category_attribute = this.category_attribute
            };
        }
    }
}