CREATE TABLE [dbo].[bundle_inventory] (
    [bundle_domain_id] SMALLINT NOT NULL,
    [bundle_id]        INT      NOT NULL,
    [inventory_id]     INT      NOT NULL,
    [bundle_qty]       INT      DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([bundle_domain_id] ASC, [bundle_id] ASC, [inventory_id] ASC),
    CHECK ([bundle_qty]>(0)),
    CONSTRAINT [bundle_inventory_bundle_fk] FOREIGN KEY ([bundle_domain_id], [bundle_id]) REFERENCES [dbo].[bundle] ([domain_id], [bundle_id]),
    CONSTRAINT [bundle_inventory_project_room_inventory_fk] FOREIGN KEY ([inventory_id]) REFERENCES [dbo].[project_room_inventory] ([inventory_id])
);

