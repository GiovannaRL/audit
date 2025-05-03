CREATE VIEW [dbo].[joined_category_subcategory]
	AS SELECT 
	 CASE WHEN asub.domain_id = 1 THEN CONCAT(ac.description, ' - ', asub.description) ELSE CONCAT(ac.description, ' - ', asub.description, ' *') END AS description,
	 asub.domain_id AS subcategory_domain_id,
	 asub.category_domain_id,
	 asub.category_id,
	 asub.subcategory_id,
	 CASE WHEN asub.use_category_settings = 1 THEN ac.asset_code_domain_id ELSE asub.asset_code_domain_id END AS asset_code_domain,
	 CASE WHEN asub.use_category_settings = 1 THEN ac.asset_code ELSE asub.asset_code END AS asset_code
	 FROM assets_subcategory asub 
	 LEFT JOIN assets_category ac ON asub.category_domain_id = ac.domain_id AND asub.category_id = ac.category_id;

