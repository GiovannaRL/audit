CREATE TABLE [dbo].[phase_documents] (
    [project_id] INT          NOT NULL,
    [phase_id]   INT          NOT NULL,
    [drawing_id] INT          IDENTITY (1, 1) NOT NULL,
    [filename]   VARCHAR (50) NOT NULL,
    [date_added] DATE         NOT NULL,
    [domain_id]  SMALLINT     NOT NULL,
    CONSTRAINT [equipment_drawing_pk] PRIMARY KEY CLUSTERED ([drawing_id] ASC),
    CONSTRAINT [project_phase_equipment_drawing_fk] FOREIGN KEY ([project_id], [phase_id], [domain_id]) REFERENCES [dbo].[project_phase] ([project_id], [phase_id], [domain_id]) ON DELETE CASCADE
);

