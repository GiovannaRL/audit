CREATE TABLE [dbo].[assets_category] (
    [category_id]   INT          IDENTITY (1, 1) NOT NULL,
    [description]   VARCHAR (200) NOT NULL,
    [domain_id]     SMALLINT     NOT NULL,
    [Gases]         CHAR (1)     CONSTRAINT [DF_category_Gases] DEFAULT ('E') NOT NULL,
    [IT]            CHAR (1)     CONSTRAINT [DF_category_IT] DEFAULT ('E') NOT NULL,
    [Electrical]    CHAR (1)     CONSTRAINT [DF_category_Electrical] DEFAULT ('E') NOT NULL,
    [HVAC]          CHAR (1)     CONSTRAINT [DF_category_HVAC] DEFAULT ('E') NOT NULL,
    [Plumbing]      CHAR (1)     CONSTRAINT [DF_category_Plumbing] DEFAULT ('E') NOT NULL,
    [Support]       CHAR (1)     CONSTRAINT [DF_category_Support] DEFAULT ('E') NOT NULL,
    [Physical]      CHAR (1)     CONSTRAINT [DF_category_Physical] DEFAULT ('E') NOT NULL,
    [Environmental] CHAR (1)     CONSTRAINT [DF_category_Environmental] DEFAULT ('E') NOT NULL,
    [asset_code] VARCHAR(3) NULL, 
    [asset_code_domain_id] SMALLINT NULL, 
    [date_added]             DATE            NULL,
    [added_by]               VARCHAR (50)    NULL,
    CONSTRAINT [equipment_category_pk] PRIMARY KEY CLUSTERED ([category_id] ASC, [domain_id] ASC),
    CONSTRAINT [CK_category_Electrical] CHECK ([Electrical]='E' OR [Electrical]='D' OR [Electrical]='R'),
    CONSTRAINT [CK_category_Environmental] CHECK ([Environmental]='E' OR [Environmental]='D' OR [Environmental]='R'),
    CONSTRAINT [CK_category_Gases] CHECK ([Gases]='E' OR [Gases]='D' OR [Gases]='R'),
    CONSTRAINT [CK_category_HVAC] CHECK ([HVAC]='E' OR [HVAC]='D' OR [HVAC]='R'),
    CONSTRAINT [CK_category_IT] CHECK ([IT]='E' OR [IT]='D' OR [IT]='R'),
    CONSTRAINT [CK_category_Physical] CHECK ([Physical]='E' OR [Physical]='D' OR [Physical]='R'),
    CONSTRAINT [CK_category_Plumbing] CHECK ([Plumbing]='E' OR [Plumbing]='D' OR [Plumbing]='R'),
    CONSTRAINT [CK_category_Support] CHECK ([Support]='E' OR [Support]='D' OR [Support]='R'),
    CONSTRAINT [domain_fk] FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]),
	CONSTRAINT [cat_asset_code_fk] FOREIGN KEY ([asset_code], [asset_code_domain_id]) REFERENCES [dbo].[assets_codes] ([prefix], [domain_id]),
    UNIQUE NONCLUSTERED ([domain_id] ASC, [description] ASC)
);

GO

CREATE TRIGGER [dbo].[assets_category_updated]
ON [dbo].[assets_category]
AFTER UPDATE
AS
BEGIN
	DECLARE
		@category_id int,
		@category_domain_id int,
		@category varchar(100);

	SELECT @category_id = category_id, @category_domain_id = domain_id, @category = description FROM inserted;

	UPDATE  a
	SET     a.asset_description = (case when @category = s.description 
									then @category + CASE WHEN a.asset_suffix is null or a.asset_suffix = '' THEN '' ELSE ', ' + a.asset_suffix END  
									else @category + ', ' + s.description + CASE WHEN a.asset_suffix is null or a.asset_suffix = '' THEN '' ELSE ', ' + a.asset_suffix END   end)
	from assets a 
	inner join assets_subcategory s on s.subcategory_id = a.subcategory_id and s.domain_id = a.subcategory_domain_id
	where s.category_id = @category_id and s.domain_id = @category_domain_id

END

GO

CREATE NONCLUSTERED INDEX [DescriptionIndex] ON [dbo].[assets_category] ([description])

GO
CREATE NONCLUSTERED INDEX [DomainIdIndex] ON [dbo].[assets_category] ([domain_id])

GO
CREATE NONCLUSTERED INDEX [AddedByIndex] ON [dbo].[assets_category] ([added_by])


