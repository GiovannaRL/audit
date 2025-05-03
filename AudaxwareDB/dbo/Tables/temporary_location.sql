CREATE TABLE [dbo].[temporary_location] (
    [id]					  INT           IDENTITY (1, 1) NOT NULL,
    [project_id]              INT           NULL,
    [domain_id]               SMALLINT      NOT NULL,
	[name]					  VARCHAR(200)  NOT NULL,
	CONSTRAINT [temporary_location_pkey] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [temporary_location_project_fk] FOREIGN KEY ([project_id], [domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]) ON DELETE CASCADE,
    CONSTRAINT [temporary_location_unique] UNIQUE NONCLUSTERED ([domain_id] ASC, [project_id] ASC, [name] ASC)
);
