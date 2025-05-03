CREATE PROCEDURE [dbo].[copy_room_document]
	@domain_id SMALLINT,
	@project_id INTEGER,
	@phase_id INTEGER,
	@department_id INTEGER,
	@room_id INTEGER,
	@copy_room_id INTEGER,
	@added_by VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @document_id INT, @document_phase_id INT, @document_department_id INT, @document_room_id INT, @new_document_id INT, @filename VARCHAR(200);

	DECLARE room_doc_cursor CURSOR FOR
		SELECT da.document_id, da.phase_id, da.department_id, da.room_id, pd.filename FROM documents_associations da
		INNER JOIN project_documents pd ON da.document_id = pd.id
		WHERE room_id = @copy_room_id AND inventory_id IS NULL

	OPEN room_doc_cursor
	FETCH NEXT FROM room_doc_cursor INTO @document_id, @document_phase_id, @document_department_id, @document_room_id, @filename
	WHILE @@FETCH_STATUS = 0
		BEGIN

			INSERT INTO project_documents (filename, type_id, date_added, project_domain_id, project_id, blob_file_name, version, status, file_extension, label, added_by)
			SELECT filename, type_id, GETDATE(), @domain_id, @project_id, blob_file_name, version, status, file_extension, label, @added_by 
			FROM project_documents WHERE id = @document_id;

			SET @new_document_id = SCOPE_IDENTITY();

			INSERT INTO documents_associations (project_domain_id, project_id, document_id, phase_id, department_id, room_id, date_added, added_by)
			VALUES (@domain_id, @project_id, @new_document_id, @phase_id, @department_id, @room_id, GETDATE(), @added_by);

	FETCH NEXT FROM room_doc_cursor INTO @document_id, @document_phase_id, @document_department_id, @document_room_id, @filename;
	END
	CLOSE  room_doc_cursor;
	DEALLOCATE room_doc_cursor;
END
