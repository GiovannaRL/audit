CREATE PROCEDURE [dbo].[copy_from_project](
	@domain_id SMALLINT,
    @project_id INTEGER,
    @phase_id INTEGER,
    @department_id INTEGER,
    @room_id INTEGER,
	@copy_domain_id SMALLINT,
    @copy_project_id INTEGER,
    @copy_phase_id INTEGER,
    @copy_department_id INTEGER,
    @copy_room_id INTEGER,
    @added_by VARCHAR(50),
    @copy_opt_col BIT,
	@return_var varchar(500) OUT)
AS
BEGIN
	SET NOCOUNT ON;
	
	SET @return_var = '';

	IF @room_id <> -1
		BEGIN
			EXEC copy_inventory @domain_id, @project_id, @phase_id, @department_id, @room_id, @copy_domain_id, @copy_project_id,
				@copy_phase_id, @copy_department_id, @copy_room_id, @added_by, @copy_opt_col, null, 1, 0, @return_var OUTPUT;
		END
	ELSE
		IF @department_id <> -1
			BEGIN
				EXEC copy_rooms @domain_id, @project_id, @phase_id, @department_id, @copy_domain_id, @copy_project_id, @copy_phase_id, 
					@copy_department_id, @copy_room_id, @added_by, @copy_opt_col, null, null,0, 0, 1, 0, @return_var OUTPUT;
			END
		ELSE
			IF @phase_id <> -1
				BEGIN
					EXEC copy_departments @domain_id, @project_id, @phase_id, @copy_domain_id, @copy_project_id, @copy_phase_id, 
						@copy_department_id, @copy_room_id, @added_by, @copy_opt_col, 1, @return_var OUTPUT;
				END
			ELSE
				BEGIN
					EXEC copy_phases @domain_id, @project_id, @copy_domain_id, @copy_project_id, @copy_phase_id, 
						@copy_department_id, @copy_room_id, @added_by, @copy_opt_col, 1, @return_var OUTPUT;
				END

	RETURN 0
END