CREATE TABLE [dbo].[bundle] (
    [domain_id]         SMALLINT       NOT NULL,
    [bundle_id]         INT            IDENTITY (1, 1) NOT NULL,
    [name]              VARCHAR (200)  NOT NULL,
    [project_domain_id] SMALLINT       NULL,
    [project_id]        INT            NULL,
    [added_by]          NVARCHAR (256) NOT NULL,
    [date_added]        DATETIME       NOT NULL,
    [comment]           VARCHAR (1000) NULL,
    PRIMARY KEY CLUSTERED ([domain_id] ASC, [bundle_id] ASC),
    CONSTRAINT [bundle_project_domain_check] CHECK ([project_domain_id] IS NULL OR [project_domain_id]=[domain_id]),
    CONSTRAINT [bundle_domain_fk] FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]),
    CONSTRAINT [buneld_project_fk] FOREIGN KEY ([project_id], [project_domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]),
    CONSTRAINT [bundle_unique] UNIQUE NONCLUSTERED ([domain_id] ASC, [project_domain_id] ASC, [project_id] ASC, [name] ASC)
);

