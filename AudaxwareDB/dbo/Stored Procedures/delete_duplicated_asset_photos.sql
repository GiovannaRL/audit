CREATE PROCEDURE [dbo].[delete_duplicated_asset_photos]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @filename VARCHAR(200), @project_id INT, @inventory_id INT, @type_id INT, @count INT;
    DECLARE delete_cursor CURSOR FOR
    SELECT 
        pd.filename,
        da.project_id,
        da.inventory_id, 
        pd.type_id, 
        COUNT(*) 
    FROM 
        project_documents pd
    INNER JOIN 
        documents_associations da ON pd.id = da.document_id
    WHERE 
        da.inventory_id IS NOT NULL AND 
        pd.type_id IN (4,6,7)
    GROUP BY 
        pd.filename, da.project_id, da.inventory_id, pd.type_id
    HAVING 
        COUNT(*) > 1
    ORDER BY 
        da.project_id, da.inventory_id, pd.filename, pd.type_id;

    OPEN delete_cursor;
    FETCH NEXT FROM delete_cursor INTO @filename, @project_id, @inventory_id, @type_id, @count;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DELETE FROM documents_associations
        WHERE id IN (
            SELECT id FROM (
                SELECT TOP (@count - 1) da.id
                FROM documents_associations da
                INNER JOIN project_documents pd ON da.document_id = pd.id
                WHERE pd.filename = @filename AND da.project_id = @project_id AND da.inventory_id = @inventory_id AND pd.type_id = @type_id
                ORDER BY da.id DESC
            ) AS duplicates
        );

        FETCH NEXT FROM delete_cursor INTO @filename, @project_id, @inventory_id, @type_id, @count;
    END;

    CLOSE delete_cursor;
    DEALLOCATE delete_cursor;  
END;
