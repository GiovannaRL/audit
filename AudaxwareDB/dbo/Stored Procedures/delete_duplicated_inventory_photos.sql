CREATE PROCEDURE [dbo].[delete_duplicated_inventory_photos]
	@project_id INTEGER
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @filename VARCHAR(200), @inventory_id INT, @count INT;
	DECLARE delete_cursor CURSOR FOR
	SELECT pd.filename, da.inventory_id, COUNT(*) FROM project_documents pd
	INNER JOIN documents_associations da ON pd.id = da.document_id
	WHERE da.project_id = @project_id AND da.inventory_id IS NOT NULL
	GROUP BY filename, inventory_id
	HAVING COUNT(*) > 1

	OPEN delete_cursor
	FETCH NEXT FROM delete_cursor INTO @filename, @inventory_id, @count
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DELETE TOP(@count - 1) pd FROM project_documents pd
		INNER JOIN documents_associations da ON pd.id = da.document_id
		WHERE da.project_id = @project_id AND pd.filename = @filename AND da.inventory_id = @inventory_id AND pd.type_id = 4
	FETCH NEXT FROM delete_cursor INTO @filename, @inventory_id, @count
	END
	CLOSE delete_cursor;
	DEALLOCATE delete_cursor;
END