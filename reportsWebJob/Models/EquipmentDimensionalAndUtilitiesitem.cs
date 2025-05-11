using System;

namespace reportsWebJob.Models
{
    class EquipmentDimensionalAndUtilitiesitem
    {
        public string asset_code { get; set; }
        public string placement { get; set; }
        public string resp { get; set; }
        public string manufacturer_description { get; set; }
        public string asset_description { get; set; }
        public string serial_number { get; set; }
        public string serial_name { get; set; }
        public string btus { get; set; }
        public string data_desc { get; set; }
        public Nullable<int> data_option { get; set; }
        public Nullable<int> electrical_option { get; set; }
        public string electrical { get; set; }
        public string volts { get; set; }
        public Nullable<int> phases { get; set; }
        public string hertz { get; set; }
        public string amps { get; set; }
        public Nullable<decimal> volt_amps { get; set; }
        public string watts { get; set; }
        public string  height { get; set; }
        public string width { get; set; }
        public string depth { get; set; }
        public Nullable<decimal> clearance_top { get; set; }
        public Nullable<decimal> clearance_back { get; set; }
        public Nullable<decimal> clearance_front { get; set; }
        public Nullable<decimal> clearance_bottom { get; set; }
        public Nullable<decimal> clearance_left { get; set; }
        public Nullable<decimal> clearance_right { get; set; }
        public string weight { get; set; }
        public Nullable<int> plumbing_option { get; set; }
        public string plumbing { get; set; }
        public string plu_cold_water { get; set; }
        public string plu_hot_water { get; set; }
        public string plu_drain { get; set; }

    }
}
