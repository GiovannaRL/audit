CREATE TABLE [dbo].[assets_options] (
    [asset_option_id] INT             IDENTITY (1, 1) NOT NULL,
    [asset_id]        INT             NULL,
    [code]            VARCHAR (50)    NULL,
    [description]     VARCHAR (270)   NOT NULL,
    [added_by]        VARCHAR (50)    NULL,
    [date_added]      DATE            NULL,
    [domain_id]       SMALLINT        NOT NULL,
    [data_type]       VARCHAR (2)     NOT NULL,
    [min_cost]        NUMERIC (10, 2) NULL,
    [max_cost]        NUMERIC (10, 2) NULL,
    [avg_cost]        NUMERIC (10, 2) NULL,
    [last_cost]       NUMERIC (10, 2) NULL,
    [unit_budget]     NUMERIC (10, 2) NULL,
    [old_id] INT NULL,
	[display_code]    VARCHAR(50),
	[asset_domain_id]SMALLINT        NULL,
	[project_domain_id] SMALLINT NULL,
	[project_id]	  INT NULL,
	[document_id] INT NULL,
	[document_domain_id] SMALLINT NULL,
	[settings] TEXT NULL,
	[scope] INT NULL,
    CONSTRAINT [equipment_options_id_pk] PRIMARY KEY CLUSTERED ([asset_option_id] ASC, [domain_id] ASC),
    CONSTRAINT [CK_max_min] CHECK ([max_cost] IS NULL OR [min_cost] IS NULL OR [max_cost]>=[min_cost]),
    CONSTRAINT [CK_type] CHECK ([data_type]='A' OR [data_type]='C' OR [data_type]='CO' OR [data_type]='D' OR [data_type]='I' OR [data_type]='W' OR [data_type]='FR' OR [data_type]='FI'),
    CONSTRAINT [asset_domain1_fk] FOREIGN KEY ([asset_id], [asset_domain_id]) REFERENCES [dbo].[assets] ([asset_id], [domain_id]) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT [assets_options_project_domain_id_project_id_fk] FOREIGN KEY ([project_id], [project_domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT [assets_options_document_fk] FOREIGN KEY ([document_domain_id], [document_id]) REFERENCES [dbo].[domain_document] ([domain_id], [id]) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT [assets_options_scope_fk] FOREIGN KEY ([scope]) REFERENCES asset_option_scope([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
);


GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-asset-domain]
    ON [dbo].[assets_options]([asset_id] ASC, [domain_id] ASC);


GO

CREATE TRIGGER [dbo].[update_inventory_profile]
    ON [dbo].[assets_options]
    AFTER UPDATE
    AS
    BEGIN
        DECLARE @code VARCHAR(500), @domain_id SMALLINT, @asset_option_id INTEGER, @old_code VARCHAR(50), 
			@asset_id INTEGER, @old_option_id INTEGER, @max INTEGER;

		DECLARE assets_options_cur_upd CURSOR LOCAL FOR SELECT i.domain_id, i.asset_option_id, i.code, i.asset_id, d.code AS old_code 
			FROM inserted i left join deleted d on i.domain_id = d.domain_id AND i.asset_option_id = d.asset_option_id;
		OPEN assets_options_cur_upd;
		FETCH NEXT FROM assets_options_cur_upd INTO @domain_id, @asset_option_id, @code, @asset_id, @old_code;
		WHILE @@FETCH_STATUS = 0
		BEGIN

			IF @code is not null AND (@old_code is null OR @old_code <> @code)
				BEGIN

					UPDATE project_room_inventory SET asset_profile =
						REPLACE(asset_profile, @old_code, @code) WHERE asset_domain_id = @domain_id AND
						asset_id = @asset_id AND asset_profile IS NOT NULL;

					SELECT @old_option_id = asset_option_id FROM assets_options WHERE domain_id = @domain_id AND 
						asset_id = @asset_id AND display_code = @code AND @old_option_id <> @asset_option_id;

					IF @old_option_id IS NOT NULL
						BEGIN
							DECLARE @c INTEGER;
							SET @c = 1;

							WHILE EXISTS (SELECT * FROM assets_options WHERE domain_id = @domain_id AND asset_id = @asset_id AND display_code = CONCAT(@code, @c))
								BEGIN
									SET @c = @c+1;
								END

							UPDATE assets_options SET display_code = CONCAT(@code, @c) WHERE domain_id = @domain_id AND asset_option_id = 
								@old_option_id;
						END

					UPDATE assets_options SET display_code = code where domain_id = @domain_id AND asset_option_id = @asset_option_id;
				END

			FETCH NEXT FROM assets_options_cur_upd INTO @domain_id, @asset_option_id, @code, @asset_id, @old_code;
		END
		CLOSE assets_options_cur_upd;
		DEALLOCATE assets_options_cur_upd;
    END
GO

CREATE TRIGGER [dbo].[insert_display_code]
    ON [dbo].[assets_options]
    AFTER INSERT
    AS
    BEGIN

		DECLARE @code VARCHAR(500), @domain_id SMALLINT, @asset_option_id INTEGER, @asset_id INTEGER, @old_option_id INTEGER, @max INTEGER,
			@desc VARCHAR(270), @c INTEGER, @descLEN INTEGER;

        DECLARE assets_options_cur CURSOR LOCAL FOR SELECT domain_id, asset_option_id, code, asset_id, description FROM inserted;
		OPEN assets_options_cur;
		FETCH NEXT FROM assets_options_cur INTO @domain_id, @asset_option_id, @code, @asset_id, @desc;
		WHILE @@FETCH_STATUS = 0
		BEGIN

			IF @code IS NOT NULL
				BEGIN
					SELECT @old_option_id = asset_option_id FROM assets_options WHERE domain_id = @domain_id AND asset_id = @asset_id AND 
						display_code = @code AND @old_option_id <> @asset_option_id;

					IF @old_option_id IS NOT NULL
						BEGIN
							SET @c = 1;

							WHILE EXISTS (SELECT * FROM assets_options WHERE domain_id = @domain_id AND asset_id = @asset_id AND display_code = CONCAT(@code, @c))
								BEGIN
									SET @c = @c+1;
								END

							UPDATE assets_options SET display_code = CONCAT(@code, @c) WHERE domain_id = @domain_id AND asset_option_id = 
									@old_option_id;
						END

					UPDATE assets_options SET display_code = code where domain_id = @domain_id AND asset_option_id = @asset_option_id;
				END
			ELSE
				BEGIN
					SET @desc = REPLACE(@desc, ' ', '');
					SET @code = LEFT(@desc, 10);
					SET @descLen = LEN(@desc)

					IF NOT EXISTS(SELECT * FROM assets_options WHERE domain_id = @domain_id AND asset_id = @asset_id AND display_code = CASE WHEN @descLen > 10 THEN CONCAT(@code, '...') ELSE @code END)
						UPDATE assets_options SET display_code = CASE WHEN @descLen > 10 THEN CONCAT(@code, '...') ELSE @code END WHERE domain_id = @domain_id AND asset_option_id = @asset_option_id;
					ELSE
						BEGIN
							SET @c = 1;

							WHILE EXISTS (SELECT * FROM assets_options WHERE domain_id = @domain_id AND asset_id = @asset_id AND display_code =  CASE WHEN @descLEN > 10 THEN CONCAT(@code, @c, '...') ELSE CONCAT(@code, @c) END)
								BEGIN
									SET @c = @c+1;
								END

							UPDATE assets_options SET display_code = CASE WHEN @descLEN > 10 THEN CONCAT(@code, @c, '...') ELSE CONCAT(@code, @c) END WHERE domain_id = @domain_id AND asset_option_id = @asset_option_id;
						END
				END

			FETCH NEXT FROM assets_options_cur INTO @domain_id, @asset_option_id, @code, @asset_id, @desc;
		END
		CLOSE assets_options_cur;
		DEALLOCATE assets_options_cur;
    END