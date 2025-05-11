CREATE PROCEDURE add_room_picture
	@domain_id SMALLINT,
	@project_id INT,
	@phase_id INT,
	@department_id INT,
	@room_id INT,
	@filename VARCHAR(200),
	@blobFileNameWithExtension VARCHAR(100),
	@extension VARCHAR(10), 
	@addedBy VARCHAR(50)
AS
BEGIN
	/* Insert picture and and association in case it does not exist */
	IF NOT EXISTS (SELECT TOP 1 project_domain_id  from project_documents where project_domain_id = @domain_id and project_id = @project_id AND blob_file_name = @blobFileNameWithExtension AND type_id = 3)
	BEGIN 
		INSERT INTO project_documents(filename, type_id, date_added, project_domain_id, project_id, blob_file_name, file_extension, added_by)
			values(@filename, 3, GETDATE(), @domain_id, @project_id, @blobFileNameWithExtension, @extension, @addedBy);
	END

	DECLARE @document_id INTEGER = 
		(SELECT TOP 1 id FROM project_documents WHERE project_domain_id = @domain_id AND 
			project_id = @project_id AND type_id = 3  AND filename = @filename AND blob_file_name = @blobFileNameWithExtension);

	IF NOT EXISTS (SELECT TOP 1 project_domain_id from documents_associations WHERE project_domain_id = @domain_id AND
			project_id = @project_id AND document_id = @document_id AND phase_id = @phase_id AND 
			department_id = @department_id AND room_id = @room_id )
	BEGIN
		INSERT INTO documents_associations(project_domain_id, project_id, document_id, phase_id, department_id, room_id, added_by, date_added) 
			VALUES(@domain_id, @project_id, @document_id, @phase_id, @department_id, @room_id, @addedBy, GETDATE());
	END
END