CREATE PROCEDURE [dbo].[edit_assets_profile]
	@project_domain_id integer, 
	@project_id INTEGER, 
	@old_profile VARCHAR(3000), 
	@old_profile_budget VARCHAR(2000),
	@old_detailed_budget BIT,
	@inventory_id_new_profile INTEGER
AS
BEGIN
	DECLARE @new_profile VARCHAR(3000), @new_profile_budget VARCHAR(2000), @detailed_budget BIT, @asset_domain_id SMALLINT, @asset_id INT;
	DECLARE @option_id INTEGER, @domain_id SMALLINT, @quantity INT, @unit_price NUMERIC(10,2);
	DECLARE @inventories VARCHAR(5000), @options VARCHAR(5000);

	IF EXISTS(SELECT * FROM project_room_inventory WHERE domain_id = @project_domain_id AND project_id = @project_id AND inventory_id = @inventory_id_new_profile)
		BEGIN
			SELECT @detailed_budget = detailed_budget, @asset_domain_id = asset_domain_id, @asset_id = asset_id
			FROM project_room_inventory 
			WHERE domain_id = @project_domain_id AND project_id = @project_id AND inventory_id = 
				@inventory_id_new_profile;
					
			SELECT @inventories = STUFF(( SELECT distinct ';' + CAST(inventory_id AS VARCHAR(20))
					FROM project_room_inventory WHERE domain_id = @project_domain_id AND project_id = @project_id AND 
					asset_domain_id = @asset_domain_id AND asset_id = @asset_id AND asset_profile = 
					@old_profile AND detailed_budget = @old_detailed_budget AND (@old_detailed_budget 
					= 0 OR asset_profile_budget = @old_profile_budget) FOR XML PATH('')),1 ,1, '')

			update project_room_inventory set detailed_budget = @detailed_budget WHERE
				inventory_id IN (SELECT inventory_id FROM project_room_inventory WHERE domain_id = @project_domain_id 
					AND project_id = @project_id AND asset_domain_id = @asset_domain_id AND 
					asset_id = @asset_id AND asset_profile = @old_profile AND detailed_budget = 
					@old_detailed_budget AND (@old_detailed_budget = 0 OR asset_profile_budget = 
					@old_profile_budget));

			IF (@inventories IS NOT NULL)
				BEGIN
					SET @options = '';

					DECLARE io_cursor CURSOR LOCAL FOR SELECT option_id, domain_id, quantity, unit_price
					FROM inventory_options WHERE inventory_id = @inventory_id_new_profile;
					OPEN io_cursor;
					FETCH NEXT FROM io_cursor INTO @option_id, @domain_id, @quantity, @unit_price;
					WHILE @@FETCH_STATUS = 0
						BEGIN
							exec update_assets_option @domain_id, @inventories, @option_id, @quantity,
								@unit_price;

							SET @options = CONCAT(@options, ';', @option_id);

							FETCH NEXT FROM io_cursor INTO @option_id, @domain_id, @quantity, @unit_price;
						END
					CLOSE io_cursor;  
					DEALLOCATE io_cursor;

					exec delete_assets_options @inventories, @options;
				END
		END
END