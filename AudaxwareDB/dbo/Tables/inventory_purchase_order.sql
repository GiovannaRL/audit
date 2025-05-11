CREATE TABLE [dbo].[inventory_purchase_order] (
    [project_id]      INT              NOT NULL,
    [po_id]           INT              NOT NULL,
    [po_qty]          INT              NULL,
    [po_unit_amt]     DECIMAL (28, 16) NULL,
    [po_status]       VARCHAR (20)     NULL,
    [date_added]      DATE             NULL,
    [added_by]        VARCHAR (50)     NULL,
    [inventory_id]    INT              NOT NULL,
    [asset_id]        INT              NOT NULL,
    [asset_domain_id] INT              NOT NULL,
    [po_domain_id]    SMALLINT         NOT NULL,
    CONSTRAINT [inventory_purchase_order_pk] PRIMARY KEY CLUSTERED ([inventory_id] ASC, [po_id] ASC, [po_domain_id] ASC, [project_id] ASC),
    CONSTRAINT [project_room_inventory_inventory_purchase_order_fk] FOREIGN KEY ([inventory_id]) REFERENCES [dbo].[project_room_inventory] ([inventory_id]),
    CONSTRAINT [purchase_order_inventory_purchase_order_fk] FOREIGN KEY ([po_id], [po_domain_id], [project_id]) REFERENCES [dbo].[purchase_order] ([po_id], [domain_id], [project_id]) ON DELETE CASCADE/*,
	UNIQUE NONCLUSTERED ([inventory_id] ASC)*/
);


GO
CREATE NONCLUSTERED INDEX [inventory_id_ipo]
    ON [dbo].[inventory_purchase_order]([inventory_id] ASC);

GO

CREATE NONCLUSTERED INDEX [project_po_domain_asset_qty_inventory_purchase_order] 
	ON [dbo].[inventory_purchase_order] ([project_id], [po_id], [po_domain_id], [asset_domain_id], [asset_id], [po_qty]) INCLUDE ([po_unit_amt]) WITH (ONLINE = ON)
GO


CREATE TRIGGER [dbo].[ipo_insert]
    ON [dbo].[inventory_purchase_order]
    AFTER INSERT
	AS
    BEGIN
        SET NoCount ON;

		DECLARE @asset_domain_id SMALLINT, @asset_id INTEGER, @vendor_domain_id SMALLINT, @vendor_id INTEGER;
	
		DECLARE insert_ipo_cursor CURSOR LOCAL FOR SELECT i.asset_domain_id, i.asset_id, po.vendor_domain_id, po.vendor_id
			FROM inserted i LEFT JOIN purchase_order po 
			ON i.po_domain_id = po.domain_id AND i.po_id = po.po_id AND i.project_id = po.project_id;

		OPEN insert_ipo_cursor;
		FETCH NEXT FROM insert_ipo_cursor INTO @asset_domain_id, @asset_id, @vendor_domain_id, @vendor_id;  
		WHILE @@FETCH_STATUS = 0
		BEGIN

			IF NOT EXISTS(SELECT * FROM assets_vendor WHERE asset_domain_id = @asset_domain_id AND
				asset_id = @asset_id AND vendor_domain_id = @vendor_domain_id AND vendor_id = @vendor_id)
				INSERT INTO assets_vendor(asset_domain_id, asset_id, vendor_domain_id, vendor_id)
					VALUES (@asset_domain_id, @asset_id, @vendor_domain_id, @vendor_id);

			FETCH NEXT FROM insert_ipo_cursor INTO @asset_domain_id, @asset_id, @vendor_domain_id, @vendor_id;
		END
		CLOSE insert_ipo_cursor;  
		DEALLOCATE insert_ipo_cursor;
    END
GO

CREATE TRIGGER [dbo].[ipo_delete]
    ON [dbo].[inventory_purchase_order]
    AFTER DELETE
    AS
    BEGIN
        SET NoCount ON;

		DECLARE @asset_domain_id SMALLINT, @asset_id INTEGER, @vendor_domain_id SMALLINT, 
			@vendor_id INTEGER;
	
		DECLARE delete_ipo_cursor CURSOR LOCAL FOR SELECT d.asset_domain_id, d.asset_id, po.vendor_domain_id, po.vendor_id
			FROM deleted d LEFT JOIN
			purchase_order po ON d.po_domain_id = po.domain_id AND d.po_id = po.po_id AND d.project_id = po.project_id;

		OPEN delete_ipo_cursor;
		FETCH NEXT FROM delete_ipo_cursor INTO @asset_domain_id, @asset_id, @vendor_domain_id,
			@vendor_id;  
		WHILE @@FETCH_STATUS = 0
		BEGIN

			IF NOT EXISTS(SELECT * FROM inventory_purchase_order ipo LEFT JOIN purchase_order po ON
				ipo.po_domain_id = po.domain_id AND ipo.po_id = po.po_id AND ipo.project_id = po.project_id
				WHERE ipo.asset_domain_id = @asset_domain_id AND ipo.asset_id = @asset_id AND 
					po.vendor_domain_id = @vendor_id AND po.vendor_id = @vendor_id)
				DELETE FROM assets_vendor WHERE asset_domain_id = @asset_domain_id AND asset_id = @asset_id
					AND vendor_domain_id = @vendor_domain_id AND vendor_id = @vendor_id

			FETCH NEXT FROM delete_ipo_cursor INTO @asset_domain_id, @asset_id, @vendor_domain_id, @vendor_id;
		END
		CLOSE delete_ipo_cursor;  
		DEALLOCATE delete_ipo_cursor;
    END
GO

CREATE TRIGGER [dbo].[update_purchase_order_invalid_po]
	ON [dbo].[inventory_purchase_order]
	AFTER INSERT, UPDATE, DELETE
	AS
	BEGIN
		DECLARE @domain_id SMALLINT, @project_id INT, @po_id INTEGER;

		IF EXISTS (SELECT * FROM deleted) AND NOT EXISTS (SELECT * FROM inserted)
		BEGIN
			SELECT @domain_id = po_domain_id, @project_id = project_id, @po_id = po_id FROM deleted;		
		END
		ELSE
		BEGIN
			SELECT @domain_id = po_domain_id, @project_id = project_id, @po_id = po_id FROM inserted;
		END

		EXEC [dbo].[update_valid_po] @domain_id, @project_id, @po_id;
	END
GO
