CREATE PROCEDURE [dbo].[add_inventory_picture]
	@domain_id SMALLINT,
	@project_id INT,
	@filename VARCHAR(200),
	@blobFileNameWithExtension VARCHAR(100),
	@extension VARCHAR(10),
	@inventoryID INTEGER,
	@document_type_id INTEGER = 4, 
	@label VARCHAR(50), 
	@addedBy VARCHAR(50)
AS
BEGIN
	DECLARE @total int;

	--IF DOCUMENT_TYPE_ID = 0 CHANGE TO 4
	IF @document_type_id = 0 BEGIN
		SET @document_type_id = 4
	END

	/* To insert on inventory */
	IF NOT EXISTS (SELECT TOP 1 pd.project_domain_id FROM project_documents pd INNER JOIN documents_associations da ON pd.id = da.document_id
	WHERE pd.project_domain_id = @domain_id and da.project_id = @project_id AND pd.blob_file_name = @blobFileNameWithExtension AND da.inventory_id = @inventoryID)
	BEGIN 
		IF @document_type_id = 4 BEGIN
			--SET FIRST DOCUMENT IF SIMPLE IMAGE AS ASSET PHOTO
			SELECT @total = COUNT(*) FROM project_documents
			WHERE project_domain_id = @domain_id AND type_id = 6
			AND id in(SELECT document_id FROM documents_associations where inventory_id = @inventoryID and project_domain_id = @domain_id and project_id = @project_id);

			IF @total = 0 BEGIN
				SET @document_type_id = 6;
			END 
		END
		ELSE IF @document_type_id in(6, 7) BEGIN
			UPDATE project_documents SET type_id = 4 
			WHERE project_domain_id = @domain_id AND type_id = @document_type_id
			AND id in(SELECT document_id FROM documents_associations where inventory_id = @inventoryID and project_domain_id = @domain_id and project_id = @project_id);
		END
		
		INSERT INTO project_documents(filename, type_id, date_added, project_domain_id, project_id, blob_file_name, file_extension, label, added_by)
			VALUES(@filename, @document_type_id, GETDATE(), @domain_id, @project_id, @blobFileNameWithExtension, @extension, @label, @addedBy);

			DECLARE @document_id INTEGER = 
		(SELECT TOP 1  id FROM project_documents WHERE project_domain_id = @domain_id AND 
			project_id = @project_id AND type_id = @document_type_id AND filename = @filename AND blob_file_name = @blobFileNameWithExtension);

		IF NOT EXISTS (SELECT TOP 1 project_domain_id from documents_associations WHERE project_domain_id = @domain_id AND
			project_id = @project_id AND document_id = @document_id AND 
			inventory_id = @inventoryID)
		BEGIN
		INSERT INTO documents_associations(project_domain_id, project_id, document_id, inventory_id, added_by, date_added) 
		VALUES(@domain_id, @project_id, @document_id, @inventoryID, @addedBy, GETDATE());
		END

	END

	
END