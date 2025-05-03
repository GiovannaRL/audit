CREATE PROCEDURE [dbo].[update_inventories_cutsheet_filename]
	@inventories_id VARCHAR(5000),
	@cut_sheet_filename VARCHAR(100) = null
AS
	UPDATE project_room_inventory SET cut_sheet_filename = @cut_sheet_filename
	WHERE inventory_id in (SELECT CAST(value AS INTEGER) AS inventory_id FROM STRING_SPLIT(@inventories_id, ';'));
