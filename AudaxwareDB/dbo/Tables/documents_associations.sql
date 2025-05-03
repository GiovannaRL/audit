CREATE TABLE [dbo].[documents_associations]	
(
	[id] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	project_domain_id SMALLINT NOT NULL,
	project_id INT NOT NULL,
	document_id INT NOT NULL,
	phase_id INT,
	department_id INT,
	room_id INT,
	inventory_id INTEGER,
	asset_domain_id SMALLINT,
	asset_id INT,
	[added_by] VARCHAR(50) NULL, 
    [date_added] DATE NULL, 
    CONSTRAINT doc_association_project_fk FOREIGN KEY (project_id, project_domain_id) REFERENCES project(project_id, domain_id),
	CONSTRAINT doc_association_asset_fk FOREIGN KEY (asset_id, asset_domain_id) REFERENCES assets(asset_id, domain_id),
	CONSTRAINT doc_association_doc_fk FOREIGN KEY (document_id) REFERENCES project_documents(id) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT doc_association_inventory_fk FOREIGN KEY (inventory_id) REFERENCES project_room_inventory(inventory_id) ON DELETE CASCADE ON UPDATE CASCADE
)

GO

CREATE TRIGGER [dbo].[documents_associations_check]
    ON [dbo].[documents_associations]
    AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NoCount ON;

		DECLARE @project_domain_id SMALLINT, @project_id INT, @phase_id INT, @department_id INT,
			@room_id INT, @asset_domain_id SMALLINT, @asset_id INT, @canAdd BIT, @document_id INT, @id INT, @inventory_id INT;

		DECLARE doc_assoc_cursor CURSOR LOCAL FOR 
			SELECT project_domain_id, project_id, phase_id, department_id, room_id, asset_domain_id, asset_id, 
				document_id, id, inventory_id
			FROM inserted;

		OPEN doc_assoc_cursor;
		FETCH NEXT FROM doc_assoc_cursor INTO @project_domain_id, @project_id, @phase_id, @department_id,
			@room_id, @asset_domain_id, @asset_id, @document_id, @id, @inventory_id;  
		WHILE @@FETCH_STATUS = 0
		BEGIN

			SET @canAdd = 0;

			IF @asset_domain_id IS NOT NULL AND @asset_id IS NOT NULL
				SELECT @canAdd = 1 WHERE EXISTS(SELECT * FROM project_room_inventory WHERE domain_id = @project_domain_id
					AND project_id = @project_id AND phase_id = @phase_id AND department_id = @department_id
					AND room_id = @room_id AND asset_domain_id = @asset_domain_id AND asset_id = @asset_id);
			ELSE IF @room_id IS NOT NULL
				SELECT @canAdd = 1 WHERE EXISTS(SELECT * FROM project_room WHERE domain_id = @project_domain_id
					AND project_id = @project_id AND phase_id = @phase_id AND department_id = @department_id
					AND room_id = @room_id);
			ELSE IF @department_id IS NOT NULL
				SELECT @canAdd = 1 WHERE EXISTS(SELECT * FROM project_department WHERE domain_id = @project_domain_id
					AND project_id = @project_id AND phase_id = @phase_id AND department_id = @department_id);
			ELSE IF @phase_id IS NOT NULL
				SELECT @canAdd = 1 WHERE EXISTS(SELECT * FROM project_phase WHERE domain_id = @project_domain_id
					AND project_id = @project_id AND phase_id = @phase_id);
			ELSE
				SET @canAdd = 1;

			IF @canAdd = 0
				BEGIN
				IF EXISTS(SELECT * FROM deleted WHERE id = @id)
					BEGIN
						UPDATE da SET da.phase_id = d.phase_id, da.department_id = d.department_id,
							da.room_id = d.room_id, da.asset_domain_id = d.asset_domain_id, da.asset_id =
							d.asset_id, da.inventory_id = d.inventory_id 
							FROM documents_associations AS da 
							LEFT JOIN deleted AS d ON d.id = da.id
							WHERE da.id = @id;
					END
				ELSE
					BEGIN
						DELETE FROM documents_associations WHERE id = @id;
					END
				END
			ELSE
				BEGIN
						IF @inventory_id IS NOT NULL
						BEGIN
							EXEC update_link_insert_inventory_pictures @project_domain_id, @inventory_id, NULL, @document_id
						END
				END

			FETCH NEXT FROM doc_assoc_cursor INTO @project_domain_id, @project_id, @phase_id, @department_id, @room_id, 
				@asset_domain_id, @asset_id, @document_id, @id, @inventory_id;
		END
		CLOSE doc_assoc_cursor;
		DEALLOCATE doc_assoc_cursor;
    END

GO 


CREATE INDEX [idx_documents_associations_room] ON [dbo].[documents_associations] ([project_domain_id], [project_id], [document_id], [phase_id], [department_id], [room_id])
GO


CREATE INDEX [idx_documents_associations_inventory] ON [dbo].[documents_associations] ([project_domain_id], [project_id], [document_id], [inventory_id])
GO

CREATE NONCLUSTERED INDEX [idx_document_associations_azure_diagnostics_recommended]
	ON [dbo].[documents_associations] ([inventory_id], [project_domain_id], [project_id], [asset_domain_id], [asset_id], [department_id], [phase_id], [room_id]) WITH (ONLINE = ON)
GO

