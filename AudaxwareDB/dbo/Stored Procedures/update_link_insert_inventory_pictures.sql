-- This procedure is called from the insert/update trigger in the document_associations table and also on the update_link_inventory
CREATE PROCEDURE [dbo].[update_link_insert_inventory_pictures]
	@domain_id smallint,
	@from_id INT,
	@addedBy VARCHAR(50),
	@to_id INT = NULL,
	@from_document_id INT = NULL
AS
BEGIN
	DECLARE @to_project_id INT, @project_id INT, @document_id INT

	DECLARE linked_cursor CURSOR LOCAL FOR SELECT inventory_id, project_id	from project_room_inventory WHERE inventory_source_id = @from_id OR inventory_target_id = @from_id AND 
		(@to_id IS NULL OR inventory_id = @to_id);

	DECLARE doc_assoc_cursor CURSOR LOCAL FOR SELECT document_id FROM documents_associations WHERE project_domain_id = @domain_id AND inventory_id = @from_id
			AND (@from_document_id IS NULL OR document_id = @from_document_id);


	OPEN linked_cursor;
	FETCH NEXT FROM linked_cursor INTO @to_id, @to_project_id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		OPEN doc_assoc_cursor;
		FETCH NEXT FROM doc_assoc_cursor INTO @document_id;  
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF NOT EXISTS (SELECT TOP 1 project_domain_id from documents_associations WHERE project_domain_id = @domain_id AND
					project_id = @to_project_id AND document_id = @document_id AND 
					inventory_id = @to_id)
			BEGIN
				INSERT INTO documents_associations(project_domain_id, project_id, document_id, inventory_id, added_by, date_added) 
					VALUES(@domain_id, @to_project_id, @document_id, @to_id, @addedBy, GETDATE());
			END
			FETCH NEXT FROM doc_assoc_cursor INTO @document_id;  
		END
		CLOSE doc_assoc_cursor;
		FETCH NEXT FROM linked_cursor INTO @to_id, @to_project_id
	END
	DEALLOCATE doc_assoc_cursor;
	CLOSE linked_cursor;
	DEALLOCATE linked_cursor;
END
