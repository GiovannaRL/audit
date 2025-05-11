CREATE TABLE [dbo].[project_contact] (
    [project_id] INT      NOT NULL,
    [contact_id] INT      NOT NULL,
    [domain_id]  SMALLINT NOT NULL,
    CONSTRAINT [project_contact_pk] PRIMARY KEY CLUSTERED ([project_id] ASC, [contact_id] ASC, [domain_id]),
    CONSTRAINT [global_contact_project_contact_fk] FOREIGN KEY ([contact_id]) REFERENCES [dbo].[global_contact] ([contact_id]),
    CONSTRAINT [project_project_contact_fk] FOREIGN KEY ([project_id], [domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]) ON DELETE CASCADE
);

