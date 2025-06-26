CREATE TABLE [dbo].[project_room_inventory] (
    [project_id]              INT             NOT NULL,
    [department_id]           INT             NOT NULL,
    [room_id]                 INT             NOT NULL,
    [asset_id]                INT             NOT NULL,
    [status]                  VARCHAR (20)    NULL,
    [resp]                    CHAR (10)     NOT NULL,
    [budget_qty]              INT             NULL,
    [dnp_qty]                 INT             NULL,
    [unit_budget]             NUMERIC (10, 2) NULL,
    [buyout_delta]            NUMERIC (10, 2) NULL,
    [estimated_delivery_date] DATE            NULL,
    [current_location]        VARCHAR (50)    NOT NULL, -- This is the actual status field in the asset configuration
    [inventory_type]          VARCHAR (25)    NULL,
    [date_added]              DATE            NULL,
    [added_by]                VARCHAR (50)    NULL,
    [comment]                 VARCHAR (1000)  NULL,
    [lease_qty]               INT             NULL,
    [cost_center_id]          INT             NULL,
    [tag]                     VARCHAR (5000)  NULL,
    [cad_id]                  VARCHAR (25)    NULL,
    [none_option]             BIT             NULL,
    [domain_id]               SMALLINT        NOT NULL,
    [phase_id]                INT             NOT NULL,
    [asset_domain_id]         SMALLINT        NOT NULL,
    [inventory_id]            INT             IDENTITY (1, 1) NOT NULL,
    [option_ids]              VARCHAR (100)   NULL,
    [locked_unit_budget] NUMERIC(10, 2) NULL, 
    [locked_budget_qty] INT NULL, 
    [locked_dnp_qty] INT NULL,
	[options_unit_price] NUMERIC(10, 2) DEFAULT(0) NOT NULL,
	[options_price] AS COALESCE(options_unit_price, 0) * (COALESCE(budget_qty, 1) - COALESCE(dnp_qty, 0)),
	[asset_total_budget] AS COALESCE(unit_budget, 0) * (COALESCE(budget_qty, 1) - COALESCE(dnp_qty, 0)),
	[total_budget_amt] AS (COALESCE(unit_budget, 0) + options_unit_price) * (COALESCE(budget_qty, 1) - COALESCE(dnp_qty, 0)),
	[linked_id_template] INT NULL, 
	[asset_profile] VARCHAR(3000),
	[asset_profile_budget] VARCHAR(2000),
	[detailed_budget] BIT NOT NULL DEFAULT(0),
    [locked_room_quantity] INT NULL, 
	[linked_document] INT NULL,
    [lead_time] INT NULL , 
    [clin] VARCHAR(50) NULL, 
    [inventory_source_id] INT NULL, 
    [inventory_target_id] INT NULL, 
    [unit_markup] DECIMAL(10, 2) NULL , 
    [unit_escalation] DECIMAL(10, 2) NULL , 
    [unit_tax] DECIMAL(10, 2) NULL , 
    [unit_install_net] DECIMAL(10, 2) NULL , 
    [unit_install_markup] DECIMAL(10, 2) NULL , 
    [unit_freight_net] DECIMAL(10, 2) NULL , 
    [unit_freight_markup] DECIMAL(10, 2) NULL , 
	[total_unit_budget] as coalesce(unit_budget,0) + CASE detailed_budget WHEN 1 THEN options_unit_price ELSE 0 END,
	[unit_markup_calc] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[unit_escalation_calc] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[unit_budget_adjusted] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[unit_tax_calc] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[unit_install] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[unit_freight] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[unit_budget_total] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[total_install_net] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[total_budget_adjusted] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[total_tax] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[total_install] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[total_freight_net] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[total_freight] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[total_budget] DECIMAL(10, 2) NULL, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[net_new] INT null, -- This column is calculated in the updated_calculated_fields_and_profile trigger
	[asset_description_ow] BIT DEFAULT(0),
    [asset_description]      VARCHAR (300)	NULL,
    [ECN]  VARCHAR (50)       NULL,
    [placement_ow] BIT NULL DEFAULT (0), 
    [placement] VARCHAR(20) NULL, 
    [biomed_check_required] BIT NULL DEFAULT (0), 
	[temporary_location] varchar(200) NULL,
    [height_ow] BIT NULL DEFAULT (0), 
    [height]                 VARCHAR(25) NULL,
    [width_ow] BIT NULL DEFAULT (0), 
    [width]                  VARCHAR(25) NULL,
    [depth_ow] BIT NULL DEFAULT (0), 
    [depth]                  VARCHAR(25) NULL,
    [mounting_height_ow] BIT NULL DEFAULT (0), 
    [mounting_height] VARCHAR(25) NULL,
    [class_ow] BIT NULL DEFAULT (0), 
    [class] INT NULL, 
    [jsn_code] VARCHAR(10) NULL, 
    [jsn_utility1] VARCHAR(10) NULL, 
    [jsn_utility2] VARCHAR(10) NULL, 
    [jsn_utility3] VARCHAR(10) NULL, 
    [jsn_utility4] VARCHAR(10) NULL, 
    [jsn_utility5] VARCHAR(10) NULL, 
    [jsn_utility6] VARCHAR(10) NULL, 
    [jsn_utility7] VARCHAR(10) NULL, 
	-- IMPORTANT: If you add any other field as _ow(to overwrite asset data), make sure you also insert this field in the assets.sql, under the UPDATE command that sets the update_trigger
	[update_trigger] BIT NULL,
	[copy_link] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
	[jsn_ow] BIT DEFAULT(0),
    [manufacturer_description] VARCHAR (100) NULL,
	[manufacturer_description_ow] BIT DEFAULT(0),
    [serial_number]          VARCHAR (100)   NULL,
	[serial_number_ow] BIT DEFAULT(0),
    [serial_name]            VARCHAR (150)   NULL,
	[serial_name_ow] BIT DEFAULT(0), 
	[cut_sheet_filename] VARCHAR(100), -- O campo será nulo caso os dados _ow tenham sido editados e o cover_sheet não tenha sido gerado desde então, ou não há dados _ow
    [final_disposition] varchar (200) NULL,
	[delivered_date] DATE NULL, 
    [received_date] DATE NULL, 
	[connection_type] VARCHAR(20) NULL,
	[connection_type_ow] BIT DEFAULT (0),
	[plug_type] VARCHAR(20) NULL,
	[plug_type_ow] BIT DEFAULT (0),
	[lan] INT NULL,
	[lan_ow] BIT DEFAULT (0),
	[network_type] VARCHAR (20) NULL,
	[network_type_ow] BIT DEFAULT (0),
	[network_option] INT NULL,
	[network_option_ow] BIT DEFAULT (0),
	[ports] NUMERIC (10) NULL,
	[ports_ow] BIT DEFAULT (0),
	[bluetooth] BIT NULL,
	[bluetooth_ow] BIT DEFAULT (0),
	[cat6] BIT NULL,
	[cat6_ow] BIT DEFAULT (0),
	[displayport] BIT NULL,
	[displayport_ow] BIT DEFAULT (0),
	[dvi] BIT NULL,
	[dvi_ow] BIT DEFAULT (0),
	[hdmi] BIT NULL,
	[hdmi_ow] BIT DEFAULT (0),
	[wireless] BIT NULL,
	[wireless_ow] BIT DEFAULT (0),
	[volts] VARCHAR (20) NULL,
	[volts_ow] BIT DEFAULT (0),
	[amps] VARCHAR (20) NULL,
	[amps_ow] BIT DEFAULT (0),
	[date_modified] DATE NULL,
	[model_number] VARCHAR (100)   NULL,
	[model_number_ow] BIT DEFAULT(0),
	[model_name] VARCHAR (150)   NULL,
	[model_name_ow] BIT DEFAULT(0)


    CONSTRAINT [inventory_pk] PRIMARY KEY CLUSTERED ([inventory_id] ASC),
    CONSTRAINT [asset_project_room_inventory_fk] FOREIGN KEY ([asset_id], [asset_domain_id]) REFERENCES [dbo].[assets] ([asset_id], [domain_id]),
    CONSTRAINT [cost_center1_fk] FOREIGN KEY ([cost_center_id]) REFERENCES [dbo].[cost_center] ([id]),
    CONSTRAINT [project_room_project_room_inventory_fk] FOREIGN KEY ([project_id], [phase_id], [department_id], [room_id], [domain_id]) REFERENCES [dbo].[project_room] ([project_id], [phase_id], [department_id], [room_id], [domain_id]) ON DELETE CASCADE,
	CONSTRAINT [linked_document_inventory_fk] FOREIGN KEY ([linked_document]) REFERENCES project_documents (id)
);


GO
CREATE NONCLUSTERED INDEX [room_id_indx1]
    ON [dbo].[project_room_inventory]([room_id] ASC);


GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20160128-163740]
    ON [dbo].[project_room_inventory]([asset_domain_id] ASC);


GO
CREATE NONCLUSTERED INDEX [Index-project_asset]
    ON [dbo].[project_room_inventory]([project_id] ASC, [asset_id] ASC, [asset_domain_id] ASC);


GO
CREATE NONCLUSTERED INDEX [project_room_id_indx1]
    ON [dbo].[project_room_inventory]([domain_id] ASC, [project_id] ASC, [phase_id] ASC, [department_id] ASC, [room_id] ASC);


GO

CREATE NONCLUSTERED INDEX [pri_indx1]
ON [dbo].[project_room_inventory] ([project_id],[domain_id])
INCLUDE ([department_id],[room_id],[asset_id],[status],[resp],[budget_qty],[dnp_qty],[unit_budget],[buyout_delta],[estimated_delivery_date],[current_location],[inventory_type],[date_added],[added_by],[comment],[lease_qty],[cost_center_id],[tag],[cad_id],[none_option],[phase_id],[asset_domain_id],[inventory_id],[option_ids],[locked_unit_budget],[locked_budget_qty],[locked_dnp_qty])

GO
CREATE NONCLUSTERED INDEX [copy_link_indx1]
    ON [dbo].[project_room_inventory]([copy_link] ASC);

GO

CREATE INDEX [pri_asset_profile_idx] ON [dbo].[project_room_inventory] ([asset_profile])
GO

CREATE NONCLUSTERED INDEX [AddedByIndex] ON [dbo].[project_room_inventory] ([added_by])
GO


CREATE NONCLUSTERED INDEX [inventory_source_id_idx]
    ON [dbo].[project_room_inventory]([inventory_source_id] ASC);
GO

CREATE NONCLUSTERED INDEX [inventory_target_id_idx]
    ON [dbo].[project_room_inventory]([inventory_target_id] ASC);
GO

CREATE NONCLUSTERED INDEX [project_room_template_index]
	ON [dbo].[project_room_inventory] ([asset_domain_id], [asset_id], [linked_id_template]) INCLUDE ([asset_description_ow], [class_ow], [depth_ow], [domain_id], [height_ow], [jsn_ow], [mounting_height_ow], [placement_ow], [update_trigger], [width_ow]) WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [inventory_date_modified]
ON [dbo].[project_room_inventory] ([date_modified])
INCLUDE ([domain_id])
GO

CREATE TRIGGER update_computed_fields
ON project_room_inventory AFTER INSERT, UPDATE
AS

	--TRIS IS TEMPORARY AND WILL BE REMOVED IN THE FUTURE WHEN WE REMOVE THE SERIAL_NUMBER AND SERIAL_NAME COLUMNS FROM THIS TABLE
    UPDATE pri
    SET 
        model_number = i.serial_number,
        model_name = i.serial_name,
		model_number_ow = i.serial_number_ow,
		model_name_ow = i.serial_name_ow
    FROM dbo.project_room_inventory pri
    INNER JOIN inserted i ON pri.inventory_id = i.inventory_id AND pri.domain_id = i.domain_id;

	DECLARE @is_insert BIT, @none_option BIT, @inventory_id INTEGER, @inventory_source_id INTEGER, @inventory_target_id INTEGER, @asset_profile VARCHAR(3000), @old_asset_profile VARCHAR(3000),
		@asset_domain_id SMALLINT, @asset_id INTEGER, @detailed_budget BIT, @old_detailed_budget BIT,
		@old_none_option BIT, @domain_id smallint, @total_unit_budget numeric(10,2), @unit_markup numeric(10,2), @unit_escalation numeric(10,2), 
		@unit_markup_calc numeric(10,2), @unit_escalation_calc numeric(10,2),  @unit_tax NUMERIC(10, 2), @unit_install_net NUMERIC(10, 2), 
		@unit_install_markup NUMERIC(10, 2), @unit_freight_net NUMERIC(10, 2), @unit_freight_markup NUMERIC(10, 2), @unit_budget_adjusted NUMERIC(10, 2),
		@unit_tax_calc NUMERIC(10, 2), @unit_install NUMERIC(10, 2), @unit_freight NUMERIC(10, 2), @unit_budget_total NUMERIC(10, 2),
		@total_install_net NUMERIC(10, 2), @total_budget_adjusted NUMERIC(10, 2), @total_tax NUMERIC(10, 2), @total_install NUMERIC(10, 2),
		@total_freight_net NUMERIC(10, 2), @total_freight NUMERIC(10, 2), @total_budget NUMERIC(10, 2), @net_new int,
		@asset_description_ow BIT, @asset_description VARCHAR(300), @placement_ow BIT, @placement VARCHAR(20),
		@height_ow BIT, @height VARCHAR(25), @width_ow BIT, @width VARCHAR(25), @depth_ow BIT, @depth VARCHAR(25),
		@mounting_height_ow BIT, @mounting_height VARCHAR(25), @class_ow BIT, @class INT, @resp varchar(10),
		@jsn_code VARCHAR(10), @jsn_id INT, @jsn_domain_id SMALLINT,  @jsn_suffix VARCHAR(4),
		@jsn_utility1 VARCHAR(10), @jsn_utility2 VARCHAR(10), @jsn_utility3 VARCHAR(10), @jsn_utility4 VARCHAR(10), 
		@jsn_utility5 VARCHAR(10), @jsn_utility6 VARCHAR(10), @jsn_utility7 VARCHAR(10), @jsn_ow BIT,
        @manufacturer_description VARCHAR (100), @manufacturer_description_ow BIT,
		@serial_number VARCHAR (100), @serial_number_ow BIT, @serial_name  VARCHAR (150),
		@serial_name_ow BIT, @manufacturer_id  INT,  @manufacturer_domain_id SMALLINT,
		@plug_type VARCHAR (20), @plug_type_ow BIT, @connection_type VARCHAR (20), @connection_type_ow BIT, @lan INT, @lan_ow BIT,
		@network_type VARCHAR(20), @network_type_ow BIT, @network_option INT, @network_option_ow BIT, @ports NUMERIC(10), @ports_ow BIT,
		@bluetooth BIT, @bluetooth_ow BIT, @cat6 BIT, @cat6_ow BIT, @displayport BIT, @displayport_ow BIT, @dvi BIT, @dvi_ow BIT,
		@hdmi BIT, @hdmi_ow BIT, @wireless BIT, @wireless_ow BIT, @volts VARCHAR(20), @volts_ow BIT, @amps VARCHAR(20), @amps_ow BIT

	SET @is_insert = 1;
	IF EXISTS (SELECT * FROM DELETED) BEGIN
		SET @is_insert = 0;
	END

	DECLARE pri_opt_cursor CURSOR LOCAL FOR SELECT i.inventory_id, i.inventory_source_id, i.inventory_target_id, i.asset_profile, i.none_option, 
		d.asset_profile, i.asset_domain_id, i.asset_id, i.detailed_budget,
		d.detailed_budget, d.none_option, i.domain_id, coalesce(i.total_unit_budget,0), 
		coalesce(i.unit_markup,0), coalesce(i.unit_escalation,0), coalesce(i.unit_tax,0), coalesce(i.unit_install_net,0), 
		coalesce(i.unit_install_markup,0), coalesce(i.unit_freight_markup,0), coalesce(i.unit_freight_net,0),
		(CASE WHEN r.isNew = 0 or i.inventory_source_id is not null THEN 0 ELSE (COALESCE(i.budget_qty,0) - COALESCE(i.dnp_qty,0)) * pr.room_quantity END),
		coalesce(i.asset_description_ow, 0), i.asset_description, i.placement, coalesce(i.placement_ow, 0), i.resp,
		coalesce(i.height_ow, 0), i.height, coalesce(i.width_ow, 0), i.width, coalesce(i.depth_ow, 0), i.depth, coalesce(i.mounting_height_ow, 0), i.mounting_height,
		coalesce(i.class_ow, 0), i.class, i.jsn_code, i.jsn_utility1, i.jsn_utility2,
		i.jsn_utility3, i.jsn_utility4, i.jsn_utility5, i.jsn_utility6, i.jsn_utility7, coalesce(i.jsn_ow, 0),
		i.manufacturer_description, coalesce(i.manufacturer_description_ow, 0), i.serial_number,
		coalesce(i.serial_number_ow, 0), i.serial_name, coalesce(i.serial_name_ow, 0),
		i.plug_type, coalesce(i.plug_type_ow, 0), i.connection_type, coalesce(i.connection_type_ow, 0), i.lan, coalesce(i.lan_ow, 0), i.network_type, coalesce(i.network_type_ow, 0),
		i.network_option, coalesce(i.network_option_ow, 0), i.ports, coalesce(i.ports_ow, 0), i.bluetooth, coalesce(i.bluetooth_ow, 0), i.cat6, coalesce(i.cat6_ow, 0),
		i.displayport, coalesce(i.displayport_ow, 0), i.dvi, coalesce(i.dvi_ow, 0), i.hdmi, coalesce(i.hdmi_ow, 0), i.wireless, coalesce(i.wireless_ow, 0),
		i.volts, coalesce(i.volts_ow, 0), i.amps, coalesce(i.amps_ow, 0)
		FROM inserted i 
			LEFT JOIN deleted d on i.inventory_id = d.inventory_id
			LEFT JOIN project_room pr ON pr.project_id = i.project_id AND pr.domain_id = i.domain_id AND pr.phase_id = i.phase_id AND pr.department_id = i.department_id AND pr.room_id = i.room_id 
			LEFT JOIN responsability r on trim(r.name) = trim(i.resp);

	OPEN pri_opt_cursor;

	FETCH NEXT FROM pri_opt_cursor INTO @inventory_id, @inventory_source_id, @inventory_target_id, @asset_profile, @none_option, @old_asset_profile, 
		@asset_domain_id, @asset_id, @detailed_budget, @old_detailed_budget, @old_none_option, @domain_id, @total_unit_budget, @unit_markup, 
		@unit_escalation, @unit_tax, @unit_install_net, @unit_install_markup, @unit_freight_markup, @unit_freight_net, @net_new,
		@asset_description_ow, @asset_description, @placement, @placement_ow, @resp,
		@height_ow, @height, @width_ow, @width, @depth_ow, @depth, @mounting_height_ow, @mounting_height,
		@class_ow, @class, @jsn_code, @jsn_utility1, @jsn_utility2, 
		@jsn_utility3, @jsn_utility4, @jsn_utility5, @jsn_utility6, @jsn_utility7, @jsn_ow,
		@manufacturer_description, @manufacturer_description_ow, @serial_number,
		@serial_number_ow, @serial_name, @serial_name_ow, @plug_type, @plug_type_ow, @connection_type, @connection_type_ow, @lan, @lan_ow, @network_type, @network_type_ow,
		@network_option, @network_option_ow, @ports, @ports_ow, @bluetooth, @bluetooth_ow, @cat6, @cat6_ow, @displayport, @displayport_ow, @dvi, @dvi_ow, @hdmi, @hdmi_ow,
		@wireless, @wireless_ow, @volts, @volts_ow, @amps, @amps_ow
	WHILE @@FETCH_STATUS = 0
		BEGIN
			IF @none_option = 1 AND @old_none_option = 0
				BEGIN
					DELETE FROM inventory_options WHERE inventory_id = @inventory_id;
				END

			IF @detailed_budget = 0 AND @old_detailed_budget = 1
				BEGIN
					UPDATE project_room_inventory SET options_unit_price = 0 WHERE inventory_id = @inventory_id;
					UPDATE inventory_options SET unit_price = 0 WHERE inventory_id = @inventory_id;
				END

			IF COALESCE(@asset_profile, '') <> COALESCE(@old_asset_profile, '')
				IF @old_asset_profile IS NULL
					BEGIN
						IF NOT EXISTS (SELECT * FROM [profile] WHERE asset_domain_id = @asset_domain_id AND asset_id = @asset_id AND [profile] = @asset_profile)
							INSERT INTO [profile](asset_domain_id, asset_id, [profile]) VALUES(@asset_domain_id, @asset_id, @asset_profile);
					END
				ELSE
					BEGIN
						IF @asset_profile IS NOT NULL AND EXISTS (SELECT * FROM project_room_inventory WHERE asset_domain_id = @asset_domain_id AND asset_id = @asset_id AND asset_profile = @old_asset_profile)
							BEGIN
								IF NOT EXISTS (SELECT * FROM [profile] WHERE asset_domain_id = @asset_domain_id AND asset_id = @asset_id AND [profile] = @asset_profile)
									INSERT INTO [profile](asset_domain_id, asset_id, [profile]) VALUES(@asset_domain_id, @asset_id, @asset_profile);
							END
						ELSE
							IF @asset_profile IS NOT NULL AND NOT EXISTS (SELECT * FROM [profile] WHERE asset_domain_id = @asset_domain_id AND asset_id = @asset_id AND [profile] = @asset_profile)
								UPDATE [profile] SET [profile] = @asset_profile WHERE asset_domain_id = @asset_domain_id AND asset_id = @asset_id
									AND [profile] = @old_asset_profile;
							ELSE
								DELETE FROM [profile] WHERE asset_domain_id = @asset_domain_id AND asset_id = @asset_id
									AND [profile] = @old_asset_profile;
					END

			SET @unit_markup_calc = (@unit_markup/100) * @total_unit_budget;
			SET @unit_escalation_calc = (@unit_escalation/100) * @total_unit_budget;
			SET @unit_budget_adjusted = @total_unit_budget + @unit_markup_calc + @unit_escalation_calc;
			SET @unit_tax_calc = @unit_budget_adjusted * (@unit_tax/100);
			SET @unit_install = @unit_install_net * (1 + (@unit_install_markup/100));
			SET @unit_freight = @unit_freight_net * (1 + (@unit_freight_markup/100));
			SET @unit_budget_total = @unit_budget_adjusted + @unit_freight + @unit_install + @unit_tax_calc; 
			SET @total_install_net =  @unit_install_net * @net_new;
			SET @total_budget_adjusted = @unit_budget_adjusted * @net_new;
			SET @total_tax =  @unit_tax_calc * @net_new;
			SET @total_install = @unit_install * @net_new; 
			SET @total_freight_net =  @unit_freight_net * @net_new;
			SET @total_freight =  @unit_freight * @net_new;
			SET @total_budget =  @total_freight + @total_tax + @total_budget_adjusted + @total_install;

			-- Checks for overwriten fields an update them accordingly
			IF (@asset_description_ow <> 1 OR @asset_description IS NULL) OR
				(@placement_ow <> 1) OR (@height_ow <> 1) OR
				(@width_ow <> 1) OR (@depth_ow <> 1) OR
				(@mounting_height_ow <> 1) OR (@class_ow <> 1) OR
				(@jsn_ow <> 1) OR (@serial_number_ow <> 1)  OR (@serial_name_ow <> 1) OR
                (@manufacturer_description_ow <> 1) OR
				(@placement_ow <> 1) OR
				(@class_ow <> 1) OR (@plug_type_ow <> 1) OR (@connection_type_ow <> 1) OR (@lan_ow <> 1) OR (@network_type_ow <> 1) OR (@network_option_ow <> 1)
			BEGIN
				SELECT
					@asset_description = CASE WHEN @asset_description_ow <> 1 OR @asset_description IS NULL THEN  [asset_description]  ELSE @asset_description END,
					@placement = CASE WHEN @placement_ow <> 1 THEN  [placement]  ELSE @placement END,
					@height = CASE WHEN @height_ow <> 1  THEN  [height]  ELSE @height END,
					@width = CASE WHEN @width_ow <> 1  THEN  [width]  ELSE @width END,
					@depth = CASE WHEN @depth_ow <> 1  THEN  [depth]  ELSE @depth END,
					@mounting_height = CASE WHEN @mounting_height_ow <> 1  THEN  [mounting_height]  ELSE @mounting_height END,
					@class = CASE WHEN @class_ow <> 1 THEN  [class]  ELSE @class END,
					@jsn_id = jsn_id, @jsn_domain_id = jsn_domain_id, @jsn_suffix = jsn_suffix,
					@jsn_utility1 = CASE WHEN @jsn_ow <> 1 THEN  [jsn_utility1]  ELSE @jsn_utility1 END,
					@jsn_utility2 = CASE WHEN @jsn_ow <> 1 THEN  [jsn_utility2]  ELSE @jsn_utility2 END,
					@jsn_utility3 = CASE WHEN @jsn_ow <> 1 THEN  [jsn_utility3]  ELSE @jsn_utility3 END,
					@jsn_utility4 = CASE WHEN @jsn_ow <> 1 THEN  [jsn_utility4]  ELSE @jsn_utility4 END,
					@jsn_utility5 = CASE WHEN @jsn_ow <> 1 THEN  [jsn_utility5]  ELSE @jsn_utility5 END,
					@jsn_utility6 = CASE WHEN @jsn_ow <> 1 THEN  [jsn_utility6]  ELSE @jsn_utility6 END,
					@jsn_utility7 = CASE WHEN @jsn_ow <> 1 THEN  [jsn_utility7]  ELSE @jsn_utility7 END,
					@serial_name = CASE WHEN @serial_name_ow <> 1 THEN  [serial_name]  ELSE @serial_name END,
					@serial_number = CASE WHEN @serial_number_ow <> 1 THEN  [serial_number]  ELSE @serial_number END,
					@manufacturer_id  = [manufacturer_id],
					@manufacturer_domain_id = [manufacturer_domain_id],
					@placement = CASE WHEN @placement_ow <> 1 THEN [placement] ELSE @placement END,
					@class = CASE WHEN @class_ow <> 1 THEN [class] ELSE @class END,
					@plug_type = CASE WHEN @plug_type_ow <> 1  THEN  [plug_type]  ELSE @plug_type END,
					@connection_type = CASE WHEN @connection_type_ow <> 1 THEN [connection_type] ELSE @connection_type END,
					@lan = CASE WHEN @lan_ow <> 1 THEN [lan] ELSE @lan END,
					@network_type = CASE WHEN @network_type_ow <> 1 THEN [network_type] ELSE @network_type END,
					@network_option = CASE WHEN @network_option_ow <> 1 THEN [network_option] ELSE @network_option END,
					@ports = CASE WHEN @ports_ow <> 1 THEN [ports] ELSE @ports END,
					@bluetooth = CASE WHEN @bluetooth_ow <> 1 THEN [bluetooth] ELSE @bluetooth END,
					@cat6 = CASE WHEN @cat6_ow <> 1 THEN [cat6] ELSE @cat6 END,
					@displayport = CASE WHEN @displayport_ow <> 1 THEN [displayport] ELSE @displayport END,
					@dvi = CASE WHEN @dvi_ow <> 1 THEN [dvi] ELSE @dvi END,
					@hdmi = CASE WHEN @hdmi_ow <> 1 THEN [hdmi] ELSE @hdmi END,
					@wireless = CASE WHEN @wireless_ow <> 1 THEN [wireless] ELSE @wireless END,
					@volts = CASE WHEN @volts_ow <> 1 THEN [volts] ELSE @volts END,
					@amps = CASE WHEN @amps_ow <> 1 THEN [amps] ELSE @amps END
					FROM [assets] where asset_id = @asset_id AND domain_id = @asset_domain_id;

				IF @jsn_ow <> 1 
				BEGIN
					SELECT TOP 1 @jsn_code = jsn_code from jsn where Id = @jsn_id AND domain_id = @jsn_domain_id;

					IF ISNULL(@jsn_suffix, '') <> ''
					BEGIN
						SET @jsn_code = @jsn_code + '.' + @jsn_suffix;
					END
				END

				IF @manufacturer_description_ow <> 1  BEGIN
					SELECT @manufacturer_description = manufacturer_description from manufacturer where manufacturer_id = @manufacturer_id AND domain_id = @manufacturer_domain_id;
                END
			END


			UPDATE project_room_inventory SET unit_markup_calc = @unit_markup_calc, unit_escalation_calc = @unit_escalation_calc, unit_budget_adjusted = @unit_budget_adjusted,
			unit_tax_calc = @unit_tax_calc, unit_install = @unit_install, unit_freight = @unit_freight, unit_budget_total = @unit_budget_total, total_install_net = @total_install_net,
			total_budget_adjusted = @total_budget_adjusted, total_tax = @total_tax, total_install = @total_install, total_freight_net = @total_freight_net, total_freight = @total_freight,
			total_budget = @total_budget, net_new = @net_new, asset_description = @asset_description, placement = @placement, height = @height,
			width = @width, depth = @depth, mounting_height = @mounting_height, class = @class, jsn_code = @jsn_code, jsn_utility1 = @jsn_utility1, jsn_utility2 = @jsn_utility2,
			jsn_utility3 = @jsn_utility3, jsn_utility4 = @jsn_utility4, jsn_utility5 = @jsn_utility5, jsn_utility6 = @jsn_utility6, jsn_utility7 = @jsn_utility7,
			manufacturer_description = @manufacturer_description, serial_number = @serial_number, serial_name = @serial_name, plug_type = @plug_type, connection_type = @connection_type,
			lan = @lan, network_type = @network_type, network_option = @network_option, network_option_ow = @network_option_ow, ports = @ports, ports_ow = @ports_ow,
			bluetooth = @bluetooth, bluetooth_ow = @bluetooth_ow, cat6 = @cat6, cat6_ow = @cat6_ow, displayport = @displayport, displayport_ow = @displayport_ow, 
			dvi = @dvi, dvi_ow = @dvi_ow, hdmi = @hdmi, hdmi_ow = @hdmi_ow, wireless = @wireless, wireless_ow = @wireless_ow,
			volts = @volts, volts_ow = @volts_ow, amps = @amps, amps_ow = @amps_ow, date_modified = GETDATE()
			WHERE inventory_id = @inventory_id and domain_id = @domain_id;

			-- If we have either source or target id, we update the linked items accordingly
			IF @is_insert = 0 AND ( @inventory_source_id IS NOT NULL OR @inventory_target_id IS NOT NULL) BEGIN
				EXEC update_link_inventory @domain_id, @inventory_id
			END

			FETCH NEXT FROM pri_opt_cursor INTO @inventory_id, @inventory_source_id, @inventory_target_id, @asset_profile, @none_option, @old_asset_profile, 
			@asset_domain_id, @asset_id, @detailed_budget, @old_detailed_budget, @old_none_option, @domain_id, @total_unit_budget, @unit_markup, 
			@unit_escalation, @unit_tax, @unit_install_net, @unit_install_markup, @unit_freight_markup, @unit_freight_net, @net_new,
			@asset_description_ow, @asset_description, @placement, @placement_ow, @resp,
			@height_ow, @height, @width_ow, @width, @depth_ow, @depth, @mounting_height_ow, @mounting_height,
			@class_ow, @class, @jsn_code, @jsn_utility1, @jsn_utility2, 
			@jsn_utility3, @jsn_utility4, @jsn_utility5, @jsn_utility6, @jsn_utility7, @jsn_ow,
			@manufacturer_description, @manufacturer_description_ow, @serial_number,
			@serial_number_ow, @serial_name, @serial_name_ow, @plug_type, @plug_type_ow, @connection_type, @connection_type_ow, @lan, @lan_ow, @network_type, @network_type_ow,
			@network_option, @network_option_ow, @ports, @ports_ow, @bluetooth, @bluetooth_ow, @cat6, @cat6_ow, @displayport, @displayport_ow, @dvi, @dvi_ow, @hdmi, @hdmi_ow,
			@wireless, @wireless_ow, @volts, @volts_ow, @amps, @amps_ow
		 END
	CLOSE pri_opt_cursor;
	DEALLOCATE pri_opt_cursor;
GO



CREATE TRIGGER delete_profile_options
ON project_room_inventory AFTER DELETE
AS
	DECLARE @old_asset_profile VARCHAR(3000), @asset_domain_id SMALLINT, @asset_id INTEGER;

	DECLARE cur CURSOR LOCAL FOR SELECT asset_profile, asset_domain_id, asset_id FROM deleted;
	OPEN cur;
	FETCH NEXT FROM cur INTO @old_asset_profile, @asset_domain_id, @asset_id;
	WHILE @@FETCH_STATUS = 0
		BEGIN

			IF NOT EXISTS (SELECT * FROM project_room_inventory WHERE asset_domain_id = @asset_domain_id AND
				asset_id = @asset_id AND asset_profile = @old_asset_profile)
				DELETE FROM [profile] WHERE asset_domain_id = @asset_domain_id AND asset_id = @asset_id AND [profile] = @old_asset_profile;

			FETCH NEXT FROM cur INTO @old_asset_profile, @asset_domain_id, @asset_id;
		END
	CLOSE cur;
	DEALLOCATE cur;
GO





