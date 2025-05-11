
CREATE PROCEDURE [dbo].[unlink_inventory](
	@domain_id SMALLINT,
	@inventory_target_id INTEGER, 
	@userId NVARCHAR(128))
AS
BEGIN
	DECLARE @source_id INTEGER, @projectId INTEGER, @auditOriginal VARCHAR(100), @pkSource VARCHAR(100), @pkTarget VARCHAR(100)

	SELECT TOP 1 @source_id = inventory_source_id, @projectId = project_id from project_room_inventory where domain_id = @domain_id AND inventory_id = @inventory_target_id
	UPDATE project_room_inventory set inventory_source_id = NULL where domain_id = @domain_id AND inventory_id = @inventory_target_id
	UPDATE project_room_inventory set inventory_target_id = NULL where domain_id = @domain_id AND inventory_id = @source_id

	-- Unlink pictures
	DELETE document FROM documents_associations AS document WHERE inventory_id = @inventory_target_id
		AND EXISTS (SELECT 1 FROM documents_associations WHERE inventory_id = @source_id
			AND document_id = document.document_id);

	-- AUDIT
	SET @pkTarget = '{"inventory_id": "' + TRIM(cast(@inventory_target_id as varchar(15))) + '"}';
	SET @auditOriginal = '{"inventory_target_id": "' + TRIM(cast(@inventory_target_id as varchar(15))) + '","inventory_source_id":"' + TRIM(cast(@source_id as varchar(15))) + '"}'
	
	INSERT INTO audit_log(domain_id, user_id, operation, table_name, table_pk, original, modified, modified_date, project_id)
	VALUES(@domain_id, @userId, 'DELETE', 'UNLINK INVENTORY', @pkTarget, @auditOriginal, '{}', GETDATE(), @projectId)
	
END
