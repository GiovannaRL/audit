CREATE TABLE [dbo].[asset_it_connectivity]
(
	[connectivity_id] INT  IDENTITY (1, 1) NOT NULL, 
    [project_id] INT NOT NULL, 
    [domain_id] SMALLINT NOT NULL, 
    [inventory_id_in] INT NOT NULL, 
    [inventory_id_out] INT NOT NULL, 
    [connection_type] VARCHAR(20) NULL DEFAULT 'cat6', 
    [port_number] VARCHAR(20) NULL, 
    [comment] VARCHAR(1000) NULL, 
    [date_added] DATETIME NOT NULL DEFAULT GETDATE(), 
    [added_by] VARCHAR(50) NOT NULL,
    [port_number_out] VARCHAR(20) NULL,

    CONSTRAINT [connectivity_id_pk] PRIMARY KEY CLUSTERED ([connectivity_id] ASC, [domain_id] ASC),
    CONSTRAINT [project_it_fk] FOREIGN KEY ([project_id],[domain_id]) REFERENCES [dbo].[project] ([project_id],[domain_id])ON DELETE CASCADE,
    CONSTRAINT [inventory_in_fk] FOREIGN KEY ([inventory_id_in]) REFERENCES [dbo].[project_room_inventory] ([inventory_id]),
    CONSTRAINT [inventory_out_fk] FOREIGN KEY ([inventory_id_out]) REFERENCES [dbo].[project_room_inventory] ([inventory_id])
)
