CREATE PROCEDURE [dbo].[add_asset_option]
	@domain_id SMALLINT,
	@code VARCHAR(50),
	@data_type VARCHAR(2),
	@description VARCHAR(270),
	@min_cost NUMERIC(10, 2),
	@max_cost NUMERIC(10, 2),
	@unit_budget NUMERIC(10, 2),
	@asset_domain_id SMALLINT,
	@asset_id INT,
	@added_by VARCHAR(50),
	@settings TEXT,
	@scope INT,
	@project_domain_id SMALLINT = NULL,
	@project_id INT = NULL
AS
	
	INSERT INTO assets_options(domain_id, code, data_type, description, min_cost, max_cost, 
		unit_budget, asset_domain_id, asset_id, project_domain_id, project_id, added_by, date_added,
		settings, scope)
	VALUES(@domain_id, @code, @data_type, @description, @min_cost, @max_cost, @unit_budget, 
		@asset_domain_id, @asset_id, @project_domain_id, @project_id, @added_by, GETDATE(), @settings, @scope);

	SELECT TOP 1 CAST(SCOPE_IDENTITY() AS INTEGER) AS option_id;

RETURN 0
