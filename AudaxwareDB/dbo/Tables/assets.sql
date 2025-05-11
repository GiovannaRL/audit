CREATE TABLE [dbo].[assets] (
    [asset_id]               INT             IDENTITY (1, 1) NOT NULL,
    [asset_code]             VARCHAR (25)    NULL,
    [manufacturer_id]        INT             NOT NULL,
    [asset_description]      VARCHAR (300)   NOT NULL DEFAULT '-',
    [subcategory_id]         INT             NOT NULL,
    [height]                 VARCHAR(25) NULL,
    [width]                  VARCHAR(25) NULL,
    [depth]                  VARCHAR(25) NULL,
    [weight]                 VARCHAR(25) NULL,
    [serial_number]          VARCHAR (100)   NULL,
    [min_cost]               NUMERIC (10, 2) NULL,
    [max_cost]               NUMERIC (10, 2) NULL,
    [avg_cost]               NUMERIC (10, 2) NULL,
    [last_cost]              NUMERIC (10, 2) NULL,
    [default_resp]           VARCHAR (10)     NOT NULL,
    [cut_sheet]              VARCHAR (100)   NULL,
    [date_added]             DATE            NULL,
    [added_by]               VARCHAR (50)    NULL,
    [comment]                VARCHAR (1000)  NULL,
    [cad_block]              VARCHAR (100)   NULL,
    [water]                  VARCHAR (150)   NULL,
    [plumbing]               VARCHAR (300)   NULL,
    [data]                   VARCHAR (150)   NULL,
    [electrical]             VARCHAR (150)   NULL,
    [mobile]                 VARCHAR (50)    NULL,
    [blocking]               VARCHAR (50)    NULL,
    [medgas]                 VARCHAR (150)   NULL,
    [supports]               VARCHAR (150)   NULL,
    [discontinued]           BIT             NULL,
    [last_budget_update]     DATE            NULL,
    [photo]                  VARCHAR (100)   NULL,
    [eq_measurement_id]      INT             NULL,
    [water_option]           INT             DEFAULT ((0)) NULL,
    [plumbing_option]        INT             DEFAULT ((0)) NULL,
    [data_option]            INT             DEFAULT ((0)) NULL,
    [electrical_option]      INT             DEFAULT ((0)) NULL,
    [mobile_option]          INT             DEFAULT ((0)) NULL,
    [blocking_option]        INT             DEFAULT ((0)) NULL,
    [medgas_option]          INT             DEFAULT ((0)) NULL,
    [supports_option]        INT             DEFAULT ((0)) NULL,
    [revit]                  VARCHAR (100)   NULL,
    [placement]              VARCHAR (20)    NULL,
    [clearance_left]         NUMERIC (10, 2) NULL,
    [clearance_right]        NUMERIC (10, 2) NULL,
    [clearance_front]        NUMERIC (10, 2) NULL,
    [clearance_back]         NUMERIC (10, 2) NULL,
    [clearance_top]          NUMERIC (10, 2) NULL,
    [clearance_bottom]       NUMERIC (10, 2) NULL,
    [volts]                  VARCHAR (20)    NULL,
    [phases]                 INT             NULL,
    [hertz]                  VARCHAR (20)    NULL,
    [amps]                   VARCHAR (20)    NULL,
    [volt_amps]              NUMERIC (10, 2) NULL,
    [watts]                  VARCHAR (20)    NULL,
    [cfm]                    VARCHAR (150)   NULL,
    [btus]                   VARCHAR (150)   NULL,
    [misc_ase]               BIT             DEFAULT ((0)) NULL,
    [misc_ada]               BIT             DEFAULT ((0)) NULL,
    [misc_seismic]           BIT             DEFAULT ((0)) NULL,
    [misc_antimicrobial]     BIT             DEFAULT ((0)) NULL,
    [misc_ecolabel]          BIT             DEFAULT ((0)) NULL,
    [misc_ecolabel_desc]     VARCHAR (150)   NULL,
    [mapping_code]           VARCHAR (20)    NULL,
    [medgas_oxygen]          BIT             DEFAULT ((0)) NULL,
    [medgas_nitrogen]        BIT             DEFAULT ((0)) NULL,
    [medgas_air]             BIT             DEFAULT ((0)) NULL,
    [medgas_n2o]             BIT             DEFAULT ((0)) NULL,
    [medgas_vacuum]          BIT             DEFAULT ((0)) NULL,
    [medgas_wag]             BIT             DEFAULT ((0)) NULL,
    [medgas_co2]             BIT             DEFAULT ((0)) NULL,
    [medgas_other]           BIT             DEFAULT ((0)) NULL,
    [medgas_steam]           BIT             DEFAULT ((0)) NULL,
    [medgas_natgas]          BIT             DEFAULT ((0)) NULL,
    [plu_hot_water]          BIT             DEFAULT ((0)) NULL,
    [plu_drain]              BIT             DEFAULT ((0)) NULL,
    [plu_cold_water]         BIT             DEFAULT ((0)) NULL,
    [plu_return]             BIT             DEFAULT ((0)) NULL,
    [plu_treated_water]      BIT             DEFAULT ((0)) NULL,
    [plu_relief]             BIT             DEFAULT ((0)) NULL,
    [plu_chilled_water]      BIT             DEFAULT ((0)) NULL,
    [serial_name]            VARCHAR (150)   NULL,
    [useful_life]            INT             NULL,
    [loaded_weight]          NUMERIC (10, 2) NULL,
    [ship_weight]            NUMERIC (10, 2) NULL,
    [alternate_asset]        VARCHAR (50)    NULL,
    [updated_at]             DATE            NULL,
    [domain_id]              SMALLINT        NOT NULL,
    [manufacturer_domain_id] SMALLINT        NOT NULL,
    [no_options]             BIT             DEFAULT ((0)) NULL,
    [no_colors]              BIT             DEFAULT ((0)) NULL,
    [subcategory_domain_id]  SMALLINT        NOT NULL,
    [category_attribute]     VARCHAR (2)     NULL,
    [asset_suffix] VARCHAR(200) NULL,
    [jsn_id] INT NULL, 
    [jsn_domain_id] SMALLINT NULL, 
    [class] INT NULL, 
    [gas_liquid_co2] BIT NULL DEFAULT ((0)), 
    [gas_liquid_nitrogen] BIT NULL DEFAULT ((0)), 
    [gas_instrument_air] BIT NULL DEFAULT ((0)), 
    [gas_liquid_propane_gas] BIT NULL DEFAULT ((0)), 
    [gas_methane] BIT NULL DEFAULT ((0)), 
    [gas_butane] BIT NULL DEFAULT ((0)), 
    [gas_propane] BIT NULL DEFAULT ((0)), 
    [gas_hydrogen] BIT NULL DEFAULT ((0)), 
    [gas_acetylene] BIT NULL DEFAULT ((0)), 
    [medgas_high_pressure] BIT NULL DEFAULT ((0)), 
    [misc_shielding_lead_line] BIT NULL DEFAULT ((0)), 
    [misc_shielding_magnetic] BIT NULL DEFAULT ((0)), 
    [jsn_suffix] VARCHAR(4) NULL, 
    [jsn_utility1_ow] BIT NULL DEFAULT ((0)), 
    [jsn_utility1] VARCHAR(10) NULL, 
    [jsn_utility2_ow] BIT NULL DEFAULT ((0)), 
    [jsn_utility2] VARCHAR(10) NULL, 
    [jsn_utility3_ow] BIT NULL DEFAULT ((0)), 
    [jsn_utility3] VARCHAR(10) NULL, 
    [jsn_utility4_ow] BIT NULL DEFAULT ((0)), 
    [jsn_utility4] VARCHAR(10) NULL, 
    [jsn_utility5_ow] BIT NULL DEFAULT ((0)), 
    [jsn_utility5] VARCHAR(10) NULL, 
    [jsn_utility6_ow] BIT NULL DEFAULT ((0)), 
    [jsn_utility6] VARCHAR(10) NULL, 
    [jsn_utility7_ow] BIT NULL DEFAULT ((0)), 
    [jsn_utility7] VARCHAR(10) NULL, 
	[imported_by_project_id] INT NULL,
    [mounting_height] VARCHAR(25) NULL,
    [weight_limit] VARCHAR(25) NULL, 
	[approval_pending_domain] SMALLINT NULL,
	[created_from]   VARCHAR (25) NULL,
    [approval_modify_aw_asset] BIT NULL DEFAULT ((0)), 
    [connection_type] VARCHAR (20) NULL,
    [plug_type] VARCHAR (20) NULL,
    [photo_rotate] INT NULL,
    [lan]            INT             DEFAULT ((0)) NULL,
    [network_type] VARCHAR (20) NULL,
    [network_option] INT DEFAULT ((0)) NULL,
    [standart_deviation_cost] NUMERIC (10,2) NULL,
    [ports] NUMERIC (10) NULL,
    [bluetooth] BIT NULL DEFAULT ((0)),
    [cat6] BIT NULL DEFAULT ((0)),
    [displayport] BIT NULL DEFAULT ((0)),
    [dvi] BIT NULL DEFAULT ((0)),
    [hdmi] BIT NULL DEFAULT ((0)),
    [wireless] BIT NULL DEFAULT ((0)),

    CONSTRAINT [equipment_domain_pk] PRIMARY KEY CLUSTERED ([asset_id] ASC, [domain_id] ASC),
    CONSTRAINT [category_attribute_check] CHECK ([category_attribute]='F' OR [category_attribute]='MJ' OR [category_attribute]='MN'),
    CONSTRAINT [asset_measurement_fk] FOREIGN KEY ([eq_measurement_id]) REFERENCES [dbo].[assets_measurement] ([eq_unit_measure_id]),
    CONSTRAINT [asset_subcategory_asset_fk] FOREIGN KEY ([subcategory_id], [subcategory_domain_id]) REFERENCES [dbo].[assets_subcategory] ([subcategory_id], [domain_id]),
    CONSTRAINT [domain2_fk] FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]),
    CONSTRAINT [manufacturer_equipment_fk] FOREIGN KEY ([manufacturer_id], [manufacturer_domain_id]) REFERENCES [dbo].[manufacturer] ([manufacturer_id], [domain_id]),
    CONSTRAINT [UNQ_asset_code] UNIQUE NONCLUSTERED ([domain_id] ASC, [asset_code] ASC), 
    CONSTRAINT [jsn_fk] FOREIGN KEY ([jsn_id], [jsn_domain_id]) REFERENCES [jsn]([id], [domain_id]),
	CONSTRAINT [approval_pending_domain_fk] FOREIGN KEY ([approval_pending_domain]) REFERENCES [domain]([domain_id])
);


GO
ALTER TABLE [dbo].[assets] NOCHECK CONSTRAINT [asset_measurement_fk];


GO
CREATE NONCLUSTERED INDEX [DescriptionIndex]
    ON [dbo].[assets]([asset_description] ASC);

GO
CREATE NONCLUSTERED INDEX [JSNSuffix]
    ON [dbo].[assets]([jsn_suffix] ASC);

GO
CREATE NONCLUSTERED INDEX [JSNAndDomain]
    ON [dbo].[assets]([jsn_suffix] ASC, [jsn_domain_id] ASC);

GO
CREATE NONCLUSTERED INDEX [AssetCodeIndex]
    ON [dbo].[assets]([asset_code] ASC);

GO
CREATE NONCLUSTERED INDEX [AssetApprovalPending]
    ON [dbo].[assets]([approval_pending_domain] ASC);

GO
CREATE NONCLUSTERED INDEX [asset_domain_discontinued]
    ON [dbo].[assets]([domain_id] ASC, [discontinued] ASC, [asset_id] ASC);

GO

CREATE NONCLUSTERED INDEX [DomainIdIndex] ON [dbo].[assets] ([domain_id])

GO

CREATE NONCLUSTERED INDEX [ModelNameIndex] ON [dbo].[assets] ([serial_number])

GO

CREATE NONCLUSTERED INDEX [SubcategoryIndex] ON [dbo].[assets] ([subcategory_id])

GO

CREATE NONCLUSTERED INDEX [SubcategoryDomainIndex] ON [dbo].[assets] ([subcategory_domain_id])

GO

CREATE NONCLUSTERED INDEX [AddedByIndex] ON [dbo].[assets] ([added_by])

GO

CREATE TRIGGER [dbo].[assets_insert]
ON [dbo].[assets]
AFTER INSERT
AS
BEGIN
	DECLARE
		@asset_id int,
		@asset_domain_id int,
		@asset_suffix varchar(200),
		@subcategory_id int,
		@subcategory_domain_id int,
		@category_id int,
		@category_domain_id int,
		@category varchar(100),
		@subcategory varchar(100),
		@asset_description varchar(400);

	SELECT @asset_id = asset_id, @asset_domain_id = domain_id, @asset_suffix = asset_suffix, @subcategory_id = subcategory_id, @subcategory_domain_id = subcategory_domain_id FROM inserted;

	SELECT @subcategory = description, @category_id = category_id, @category_domain_id = category_domain_id  from assets_subcategory where subcategory_id = @subcategory_id and domain_id = @subcategory_domain_id;
	SELECT @category = description from assets_category where category_id = @category_id and domain_id = @category_domain_id;

	SET @asset_description = @category
	
	IF @category <> @subcategory BEGIN
		SET @asset_description = @asset_description + ', ' + @subcategory
	END
	
	IF @asset_suffix is not null and @asset_suffix <> '' BEGIN
		SET @asset_description = @asset_description + ', ' + @asset_suffix 
	END
	
	UPDATE assets set asset_description = @asset_description
	where asset_id =  @asset_id and domain_id = @asset_domain_id;

END

GO

CREATE TRIGGER update_asset_data
ON assets AFTER UPDATE
AS
	DECLARE @domain_id SMALLINT, @asset_id INTEGER, @jsn_id INTEGER, @jsn_domain_id SMALLINT,
        @jsn_utility1_ow BIT, @jsn_utility1 VARCHAR(10), @jsn_utility2_ow BIT, @jsn_utility2 VARCHAR(10), @jsn_utility3_ow BIT, @jsn_utility3 VARCHAR(10),
        @jsn_utility4_ow BIT, @jsn_utility4 VARCHAR(10), @jsn_utility5_ow BIT, @jsn_utility5 VARCHAR(10), @jsn_utility6_ow BIT, @jsn_utility6 VARCHAR(10),
        @jsn_utility7_ow BIT, @jsn_utility7 VARCHAR(10);

	DECLARE cur CURSOR LOCAL FOR SELECT domain_id, asset_id, jsn_id, jsn_domain_id,
            jsn_utility1_ow, jsn_utility1, jsn_utility2_ow, jsn_utility2, jsn_utility3_ow, jsn_utility3, jsn_utility4_ow, jsn_utility4, jsn_utility5_ow, jsn_utility5, 
            jsn_utility6_ow, jsn_utility6, jsn_utility7_ow, jsn_utility7 FROM inserted;
	OPEN cur;
	FETCH NEXT FROM cur INTO @domain_id, @asset_id, @jsn_id, @jsn_domain_id,
        @jsn_utility1_ow, @jsn_utility1, @jsn_utility2_ow, @jsn_utility2, @jsn_utility3_ow, @jsn_utility3,
        @jsn_utility4_ow, @jsn_utility4, @jsn_utility5_ow, @jsn_utility5, @jsn_utility6_ow, @jsn_utility6,
        @jsn_utility7_ow, @jsn_utility7
	WHILE @@FETCH_STATUS = 0
		BEGIN
			-- FORCES AN UPDTATE TRIGGER FOR ALL ASSETS THAT DO NOT HAVE ANYTHING OVERWRITEN
			UPDATE project_room_inventory SET update_trigger = CASE WHEN update_trigger = 1 THEN 0 ELSE 1 END WHERE asset_id = @asset_id AND asset_domain_id = @domain_id
				AND (
				(coalesce(asset_description_ow, 0) <> 1) OR (coalesce(placement_ow, 0) <> 1) OR (coalesce(height_ow, 0) <> 1) OR (coalesce(width_ow, 0) <> 1) OR 
				(coalesce(depth_ow, 0) <> 1) OR (coalesce(mounting_height_ow, 0) <> 1) OR (coalesce(class_ow, 0) <> 1) OR (coalesce(jsn_ow, 0) <> 1) OR
				(coalesce(manufacturer_description_ow, 0) <> 1) OR (coalesce(serial_name_ow, 0) <> 1) OR (coalesce(serial_number_ow, 0) <> 1)
				)

            -- UPDATES THE UTILITIES
			IF (@jsn_id IS NOT NULL AND @jsn_domain_id IS NOT NULL) AND ( ( @jsn_utility2_ow = 1 OR @jsn_utility2 IS NULL ) OR ( @jsn_utility2_ow = 1 OR @jsn_utility2 IS NULL ) 
                OR ( @jsn_utility3_ow = 1 OR @jsn_utility3 IS NULL ) OR ( @jsn_utility4_ow = 1 OR @jsn_utility4 IS NULL ) OR ( @jsn_utility5_ow = 1 OR @jsn_utility5 IS NULL ) 
            OR ( @jsn_utility6_ow = 1 OR @jsn_utility6 IS NULL ) OR ( @jsn_utility7_ow = 1 OR @jsn_utility7 IS NULL ))
			BEGIN
				SELECT TOP 1 
					@jsn_utility1 = CASE WHEN @jsn_utility1_ow <> 1 OR @jsn_utility1 IS NULL THEN  [utility1]  ELSE @jsn_utility1 END,
					@jsn_utility2 = CASE WHEN @jsn_utility2_ow <> 1 OR @jsn_utility2 IS NULL THEN  [utility2]  ELSE @jsn_utility2 END,
					@jsn_utility3 = CASE WHEN @jsn_utility3_ow <> 1 OR @jsn_utility3 IS NULL THEN  [utility3]  ELSE @jsn_utility3 END,
					@jsn_utility4 = CASE WHEN @jsn_utility4_ow <> 1 OR @jsn_utility4 IS NULL THEN  [utility4]  ELSE @jsn_utility4 END,
					@jsn_utility5 = CASE WHEN @jsn_utility5_ow <> 1 OR @jsn_utility5 IS NULL THEN  [utility5]  ELSE @jsn_utility5 END,
					@jsn_utility6 = CASE WHEN @jsn_utility6_ow <> 1 OR @jsn_utility6 IS NULL THEN  [utility6]  ELSE @jsn_utility6 END,
					@jsn_utility7 = CASE WHEN @jsn_utility7_ow <> 1 OR @jsn_utility7 IS NULL THEN  [utility7]  ELSE @jsn_utility7 END
					FROM [jsn] where Id = @jsn_id AND domain_id = @jsn_domain_id
                UPDATE assets SET jsn_utility1 = @jsn_utility1, jsn_utility2 = @jsn_utility2, jsn_utility3 = @jsn_utility3,
                    jsn_utility4 = @jsn_utility4, jsn_utility5 = @jsn_utility5,  jsn_utility6 = @jsn_utility6, jsn_utility7 = @jsn_utility7 where
					asset_id = @asset_id AND domain_id = @domain_id
			END
		FETCH NEXT FROM cur INTO @domain_id, @asset_id, @jsn_id, @jsn_domain_id,
			@jsn_utility1_ow, @jsn_utility1, @jsn_utility2_ow, @jsn_utility2, @jsn_utility3_ow, @jsn_utility3,
			@jsn_utility4_ow, @jsn_utility4, @jsn_utility5_ow, @jsn_utility5, @jsn_utility6_ow, @jsn_utility6,
			@jsn_utility7_ow, @jsn_utility7;
	END
	CLOSE cur;
	DEALLOCATE cur;
GO
