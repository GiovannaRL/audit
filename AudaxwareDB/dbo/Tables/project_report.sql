CREATE TABLE [dbo].[project_report] (
    [id]                     INT             IDENTITY (1, 1) NOT NULL,
    [report_type_id]         INT             NOT NULL,
    [project_domain_id]      SMALLINT        NOT NULL,
    [project_id]             INT             NOT NULL,
    [isPrivate]              BIT             DEFAULT ((0)) NOT NULL,
    [generated_by]           NVARCHAR (128)  NOT NULL,
    [name]                   VARCHAR (50)    NOT NULL,
    [description]            VARCHAR (200)   NULL,
    [last_run]               DATETIME        NOT NULL,
    [cost_center]            INT             NULL,
    [use_cad_id]             BIT             DEFAULT ((0)) NULL,
    [include_cutsheets]      BIT             DEFAULT ((0)) NULL,
    [phase_id]               INT             NULL,
    [department_id]          INT             NULL,
    [room_id]                INT             NULL,
    [file_name]              VARCHAR (100)   NULL,
    [include_po_cover]       BIT             DEFAULT ((0)) NOT NULL,
    [include_po_uploaded]    BIT             DEFAULT ((0)) NOT NULL,
    [include_quote_uploaded] BIT             DEFAULT ((0)) NOT NULL,
    [po_status]              VARCHAR (15)    NULL,
    [status]                 DECIMAL (10, 2) NOT NULL,
	[include_docs_without_link] BIT			 DEFAULT((0)) NOT NULL,
	[include_budgets]		 BIT		     DEFAULT((0)) NOT NULL, 
	[include_jsn]		 BIT		     DEFAULT((0)) NOT NULL, 
	[include_code]		 BIT		     DEFAULT((0)) NOT NULL, 
	[include_total_budget]		 BIT		     DEFAULT((0)) NOT NULL,
    [compare_with_project_id] INT NULL, 
    [include_photo] BIT NULL DEFAULT ((0)),
	[status_category]		 VARCHAR(18) NOT NULL DEFAULT('Waiting'),
	[remove_logo] BIT NULL DEFAULT ((0)),
	[ignore_description_difference] BIT DEFAULT ((0)) NULL,
	[remove_ecn] BIT DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [CHK_status] CHECK ([status]>=(-1) AND [status]<=(100)),
    FOREIGN KEY ([cost_center]) REFERENCES [dbo].[cost_center] ([id]),
    FOREIGN KEY ([generated_by]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    FOREIGN KEY ([report_type_id]) REFERENCES [dbo].[report_type] ([id]),
    FOREIGN KEY ([project_id], [project_domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]) ON DELETE CASCADE,
	FOREIGN KEY ([project_id], [phase_id], [department_id], [room_id], [project_domain_id]) REFERENCES [dbo].[project_room] ([project_id], [phase_id], [department_id], [room_id], [domain_id]) ON DELETE CASCADE,
	CONSTRAINT [CHK_status_Category] CHECK (status_category = 'Initializing' OR status_category = 'Error' OR status_category = 'Downloading Photos' OR status_category = 'Completed' OR status_category = 'Generating' OR status_category = 'Uploading' OR status_category = 'Waiting')
);


GO
CREATE NONCLUSTERED INDEX [project_report_project]
    ON [dbo].[project_report]([project_domain_id] ASC, [project_id] ASC);


GO
CREATE NONCLUSTERED INDEX [project_report_user]
    ON [dbo].[project_report]([project_domain_id] ASC, [project_id] ASC, [generated_by] ASC, [isPrivate] ASC);


GO

CREATE TRIGGER [dbo].[project_report_insert_update]
ON [dbo].[project_report]
AFTER INSERT, UPDATE
AS 
	DECLARE @project_id INTEGER, @domain_id SMALLINT, @phase_id INTEGER, @department_id INTEGER, @room_id INTEGER, @id INTEGER, @type INTEGER,
		@cutsheet BIT, @cad_id BIT, @cost_center INTEGER, @po_cover BIT, @po_uploaded BIT, @quote_uploaded BIT;
	DECLARE @count INTEGER, @type_name VARCHAR(50), @po_status VARCHAR(14);

	SET @count = 1;

	DECLARE [cursor] CURSOR LOCAL FOR SELECT id, project_domain_id, project_id, phase_id, department_id, room_id, report_type_id, use_cad_id,
		include_cutsheets, cost_center, include_po_cover, include_po_uploaded, include_quote_uploaded, po_status FROM inserted;
	OPEN [cursor];

	FETCH NEXT FROM [cursor] INTO @id, @domain_id, @project_id, @phase_id, @department_id, @room_id, @type, @cad_id, @cutsheet, @cost_center,
		@po_cover, @po_uploaded, @quote_uploaded, @po_status;
	WHILE @@FETCH_STATUS = 0
		BEGIN

			IF @room_id IS NOT NULL
				SELECT @count = COUNT(*) FROM project_room WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id
					AND department_id = @department_id AND room_id = @room_id;
			ELSE IF @department_id IS NOT NULL
				SELECT @count = COUNT(*) FROM project_department WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id
					AND department_id = @department_id;
			ELSE IF @phase_id IS NOT NULL
				SELECT @count = COUNT(*) FROM project_phase WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id;

			IF @count > 0
				BEGIN
					SELECT @type_name = lower(name) FROM report_type WHERE id = @type;

					IF @type_name = 'it connectivity'
						SET @cost_center = null;

					IF @type_name <> 'asset book'
						BEGIN
							SET @cutsheet = 0;
							IF @type_name <> 'asset by room'
								SET @cad_id = 0;
						END

					IF @type_name <> 'procurement'
						BEGIN
							SET @po_cover = 0
							SET @po_uploaded = 0
							SET @quote_uploaded = 0
							SET @po_status = NULL
						END

					UPDATE project_report SET cost_center = @cost_center, include_cutsheets = @cutsheet, use_cad_id = @cad_id,
						include_po_cover = @po_cover, include_po_uploaded = @po_uploaded, include_quote_uploaded = @quote_uploaded
						WHERE id = @id;
				END
			ELSE
				BEGIN
					DELETE FROM report_location WHERE report = @id;
					DELETE FROM project_report WHERE id = @id;
				END

			FETCH NEXT FROM [cursor] INTO @id, @domain_id, @project_id, @phase_id, @department_id, @room_id, @type, @cad_id, @cutsheet, @cost_center,
				@po_cover, @po_uploaded, @quote_uploaded, @po_status;
		END
	CLOSE [cursor];
	DEALLOCATE [cursor];

