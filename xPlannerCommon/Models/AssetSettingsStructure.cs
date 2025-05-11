using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xPlannerCommon.Models
{
    public class AssetSettingsStructure
    {
        public string group_name { get; set; }
        public string property_name { get; set; }
        public string value { get; set; }
        public string asset_field { get; set; }
        public string editor_type { get; set; }
        public bool required { get; set; }

        public AssetSettingsStructure(string group_name, string property_name, string value, string asset_field, string type_editor, bool required = false)
        {
            this.group_name = group_name;
            this.property_name = property_name;
            this.value = value;
            this.asset_field = asset_field;
            this.editor_type = type_editor;
            this.required = required;
        }
    }
}
