CREATE TABLE [dbo].[purchase_order] (
    [po_id]                INT             IDENTITY (1, 1) NOT NULL,
    [project_id]           INT             NOT NULL,
    [po_number]            VARCHAR (50)    NULL,
    [quote_number]         VARCHAR (50)    NULL,
    [description]          VARCHAR (250)   NULL,
    [vendor_id]            INT             NOT NULL,
    [quote_requested_date] DATE            NULL,
    [quote_received_date]  DATE            NULL,
    [po_requested_date]    DATE            NULL,
    [po_received_date]     DATE            NULL,
    [freight]              NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [warehouse]            NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [tax]                  NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [warranty]             NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [misc]                 NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [status]               VARCHAR (25)    NULL,
    [date_added]           DATE            NULL,
    [added_by]             VARCHAR (50)    NULL,
    [comment]              VARCHAR (1000)  NULL,
    [quote_file]           VARCHAR (100)   NULL,
    [po_file]              VARCHAR (100)   NULL,
    [upd_asset_value]      BIT             DEFAULT ((1)) NOT NULL,
    [vendor_domain_id]     SMALLINT        NOT NULL,
    [po_requested_number]  VARCHAR (50)    NULL,
    [domain_id]            SMALLINT        NOT NULL,
    [ship_to]              INT             NULL,
    [phase_id]             INT             NULL,
    [department_id]        INT             NULL,
    [room_id]              INT             NULL,
    [quote_amount]         NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [allow_assets_update]  BIT             DEFAULT ((0)) NULL,
    [old_po_id] INT NULL, 
    [install] NUMERIC(15, 2) NULL DEFAULT ((0)), 
	[copy_link] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [invalid_po] BIT NOT NULL DEFAULT ((0)), /*This field is set to true when the PO quote does not match the sum of all values*/
	[quote_expiration_date]  DATE            NULL,
    [quote_discount] NUMERIC(15, 2) NULL DEFAULT ((0)), 
    CONSTRAINT [purchase_order_pk] PRIMARY KEY CLUSTERED ([po_id] ASC, [domain_id] ASC, [project_id] ASC),
    CONSTRAINT [project_addresses_fk] FOREIGN KEY ([ship_to]) REFERENCES [dbo].[project_addresses] ([id]),
    CONSTRAINT [project_purchase_order_fk] FOREIGN KEY ([project_id], [domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]) ON DELETE CASCADE,
    CONSTRAINT [vendor_purchase_order_fk] FOREIGN KEY ([vendor_id], [vendor_domain_id]) REFERENCES [dbo].[vendor] ([vendor_id], [domain_id]),
	CONSTRAINT [project_room_po_fk] FOREIGN KEY ([project_id], [phase_id], [department_id], [room_id], [domain_id]) REFERENCES [dbo].[project_room] ([project_id], [phase_id], [department_id], [room_id], [domain_id])
);


GO

CREATE NONCLUSTERED INDEX [po_index] 
	ON [dbo].[purchase_order] ([domain_id], [project_id], [department_id], [phase_id], [room_id]) INCLUDE ([freight], [install], [misc], [tax], [warehouse], [warranty]) WITH (ONLINE = ON)
GO

CREATE TRIGGER [dbo].[purchase_order_ship_to]
ON [dbo].[purchase_order]
AFTER INSERT 
AS 
	DECLARE @project_id int, @domain_id smallint, @po_id INTEGER, @id INT;

	DECLARE [cursor] CURSOR LOCAL FOR SELECT domain_id, project_id, po_id from inserted;
	OPEN [cursor];

	FETCH NEXT FROM [cursor] INTO @domain_id, @project_id, @po_id;
	WHILE @@FETCH_STATUS = 0
		BEGIN

			SET @id = NULL;

			SELECT @id = id FROM project_addresses WHERE domain_id = @domain_id AND project_id = @project_id AND is_default = 1;

			IF @id IS NOT NULL
				UPDATE purchase_order SET ship_to = @id WHERE domain_id = @domain_id AND project_id = @project_id AND po_id = @po_id;	

			FETCH NEXT FROM [cursor] INTO @domain_id, @project_id, @po_id;
		END
	CLOSE [cursor];
	DEALLOCATE [cursor];
    GO

    CREATE TRIGGER [dbo].[update_invalid_po]
	ON [dbo].[purchase_order]
	AFTER UPDATE
	AS
	BEGIN
		DECLARE @domain_id SMALLINT, @project_id INT, @po_id INTEGER;
        SELECT @domain_id = domain_id, @project_id = project_id, @po_id = po_id FROM inserted;


		EXEC [dbo].[update_valid_po] @domain_id, @project_id, @po_id;
	END
GO