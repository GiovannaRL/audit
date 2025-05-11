CREATE PROCEDURE [dbo].[copy_phases](
	@domain_id SMALLINT,
    @project_id INTEGER,
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

	DECLARE @description_var VARCHAR(500)
	DECLARE @current_phase_id INTEGER
	DECLARE @aux VARCHAR(500)

	DECLARE @phase_id INTEGER, @description VARCHAR(50), @start_date DATE, @end_date DATE, @plan_end DATE, @plan_start DATE, @sd_date DATE
	DECLARE @dd_date DATE, @cd_date DATE, @equip_move_in_date DATE, @occupancy_date DATE, @comment VARCHAR(1000), @ofci_delivery DATE, @copy_link uniqueidentifier

	SET @return_var = '';
	SET @aux = '';

	IF @copy_phase_id <> -1
		BEGIN
			DECLARE phases_cursor CURSOR LOCAL FOR SELECT phase_id, description, start_date, end_date, plan_end, plan_start, sd_date, dd_date, cd_date, equip_move_in_date, occupancy_date, comment, ofci_delivery, copy_link 
				FROM project_phase WHERE domain_id = @copy_domain_id AND project_id = @copy_project_id AND phase_id = @copy_phase_id;
			SET @description_var = STUFF(( SELECT ', ' + pp.description FROM project_phase AS pp WHERE domain_id = @copy_domain_id AND project_id = @copy_project_id AND phase_id = @copy_phase_id AND description in (SELECT description FROM project_phase WHERE domain_id = @domain_id AND project_id = @project_id) FOR XML PATH('') ), 1,1,'')
		END
	ELSE
		BEGIN
			DECLARE phases_cursor CURSOR LOCAL FOR SELECT phase_id, description, start_date, end_date, plan_end, plan_start, sd_date, dd_date, cd_date, equip_move_in_date, occupancy_date, comment, ofci_delivery, copy_link 
				FROM project_phase WHERE domain_id = @copy_domain_id AND project_id = @copy_project_id AND description NOT IN(SELECT description FROM project_phase WHERE domain_id = @domain_id AND project_id = @project_id)
			SET @description_var = STUFF(( SELECT ', ' + pp.description FROM project_phase AS pp WHERE domain_id = @copy_domain_id AND project_id = @copy_project_id AND description in (SELECT description FROM project_phase WHERE domain_id = @domain_id AND project_id = @project_id) FOR XML PATH('') ), 1,1,'')
		END

	IF @description_var <> ''
		BEGIN
			SET @return_var = CONCAT('The following phase(s) are repeated and were not included: ', @description_var, '\n');
		END

	OPEN phases_cursor
	FETCH NEXT FROM phases_cursor INTO @phase_id, @description, @start_date, @end_date, @plan_end, @plan_start, @sd_date, @dd_date, 
		@cd_date, @equip_move_in_date, @occupancy_date, @comment, @ofci_delivery, @copy_link;
	WHILE @@FETCH_STATUS = 0
		BEGIN
			
			IF @description NOT IN(SELECT description FROM project_phase WHERE domain_id = @domain_id AND project_id = @project_id)
				BEGIN
					INSERT INTO project_phase(domain_id, project_id, description, start_date, end_date, plan_end, plan_start, sd_date, dd_date, cd_date, equip_move_in_date, occupancy_date, date_added, added_by, comment, ofci_delivery) 
						VALUES(@domain_id, @project_id, @description,  @start_date, @end_date, @plan_end, @plan_start, @sd_date, @dd_date, @cd_date,  @equip_move_in_date, @occupancy_date, GETDATE(), @added_by, @comment, @ofci_delivery);
					SELECT @current_phase_id = max(phase_id) from project_phase where domain_id = @domain_id and project_id = @project_id;
					
					IF @project_id != @copy_project_id BEGIN
						IF @copy_link = CAST(CAST(0 AS BINARY) AS UNIQUEIDENTIFIER) BEGIN
							SET @copy_link = NEWID();
							update project_phase set copy_link = @copy_link where domain_id = @copy_domain_id and project_id = @copy_project_id  and phase_id = @phase_id
						END;
						update project_phase set copy_link = @copy_link where project_id = @project_id and phase_id = @current_phase_id and domain_id = @domain_id;
					END
				END
			ELSE
				SELECT @current_phase_id = phase_id FROM project_phase WHERE domain_id = @domain_id AND project_id = @project_id AND description = @description;
					
			EXEC copy_departments @domain_id, @project_id, @current_phase_id, @copy_domain_id, @copy_project_id, @phase_id, 
				@copy_department_id, @copy_room_id, @added_by, @copy_opt_col, @from_project, @aux OUTPUT

			SET @return_var = CONCAT(@return_var, @aux)
			FETCH NEXT FROM phases_cursor INTO @phase_id, @description, @start_date, @end_date, @plan_end, @plan_start, @sd_date, @dd_date, 
				@cd_date, @equip_move_in_date, @occupancy_date, @comment, @ofci_delivery, @copy_link;
		END
	CLOSE phases_cursor;
	DEALLOCATE phases_cursor;
END