CREATE PROCEDURE [dbo].[apply_template](
	@domain_id SMALLINT,
    @project_id INTEGER,
	@phase_id INTEGER,
    @department_id INTEGER,
    @room_id INTEGER,
    @template_id INTEGER,
    @cost_col VARCHAR(25),
    @added_by VARCHAR(60),
    @delete_equipment BIT, 
	@linked_template BIT)
AS
BEGIN

	DECLARE 
		@template_project_id int,
		@template_phase_id int,
		@template_department_id int,
		@template_room_id int,
		@template_domain_id int,
		@sqlstr  VARCHAR(1000), 
		@return_var INTEGER--, 
		--@cost_col_var VARCHAR(25)

 --   IF @cost_col = 'default' BEGIN
 --       SELECT @cost_col_var = default_cost_field FROM project WHERE domain_id = @domain_id AND project_id = @project_id;
	--END
	--ELSE IF @cost_col = '-1' BEGIN
	--	SET @cost_col_var = 'pri.unit_budget';
	--END
 --   ELSE BEGIN
	--	SET @cost_col_var = 'a.' + @cost_col;
	--END
	
	IF @delete_equipment = 1 BEGIN
		DELETE FROM project_room_inventory WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id 
			AND department_id = @department_id AND room_id = @room_id;
	END


	update project_room set applied_id_template = @template_id, linked_template = @linked_template where project_id = @project_id and domain_id = @domain_id and phase_id = @phase_id and department_id = @department_id and room_id = @room_id;
	

	select @template_project_id = project_id, @template_phase_id = phase_id, @template_department_id = department_id, @template_room_id = room_id, @template_domain_id = domain_id 
	from project_room where id = @template_id;
		
	DECLARE @sql nvarchar(4000), @inventory_id int;
		
	IF @linked_template = 0 BEGIN
		SET @template_id = 0;
	END

	CREATE TABLE #inventories(inventory_id int);
	INSERT INTO #inventories 
	SELECT inventory_id FROM project_room_inventory
	WHERE project_id = @template_project_id and phase_id = @template_phase_id and department_id = @template_department_id and room_id = @template_room_id and domain_id = @template_domain_id;

	BEGIN TRAN

		DECLARE inventory_cursor CURSOR LOCAL FOR 
			SELECT inventory_id FROM #inventories;
		OPEN inventory_cursor;
		FETCH NEXT FROM inventory_cursor INTO @inventory_id;
		WHILE @@FETCH_STATUS = 0 BEGIN

			EXEC copy_inventory_item @template_domain_id, @inventory_id, null, @project_id, @phase_id, @department_id, @room_id, @domain_id, @cost_col, @added_by, 1

			FETCH NEXT FROM inventory_cursor INTO @inventory_id
		END
		CLOSE inventory_cursor;
		DEALLOCATE inventory_cursor;

	COMMIT TRAN

END
GO


