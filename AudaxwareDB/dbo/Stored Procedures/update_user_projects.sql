CREATE PROCEDURE [dbo].[update_user_projects]
	@userId nvarchar(128),
	@projects_domain_id SMALLINT,
	@projects_id VARCHAR(5000), 
	@role_id varchar(1)
AS
BEGIN

	IF @role_id = '1' BEGIN
		DELETE FROM project_user WHERE user_pid = @userId AND project_domain_id = @projects_domain_id
	END
	ELSE BEGIN
		INSERT INTO project_user SELECT @projects_domain_id, CAST([value] AS INTEGER), @userId
			FROM STRING_SPLIT(@projects_id, ';') WHERE CAST([value] AS INTEGER) NOT IN (SELECT
			project_id FROM project_user WHERE user_pid = @userId AND project_domain_id = @projects_domain_id);

		DELETE FROM project_user WHERE user_pid = @userId AND project_domain_id = @projects_domain_id
			AND project_id NOT IN (SELECT CAST([value] AS INTEGER) FROM STRING_SPLIT(@projects_id, ';'))
	END
END