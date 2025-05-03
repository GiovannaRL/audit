CREATE PROCEDURE [dbo].[copy_options](
	@from_inventory_id INTEGER,
    @to_inventory_id INTEGER,
	@to_domain_id INTEGER,
	@options_unit_price numeric(10,2) = 0, 
	@asset_profile varchar(3000) = null, 
	@asset_profile_budget varchar(2000) = null,
	@erase_pre_existing_data bit = 0)
AS
BEGIN

	
	DECLARE @option_ids varchar(1000);
	DECLARE @project_id_to INT;
	DECLARE @newAssetOptions TABLE
	(
	  id int,
	  domain_id int,
	  origin_id int
	)

	IF @erase_pre_existing_data = 1 BEGIN
		DELETE FROM inventory_options where inventory_id = @to_inventory_id and inventory_domain_id = @to_domain_id;
	END

	-- Get target project
	SELECT @project_id_to = pri.project_id
	FROM project_room_inventory pri
		INNER JOIN project p ON pri.project_id = p.project_id
	WHERE pri.inventory_id = @to_inventory_id;
	
	-- This merge will create new options on the target project 
	-- in the case of project specific options
	MERGE INTO assets_options USING (
		SELECT
			o.asset_option_id, o.unit_budget, o.settings, scope, min_cost, max_cost, last_cost, 
			o.document_id, o.document_domain_id, display_code, description, date_added, 
			data_type, code, avg_cost, asset_id, asset_domain_id
		FROM assets_options o
			INNER JOIN inventory_options i_o ON o.asset_option_id = i_o.option_id
		WHERE i_o.inventory_id = @from_inventory_id 
			AND o.scope = 2
			AND o.project_id IS NOT NULL AND o.project_id <> @project_id_to
			--Check if it project specific and if project is different
	) as asset_data ON 1 = 0 -- Merge will never match. Why merge? more on that below
	WHEN NOT MATCHED THEN
		INSERT (unit_budget,
				settings,
				scope,
				project_id,
				project_domain_id,
				min_cost,
				max_cost,
				last_cost,
				domain_id,
				document_id,
				document_domain_id,
				display_code,
				description,
				date_added,
				data_type,
				code,
				avg_cost,
				asset_id,
				asset_domain_id,
				added_by
		)
		VALUES (asset_data.unit_budget, asset_data.settings, asset_data.scope, @project_id_to, @to_domain_id, asset_data.min_cost, asset_data.max_cost, asset_data.last_cost, 
		   @to_domain_id, asset_data.document_id, asset_data.document_domain_id, asset_data.display_code, asset_data.description, GETDATE(), asset_data.data_type, asset_data.code,
		   asset_data.avg_cost, asset_data.asset_id, asset_data.asset_domain_id, 'copy_options' )
		-- OUTPUT to map the inserted IDs with the original one. This is why MERGE:
		-- The INSERT INTO SELECT statement would not allow to OUTPUT a property that was not inserted
		-- which is the case of asset_data.asset_option_id
		OUTPUT inserted.asset_option_id, inserted.domain_id, asset_data.asset_option_id INTO @newAssetOptions (id, domain_id, origin_id);

	INSERT INTO inventory_options(inventory_id, option_id, domain_id, unit_price, quantity, inventory_domain_id) 
	SELECT @to_inventory_id, 
		   COALESCE(newAssets.id, i.option_id), 
		   COALESCE(newAssets.domain_id, i.domain_id),
		   i.unit_price, 
		   i.quantity, 
		   @to_domain_id  
	   FROM inventory_options i
		INNER JOIN assets_options o ON i.option_id = o.asset_option_id
		LEFT JOIN @newAssetOptions newAssets ON newAssets.origin_id = o.asset_option_id
		-- We need to use the new asset IDs in case a new one was created
		WHERE i.inventory_id = @from_inventory_id; 

	select @option_ids = STRING_AGG(option_id, ',') from inventory_options where inventory_id = @to_inventory_id and inventory_domain_id = @to_domain_id

	-- This if was added because when using the copy functions, even if this is empty, we would generate an update
	-- that would trigger an update link. Because this is used only on copies for insert, it should not be a problem
	--  not to call set in case the field is empty
	IF Len(@option_ids) > 0
	BEGIN
		UPDATE project_room_inventory set option_ids = @option_ids, 
		options_unit_price = @options_unit_price, asset_profile = @asset_profile, asset_profile_budget = @asset_profile_budget
		WHERE inventory_id = @to_inventory_id and domain_id = @to_domain_id;
	END
END