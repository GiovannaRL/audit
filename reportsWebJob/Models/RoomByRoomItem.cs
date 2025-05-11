using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class RoomByRoomItem //: IEquatable<RoomByRoomItem>
    {
        public string resp { get; set; }
        public string asset_code { get; set; }
        public string cad_id { get; set; }
        public string jsn_code { get; set; }
        public string asset_description { get; set; }
        public string tag { get; set; }
        public string manufacturer_description { get; set; }
        public string serial_number { get; set; }
        public string serial_name { get; set; }
        public int budget_qty { get; set; }
        public int lease_qty { get; set; }
        public int dnp_qty { get; set; }
        public int po_qty { get; set; }
        public int? water_option { get; set; }
        public int? plumbing_option { get; set; }
        public int? data_option { get; set; }
        public int? electrical_option { get; set; }
        public int? mobile_option { get; set; }
        public int? blocking_option { get; set; }
        public int? medgas_option { get; set; }
        public int? supports_option { get; set; }
        public int domain_id { get; set; }
        public int project_id { get; set; }
        public int phase_id { get; set; }
        public int department_id { get; set; }
        public int room_id { get; set; }
        public string phase_description { get; set; }
        public string department_description { get; set; }
        public string drawing_room_number { get; set; }
        public string drawing_room_name { get; set; }


        //public bool Equals(RoomByRoomItem other)
        //{
        //    return this.cad_id == other.cad_id && this.water_option == other.water_option &&
        //        this.plumbing_option == other.plumbing_option && this.data_option == other.data_option
        //        && this.electrical_option == other.electrical_option && this.mobile_option == other.mobile_option
        //        && this.blocking_option == other.blocking_option && this.medgas_option == other.medgas_option
        //        && this.supports_option == other.supports_option && this.resp == other.resp && this.asset_code
        //        == other.asset_code && this.asset_description == other.asset_description && this.budget_qty ==
        //        other.budget_qty && this.lease_qty == other.lease_qty && this.dnp_qty == other.dnp_qty &&
        //        this.po_qty == other.po_qty && this.tag == other.tag && this.manufacturer_description ==
        //        other.manufacturer_description && this.serial_number == other.serial_number && this.serial_name
        //        == other.serial_name;
        //}

        //public override int GetHashCode()
        //{
        //    int hashCadId = this.cad_id == null ? 0 : this.cad_id.GetHashCode();
        //    int hashWater = this.water_option == null ? 0 : this.water_option.GetHashCode();
        //    int hashPlumbing = this.plumbing_option == null ? 0 : this.plumbing_option.GetHashCode();
        //    int hashData = this.data_option == null ? 0 : this.data_option.GetHashCode();
        //    int hashElectrical = this.electrical_option == null ? 0 : this.electrical_option.GetHashCode();
        //    int hashMobile = this.mobile_option == null ? 0 : this.mobile_option.GetHashCode();
        //    int hashBlocking = this.blocking_option == null ? 0 : this.blocking_option.GetHashCode();
        //    int hashMedgas = this.medgas_option == null ? 0 : this.medgas_option.GetHashCode();
        //    int hashSupports = this.supports_option == null ? 0 : this.supports_option.GetHashCode();
        //    int hashResp = this.resp == null ? 0 : this.resp.GetHashCode();
        //    int hashAssetCode = this.asset_code == null ? 0 : this.asset_code.GetHashCode();
        //    int hashAssetDesc = this.asset_description == null ? 0 : this.asset_description.GetHashCode();
        //    int hashBudget = this.budget_qty.GetHashCode();
        //    int hashLease = this.lease_qty.GetHashCode();
        //    int hashDNP = this.dnp_qty.GetHashCode();
        //    int hashPO = this.po_qty.GetHashCode();
        //    int hashTag = this.tag == null ? 0 : this.tag.GetHashCode();
        //    int hashManufacturer = this.manufacturer_description == null ? 0 : this.manufacturer_description.GetHashCode();
        //    int hashSerialNumber = this.serial_number == null ? 0 : this.serial_number.GetHashCode();
        //    int hashSerialName = this.serial_name == null ? 0 : this.serial_name.GetHashCode();

        //    return hashCadId ^ hashWater ^ hashPlumbing ^ hashData ^ hashElectrical ^ hashMobile ^ hashBlocking
        //        ^ hashMedgas ^ hashSupports ^ hashResp ^ hashAssetCode ^ hashAssetDesc ^ hashBudget ^ hashLease
        //        ^ hashDNP ^ hashPO ^ hashTag ^ hashManufacturer ^ hashSerialNumber ^ hashSerialNumber;
        //}
    }
}
