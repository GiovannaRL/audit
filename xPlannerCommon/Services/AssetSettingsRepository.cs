using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerCommon.Models;

namespace xPlannerCommon.Services
{
    public class AssetSettingsRepository : IDisposable
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public AssetSettingsRepository()
        {
            _db = new audaxwareEntities();
        }

        public AssetSettingsRepository(short domainId)
        {
            _db = new audaxwareEntities();
            using (var cmd = _db.Database.Connection.CreateCommand())
            {
                _db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '" + domainId + "';";
                cmd.ExecuteNonQuery();
            }
        }


        public IEnumerable<AssetSettingsStructure> GetNotDisabledJSN(asset asset, bool? includePlacement = false)
        {
            IEnumerable<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            var values = GetCategorySubcategoryValues(asset.assets_subcategory);

            settings = !values.Gases.Equals("D") ? settings.Concat(GetGasesSettings(asset, values.Gases.Equals("R"))) : settings;
            settings = !values.Plumbing.Equals("D") ? settings.Concat(GetPlumbingSettings(asset, values.Plumbing.Equals("R"))) : settings;
            settings = !values.HVAC.Equals("D") ? settings.Concat(GetHVACSettings(asset, values.HVAC.Equals("R"))) : settings;
            settings = !values.IT.Equals("D") ? settings.Concat(GetITSettings(asset, values.IT.Equals("R"))) : settings;
            settings = !values.Electrical.Equals("D") ? settings.Concat(GetElectricalSettings(asset, values.Electrical.Equals("R"))) : settings;
            settings = !values.Support.Equals("D") ? settings.Concat(GetSupportSettings(asset, values.Support.Equals("R"))) : settings;
            settings = !values.Physical.Equals("D") ? settings.Concat(GetPhysicalSettings(asset, values.Physical.Equals("R"), includePlacement)) : settings;
            settings = !values.Environmental.Equals("D") ? settings.Concat(GetEnvironmentalSettings(asset, values.Environmental.Equals("R"))) : settings;

            return settings;
        }

        public IEnumerable<AssetSettingsStructure> GetSettings(int assetDomainId, int assetId, bool? includePlacement = false)
        {
            IEnumerable<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            var asset = _db.assets.Include("assets_subcategory.assets_category").FirstOrDefault(a => a.domain_id == assetDomainId && a.asset_id == assetId);
            var values = GetCategorySubcategoryValues(asset?.assets_subcategory);

            settings = !values.Gases.Equals("D") ? settings.Concat(GetGasesSettings(asset, values.Gases.Equals("R"))) : settings;
            settings = !values.Plumbing.Equals("D") ? settings.Concat(GetPlumbingSettings(asset, values.Plumbing.Equals("R"))) : settings;
            settings = !values.HVAC.Equals("D") ? settings.Concat(GetHVACSettings(asset, values.HVAC.Equals("R"))) : settings;
            settings = !values.IT.Equals("D") ? settings.Concat(GetITSettings(asset, values.IT.Equals("R"))) : settings;
            settings = !values.Electrical.Equals("D") ? settings.Concat(GetElectricalSettings(asset, values.Electrical.Equals("R"))) : settings;
            settings = !values.Support.Equals("D") ? settings.Concat(GetSupportSettings(asset, values.Support.Equals("R"))) : settings;
            settings = !values.Physical.Equals("D") ? settings.Concat(GetPhysicalSettings(asset, values.Physical.Equals("R"), includePlacement)) : settings;
            settings = !values.Environmental.Equals("D") ? settings.Concat(GetEnvironmentalSettings(asset, values.Environmental.Equals("R"))) : settings;

            return settings;
        }

        
        public IEnumerable<AssetSettingsStructure> GetInventorySettings(int assetDomainId, int inventoryId)
        {
            IEnumerable<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            var assetInventory = _db.project_room_inventory.FirstOrDefault(a => a.domain_id == assetDomainId && a.inventory_id == inventoryId);
            var asset = _db.assets.Include("assets_subcategory.assets_category").FirstOrDefault(a => a.asset_id == assetInventory.asset_id);                    


            var values = GetCategorySubcategoryValues(asset?.assets_subcategory);

            settings = !values.Physical.Equals("D") ? settings.Concat(GetPhysicalInventorySettings(assetInventory, asset, values.Physical.Equals("R"))) : settings;
            settings = !values.Electrical.Equals("D") ? settings.Concat(GetElectricalInventorySettings(assetInventory, asset, values.Electrical.Equals("R"))) : settings;
            settings = !values.Gases.Equals("D") ? settings.Concat(GetGasesInventorySettings(assetInventory, asset, values.Gases.Equals("R"))) : settings;
            settings = !values.IT.Equals("D") ? settings.Concat(GetItInventorySettings(assetInventory, asset, values.IT.Equals("R"))) : settings;
            return settings;
        }


        private CategorySettingsValue GetCategorySubcategoryValues(assets_subcategory sub)
        {
            if (sub == null)
                return null;

            if (sub.use_category_settings)
            {
                var item = sub.assets_category;
                return new CategorySettingsValue(item.Gases, item.Plumbing, item.HVAC, item.IT,
                    item.Electrical, item.Support, item.Physical, item.Environmental);
            }

            return new CategorySettingsValue(sub.Gases, sub.Plumbing, sub.HVAC, sub.IT, sub.Electrical,
                sub.Support, sub.Physical, sub.Environmental);
        }

        private IEnumerable<AssetSettingsStructure> GetGasesSettings(asset asset, bool required)
        {
            var settings = new List<AssetSettingsStructure>
            {
                new AssetSettingsStructure("Gases", "CO2", asset.medgas_co2.ToString(), "medgas_co2", "checkbox",
                    required),
                new AssetSettingsStructure("Gases", "Air Low Pressure", asset.medgas_other.ToString(), "medgas_other",
                    "checkbox", required),
                new AssetSettingsStructure("Gases", "Air High Pressure", asset.medgas_high_pressure.ToString(),
                    "medgas_high_pressure", "checkbox", required),
                new AssetSettingsStructure("Gases", "Gases Description", asset.medgas, "medgas", "text", required),
                new AssetSettingsStructure("Gases", "Gases Required", asset.medgas_option.ToString(), "medgas_option",
                    "dropDown", required),
                new AssetSettingsStructure("Gases", "MedAir", asset.medgas_air.ToString(), "medgas_air", "checkbox",
                    required),
                new AssetSettingsStructure("Gases", "N2O", asset.medgas_n2o.ToString(), "medgas_n2o", "checkbox",
                    required),
                new AssetSettingsStructure("Gases", "NatGas", asset.medgas_natgas.ToString(), "medgas_natgas",
                    "checkbox", required),
                new AssetSettingsStructure("Gases", "Nitrogen", asset.medgas_nitrogen.ToString(), "medgas_nitrogen",
                    "checkbox", required),
                new AssetSettingsStructure("Gases", "Oxygen", asset.medgas_oxygen.ToString(), "medgas_oxygen",
                    "checkbox", required),
                new AssetSettingsStructure("Gases", "Steam", asset.medgas_steam.ToString(), "medgas_steam", "checkbox",
                    required),
                new AssetSettingsStructure("Gases", "Vacuum", asset.medgas_vacuum.ToString(), "medgas_vacuum",
                    "checkbox", required),
                new AssetSettingsStructure("Gases", "WAG", asset.medgas_wag.ToString(), "medgas_wag", "checkbox",
                    required),
                new AssetSettingsStructure("Gases", "Acetylene", asset.gas_acetylene.ToString(), "gas_acetylene",
                    "checkbox", required),
                new AssetSettingsStructure("Gases", "Butane", asset.gas_butane.ToString(), "gas_butane", "checkbox",
                    required),
                new AssetSettingsStructure("Gases", "Hydrogen", asset.gas_hydrogen.ToString(), "gas_hydrogen",
                    "checkbox", required),
                new AssetSettingsStructure("Gases", "Instrument Air", asset.gas_instrument_air.ToString(),
                    "gas_instrument_air", "checkbox", required),
                new AssetSettingsStructure("Gases", "Liquid CO2", asset.gas_liquid_co2.ToString(), "gas_liquid_co2",
                    "checkbox", required),
                new AssetSettingsStructure("Gases", "Liquid Nitrogen", asset.gas_liquid_nitrogen.ToString(),
                    "gas_liquid_nitrogen", "checkbox", required),
                new AssetSettingsStructure("Gases", "Liquid Propane Gas", asset.gas_liquid_propane_gas.ToString(),
                    "gas_liquid_propane_gas", "checkbox", required),
                new AssetSettingsStructure("Gases", "Methane", asset.gas_methane.ToString(), "gas_methane", "checkbox",
                    required),
                new AssetSettingsStructure("Gases", "Propane", asset.gas_propane.ToString(), "gas_propane", "checkbox",
                    required),
                new AssetSettingsStructure("Gases", "Connection Type", asset.connection_type, "connection_type", "dropDown",
                    required)
        };
            return settings;
        }

        private static IEnumerable<AssetSettingsStructure> GetEnvironmentalSettings(asset asset, bool required)
        {
            var settings = new List<AssetSettingsStructure>
            {
                new AssetSettingsStructure("Environmental", "Antimicrobial", asset.misc_antimicrobial.ToString(),
                    "misc_antimicrobial", "checkbox", required),
                new AssetSettingsStructure("Environmental", "Eco-Label", asset.misc_ecolabel.ToString(),
                    "misc_ecolabel", "checkbox", required),
                new AssetSettingsStructure("Environmental", "Eco-Label Description", asset.misc_ecolabel_desc,
                    "misc_ecolabel_desc", "text", required)
            };


            return settings;
        }

        private static IEnumerable<AssetSettingsStructure> GetPlumbingSettings(asset asset, bool required)
        {
            var settings = new List<AssetSettingsStructure>
            {
                new AssetSettingsStructure("Plumbing", "Chilled Water", asset.plu_chilled_water.ToString(),
                    "plu_chilled_water", "checkbox", required),
                new AssetSettingsStructure("Plumbing", "Cold Water", asset.plu_cold_water.ToString(), "plu_cold_water",
                    "checkbox", required),
                new AssetSettingsStructure("Plumbing", "Drain", asset.plu_drain.ToString(), "plu_drain", "checkbox",
                    required),
                new AssetSettingsStructure("Plumbing", "Hot Water", asset.plu_hot_water.ToString(), "plu_hot_water",
                    "checkbox", required),
                new AssetSettingsStructure("Plumbing", "Plumbing Required", asset.plumbing_option.ToString(),
                    "plumbing_option", "dropDown", required),
                new AssetSettingsStructure("Plumbing", "Plumbing Description", asset.plumbing, "plumbing", "text",
                    required),
                new AssetSettingsStructure("Plumbing", "Relief", asset.plu_relief.ToString(), "plu_relief", "checkbox",
                    required),
                new AssetSettingsStructure("Plumbing", "Return", asset.plu_return.ToString(), "plu_return", "checkbox",
                    required),
                new AssetSettingsStructure("Plumbing", "Treated Water", asset.plu_treated_water.ToString(),
                    "plu_treated_water", "checkbox", required)
            };


            return settings;
        }

        private List<AssetSettingsStructure> GetHVACSettings(asset asset, bool required)
        {
            List<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            settings.Add(new AssetSettingsStructure("HVAC", "BTUs", asset.btus, "btus", "text", required));
            settings.Add(new AssetSettingsStructure("HVAC", "CFM", asset.cfm, "cfm", "text", required));
            settings.Add(new AssetSettingsStructure("HVAC", "Exhaust Description", asset.water, "water", "text", required));
            settings.Add(new AssetSettingsStructure("HVAC", "Exhaust Required", asset.water_option.ToString(), "water_option", "dropDown", required));

            return settings;
        }

        private List<AssetSettingsStructure> GetITSettings(asset asset, bool required)
        {
            List<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            settings.Add(new AssetSettingsStructure("IT", "Data Description", asset.data, "data", "text", required));
            settings.Add(new AssetSettingsStructure("IT", "Data Required", asset.data_option.ToString(), "data_option", "dropDown", required));
            settings.Add(new AssetSettingsStructure("IT", "Network Required", asset.network_option.ToString(), "network_option", "dropDown", required));
            //settings.Add(new AssetSettingsStructure("IT", "Network Type", asset.network_type, "network_type", "dropDown", required));
            settings.Add(new AssetSettingsStructure("IT", "Number of Ports", asset.ports.ToString(), "ports", "numeric", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, Bluetooth", asset.bluetooth.ToString(), "bluetooth", "checkbox", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, Cat6", asset.cat6.ToString(), "cat6", "checkbox", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, Displayport", asset.displayport.ToString(), "displayport", "checkbox", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, DVI", asset.dvi.ToString(), "dvi", "checkbox", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, HDMI", asset.hdmi.ToString(), "hdmi", "checkbox", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, Wireless", asset.wireless.ToString(), "wireless", "checkbox", required));

            return settings;
        }

        private List<AssetSettingsStructure> GetElectricalSettings(asset asset, bool required)
        {
            List<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            settings.Add(new AssetSettingsStructure("Electrical", "Amps", asset.amps, "amps", "numeric", required));
            settings.Add(new AssetSettingsStructure("Electrical", "Electrical Description", asset.electrical, "electrical", "text", required));
            settings.Add(new AssetSettingsStructure("Electrical", "Electrical Required", asset.electrical_option.ToString(), "electrical_option", "dropDown", required));
            settings.Add(new AssetSettingsStructure("Electrical", "Hertz", asset.hertz, "hertz", "dropDown", required));
            settings.Add(new AssetSettingsStructure("Electrical", "VoltAmps", asset.volt_amps.ToString(), "volt_amps", "numeric", required));
            settings.Add(new AssetSettingsStructure("Electrical", "Volts", asset.volts, "volts", "regularExpression", required));
            settings.Add(new AssetSettingsStructure("Electrical", "Watts", asset.watts, "watts", "numeric", required));
            settings.Add(new AssetSettingsStructure("Electrical", "Phases", asset.phases.ToString(), "phases", "dropDown", required));
            settings.Add(new AssetSettingsStructure("Electrical", "Plug Type", asset.plug_type, "plug_type", "dropDown", required));

            return settings;
        }

        private List<AssetSettingsStructure> GetSupportSettings(asset asset, bool required)
        {
            List<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            settings.Add(new AssetSettingsStructure("Support", "Blocking Description", asset.blocking, "blocking", "text", required));
            settings.Add(new AssetSettingsStructure("Support", "Blocking Required", asset.blocking_option.ToString(), "blocking_option", "dropDown", required));
            settings.Add(new AssetSettingsStructure("Support", "Seismic Rated", asset.misc_seismic.ToString(), "misc_seismic", "checkbox", required));
            settings.Add(new AssetSettingsStructure("Support", "Structural Description", asset.supports, "supports", "text", required));
            settings.Add(new AssetSettingsStructure("Support", "Structural Required", asset.supports_option.ToString(), "supports_option", "dropDown", required));
            

            return settings;
        }

        private List<AssetSettingsStructure> GetPhysicalSettings(asset asset, bool required, bool? includePlacement = false)
        {
            List<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            settings.Add(new AssetSettingsStructure("Physical", "Category", asset.category_attribute, "category_attribute", "dropDown", required));
            if (includePlacement == true)
            {
                settings.Add(new AssetSettingsStructure("Physical", "Placement", asset.placement == "OE" ? "Other Equipment" : asset.placement == "UC" ? "Under-Counter" : asset.placement == "OnCart" ? "On Cart" : asset.placement, "placement", "text"));
            }
            settings.Add(new AssetSettingsStructure("Physical", "ADA", asset.misc_ada.ToString(), "misc_ada", "checkbox", required));
            settings.Add(new AssetSettingsStructure("Physical", "ASE", asset.misc_ase.ToString(), "misc_ase", "checkbox", required));
            settings.Add(new AssetSettingsStructure("Physical", "Shielding, Lead Lined", asset.misc_shielding_lead_line.ToString(), "misc_shielding_lead_line", "checkbox", required));
            settings.Add(new AssetSettingsStructure("Physical", "Shielding, RF/Magnetic", asset.misc_shielding_magnetic.ToString(), "misc_shielding_magnetic", "checkbox", required));
            settings.Add(new AssetSettingsStructure("Physical", "Clearance, Back(in)", asset.clearance_back.ToString(), "clearance_back", "numeric", required));
            settings.Add(new AssetSettingsStructure("Physical", "Clearance, Bottom(in)", asset.clearance_bottom.ToString(), "clearance_bottom", "numeric", required));
            settings.Add(new AssetSettingsStructure("Physical", "Clearance, Front(in)", asset.clearance_front.ToString(), "clearance_front", "numeric", required));
            settings.Add(new AssetSettingsStructure("Physical", "Clearance, Left(in)", asset.clearance_left.ToString(), "clearance_left", "numeric", required));
            settings.Add(new AssetSettingsStructure("Physical", "Clearance, Right(in)", asset.clearance_right.ToString(), "clearance_right", "numeric", required));
            settings.Add(new AssetSettingsStructure("Physical", "Clearance, Top(in)", asset.clearance_top.ToString(), "clearance_top", "numeric", required));
            settings.Add(new AssetSettingsStructure("Physical", "Dimension, Depth(in)", asset.depth, "depth", "regularExpression", required));
            settings.Add(new AssetSettingsStructure("Physical", "Dimension, Height(in)", asset.height, "height", "regularExpression", required));
            settings.Add(new AssetSettingsStructure("Physical", "Dimension, Width(in)", asset.width, "width", "regularExpression", required));
            settings.Add(new AssetSettingsStructure("Physical", "Dimension, Mounting Height(in)", asset.mounting_height, "mounting_height", "regularExpression", required));
            settings.Add(new AssetSettingsStructure("Physical", "Mobile", asset.mobile_option.ToString(), "mobile_option", "dropDown", required));
            settings.Add(new AssetSettingsStructure("Physical", "Mobile Description", asset.mobile, "mobile", "text", required));
            settings.Add(new AssetSettingsStructure("Physical", "Weight, Loaded(lb)", asset.loaded_weight.ToString(), "loaded_weight", "numeric", required));
            settings.Add(new AssetSettingsStructure("Physical", "Weight, Shipping(lb)", asset.ship_weight.ToString(), "ship_weight", "numeric", required));
            settings.Add(new AssetSettingsStructure("Physical", "Weight(lb)", asset.weight, "weight", "regularExpression", required));
            
            

            return settings;
        }

        private List<AssetSettingsStructure> GetPhysicalInventorySettings(project_room_inventory assetInventory,asset asset, bool required, bool? includePlacement = false)
        {
            List<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();


            settings.Add(new AssetSettingsStructure("Physical", "Dimension, Depth(in)", assetInventory.depth, "depth", "regularExpression", required));
            settings.Add(new AssetSettingsStructure("Physical", "Dimension, Height(in)", assetInventory.height, "height", "regularExpression", required));
            settings.Add(new AssetSettingsStructure("Physical", "Dimension, Width(in)", assetInventory.width, "width", "regularExpression", required));
            settings.Add(new AssetSettingsStructure("Physical", "Dimension, Mounting Height(in)", assetInventory.mounting_height, "mounting_height", "regularExpression", required));



            return settings;
        }


        private List<AssetSettingsStructure> GetElectricalInventorySettings(project_room_inventory assetInventory, asset asset, bool required, bool? includePlacement = false)
        {
            List<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            settings.Add(new AssetSettingsStructure("Electrical", "Plug Type", assetInventory.plug_type, "plug_type", "dropDown", required));
            settings.Add(new AssetSettingsStructure("Electrical", "Volts", assetInventory.volts, "volts", "regularExpression", required));
            settings.Add(new AssetSettingsStructure("Electrical", "Amps", assetInventory.amps, "amps", "regularExpression", required));

            return settings;
        }

        private List<AssetSettingsStructure> GetGasesInventorySettings(project_room_inventory assetInventory, asset asset, bool required, bool? includePlacement = false)
        {
            List<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            settings.Add(new AssetSettingsStructure("Gases", "Connection Type", assetInventory.connection_type, "connection_type", "dropDown", required));

            return settings;
        }

        private List<AssetSettingsStructure> GetItInventorySettings(project_room_inventory assetInventory, asset asset, bool required, bool? includePlacement = false)
        {
            List<AssetSettingsStructure> settings = new List<AssetSettingsStructure>();

            settings.Add(new AssetSettingsStructure("IT", "Network Required", assetInventory.network_option.ToString(), "network_option", "dropDown", required));
            //settings.Add(new AssetSettingsStructure("IT", "Network Type", assetInventory.network_type, "network_type", "dropDown", required));
            settings.Add(new AssetSettingsStructure("IT", "Number of Ports", assetInventory.ports.ToString(), "ports", "numeric", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, Bluetooth", assetInventory.bluetooth.ToString(), "bluetooth", "checkbox", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, Cat6", assetInventory.cat6.ToString(), "cat6", "checkbox", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, Displayport", assetInventory.displayport.ToString(), "displayport", "checkbox", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, DVI", assetInventory.dvi.ToString(), "dvi", "checkbox", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, HDMI", assetInventory.hdmi.ToString(), "hdmi", "checkbox", required));
            settings.Add(new AssetSettingsStructure("IT", "Connection, Wireless", assetInventory.dvi.ToString(), "dvi", "checkbox", required));
            return settings;
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