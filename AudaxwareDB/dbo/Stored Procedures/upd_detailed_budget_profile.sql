CREATE PROCEDURE [dbo].[upd_detailed_budget_profile]
	@inventories_id VARCHAR(5000),
	@detailed_budget BIT
AS
BEGIN
	UPDATE project_room_inventory SET detailed_budget = @detailed_budget WHERE inventory_id IN
			(SELECT value AS inventory_id FROM STRING_SPLIT(@inventories_id, ';'))
END