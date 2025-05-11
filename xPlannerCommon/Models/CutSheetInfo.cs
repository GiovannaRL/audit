namespace xPlannerCommon.Models
{
    public class CutSheetInfo
    {
        public short asset_domain_id { get; set; }
        public int asset_id { get; set; }
        public string manufacturer_description { get; set; }
        public int manufacturer_id { get; set; }
        public string asset_description { get; set; }
        public string asset_code { get; set; }
        public string serial_number { get; set; }
        public string serial_name { get; set; }
        public string photo { get; set; }
        public string comment { get; set; }
        public string height { get; set; }
        public string width { get; set; }
        public string depth { get; set; }
        public string placement { get; set; }
        public string JSN { get; set; }
        public string jsn_utility1 { get; set; }
        public string jsn_utility2 { get; set; }
        public string jsn_utility3 { get; set; }
        public string jsn_utility4 { get; set; }
        public string jsn_utility5 { get; set; }
        public string jsn_utility6 { get; set; }
        public string jsn_utility7 { get; set; }
        public int? @class { get; set; }
        public string clin { get; set; }
        public string ECN { get; set; }
        public string resp { get; set; }
        public bool has_overwritten_properties { get; set; }
        public bool has_overwritten_attributes { get; set; }
        public string filename { get; set; }
        public string cut_sheet { get; set; }
        public short photo_domain_id { get; set; }
        public string inventories_id { get; set; }
        public int? photo_rotate { get; set; }
        public string cad_id { get; set; }

        public CutSheetInfo() { }

        public CutSheetInfo(asset asset) {
            asset_domain_id = asset.domain_id;
            photo_domain_id = asset.domain_id;
            asset_id = asset.asset_id;
            manufacturer_description = asset.manufacturer?.manufacturer_description;
            manufacturer_id = asset.manufacturer_id;
            asset_description = asset.asset_description;
            asset_code = asset.asset_code;
            serial_number = asset.serial_number;
            serial_name = asset.serial_name;
            photo = asset.photo;
            comment = asset.comment;
            height = asset.height;
            width = asset.width;
            depth = asset.depth;
            placement = asset.placement;
            resp = asset.default_resp;

            if (asset.jsn != null) {
                JSN = asset.jsn.jsn_code;
                jsn_utility1 = asset.jsn.utility1;
                jsn_utility2 = asset.jsn.utility2;
                jsn_utility3 = asset.jsn.utility3;
                jsn_utility4 = asset.jsn.utility4;
                jsn_utility5 = asset.jsn.utility5;
                jsn_utility6 = asset.jsn.utility6;
                jsn_utility7 = asset.jsn.utility7;
            }

            @class = asset.@class;
            clin = null;
            ECN = null;
            has_overwritten_properties = false;
            has_overwritten_attributes = false;
            filename = asset.cut_sheet;
            cut_sheet = asset.cut_sheet;
            inventories_id = null;
            photo_rotate = asset.photo_rotate;
        }

        public bool HasAnyGovernmentInformation()
        {
            return !string.IsNullOrEmpty(JSN)
                || !string.IsNullOrEmpty(jsn_utility1)
                || !string.IsNullOrEmpty(jsn_utility2)
                || !string.IsNullOrEmpty(jsn_utility3)
                || !string.IsNullOrEmpty(jsn_utility4)
                || !string.IsNullOrEmpty(jsn_utility5)
                || !string.IsNullOrEmpty(jsn_utility6)
                || !string.IsNullOrEmpty(jsn_utility7);
        }
    }
}
