using System;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class JSNRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public JSNRepository()
        {
            this._db = new audaxwareEntities();
        }

        public asset UpdateMetrics(asset item) {
            item.height = item.jsn.inches_height.ToString() ?? null;
            item.width = item.jsn.inches_width.ToString() ?? null;
            item.weight = item.jsn.weight_av.ToString() ?? null;
            item.depth = item.jsn.in_depth.ToString() ?? null;

            return item;
        }

        public asset UpdateAsset(asset item)
        {
            item = UpdateU1(item);
            item = UpdateU2(item);
            item = UpdateU3(item);
            item = UpdateU4(item);
            item = UpdateU5(item);
            item = UpdateU6(item);
            item = UpdateMetrics(item);
            return item;
        }

        public asset UpdateU1(asset item)
        {
            switch (item.jsn.utility1)
            {
                case "A":
                    item.plu_hot_water = true;
                    item.plu_cold_water = true;
                    break;
                case "B":
                    item.plu_drain = true;
                    item.plu_cold_water = true;
                    break;
                case "C":
                    item.plu_hot_water = true;
                    item.plu_drain = true;
                    break;
                case "D":
                    item.plu_hot_water = true;
                    item.plu_cold_water = true;
                    item.plu_drain = true;
                    break;
                case "E":
                    item.plu_treated_water = true;
                    break;
                case "F":
                    item.plu_treated_water = true;
                    item.plu_hot_water = true;
                    item.plu_cold_water = true;
                    item.plu_drain = true;
                    break;
                case "G":
                    item.plu_treated_water = true;
                    item.plu_cold_water = true;
                    item.plu_drain = true;
                    break;
                case "H":
                    item.plu_treated_water = true;
                    item.plu_hot_water = true;
                    item.plu_drain = true;
                    break;
                case "I":
                    item.plu_drain = true;
                    break;
                case "J":
                    item.plu_cold_water = true;
                    break;
                default:
                    item.plu_hot_water = false;
                    item.plu_cold_water = false;
                    item.plu_drain = false;
                    item.plu_treated_water = false;
                    break;
            }

            return item;
        }

        public asset UpdateU2(asset item)
        {

            item.electrical = null;
            item.volts = item.jsn.volts;
            item.watts = item.jsn.watts;

            switch (item.jsn.utility2)
            {

                case "A":
                    item.electrical_option = 1;
                    item.electrical = "Conventional Outlet";
                    item.volts = item.volts ?? "120";
                    break;
                case "B":
                    item.electrical_option = 1;
                    item.electrical = "Special Outlet";
                    item.volts = item.volts ?? "120";
                    break;
                case "C":
                    item.electrical_option = 1;
                    item.volts = item.volts ?? "208/220";
                    break;
                case "D":
                    item.electrical_option = 1;
                    item.volts = item.volts ?? "120-208/220";
                    break;
                case "E":
                    item.electrical_option = 1;
                    item.electrical = "3 Phase";
                    item.volts = item.volts ?? "440";
                    break;
                case "F":
                    item.electrical_option = 1;
                    item.electrical = "Special requirement";
                    break;
                case "G":
                    item.electrical_option = 1;
                    item.electrical = "3 Phase";
                    item.volts = item.volts ?? "208/220";
                    break;
                default:
                    item.electrical_option = 0;
                    break;
            }

            return item;
        }

        public asset UpdateU3(asset item)
        {

            item.medgas = null;
            item.medgas_air = false;
            item.medgas_co2 = false;
            item.medgas_high_pressure = false;
            item.medgas_n2o = false;
            item.medgas_natgas = false;
            item.medgas_nitrogen = false;
            item.medgas_option = 0;
            item.medgas_other = false;
            item.medgas_oxygen = false;
            item.medgas_steam = false;
            item.medgas_vacuum = false;
            item.medgas_wag = false;
            item.gas_acetylene = false;
            item.gas_butane = false;
            item.gas_hydrogen = false;
            item.gas_instrument_air = false;
            item.gas_liquid_co2 = false;
            item.gas_liquid_nitrogen = false;
            item.gas_liquid_propane_gas = false;
            item.gas_methane = false;
            item.gas_propane = false;

            switch (item.jsn.utility3)
            {
                case "A":
                    item.medgas_oxygen = true;
                    break;
                case "B":
                    item.medgas_vacuum = true;
                    break;
                case "C":
                    item.medgas_other = true; //low pressure
                    break;
                case "D":
                    item.medgas_high_pressure = true;
                    break;
                case "E":
                    item.medgas_oxygen = true;
                    item.medgas_air = true;
                    break;
                case "H":
                    item.medgas_oxygen = true;
                    item.medgas_vacuum = true;
                    item.medgas_air = true;
                    break;
                case "J":
                    item.medgas_vacuum = true;
                    item.medgas_high_pressure = true;
                    break;
                case "K":
                    item.medgas_air = true;
                    break;
            }

            return item;
        }

        public asset UpdateU4(asset item)
        {
            switch (item.jsn.utility4)
            {
                case "A":
                    item.medgas_steam = true;
                    break;
                case "B":
                    item.medgas_nitrogen = true;
                    break;
                case "C":
                    item.medgas_n2o = true;
                    break;
                case "D":
                    item.medgas_nitrogen = true;
                    item.medgas_n2o = true;
                    break;
                case "E":
                    item.medgas_co2 = true;
                    break;
                case "F":
                    item.gas_liquid_co2 = true;
                    break;
                case "G":
                    item.gas_liquid_nitrogen = true;
                    break;
                case "H":
                    item.gas_instrument_air = true;
                    break;
            }

            return item;
        }

        public asset UpdateU5(asset item)
        {
            switch (item.jsn.utility5)
            {
                case "A":
                    item.medgas_natgas = true;
                    break;
                case "B":
                    item.gas_liquid_propane_gas = true;
                    break;
                case "C":
                    item.gas_methane = true;
                    break;
                case "D":
                    item.gas_butane = true;
                    break;
                case "E":
                    item.gas_propane = true;
                    break;
                case "F":
                    item.gas_hydrogen = true;
                    break;
                case "G":
                    item.medgas = "Reserved";
                    break;
                case "H":
                    item.gas_acetylene = true;
                    break;
            }

            return item;
        }

        public asset UpdateU6(asset item)
        {
            switch (item.jsn.utility6)
            {
                case "A":
                    item.electrical = item.electrical == null ? "Earth Ground" : item.electrical + ", Earth Ground";
                    break;
                case "B":
                    item.misc_shielding_lead_line = true;
                    break;
                case "C":
                    item.electrical = item.electrical == null ? "Remote Alarm Ground" : item.electrical + ", Remote Alarm Ground";
                    break;
                case "D":
                    item.electrical = item.electrical == null ? "Empty conduit with pull cord" : item.electrical + ", Empty conduit with pull cord";
                    break;
                case "E":
                    item.medgas = item.medgas == null ? "Vent to atmosphere" : item.medgas + ", Vent to atmosphere";
                    break;
                case "F":
                    item.medgas = item.medgas == null ? "Special Gas Requirements" : item.medgas + ", Special Gas Requirements";
                    item.medgas_option = 1;
                    break;
                case "G":
                    item.medgas = item.medgas == null ? "Liquid Gas Requirements" : item.medgas + ", Liquid Gas Requirements";
                    item.medgas_option = 1;
                    break;
                case "H":
                    item.electrical = item.electrical == null ? "RF/Magnetic shielding" : item.electrical + ", RF/Magnetic shielding";
                    item.misc_shielding_magnetic = true;
                    break;
                case "J":
                    item.supports_option = 1;
                    item.supports = "Wall/Ceiling Support Required";
                    break;
                case "K":
                    item.supports_option = 1;
                    item.supports = "Empty conduit/pull cord & wall/ceiling support required";
                    break;
                case "M":
                    item.supports_option = 1;
                    item.supports = "Earth ground and wall/ceiling support required";
                    break;
                case "P":
                    item.supports_option = 1;
                    item.supports = "Lead lined walls and wall/ceiling support required";
                    break;
                case "T":
                    item.data_option = 1;
                    item.data = "CAT 6";
                    break;
            }

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