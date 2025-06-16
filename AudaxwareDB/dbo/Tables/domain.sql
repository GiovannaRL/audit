CREATE TABLE [dbo].[domain] (
    [domain_id]       SMALLINT     NOT NULL,
    [name]            VARCHAR (60) NOT NULL,
    [created_at]      DATE         DEFAULT (getdate()) NULL,
    [show_audax_info] BIT          DEFAULT ((1)) NOT NULL,
	[rls_user]		  VARCHAR(50)  NULL,
	[rls_pwd]		  CHAR(63)	   NULL,
	[pb_workspace_id]	  VARCHAR(46)  NULL,
	[pb_dataset_name]	  VARCHAR(50)  NULL,
    [pb_workspace_collection] VARCHAR(50) NULL, 
    [pb_access_key] NCHAR(150) NULL, 
	[type] CHAR NOT NULL DEFAULT('E'),
	[enabled] BIT NOT NULL DEFAULT ((1)), 
    CONSTRAINT [domain_type_chk] CHECK ([type] = 'E' OR [type] = 'M'),
    CONSTRAINT [domain_id_pk] PRIMARY KEY CLUSTERED ([domain_id] ASC)
);


GO
-- =============================================
-- Author:		Juliana Barros
-- Create date: 06/28/2016
-- Description:	Insert into project's table for template use
-- Insert dashboards from audaxware to new domain
-- =============================================
CREATE TRIGGER [dbo].[add_project_template_trigger] 
   ON  [dbo].[domain]
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @DOMAIN_ID INT, @SHOW_AUDAX_INFO BIT, @client_id int, @facility_id int, @client_domain_id int, @facility_domain_id int, @workspace_id varchar(100), @pb_workspace_collection varchar(50);

	SELECT @DOMAIN_ID = domain_id, @workspace_id = pb_workspace_id, @SHOW_AUDAX_INFO = show_audax_info, @pb_workspace_collection = pb_workspace_collection FROM INSERTED;

	set @client_id = 1;
	set @facility_id = 1;
	set @client_domain_id = 1;
	set @facility_domain_id = 1;

	IF(SELECT COUNT(*) FROM project WHERE project_id = 1 and domain_id = @DOMAIN_ID ) = 0 BEGIN
		
		SET IDENTITY_INSERT project ON

		insert into project(project_id, project_description, status, date_added, added_by, domain_id, client_id, client_domain_id, facility_id, facility_domain_id)
		values(1, 'Global', 'A', GetDate(), 'juliana.barros@audaxware.com', @DOMAIN_ID, @client_id, @client_domain_id, @facility_id, @facility_domain_id);

		SET IDENTITY_INSERT project off

		SET IDENTITY_INSERT project_phase ON


		INSERT INTO project_phase(project_id, phase_id, description, start_date, end_date, date_added, added_by, domain_id)
		values(1, 1, 'Global', GETDATE(), Getdate() + 10000, getdate(), 'juliana.barros@audaxware.com', @DOMAIN_ID);

		SET IDENTITY_INSERT project_phase off

		SET IDENTITY_INSERT project_department ON

		insert into project_department(project_id, department_id, phase_id, description, department_type_id, department_type_domain_id, date_added, added_by, domain_id)
		values(1,1,1,'Global', 22, 1, Getdate(), 'juliana.barros@audaxware.com', @DOMAIN_ID);


		SET IDENTITY_INSERT project_department off
	END

	IF @SHOW_AUDAX_INFO = 1 AND @pb_workspace_collection IS NOT NULL AND @workspace_id IS NOT NULL begin
		insert into dashboard(domain_id, workspace_collection, workspace_id, dataset_id, name, report_id, linked_dashboard_id)
		select @DOMAIN_ID, @pb_workspace_collection, @workspace_id, NEWID(), name, 0, dashboard_id from dashboard where domain_id = 1;
	END


END
