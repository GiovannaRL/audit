CREATE PROCEDURE [dbo].[copy_rooms](
	@domain_id SMALLINT,
    @project_id INTEGER,
    @phase_id INTEGER,
    @department_id INTEGER,
	@copy_domain_id SMALLINT,
    @copy_project_id INTEGER,
    @copy_phase_id INTEGER,
    @copy_department_id INTEGER,
	@copy_room_id INTEGER,
    @added_by VARCHAR(50),
    @copy_opt_col BIT,
	@room_name CHARACTER VARYING(50) = null,
	@room_number CHARACTER VARYING(10) = null,
	@template_id int = 0,
	@split_room bit = 0,
	@from_project bit = 0,
	@is_move bit = 0,
	@return_var VARCHAR(max) OUT)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @current_room_id INTEGER
	DECLARE @aux VARCHAR(500)
	DECLARE @room_id INTEGER, @drawing_room_name VARCHAR(50), @drawing_room_number VARCHAR(10), @final_room_name VARCHAR(50)
	DECLARE @final_room_number VARCHAR(10), @comment VARCHAR(1000), @linked_template bit, @room_quantity int
	DECLARE @blueprint VARCHAR(50), @functional_area VARCHAR(50), @staff VARCHAR(50), @room_area NUMERIC(10,2), @room_code VARCHAR(20), @copy_link uniqueidentifier;

	SET @return_var = '';
	SET @aux = '';
	SET @linked_template = 1;

	IF @copy_room_id <> -1
		BEGIN
			DECLARE rooms_cursor CURSOR FOR SELECT room_id, drawing_room_name, drawing_room_number, final_room_name, final_room_number, comment, room_quantity, blueprint, staff, room_area, room_code, functional_area, copy_link
				FROM project_room WHERE domain_id = @copy_domain_id AND project_id = @copy_project_id AND phase_id = @copy_phase_id AND department_id = @copy_department_id AND room_id = @copy_room_id;
		END
	ELSE
		BEGIN
			DECLARE rooms_cursor CURSOR FOR SELECT room_id, drawing_room_name, drawing_room_number, final_room_name, final_room_number, comment, room_quantity, blueprint, staff, room_area, room_code, functional_area, copy_link
				FROM project_room WHERE domain_id = @copy_domain_id AND project_id = @copy_project_id AND phase_id = @copy_phase_id AND department_id = @copy_department_id;
		END

	OPEN rooms_cursor
	FETCH NEXT FROM rooms_cursor INTO @room_id, @drawing_room_name, @drawing_room_number, @final_room_name, @final_room_number, @comment, @room_quantity, @blueprint, @staff, @room_area, @room_code, @functional_area, @copy_link
	WHILE @@FETCH_STATUS = 0
		BEGIN
			IF @room_name is not null BEGIN
				set @drawing_room_name = @room_name;
				set @drawing_room_number = @room_number;
			END

			SELECT @current_room_id = room_id FROM project_room WHERE project_id = @project_id AND @phase_id = phase_id AND 
				department_id = @department_id and drawing_room_name = @drawing_room_name and drawing_room_number = @drawing_room_number;

			--CHANGE THE ROOM_NAME FOR A NEW ONE WHEN THERE AN EXISTING ONE WITH THE SAME NAME
			IF @room_name is not null AND @current_room_id is not null BEGIN
				set @drawing_room_name = @room_name; --+ ' (S)';
				set @current_room_id = null;
			END

			IF @template_id = 0 or @template_id is null BEGIN
				SET @template_id = null;
				SET @linked_template = null;
			END

			IF @split_room = 1 BEGIN
				SET @room_quantity = 1;
			END
					
			INSERT INTO project_room(domain_id, project_id, phase_id, department_id, drawing_room_name, drawing_room_number, final_room_name, final_room_number, comment, date_added, added_by, applied_id_template, linked_template, room_quantity, blueprint, staff, room_area, room_code, functional_area) 
			SELECT @domain_id, @project_id, @phase_id, @department_id, @drawing_room_name, @drawing_room_number, @drawing_room_name, @drawing_room_number, @comment, GETDATE(), @added_by, @template_id, @linked_template, @room_quantity, @blueprint, @staff, @room_area, @room_code, @functional_area;
					
			SELECT @current_room_id = max(room_id) from project_room where domain_id = @domain_id and project_id = @project_id and phase_id = @phase_id and department_id = @department_id;

			IF @project_id != @copy_project_id BEGIN
				IF @copy_link = CAST(CAST(0 AS BINARY) AS UNIQUEIDENTIFIER) BEGIN
					SET @copy_link = NEWID();
					update project_room set copy_link = @copy_link where domain_id = @copy_domain_id and project_id = @copy_project_id  and phase_id = @copy_phase_id and department_id = @copy_department_id and room_id = @room_id;
				END;
				update project_room set copy_link = @copy_link where domain_id = @domain_id and project_id = @project_id and phase_id = @phase_id and department_id = @department_id and room_id = @current_room_id;
			END

			--PROC TO COPY PICTURES ON ROOM'S LEVEL
			EXEC copy_room_document @domain_id, @project_id, @phase_id, @department_id, @current_room_id, @room_id, @added_by;

			EXEC copy_inventory @domain_id, @project_id, @phase_id, @department_id, @current_room_id, @copy_domain_id, @copy_project_id, 
				@copy_phase_id, @copy_department_id, @room_id, @added_by, @copy_opt_col, @template_id, @from_project, @is_move, @aux OUTPUT;

			SET @return_var = CONCAT(@return_var,  @aux);
			SET @current_room_id = NULL;

			FETCH NEXT FROM rooms_cursor INTO @room_id, @drawing_room_name, @drawing_room_number, @final_room_name, @final_room_number, @comment, @room_quantity, @blueprint, @staff, @room_area, @room_code, @functional_area, @copy_link
		END
	CLOSE rooms_cursor;
	DEALLOCATE rooms_cursor;
END