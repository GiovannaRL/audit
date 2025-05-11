CREATE TABLE [dbo].[assets_project] (
    [asset_id]        INT      NOT NULL,
    [asset_domain_id] SMALLINT NOT NULL,
    [project_id]      INT      NOT NULL,
    [domain_id]       SMALLINT NOT NULL,
    CONSTRAINT [equipment_project_pk] PRIMARY KEY CLUSTERED ([asset_id] ASC, [asset_domain_id] ASC, [project_id] ASC, [domain_id] ASC),
    CONSTRAINT [domain3_fk] FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]),
    CONSTRAINT [equipment_fk] FOREIGN KEY ([asset_id], [asset_domain_id]) REFERENCES [dbo].[assets] ([asset_id], [domain_id]),
    CONSTRAINT [project_fk] FOREIGN KEY ([project_id], [domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]) ON DELETE CASCADE
);

