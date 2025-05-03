CREATE PROCEDURE [dbo].[get_global_profiles_not_project]
	@project_domain_id SMALLINT,
    @project_id INTEGER,
	@asset_domain_id SMALLINT,
    @asset_id INTEGER
AS
BEGIN
	SELECT [profile] AS profile_text, null AS profile_budget, 0 AS qty_assets_project, CAST(0 AS BIT) AS detailed_budget, profile_id 
	FROM [profile] WHERE asset_domain_id = @asset_domain_id AND asset_id = @asset_id
		AND [profile] NOT IN (SELECT asset_profile FROM project_room_inventory WHERE domain_id =
		@project_domain_id AND project_id = @project_id AND asset_domain_id = @asset_domain_id AND 
		asset_id = @asset_id AND detailed_budget = 0);
END