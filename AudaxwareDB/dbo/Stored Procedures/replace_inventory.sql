CREATE PROCEDURE [dbo].[replace_inventory](@DOMAIN_ID INTEGER, @PROJECT_ID INTEGER, @INVENTORY_ID INTEGER, @NEW_ASSET_DOMAIN_ID INTEGER, 
	@NEW_ASSET_ID INTEGER, @COST_COL VARCHAR(20), @BUDGET NUMERIC(10, 2), @RESP VARCHAR(10), @StatusLog INT OUTPUT)
AS	
BEGIN
	SET NOCOUNT ON

	DECLARE @sql  nvarchar(1000)
	DECLARE @exist_count integer;
	DECLARE @return_val integer;

	DECLARE @cost_col2 varchar(20);
	DECLARE @budget_amt numeric(10,2);
	DECLARE @budget_qty integer;

	SET @return_val = 1;
	SET @cost_col2 = @COST_COL;

	IF @COST_COL = 'default' BEGIN
		SELECT @cost_col2 = default_cost_field
		FROM project
		WHERE project_id = @PROJECT_ID;
	END
	

	SELECT @exist_count = COUNT(*)
	FROM inventory_purchase_order
	WHERE project_id = @PROJECT_ID AND inventory_id = @INVENTORY_ID;

    if(@exist_count = 0) BEGIN


		SET @sql =  N'select @retval = ' + quotename(@cost_col2) + N' from assets  where asset_id = ' + CAST(@NEW_ASSET_ID AS varchar(10)) + ' and domain_id = ' + CAST(@NEW_ASSET_DOMAIN_ID AS VARCHAR(2));
		
		exec sp_executesql @SQL, N'@retval numeric out', @retval = @budget_amt out

		IF @RESP = '-1' BEGIN
			SET @RESP = null;
		END 

		IF @BUDGET <> 0 BEGIN
			SET @BUDGET_amt = @BUDGET;
		END

		-- Update the asset and resets the overwrite for description to pick it from the asset. We do not reset the other overwrites as we could lose data
		-- The new values are gotten in the trigger on project_room_inventory
		UPDATE project_room_inventory SET asset_id = @NEW_ASSET_ID, asset_domain_id = @NEW_ASSET_DOMAIN_ID, unit_budget = @budget_amt, 
			resp = @RESP, option_ids = null, manufacturer_description_ow = null, serial_number_ow = null, serial_name_ow = null, cut_sheet_filename = null, asset_description_ow = 0
			WHERE inventory_id = @INVENTORY_ID and domain_id = @DOMAIN_ID AND project_id = @PROJECT_ID;

		DELETE FROM inventory_options WHERE inventory_id = @INVENTORY_ID;
	END
	ELSE BEGIN
	   SET @return_val = 0;
	END

	set @StatusLog = @return_val;
	return @return_val;


END