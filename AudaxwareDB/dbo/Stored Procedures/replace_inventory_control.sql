
CREATE PROCEDURE [dbo].[replace_inventory_control](@PROJECT_ID INTEGER, @PHASE_ID INTEGER, @DEPARTMENT_ID INTEGER, @ROOM_ID INTEGER, @OLD_CODE INTEGER, @OLD_DOMAIN_ID INTEGER, @NEW_CODE INTEGER, @NEW_DOMAIN_ID INTEGER, @COST_COL VARCHAR(20), @BUDGET NUMERIC, @RESP VARCHAR(10))
AS
BEGIN 

	SET NOCOUNT ON;

	DECLARE @sql  varchar(1000);
	DECLARE @exist_count integer;
	DECLARE @return_val integer;
	
	DECLARE @tab_rooms table(phase int null, department int null, room int);
	DECLARE @tab_departments table(department int);


	SET @return_val = 1;

	IF(@ROOM_ID <> -1) BEGIN

		EXEC	@return_val = [dbo].[replace_inventory]
		@PROJECT_ID = @PROJECT_ID,
		@PHASE_ID = @PHASE_ID,
		@DEPARTMENT_ID = @DEPARTMENT_ID,
		@ROOM_ID = @ROOM_ID,
		@OLD_CODE = @OLD_CODE,
		@OLD_DOMAIN_ID = @OLD_DOMAIN_ID,
		@NEW_CODE = @NEW_CODE,
		@NEW_DOMAIN_ID = @NEW_DOMAIN_ID,
		@COST_COL = @COST_COL,
		@BUDGET = @BUDGET,
		@RESP = @RESP

	END
	ELSE IF(@DEPARTMENT_ID <> -1) BEGIN
		INSERT INTO @tab_rooms
		select distinct null, null, room_id as room_id from project_room_inventory where project_id=@PROJECT_ID and department_id = @DEPARTMENT_ID and asset_id = @OLD_CODE and domain_id = @OLD_DOMAIN_ID

		WHILE EXISTS(select room from @tab_rooms) BEGIN
			SELECT top 1 @ROOM_ID = room from @tab_rooms 		
			EXEC	@return_val = [dbo].[replace_inventory]
					@PROJECT_ID = @PROJECT_ID,
					@PHASE_ID = @PHASE_ID,
					@DEPARTMENT_ID = @DEPARTMENT_ID,
					@ROOM_ID = @ROOM_ID,
					@OLD_CODE = @OLD_CODE,
					@OLD_DOMAIN_ID = @OLD_DOMAIN_ID,
					@NEW_CODE = @NEW_CODE,
					@NEW_DOMAIN_ID = @NEW_DOMAIN_ID,
					@COST_COL = @COST_COL,
					@BUDGET = @BUDGET,
					@RESP = @RESP

			DELETE FROM @TAB_ROOMS WHERE ROOM = @ROOM_ID
		END
	END
	ELSE IF(@PHASE_ID <> -1) BEGIN
		INSERT INTO @tab_rooms 
		select distinct null, department_id, room_id from project_room_inventory where project_id=@PROJECT_ID and phase_id = @PHASE_ID and asset_id = @OLD_CODE and domain_id = @OLD_DOMAIN_ID

		WHILE EXISTS(select room from @tab_rooms) BEGIN
			SELECT top 1 @DEPARTMENT_ID = department, @ROOM_ID = room from @tab_rooms 		
			EXEC	@return_val = [dbo].[replace_inventory]
					@PROJECT_ID = @PROJECT_ID,
					@PHASE_ID = @PHASE_ID,
					@DEPARTMENT_ID = @DEPARTMENT_ID,
					@ROOM_ID = @ROOM_ID,
					@OLD_CODE = @OLD_CODE,
					@OLD_DOMAIN_ID = @OLD_DOMAIN_ID,
					@NEW_CODE = @NEW_CODE,
					@NEW_DOMAIN_ID = @NEW_DOMAIN_ID,
					@COST_COL = @COST_COL,
					@BUDGET = @BUDGET,
					@RESP = @RESP

			DELETE FROM @TAB_ROOMS WHERE department = @DEPARTMENT_ID AND ROOM = @ROOM_ID
		END
	END
	ELSE IF(@PROJECT_ID <> -1) BEGIN
		INSERT INTO @tab_rooms 
		select distinct phase_id, department_id, room_id from project_room_inventory where project_id=@PROJECT_ID and asset_id = @OLD_CODE and domain_id = @OLD_DOMAIN_ID

		WHILE EXISTS(select room from @tab_rooms) BEGIN
			SELECT top 1 @PHASE_ID = phase, @DEPARTMENT_ID = department, @ROOM_ID = room from @tab_rooms 		
			EXEC	@return_val = [dbo].[replace_inventory]
					@PROJECT_ID = @PROJECT_ID,
					@PHASE_ID = @PHASE_ID,
					@DEPARTMENT_ID = @DEPARTMENT_ID,
					@ROOM_ID = @ROOM_ID,
					@OLD_CODE = @OLD_CODE,
					@OLD_DOMAIN_ID = @OLD_DOMAIN_ID,
					@NEW_CODE = @NEW_CODE,
					@NEW_DOMAIN_ID = @NEW_DOMAIN_ID,
					@COST_COL = @COST_COL,
					@BUDGET = @BUDGET,
					@RESP = @RESP

			DELETE FROM @TAB_ROOMS WHERE phase = @PHASE_ID AND department = @DEPARTMENT_ID AND ROOM = @ROOM_ID
		END


	END

	return @return_val;

END;
