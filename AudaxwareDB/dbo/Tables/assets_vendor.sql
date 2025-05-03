CREATE TABLE [dbo].[assets_vendor] (
    [asset_id]         INT             NOT NULL,
    [vendor_id]        INT             NOT NULL,
    [min_cost]         NUMERIC (10, 2) CONSTRAINT [DF_assets_vendor_min_cost] DEFAULT ((0)) NULL,
    [max_cost]         NUMERIC (10, 2) CONSTRAINT [DF_assets_vendor_max_cost] DEFAULT ((0)) NULL,
    [avg_cost]         NUMERIC (10, 2) CONSTRAINT [DF_assets_vendor_avg_cost] DEFAULT ((0)) NULL,
    [date_added]       DATE            NULL,
    [added_by]         VARCHAR (50)    NULL,
    [comment]          VARCHAR (250)   NULL,
    [model_number]     VARCHAR (50)    NULL,
    [asset_domain_id]  SMALLINT        NOT NULL,
    [vendor_domain_id] SMALLINT        NOT NULL,
    [last_cost]        NUMERIC (10, 2) NULL,
    CONSTRAINT [vendor_equipment_pk] PRIMARY KEY CLUSTERED ([asset_id] ASC, [asset_domain_id] ASC, [vendor_id] ASC, [vendor_domain_id] ASC),
    CONSTRAINT [asset_domain_fk] FOREIGN KEY ([asset_id], [asset_domain_id]) REFERENCES [dbo].[assets] ([asset_id], [domain_id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [vendor_vendor_equipment_fk] FOREIGN KEY ([vendor_id], [vendor_domain_id]) REFERENCES [dbo].[vendor] ([vendor_id], [domain_id])
);

