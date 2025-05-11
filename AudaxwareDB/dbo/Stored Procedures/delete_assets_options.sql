CREATE PROCEDURE [dbo].[delete_assets_options]
	@inventories_id VARCHAR(5000),
    @mantain_options_ids VARCHAR(5000)
AS
BEGIN

	DELETE FROM inventory_options WHERE inventory_id IN
		(SELECT CAST(value AS INTEGER) AS inventory_id FROM STRING_SPLIT(@inventories_id, ';'))
		AND option_id NOT IN (SELECT CAST(value AS INTEGER) AS option_id FROM STRING_SPLIT(@mantain_options_ids, ';'))
END