CREATE PROCEDURE [dbo].[clean_user_projects]
	@userId nvarchar(128)
AS
BEGIN
	DELETE FROM project_user WHERE user_pid = @userId;
END
