CREATE TABLE [dbo].[project_phase] (
    [project_id]         INT            NOT NULL,
    [phase_id]           INT            IDENTITY (1, 1) NOT NULL,
    [description]        VARCHAR (50)   NULL,
    [start_date]         DATE           NOT NULL,
    [end_date]           DATE           NOT NULL,
    [plan_end]           DATE           NULL,
    [plan_start]         DATE           NULL,
    [sd_date]            DATE           NULL,
    [dd_date]            DATE           NULL,
    [cd_date]            DATE           NULL,
    [equip_move_in_date] DATE           NULL,
    [occupancy_date]     DATE           NULL,
    [date_added]         DATE           NULL,
    [added_by]           VARCHAR (50)   NULL,
    [comment]            VARCHAR (1000) NULL,
    [ofci_delivery]      DATE           NULL,
    [domain_id]          SMALLINT       NOT NULL,
	[copy_link] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
    CONSTRAINT [project_phase_pk] PRIMARY KEY CLUSTERED ([project_id] ASC, [phase_id] ASC, [domain_id] ASC),
    CONSTRAINT [project_project_phase_fk] FOREIGN KEY ([project_id], [domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]) ON DELETE CASCADE, 
    CONSTRAINT [CK_project_phase_start_end_date] CHECK ([start_date] <= [end_date])
);

GO
CREATE NONCLUSTERED INDEX [DescriptionIndex] ON [dbo].[project_phase] ([description])

GO
CREATE NONCLUSTERED INDEX [DomainIdIndex] ON [dbo].[project_phase] ([domain_id])

GO
CREATE NONCLUSTERED INDEX [ProjectIdIndex] ON [dbo].[project_phase] ([project_id])


GO
CREATE NONCLUSTERED INDEX [AddedByIndex] ON [dbo].[project_phase] ([added_by])

GO

CREATE TRIGGER [dbo].[project_phase_delete]
    ON [dbo].[project_phase]
    AFTER DELETE
    AS
    BEGIN
        DELETE FROM project_report WHERE CONCAT(project_domain_id, project_id, phase_id) IN
		(SELECT CONCAT(domain_id, project_id, phase_id) FROM deleted);
    END