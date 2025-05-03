CREATE TABLE [dbo].[project_department] (
    [project_id]                INT            NOT NULL,
    [department_id]             INT            IDENTITY (1, 1) NOT NULL,
    [description]               VARCHAR (100)   NOT NULL,
    [department_type_id]        INT            NOT NULL,
    [phase_id]                  INT            NOT NULL,
    [area]                      INT            NULL,
    [contact_name]              VARCHAR (50)   NULL,
    [contact_email]             VARCHAR (50)   NULL,
    [contact_phone]             VARCHAR (25)   NULL,
    [date_added]                DATE           NULL,
    [added_by]                  VARCHAR (50)   NULL,
    [comment]                   VARCHAR (1000) NULL,
    [department_type_domain_id] SMALLINT       NOT NULL,
    [domain_id]                 SMALLINT       NOT NULL,
	[copy_link] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
    CONSTRAINT [project_department_pk] PRIMARY KEY CLUSTERED ([project_id] ASC, [phase_id] ASC, [department_id] ASC, [domain_id] ASC),
    CONSTRAINT [department_type_domain_fk] FOREIGN KEY ([department_type_domain_id]) REFERENCES [dbo].[domain] ([domain_id]),
    CONSTRAINT [department_type_project_department_fk] FOREIGN KEY ([department_type_id], [department_type_domain_id]) REFERENCES [dbo].[department_type] ([department_type_id], [domain_id]),
    CONSTRAINT [project_phase_project_department_fk] FOREIGN KEY ([project_id], [phase_id], [domain_id]) REFERENCES [dbo].[project_phase] ([project_id], [phase_id], [domain_id]) ON DELETE CASCADE
);


GO
ALTER TABLE [dbo].[project_department] NOCHECK CONSTRAINT [department_type_project_department_fk];

GO
CREATE NONCLUSTERED INDEX [DescriptionIndex] ON [dbo].[project_department] ([description])

GO
CREATE NONCLUSTERED INDEX [DomainIdIndex] ON [dbo].[project_department] ([domain_id])

GO
CREATE NONCLUSTERED INDEX [ProjectIdIndex] ON [dbo].[project_department] ([project_id])

GO
CREATE NONCLUSTERED INDEX [PhaseIdIndex] ON [dbo].[project_department] ([phase_id])


GO
CREATE NONCLUSTERED INDEX [AddedByIndex] ON [dbo].[project_department] ([added_by])

GO
CREATE TRIGGER [dbo].[project_department_delete]
    ON [dbo].[project_department]
    AFTER DELETE
    AS
    BEGIN
        SET NoCount ON

		DELETE FROM project_report WHERE CONCAT(project_domain_id, project_id, phase_id, department_id) IN
		(SELECT CONCAT(domain_id, project_id, phase_id, department_id) FROM deleted);
    END