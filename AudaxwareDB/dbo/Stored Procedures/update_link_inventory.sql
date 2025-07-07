
CREATE PROCEDURE [dbo].[update_link_inventory](
	@domain_id SMALLINT,
	@from_id INTEGER)
AS
BEGIN
	DECLARE @to_id INTEGER
	DECLARE @inventory_id INTEGER, @inventory_target_id INTEGER, @asset_domain_id SMALLINT, @asset_id INTEGER, @budget_qty INTEGER,
		@asset_description_ow BIT, @asset_description VARCHAR(300), @clin VARCHAR(50), @ECN  VARCHAR (50), @placement_ow BIT, @placement VARCHAR(20), @biomed_check_required BIT, 
		@height_ow BIT, @height VARCHAR(25), @width_ow BIT, @width VARCHAR(25), @depth_ow BIT, @depth VARCHAR(25),
		@mounting_height_ow BIT, @mounting_height VARCHAR(25), @class_ow BIT, @class INT,
		@jsn_code VARCHAR(10), 	@jsn_utility1 VARCHAR(10), @jsn_utility2 VARCHAR(10), @jsn_utility3 VARCHAR(10), @jsn_utility4 VARCHAR(10), 
		@jsn_utility5 VARCHAR(10), @jsn_utility6 VARCHAR(10), @jsn_utility7 VARCHAR(10), @jsn_ow BIT,
        @manufacturer_description VARCHAR (100), @manufacturer_description_ow BIT,
		@model_number VARCHAR (100), @model_number_ow BIT, @model_name  VARCHAR (150),
		@model_name_ow BIT, @lan INT, @lan_ow BIT, @network_type VARCHAR(20), @network_type_ow BIT;

	SELECT @inventory_id = [inventory_id], @inventory_target_id = [inventory_target_id], @asset_domain_id = [asset_domain_id], @asset_id = [asset_id], @budget_qty = [budget_qty],
		@asset_description_ow = [asset_description_ow], @asset_description = [asset_description], @clin = [clin], @ECN = [ECN], @placement_ow = [placement_ow], @placement = [placement], @biomed_check_required = [biomed_check_required],
		@height_ow = [height_ow], @height = [height], @width_ow = [width_ow], @width = [width], @depth_ow = [depth_ow], @depth = [depth],
		@mounting_height_ow = [mounting_height_ow], @mounting_height = [mounting_height], @class_ow = [class_ow], @class = [class],
		@jsn_code = [jsn_code], 	@jsn_utility1 = [jsn_utility1], @jsn_utility2 = [jsn_utility2], @jsn_utility3 = [jsn_utility3], @jsn_utility4 = [jsn_utility4], 
		@jsn_utility5 = [jsn_utility5], @jsn_utility6 = [jsn_utility6], @jsn_utility7 = [jsn_utility7], @jsn_ow = [jsn_ow],
		@manufacturer_description = manufacturer_description, @manufacturer_description_ow = manufacturer_description_ow, @model_number = model_number,
		@model_number_ow = model_number_ow, @model_name = model_name, @model_name_ow = model_name_ow, @lan = lan, @lan_ow = lan_ow, @network_type = network_type, @network_type_ow = network_type_ow
			 from project_room_inventory where domain_id = @domain_id AND inventory_id = @from_id

	DECLARE linked_cursor CURSOR LOCAL FOR SELECT inventory_id	from project_room_inventory WHERE
		inventory_source_id = @from_id OR inventory_target_id = @from_id OR inventory_id = @inventory_target_id;

	OPEN linked_cursor;
	FETCH NEXT FROM linked_cursor INTO @to_id
	WHILE @@FETCH_STATUS = 0
		BEGIN

		UPDATE project_room_inventory SET 
			[asset_domain_id] = @asset_domain_id, [asset_id] = @asset_id, [budget_qty] = @budget_qty,
			[asset_description_ow] = @asset_description_ow, [asset_description] = @asset_description, [clin] = @clin, [ECN] = @ECN, [placement_ow] = @placement_ow, [placement] = @placement, [biomed_check_required] = @biomed_check_required,
			[height_ow] = @height_ow, [height] = @height, [width_ow] = @width_ow, [width] = @width, [depth_ow] = @depth_ow, [depth] = @depth,
			[mounting_height_ow] = @mounting_height_ow, [mounting_height] = @mounting_height, [class_ow] = @class_ow, [class] = @class,
			[jsn_code] = @jsn_code, 	[jsn_utility1] = @jsn_utility1, [jsn_utility2] = @jsn_utility2, [jsn_utility3] = @jsn_utility3, [jsn_utility4] = @jsn_utility4, 
			[jsn_utility5] = @jsn_utility5, [jsn_utility6] = @jsn_utility6, [jsn_utility7] = @jsn_utility7, [jsn_ow]  = @jsn_ow,
			manufacturer_description = @manufacturer_description,
			manufacturer_description_ow = @manufacturer_description_ow	,
			model_number = @model_number	,
			model_number_ow = @model_number_ow	,
			model_name = @model_name	,
			model_name_ow = @model_name_ow	,
			lan = @lan,
			lan_ow = @lan_ow,
			network_type = @network_type,
			network_type_ow = @network_type_ow
			WHERE inventory_id = @to_id AND domain_id = @domain_id
			EXEC update_link_insert_inventory_pictures @domain_id, @from_id, @to_id, NULL

		FETCH NEXT FROM linked_cursor INTO @to_id
	END
	CLOSE linked_cursor;
END
