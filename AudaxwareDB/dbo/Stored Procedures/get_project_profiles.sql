CREATE PROCEDURE [dbo].[get_project_profiles]
	@project_domain_id SMALLINT,
    @project_id INTEGER,
	@asset_domain_id SMALLINT,
    @asset_id INTEGER
AS
BEGIN
	SELECT CONCAT(asset_profile, CASE WHEN detailed_budget = 1 THEN CONCAT(' - ',  asset_profile_budget) ELSE '' END) AS [profile], 
		MAX(inventory_id) AS inventory_id, asset_profile AS profile_text, asset_profile_budget AS profile_budget,
		detailed_budget, COUNT(inventory_id) AS qty_assets_project
		FROM project_room_inventory
		WHERE domain_id = @project_domain_id AND project_id = @project_id AND asset_domain_id
			= @asset_domain_id AND asset_id = @asset_id AND asset_profile IS NOT NULL AND asset_profile <> '' 
		GROUP BY CONCAT(asset_profile, CASE WHEN detailed_budget = 1 THEN CONCAT(' - ',  asset_profile_budget) ELSE '' END), 
			asset_profile, asset_profile_budget, detailed_budget
		ORDER BY [profile];
END