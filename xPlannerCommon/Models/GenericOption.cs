using System;
namespace xPlannerCommon.Models
{
    public class GenericOption
    {
        public string inventories_id { get; set; }
        public int option_id { get; set; }
        public short domain_id { get; set; }
        public Nullable<decimal> unit_price { get; set; }
        public int quantity { get; set; }
        public Nullable<short> inventory_domain_id { get; set; }
        public Nullable<short> document_domain_id { get; set; }
        public Nullable<int> document_id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public string data_type { get; set; }
        public Nullable<decimal> min_cost { get; set; }
        public Nullable<decimal> max_cost { get; set; }
        public string settings { get; set; }
        public FileData picture { get; set; }
        public short asset_domain_id { get; set; }
        public int asset_id { get; set; }
        public Nullable<short> project_domain_id { get; set; }
        public Nullable<int> project_id { get; set; }
        public Nullable<int> scope { get; set; }

        public assets_options ToAssetOption()
        {
            var option = this.ToAssetOptionWithoutInventoryInfo();
            option.unit_budget = unit_price;

            return option;
        }

        public assets_options MergeInfo(assets_options oldOption)
        {
            oldOption.asset_option_id = option_id;
            oldOption.domain_id = domain_id;
            oldOption.code = code;
            oldOption.description = description;
            oldOption.data_type = data_type;
            oldOption.min_cost = min_cost;
            oldOption.max_cost = max_cost;
            oldOption.settings = settings;
            oldOption.asset_domain_id = asset_domain_id;
            oldOption.asset_id = asset_id;
            oldOption.document_domain_id = document_domain_id;
            oldOption.document_id = document_id;
            oldOption.project_domain_id = project_domain_id;
            oldOption.project_id = project_id;
            oldOption.scope = scope;

            return oldOption;
        }

        public assets_options ToAssetOptionWithoutInventoryInfo()
        {
            return new assets_options
            {
                asset_option_id = option_id,
                domain_id = domain_id,
                code = code,
                description = description,
                data_type = data_type,
                min_cost = min_cost,
                max_cost = max_cost,
                settings = settings,
                asset_domain_id = asset_domain_id,
                asset_id = asset_id,
                document_domain_id = document_domain_id,
                document_id = document_id,
                project_domain_id = project_domain_id,
                project_id = project_id,
                scope = scope
            };
        }
    }
}
