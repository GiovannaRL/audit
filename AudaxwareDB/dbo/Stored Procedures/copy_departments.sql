CREATE PROCEDURE [dbo].[copy_departments](
	@domain_id SMALLINT,
    @project_id INTEGER,
    @phase_id INTEGER,
	@copy_domain_id SMALLINT,
    @copy_project_id INTEGER,
    @copy_phase_id INTEGER,
	@copy_department_id INTEGER,
	@copy_room_id INTEGER,
    @added_by VARCHAR(50),
    @copy_opt_col BIT,
	@from_project bit = 0,
	@return_var VARCHAR(500) OUT)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @current_department_id INTEGER
	DECLARE @aux VARCHAR(500)
	DECLARE @department_id INTEGER, @description VARCHAR(50), @department_type_id INTEGER, @department_type_domain_id SMALLINT
	DECLARE @area INTEGER, @contact_name VARCHAR(50), @contact_email VARCHAR(50), @contact_phone VARCHAR(25), @comment VARCHAR(1000), @copy_link uniqueidentifier

	SET @return_var = '';
	SET @aux = '';

	IF @copy_department_id <> -1
		DECLARE departments_cursor CURSOR FOR SELECT department_id, description, department_type_id, department_type_domain_id, area, contact_name, contact_email, contact_phone, comment, copy_link 
			FROM project_department WHERE domain_id = @copy_domain_id AND project_id = @copy_project_id AND phase_id = @copy_phase_id AND department_id = @copy_department_id;
	ELSE
		DECLARE departments_cursor CURSOR FOR SELECT department_id, description, department_type_id, department_type_domain_id, area, contact_name, contact_email, contact_phone, comment, copy_link 
			FROM project_department WHERE domain_id = @copy_domain_id AND project_id = @copy_project_id AND phase_id = @copy_phase_id;

	OPEN departments_cursor
	FETCH NEXT FROM departments_cursor INTO @department_id, @description, @department_type_id, @department_type_domain_id, @area, @contact_name, 
		@contact_email, @contact_phone, @comment, @copy_link;
	WHILE @@FETCH_STATUS = 0
		BEGIN
			--IF @copy_department_id = -1
				SELECT @current_department_id = department_id FROM project_department WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id 
					AND lower(description) = lower(@description);

			IF @current_department_id IS NULL
				BEGIN
					INSERT INTO project_department(domain_id, project_id, phase_id, description, department_type_id, department_type_domain_id, area, contact_name, contact_email, contact_phone, comment, date_added, added_by) 
						VALUES(@domain_id, @project_id, @phase_id, @description, @department_type_id, @department_type_domain_id, @area, @contact_name, @contact_email, @contact_phone, @comment, GETDATE(), @added_by);
					SELECT @current_department_id = max(department_id) from project_department where domain_id = @domain_id and project_id = @project_id and phase_id = @phase_id;
					
					IF @project_id != @copy_project_id BEGIN
						IF @copy_link = CAST(CAST(0 AS BINARY) AS UNIQUEIDENTIFIER) BEGIN
							SET @copy_link = NEWID();
							update project_department set copy_link = @copy_link where domain_id = @copy_domain_id and project_id = @copy_project_id  and phase_id = @copy_phase_id and department_id = @department_id;
						END;
						update project_department set copy_link = @copy_link where project_id = @project_id and department_id = @current_department_id and domain_id = @domain_id;
					END

					SET @return_var = CONCAT(@return_var,  @aux);
				END
			ELSE
				SET @return_var = CONCAT(@return_var, '\n The following department was not included: ', @description);

			EXEC copy_rooms @domain_id, @project_id, @phase_id, @current_department_id, @copy_domain_id, @copy_project_id, @copy_phase_id, 
				@department_id, @copy_room_id, @added_by, @copy_opt_col, null, null, 0, 0, @from_project, 0, @aux OUTPUT

			SET @current_department_id = null;

			FETCH NEXT FROM departments_cursor INTO @department_id, @description, @department_type_id, @department_type_domain_id, @area, @contact_name, 
				@contact_email, @contact_phone, @comment, @copy_link;
		END
	CLOSE departments_cursor;
	DEALLOCATE departments_cursor;
END