CREATE TABLE [dbo].[report_location] (
    [report]            INT      NOT NULL,
    [project_domain_id] SMALLINT NOT NULL,
    [project_id]        INT      NOT NULL,
    [phase_id]          INT      NOT NULL,
    [department_id]     INT      NOT NULL,
    [room_id]           INT      NOT NULL,
    PRIMARY KEY CLUSTERED ([report] ASC, [project_domain_id] ASC, [project_id] ASC, [phase_id] ASC, [department_id] ASC, [room_id] ASC),
    FOREIGN KEY ([report]) REFERENCES [dbo].[project_report] ([id]) ON DELETE CASCADE
);


GO

CREATE TRIGGER [dbo].[report_location_insert]
    ON [dbo].[report_location]
    INSTEAD OF INSERT
    AS
    BEGIN
        SET NoCount ON

		DECLARE @project_id INTEGER, @domain_id SMALLINT, @phase_id INTEGER, @department_id INTEGER, @room_id INTEGER, @count INTEGER, @report INTEGER;

	DECLARE [cursor] CURSOR LOCAL FOR SELECT project_domain_id, project_id, phase_id, department_id, room_id, report FROM inserted;
	OPEN [cursor];

	FETCH NEXT FROM [cursor] INTO @domain_id, @project_id, @phase_id, @department_id, @room_id, @report;
	WHILE @@FETCH_STATUS = 0
		BEGIN
			
			SELECT @count = COUNT(*) FROM project_room WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id
				AND department_id = @department_id AND room_id = @room_id;

			IF @count = 1
				BEGIN
					INSERT INTO report_location VALUES(@report, @domain_id, @project_id, @phase_id, @department_id, @room_id);
				END

			FETCH NEXT FROM [cursor] INTO @domain_id, @project_id, @phase_id, @department_id, @room_id, @report;
		END
    END