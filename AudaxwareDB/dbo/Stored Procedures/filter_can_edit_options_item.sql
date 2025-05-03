CREATE PROCEDURE [dbo].[filter_can_edit_options_item]
	@inventories_id VARCHAR(5000)
AS
	SELECT inventory_id 
	FROM project_room_inventory
	WHERE inventory_id in (SELECT CAST(value AS INTEGER) AS inventory_id FROM STRING_SPLIT(@inventories_id, ';'))
		AND (none_option IS NULL OR none_option = 0);
