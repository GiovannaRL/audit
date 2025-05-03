CREATE TABLE [dbo].[project_room] (
    [project_id]                         INT            NOT NULL,
    [department_id]                      INT            NOT NULL,
    [room_id]                            INT            IDENTITY (1, 1) NOT NULL,
    [drawing_room_name]                  VARCHAR (100)   NOT NULL,
    [drawing_room_number]                VARCHAR (32)   NULL,
    [final_room_name]                    VARCHAR (100)   NULL,
    [final_room_number]                  VARCHAR (32)   NULL,
    [date_added]                         DATE           NULL,
    [added_by]                           VARCHAR (50)   NULL,
    [comment]                            VARCHAR (1000) NULL,
    [phase_id]                           INT            NOT NULL,
    [domain_id]                          SMALLINT       NOT NULL,
    [project_id_template]                INT            NULL,
    [project_domain_id_template]         SMALLINT       NULL,
    [applied_id_template]                INT            NULL,
	[linked_template]					 BIT			NULL,
    [is_template]                        BIT            DEFAULT ((0)) NULL,
    [department_type_id_template]        INT            NULL,
    [department_type_domain_id_template] SMALLINT       NULL,
    [id]                                 INT            CONSTRAINT [DF_seq_id] DEFAULT (NEXT VALUE FOR [seq_project_room_id]) NULL,
    [room_quantity] INT NOT NULL DEFAULT 1, 
    [room_code] VARCHAR(20) NULL, 
    [room_area] NUMERIC(10, 2) NULL, 
    [blueprint] VARCHAR(50) NULL, 
    [staff] VARCHAR(50) NULL, 
    [functional_area] VARCHAR(50) NULL,
	[copy_link] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
    CONSTRAINT [project_room_pk] PRIMARY KEY CLUSTERED ([project_id] ASC, [phase_id] ASC, [department_id] ASC, [room_id] ASC, [domain_id] ASC),
    FOREIGN KEY ([department_type_id_template], [department_type_domain_id_template]) REFERENCES [dbo].[department_type] ([department_type_id], [domain_id]),
    FOREIGN KEY ([project_id_template], [project_domain_id_template]) REFERENCES [dbo].[project] ([project_id], [domain_id]),
    CONSTRAINT [project_phase_department_project_room_fk] FOREIGN KEY ([project_id], [phase_id], [department_id], [domain_id]) REFERENCES [dbo].[project_department] ([project_id], [phase_id], [department_id], [domain_id]) 
);


GO
CREATE NONCLUSTERED INDEX [template_domain]
    ON [dbo].[project_room]([domain_id] ASC, [is_template] ASC);

GO
CREATE NONCLUSTERED INDEX [DrawingRoomNumberIndex] ON [dbo].[project_room] ([drawing_room_number])

GO
CREATE NONCLUSTERED INDEX [DrawingRoomNameIndex] ON [dbo].[project_room] ([drawing_room_name])

GO
CREATE NONCLUSTERED INDEX [DomainIdIndex] ON [dbo].[project_room] ([domain_id])

GO
CREATE NONCLUSTERED INDEX [ProjectIdIndex] ON [dbo].[project_room] ([project_id])

GO
CREATE NONCLUSTERED INDEX [PhaseIdIndex] ON [dbo].[project_room] ([phase_id])

GO
CREATE NONCLUSTERED INDEX [AddedByIndex] ON [dbo].[project_room] ([added_by])


GO
CREATE TRIGGER ADD_TEMPLATE
ON project_room AFTER INSERT
AS
	DECLARE @is_template BIT, @id INT, @domain_id SMALLINT, @p_id INT, @ph_id INT, @d_id INT, @r_id INT;

	SELECT @is_template = is_template, @id = id, @domain_id = domain_id, @p_id = project_id, @ph_id = phase_id, @d_id = department_id,
		@r_id = room_id FROM inserted;

	IF @is_template = 1 AND @id IS NULL
		BEGIN
			UPDATE project_room SET id = (NEXT VALUE FOR [seq_project_room_id]) WHERE domain_id = @domain_id AND project_id = @p_id
				AND phase_id = @ph_id AND department_id = @d_id AND room_id = @r_id;
		END
	ELSE IF @is_template = 0 AND @id IS NOT NULL
		UPDATE project_room SET id = NULL WHERE domain_id = @domain_id AND project_id = @p_id AND phase_id = @ph_id AND 
			department_id = @d_id AND room_id = @r_id;

GO

--WHEN THE ROOM QUANTITY IS UPDATED THIS WILL TRIGGER THE PROJECT_ROOM_INVENTORY TO REDO THE CALCULATIONS
CREATE TRIGGER UPDATE_FINANCIALS
ON project_room AFTER UPDATE
AS

	UPDATE pri set pri.comment = pri.comment
	from project_room_inventory pri
	join inserted i on i.project_id = pri.project_id and i.domain_id = pri.domain_id and i.phase_id = pri.phase_id and i.department_id = pri.department_id and i.room_id = pri.room_id

GO


CREATE TRIGGER [dbo].[DELETE_REPORT_LOCATIONS]
    ON [dbo].[project_room]
    AFTER DELETE
    AS
    BEGIN
        SET NoCount ON;

		DELETE FROM report_location WHERE EXISTS (SELECT 1 FROM deleted d 
			WHERE d.domain_id = report_location.project_domain_id AND d.project_id = report_location.project_id
				AND d.phase_id = report_location.phase_id AND d.department_id = report_location.department_id
				AND d.room_id = report_location.room_id);
    END