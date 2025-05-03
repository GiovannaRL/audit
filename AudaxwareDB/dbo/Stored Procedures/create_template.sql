CREATE PROCEDURE [dbo].[create_template](
	@domain_id SMALLINT,
    @project_id INTEGER,
	@phase_id INTEGER,
    @department_id INTEGER,
    @room_id INTEGER,
	@new_domain_id SMALLINT,
	@description VARCHAR(50),
	@added_by VARCHAR(50),
	@related_project_domain_id SMALLINT = NULL,
	@related_project_id INTEGER = NULL,
	@fromRoom BIT = 0,
	@room_comment VARCHAR(1000) = NULL)
AS
BEGIN

	DECLARE @exist_count INTEGER, @new_room_id INTEGER, @new_inventory_id INTEGER, @new_box_id INTEGER, @dpt_type_id INTEGER, @dpt_type_domain_id SMALLINT;
	SET @dpt_type_domain_id = NULL;
	SET @dpt_type_id = NULL;

	/* Project_room_inventory variables */
	DECLARE @old_inventory_id INTEGER, @inventory_id INTEGER, @asset_id INTEGER, @status VARCHAR(20), @resp VARCHAR(10)

	SELECT @exist_count = COUNT(*) FROM project_room WHERE domain_id = @domain_id AND project_id = @project_id AND
		phase_id = @phase_id AND department_id = @department_id AND room_id = @room_id AND (is_template = 1 OR @fromRoom = 1);

	IF @exist_count = 1
		BEGIN
			SELECT @exist_count = COUNT(*) FROM project_room where domain_id = @new_domain_id AND is_template = 1 AND
				drawing_room_name = @description;

			IF @exist_count = 0
				BEGIN

					IF @fromRoom = 1
						SELECT @dpt_type_domain_id = department_type_domain_id, @dpt_type_id = department_type_id
						FROM project_department WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = 
							@phase_id AND department_id = @department_id;


					INSERT INTO project_room(domain_id, project_id, phase_id, department_id, drawing_room_name, drawing_room_number,
						date_added, added_by, comment, is_template, department_type_domain_id_template, department_type_id_template,
						project_domain_id_template, project_id_template)
						SELECT @new_domain_id, 1, 1, 1, @description, '-', GETDATE(), 
						@added_by, COALESCE(@room_comment, comment), 1, COALESCE(@dpt_type_domain_id, department_type_domain_id_template), COALESCE(@dpt_type_id, department_type_id_template), 
						@related_project_domain_id, @related_project_id FROM project_room 
						WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id AND department_id = @department_id AND room_id = @room_id;

					SELECT @new_room_id = MAX(room_id) FROM project_room WHERE domain_id = @new_domain_id AND project_id = 1 AND
						phase_id = 1 AND department_id = 1;

					/* Inventory */
					DECLARE template_cursor CURSOR LOCAL FOR SELECT inventory_id
						FROM project_room_inventory WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id 
						AND department_id = @department_id AND room_id = @room_id;
					OPEN template_cursor;
					FETCH NEXT FROM template_cursor INTO @old_inventory_id;
					WHILE @@FETCH_STATUS = 0
					BEGIN

						EXEC @new_inventory_id = copy_inventory_item @domain_id, @old_inventory_id, null, 1, 1, 1, @new_room_id, @domain_id, 'source', @added_by, 1

						FETCH NEXT FROM template_cursor INTO @old_inventory_id
					END
					CLOSE template_cursor;
					DEALLOCATE template_cursor;
					/* end inventory */

				END
		END

		IF @new_room_id IS NOT NULL
			SELECT TOP 1 * FROM project_room WHERE domain_id = @new_domain_id AND project_id = 1 AND phase_id = 1 AND department_id = 1 AND
				room_id = @new_room_id;
		ELSE
			SELECT null;
END