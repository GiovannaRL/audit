CREATE PROCEDURE [dbo].[copy_project]
	@domainId smallint,
	@projectId int,
	@projectDescription varchar(200),
	@copyUser bit,
	@addedBy varchar(100)
AS
	DECLARE @newProjectId int, @aux varchar(500), @copy_link UNIQUEIDENTIFIER;
	SET @aux = '';
	SELECT @copy_link = [copy_link] from project WHERE  project_id = @projectId and domain_id = @domainId;
	IF @copy_link = CAST(CAST(0 AS BINARY) AS UNIQUEIDENTIFIER) BEGIN
			SET @copy_link = NEWID();
			update project set copy_link = @copy_link where domain_id = @domainId and project_id = @projectId
	END;

	INSERT INTO project(project_description, project_start, project_end, status, client_project_number, facility_project_number, hsg_project_number, default_cost_field, medial_budget, freight_budget, warehouse_budget, tax_budget, warranty_budget, misc_budget, date_added, added_by, comment, domain_id, client_id, client_domain_id, facility_id, facility_domain_id, markup, escalation, tax, freight_markup, install_markup, markup_budget, escalation_budget, install_budget, date_locked, from_project_id, copy_link)
	SELECT @projectDescription, project_start, project_end, status, client_project_number, facility_project_number, hsg_project_number, default_cost_field, medial_budget, freight_budget, warehouse_budget, tax_budget, warranty_budget, misc_budget, date_added, @addedBy, comment, domain_id, client_id, client_domain_id, facility_id, facility_domain_id, markup, escalation, tax, freight_markup, install_markup, markup_budget, escalation_budget, install_budget, GETDATE(), @projectId, copy_link FROM project WHERE project_id = @projectId and domain_id = @domainId;

	SELECT @newProjectId = MAX(project_id) from project WHERE domain_id = @domainId;

	INSERT INTO project_addresses(domain_id, nickname, description, project_id, address1, address2, city, state, zip, is_default)
	SELECT domain_id, nickname, description, @newProjectId, address1, address2, city, state, zip, is_default FROM project_addresses WHERE project_id = @projectId and domain_id = @domainId;

	INSERT INTO cost_center(code, description, project_id, is_default, domain_id)
	SELECT code, description, @newProjectId, is_default, domain_id from cost_center WHERE project_id = @projectId and domain_id = @domainId;

	INSERT INTO project_contact(project_id, contact_id, domain_id)
	SELECT @newProjectId, contact_id, domain_id FROM project_contact WHERE project_id = @projectId and domain_id = @domainId;

	exec copy_phases @domainId, @newProjectId, @domainId, @projectId, -1, -1, -1, @addedBy, 1, 0, @aux OUTPUT;

	exec copy_po  @domainId, @projectId, @domainId, @newProjectId, @addedBy;

	--COPY PERMISSION TO VIEW(FOR THOSE WHO HAVE ACCESS TO THE COPIED PROJECT) FOR THE NEW PROJECT 
	IF @copyUser = 1 BEGIN
		INSERT INTO project_user(project_domain_id, project_id, user_pid)
		SELECT project_domain_id, @newProjectId, user_pid FROM project_user WHERE project_id = @projectId and project_domain_id = @domainId
	END

RETURN @aux
