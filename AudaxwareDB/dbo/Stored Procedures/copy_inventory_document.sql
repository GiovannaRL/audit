CREATE PROCEDURE [dbo].[copy_inventory_document]
	@domain_id SMALLINT,
    @project_id INTEGER,
    @phase_id INTEGER,
    @department_id INTEGER,
	@room_id INTEGER,	    
    @copy_phase_id INTEGER,
    @copy_department_id INTEGER,
	@copy_room_id INTEGER,
    @inventory_id INTEGER,
    @new_inventory_id INTEGER, 
	@addedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @asset_id INT, @asset_domain_id INT, @document_id INT, @document_phase_id INT, @document_department_id INT, @document_room_id INT, 
    @document_filename VARCHAR(200), @document_type_id INT, @document_blob_file_name VARCHAR(100), @new_document_id INT;

    DECLARE inv_doc_cursor CURSOR FOR
        SELECT document_id, phase_id, department_id, room_id, asset_domain_id, asset_id FROM documents_associations
        WHERE inventory_id = @inventory_id

    OPEN inv_doc_cursor
    FETCH NEXT FROM inv_doc_cursor INTO @document_id, @document_phase_id, @document_department_id, @document_room_id, @asset_domain_id, @asset_id   
    WHILE @@FETCH_STATUS = 0
        BEGIN

            INSERT INTO project_documents (filename, type_id, date_added, project_domain_id, project_id, blob_file_name, version, status, file_extension, label, added_by)
            SELECT filename, type_id, GETDATE(), @domain_id, @project_id, blob_file_name, version, status, file_extension, label, @addedBy 
            FROM project_documents WHERE id = @document_id
                
            SELECT @new_document_id = MAX(id) FROM project_documents
            WHERE project_domain_id = @domain_id AND project_id = @project_id;
            
            INSERT INTO documents_associations(project_domain_id, project_id, document_id, phase_id, department_id, room_id, inventory_id,asset_domain_id, asset_id, added_by, date_added)
            VALUES (@domain_id, @project_id, @new_document_id,
                CASE
                    WHEN @document_phase_id IS NULL THEN NULL
                    ELSE @phase_id
                    END,
                CASE
                    WHEN @document_department_id IS NULL THEN NULL
                    ELSE @department_id
                    END,
                CASE
                    WHEN @document_room_id IS NULL THEN NULL
                    ELSE @room_id
                    END,
                @new_inventory_id, @asset_domain_id, @asset_id, @addedBy, GETDATE());

    FETCH NEXT FROM inv_doc_cursor INTO @document_id, @document_phase_id, @document_department_id, @document_room_id, @asset_domain_id, @asset_id
    END
    CLOSE inv_doc_cursor;
	DEALLOCATE inv_doc_cursor;

END