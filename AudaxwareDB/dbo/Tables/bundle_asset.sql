CREATE TABLE [dbo].[bundle_asset] (
    [bundle_domain_id] SMALLINT NOT NULL,
    [bundle_id]        INT      NOT NULL,
    [asset_domain_id]  SMALLINT NOT NULL,
    [asset_id]         INT      NOT NULL,
    [asset_qty]        INT      DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([bundle_domain_id] ASC, [bundle_id] ASC, [asset_domain_id] ASC, [asset_id] ASC),
    CONSTRAINT [bundle_asset_qty_check] CHECK ([asset_qty]>(0)),
    CONSTRAINT [bundle_asset_asset_fk] FOREIGN KEY ([asset_id], [asset_domain_id]) REFERENCES [dbo].[assets] ([asset_id], [domain_id]),
    CONSTRAINT [bundle_asset_bundle_fk] FOREIGN KEY ([bundle_domain_id], [bundle_id]) REFERENCES [dbo].[bundle] ([domain_id], [bundle_id])
);

