/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
IF NOT EXISTS (SELECT * FROM [documents_display_levels] WHERE id = 1)
	INSERT INTO [documents_display_levels] VALUES('Display on project level');

IF NOT EXISTS (SELECT * FROM [documents_display_levels] WHERE id = 2)
	INSERT INTO [documents_display_levels] VALUES('Display on phase level');

IF NOT EXISTS (SELECT * FROM [documents_display_levels] WHERE id = 3)
	INSERT INTO [documents_display_levels] VALUES('Display on department level');

IF NOT EXISTS (SELECT * FROM [documents_display_levels] WHERE id = 4)
	INSERT INTO [documents_display_levels] VALUES('Display on room level');

IF NOT EXISTS (SELECT * FROM [documents_display_levels] WHERE id = 5)
	INSERT INTO [documents_display_levels] VALUES('Display on inventory level');

GO

UPDATE document_types SET display_level = 1 WHERE id IN (1, 2);
UPDATE document_types SET display_level = 4 WHERE id IN (3);

UPDATE document_types SET name = 'Room''s Images' where Id = 3;
INSERT INTO document_types VALUES('Inventory''s Images', 5);

GO

UPDATE project_documents SET type_id = 4
FROM project_documents pd left join documents_associations da
ON pd.id = da.document_id
WHERE da.inventory_id IS NOT NULL AND (pd.type_id = 3 or filename like 'inv_pic_%');

GO

UPDATE project_documents SET type_id = 3
FROM project_documents pd left join documents_associations da
ON pd.id = da.document_id
WHERE da.inventory_id IS NULL AND da.room_id IS NOT NULL
AND filename like 'room_pic_%';