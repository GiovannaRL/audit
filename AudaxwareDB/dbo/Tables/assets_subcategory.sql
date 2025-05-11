CREATE TABLE [dbo].[assets_subcategory] (
    [subcategory_id]        INT          IDENTITY (1, 1) NOT NULL,
    [category_id]           INT          NOT NULL,
    [description]           VARCHAR (200) NOT NULL,
    [domain_id]             SMALLINT     NOT NULL,
    [category_domain_id]    SMALLINT     NOT NULL,
    [HVAC]                  CHAR (1)     CONSTRAINT [DF_subcategory_HVAC] DEFAULT ('E') NOT NULL,
    [Plumbing]              CHAR (1)     CONSTRAINT [DF_subcategory_Plumbing] DEFAULT ('E') NOT NULL,
    [Gases]                 CHAR (1)     CONSTRAINT [DF_subcategory_Gases] DEFAULT ('E') NOT NULL,
    [IT]                    CHAR (1)     CONSTRAINT [DF_subcategory_IT] DEFAULT ('E') NOT NULL,
    [Electrical]            CHAR (1)     CONSTRAINT [DF_subcategory_Electrical] DEFAULT ('E') NOT NULL,
    [use_category_settings] BIT          NOT NULL CONSTRAINT [DF_subcategory_use_category] DEFAULT(0),
    [Support]               CHAR (1)     CONSTRAINT [DF_subcategory_Support] DEFAULT ('E') NOT NULL,
    [Physical]              CHAR (1)     CONSTRAINT [DF_subcategory_Physical] DEFAULT ('E') NOT NULL,
    [Environmental]         CHAR (1)     CONSTRAINT [DF_subcategory_Environmental] DEFAULT ('E') NOT NULL,
    [asset_code] VARCHAR(3) NULL, 
    [asset_code_domain_id] SMALLINT NULL, 
    [date_added]             DATE            NULL,
    [added_by]               VARCHAR (50)    NULL,
    CONSTRAINT [equipment_subcategory_pk] PRIMARY KEY CLUSTERED ([subcategory_id] ASC, [domain_id] ASC),
    CONSTRAINT [CK_subcategory_Electrical] CHECK ([Electrical]='E' OR [Electrical]='D' OR [Electrical]='R'),
    CONSTRAINT [CK_subcategory_Environmental] CHECK ([Environmental]='E' OR [Environmental]='D' OR [Environmental]='R'),
    CONSTRAINT [CK_subcategory_Gases] CHECK ([Gases]='E' OR [Gases]='D' OR [Gases]='R'),
    CONSTRAINT [CK_subcategory_HVAC] CHECK ([HVAC]='E' OR [HVAC]='D' OR [HVAC]='R'),
    CONSTRAINT [CK_subcategory_IT] CHECK ([IT]='E' OR [IT]='D' OR [IT]='R'),
    CONSTRAINT [CK_subcategory_Physical] CHECK ([Physical]='E' OR [Physical]='D' OR [Physical]='R'),
    CONSTRAINT [CK_subcategory_Plumbing] CHECK ([Plumbing]='E' OR [Plumbing]='D' OR [Plumbing]='R'),
    CONSTRAINT [CK_subcategory_Support] CHECK ([Support]='E' OR [Support]='D' OR [Support]='R'),
    CONSTRAINT [asset_category_asset_subcategory_fk] FOREIGN KEY ([category_id], [category_domain_id]) REFERENCES [dbo].[assets_category] ([category_id], [domain_id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [domain1_fk] FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]),
	CONSTRAINT [asset_code_fk] FOREIGN KEY ([asset_code], [asset_code_domain_id]) REFERENCES [dbo].[assets_codes] ([prefix], [domain_id]),
    UNIQUE NONCLUSTERED ([category_domain_id] ASC, [category_id] ASC, [domain_id] ASC, [description] ASC)
);

GO

CREATE TRIGGER [dbo].[assets_subcategory_updated]
ON [dbo].[assets_subcategory]
AFTER UPDATE
AS
BEGIN
	DECLARE
		@subcategory_id int,
		@subcategory_domain_id int,
		@subcategory_description VARCHAR (200),
		@category_description VARCHAR (200);

	SELECT @subcategory_id = s.subcategory_id, @subcategory_domain_id = s.domain_id, @subcategory_description = s.description,
	@category_description = c.description FROM inserted s LEFT JOIN assets_category c ON c.category_id = s.category_id AND c.domain_id = s.category_domain_id;

	UPDATE  a
	SET     a.asset_description = (case when @subcategory_description = @category_description
									then @category_description + CASE WHEN a.asset_suffix is null or a.asset_suffix = '' THEN '' ELSE ', ' + a.asset_suffix END  
									else @category_description + ', ' + @subcategory_description + CASE WHEN a.asset_suffix is null or a.asset_suffix = '' THEN '' ELSE ', ' + a.asset_suffix END   end)
	from assets a 
	where a.subcategory_id = @subcategory_id and a.subcategory_domain_id = @subcategory_domain_id
END

GO

CREATE INDEX [DescriptionIndex] ON [dbo].[assets_subcategory] ([description])

GO

CREATE INDEX [DomainIdIndex] ON [dbo].[assets_subcategory] ([domain_id])

GO

CREATE NONCLUSTERED INDEX [AddedByIndex] ON [dbo].[assets_subcategory] ([added_by])

