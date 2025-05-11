CREATE TABLE [dbo].[project_documents]
(
	[id] INT IDENTITY (1, 1) NOT NULL,
	[filename] VARCHAR(200) NOT NULL,
	-- The type id is also registered in the document_types table, but the following types are avaialble:
	-- 1 - Shopdrawing
	-- 2 - Custom
	-- 3 - Room Image (also uploaded by the offline tool)
	-- 4 - Inventory Image (also uploaded by the offline tool)
	[type_id] INT NOT NULL,
	[date_added] DATE NOT NULL,
	[project_domain_id] SMALLINT NOT NULL,
	project_id INT NOT NULL,
	blob_file_name VARCHAR(100),
	[version] VARCHAR(30),
	[status] VARCHAR(50),
	[file_extension] VARCHAR(10),
	[label] VARCHAR(50) NULL, 
	[rotate] INT NOT NULL DEFAULT 0,
    [added_by] VARCHAR(50) NULL, 
    CONSTRAINT proj_documents_type_fk FOREIGN KEY (type_id) REFERENCES document_types(id),
	CONSTRAINT proj_documents_proj_fk FOREIGN KEY (project_id, project_domain_id) REFERENCES project(project_id, domain_id),
	CONSTRAINT proj_documents_pk PRIMARY KEY (id)
)
GO

CREATE INDEX [idx_project_documents_file] ON [dbo].[project_documents] ([project_domain_id], [project_id], [type_id]) include ([filename], [blob_file_name]);
GO

CREATE NONCLUSTERED INDEX [project_documents_type_index] ON [dbo].[project_documents] ([type_id]) INCLUDE ([project_domain_id],[project_id],[blob_file_name]);

GO


CREATE TRIGGER [dbo].[project_documents_upd]
    ON [dbo].[project_documents]
    AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NoCount ON;

		DECLARE @project_domain_id SMALLINT, @project_id INT, @document_id INT, @inventory_id INT,
			@type_id INT;

		DECLARE doc_assoc_cursor CURSOR LOCAL FOR 
			SELECT project_domain_id, project_id, id, type_id
			FROM inserted;

		OPEN doc_assoc_cursor;
		FETCH NEXT FROM doc_assoc_cursor INTO @project_domain_id, @project_id, @document_id, @type_id;  
		WHILE @@FETCH_STATUS = 0
		BEGIN

			IF (@type_id = 6 OR @type_id = 7)
				UPDATE project_documents
					SET type_id = 4 
				FROM documents_associations AS da
					INNER JOIN project_documents AS pd ON da.document_id = pd.id 
				WHERE da.document_id <> @document_id AND pd.type_id = @type_id AND
					da.inventory_id IN 
						(SELECT DISTINCT inventory_id FROM documents_associations WHERE document_id = @document_id);

			FETCH NEXT FROM doc_assoc_cursor INTO @project_domain_id, @project_id, @document_id, @type_id;  
		END
		CLOSE doc_assoc_cursor;
		DEALLOCATE doc_assoc_cursor;
    END