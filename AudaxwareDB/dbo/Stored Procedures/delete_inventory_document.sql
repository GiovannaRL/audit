CREATE PROCEDURE [dbo].[delete_inventory_document](
	@domain_id SMALLINT,
	@from_inventory_id INTEGER,
	@document_id INTEGER)
AS
BEGIN

	DECLARE @my_source_id INTEGER, @my_target_id INTEGER;

	IF EXISTS (SELECT 1 FROM documents_associations WHERE project_domain_id = @domain_id AND inventory_id = @from_inventory_id
		AND document_id = @document_id)
		BEGIN

			SELECT @my_source_id = inventory_source_id, @my_target_id = inventory_target_id
			FROM project_room_inventory 
			WHERE inventory_id = @from_inventory_id AND domain_id = @domain_id;

			-- Delete document from the informed asset
			DELETE FROM documents_associations WHERE inventory_id = @from_inventory_id 
				AND project_domain_id = @domain_id AND document_id = @document_id;

			-- Delete from target and source or with same target/source
			DELETE da FROM documents_associations da
				INNER JOIN project_room_inventory pri on da.inventory_id = pri.inventory_id AND da.project_domain_id = pri.domain_id
			WHERE da.document_id = @document_id AND da.project_domain_id = @domain_id
				AND (pri.inventory_source_id = @from_inventory_id 
					OR pri.inventory_target_id = @from_inventory_id
					OR (@my_target_id IS NOT NULL AND (
							pri.inventory_target_id = @my_target_id OR pri.inventory_source_id = @my_target_id
						)
					)
					OR (
						@my_source_id IS NOT NULL AND (
							pri.inventory_source_id = @my_source_id OR pri.inventory_target_id = @my_source_id
						)
					)
				);

			-- Its necessary because of the project copies
			IF @my_source_id IS NOT NULL
				EXEC delete_inventory_document @domain_id, @my_source_id, @document_id;
			IF @my_target_id IS NOT NULL
				EXEC delete_inventory_document @domain_id, @my_target_id, @document_id;

		END
END
