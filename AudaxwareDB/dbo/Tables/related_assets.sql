CREATE TABLE [dbo].[related_assets] (
    [asset_id]          INT      NOT NULL,
    [domain_id]         SMALLINT NOT NULL,
    [related_asset_id]  INT      NOT NULL,
    [related_domain_id] SMALLINT NOT NULL,
    CONSTRAINT [equipment_domain_related_pk] PRIMARY KEY CLUSTERED ([asset_id] ASC, [domain_id] ASC, [related_asset_id] ASC, [related_domain_id] ASC),
    CONSTRAINT [equipment_domain6_fk] FOREIGN KEY ([asset_id], [domain_id]) REFERENCES [dbo].[assets] ([asset_id], [domain_id]),
    CONSTRAINT [related_equipment_domain_fk] FOREIGN KEY ([related_asset_id], [related_domain_id]) REFERENCES [dbo].[assets] ([asset_id], [domain_id]) ON DELETE CASCADE
);

