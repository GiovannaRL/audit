CREATE PROCEDURE [dbo].[duplicate_asset_new_code]
   @old_domain_id SMALLINT,
   @old_asset_id INTEGER,
   @new_domain_id SMALLINT,
   @new_code VARCHAR(15),
   @change_inventories BIT,
   @added_by VARCHAR(50), 
   @link_duplicated bit = 0,
   @need_approval bit = 0, 
   @approval_modify_aw_asset bit = 0
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @new_asset_id INTEGER, @new_options_id INTEGER, @new_colors_id INTEGER, @new_inventory_id INTEGER, @old_inventory_id INTEGER;
	DECLARE @id INTEGER, @code VARCHAR(50), @description VARCHAR(270), @domain_id SMALLINT,
		@unit_price NUMERIC(10, 2), @data_type VARCHAR(1), @vendor_domain_id SMALLINT, @vendor_name	VARCHAR(50), @vendor_id int,
		@vendor_min_cost numeric(10,2), @vendor_max_cost numeric(10,2), @vendor_avg_cost numeric(10,2), @vendor_comment varchar(250), 
		@vendor_model_number varchar(50), @vendor_last_cost numeric(10,2), 
		@old_manufacturer_id int, @old_manufacturer_domain_id smallint, @old_manufacturer_description varchar(250), @old_manufacturer_comment varchar(250), @new_manufacturer_id int, 
		@old_subcategory_id int, @old_subcategory_domain_id smallint, @new_subcategory_id int, @new_category_id int, @sub_category_id int,
		@sub_description varchar (200), @sub_domain_id smallint, @sub_category_domain_id smallint, @sub_HVAC char(1), @sub_Plumbing char(1), @sub_Gases char(1), @sub_IT char(1), 
		@sub_Electrical char(1), @sub_use_category_settings bit,   @sub_Support char(1), @sub_Physical char(1), @sub_Environmental char(1), @sub_asset_code varchar (3), @sub_asset_code_domain_id smallint,
		@cat_description varchar (200), @cat_HVAC char(1), @cat_Plumbing char(1), @cat_Gases char(1), @cat_IT char(1), 
		@cat_Electrical char(1), @cat_Support char(1), @cat_Physical char(1), @cat_Environmental char(1), @cat_asset_code varchar (3), @cat_asset_code_domain_id smallint;


	BEGIN TRANSACTION


	create table #Asset
	(	
		domain_id smallint,
		asset_code varchar(25),
		manufacturer_id int,
		manufacturer_domain_id smallint,
		asset_description varchar(300),
		subcategory_id int,
		height varchar(25),
		width varchar(25),
		depth varchar(25),
		weight varchar(25),
		serial_number varchar(100),
		min_cost numeric(10,2),
		max_cost numeric(10,2),
		avg_cost numeric(10,2),
		last_cost numeric(10,2),
		default_resp varchar(10),
		cut_sheet varchar(100),
		date_added date,
		added_by varchar(50),
		comment varchar(1000),
		cad_block varchar(100),
		water varchar(150),
		plumbing varchar(300),
		data varchar(150),
		electrical varchar(150),
		mobile varchar(50),
		blocking varchar(50),
		medgas varchar(150),
		supports varchar(150),
		discontinued bit,
		last_budget_update date,
		photo varchar(100),
		eq_measurement_id int,
		water_option int,
		plumbing_option int,
		data_option int,
		electrical_option int,
		mobile_option int,
		blocking_option int,
		medgas_option int,
		supports_option int,
		revit varchar(100),
		placement varchar(10),
		clearance_left numeric(10,2),
		clearance_right numeric(10,2),
		clearance_front numeric(10,2),
		clearance_back numeric(10,2),
		clearance_top numeric(10,2),
		clearance_bottom numeric(10,2),
		volts varchar(20),
		phases int,
		hertz varchar(20),
		amps varchar(20),
		volt_amps numeric(10,2),
		watts varchar(20),
		cfm varchar(150),
		btus varchar(150),
		misc_ase bit,
		misc_ada bit,
		misc_seismic bit,
		misc_antimicrobial bit,
		misc_ecolabel bit,
		misc_ecolabel_desc varchar(150),
		mapping_code varchar(20),
		medgas_oxygen bit,
		medgas_nitrogen bit,
		medgas_air bit,
		medgas_n2o bit,
		medgas_vacuum bit,
		medgas_wag bit,
		medgas_co2 bit,
		medgas_other bit,
		medgas_steam bit,
		medgas_natgas bit,
		plu_hot_water bit,
		plu_drain bit,
		plu_cold_water bit,
		plu_return bit,
		plu_treated_water bit,
		plu_relief bit,
		plu_chilled_water bit,
		network_option int,
		ports numeric(10),
		bluetooth bit,
		cat6 bit,
		displayport bit,
		dvi bit,
		hdmi bit,
		wireless bit,
		serial_name varchar(150),
		useful_life int,
		loaded_weight numeric(10,2),
		ship_weight numeric(10,2),
		alternate_asset varchar(50),
		updated_at date,
		subcategory_domain_id smallint,
		asset_suffix varchar(200),
		category_attribute varchar(2),
		jsn_id INT, 
		jsn_domain_id SMALLINT, 
		class INT, 
		gas_liquid_co2 BIT, 
		gas_liquid_nitrogen BIT, 
		gas_instrument_air BIT, 
		gas_liquid_propane_gas BIT, 
		gas_methane BIT, 
		gas_butane BIT, 
		gas_propane BIT, 
		gas_hydrogen BIT, 
		gas_acetylene BIT, 
		medgas_high_pressure BIT, 
		misc_shielding_lead_line BIT, 
		misc_shielding_magnetic BIT, 
		jsn_suffix VARCHAR(4), 
		jsn_utility1_ow BIT NULL, 
		jsn_utility1 VARCHAR(10), 
		jsn_utility2_ow BIT NULL, 
		jsn_utility2 VARCHAR(10), 
		jsn_utility3_ow BIT NULL,
		jsn_utility3 VARCHAR(10),
		jsn_utility4_ow BIT NULL, 
		jsn_utility4 VARCHAR(10),
		jsn_utility5_ow BIT NULL, 
		jsn_utility5 VARCHAR(10),
		jsn_utility6_ow BIT NULL, 
		jsn_utility6 VARCHAR(10),
		jsn_utility7_ow BIT NULL, 
		jsn_utility7 VARCHAR(10),
		mounting_height varchar(25),
		approval_pending_domain SMALLINT,
		created_from VARCHAR(25), 
		approval_modify_aw_asset bit
	)

	CREATE TABLE #Asset_vendor
	(
		vendor_id int,
		min_cost numeric (10,2),
		max_cost numeric (10,2),
		avg_cost numeric (10,2),
		comment varchar (250),
		model_number varchar (50),
		vendor_domain_id smallint,
		last_cost numeric (10,2)
	)


	



	insert into #Asset
	SELECT @new_domain_id, @new_code, manufacturer_id, manufacturer_domain_id, asset_description, subcategory_id, height, 
		width, depth, weight, serial_number, min_cost, max_cost, avg_cost, last_cost, default_resp, 
		CONCAT(@new_code, SUBSTRING(cut_sheet, CHARINDEX('.', cut_sheet), 10)), GETDATE(), @added_by, comment, 
		CASE WHEN cad_block IS NOT NULL THEN CONCAT(@new_code, SUBSTRING(cad_block, CHARINDEX('.', cad_block), 10)) ELSE NULL END, 
		water, plumbing, data, electrical, mobile, blocking, medgas, supports, discontinued, last_budget_update, 
		CASE WHEN photo IS NOT NULL THEN CONCAT(@new_code, SUBSTRING(photo, CHARINDEX('.', photo), 10)) ELSE NULL END, 
		eq_measurement_id, water_option, plumbing_option, data_option, electrical_option, mobile_option, blocking_option, medgas_option, 
		supports_option, CASE WHEN revit IS NOT NULL THEN CONCAT(@new_code, SUBSTRING(revit, CHARINDEX('.', revit), 10)) ELSE NULL END,
		placement, clearance_left, clearance_right, clearance_front, clearance_back, clearance_top, clearance_bottom, 
		volts, phases, hertz, amps, volt_amps, watts, cfm, btus, misc_ase, misc_ada, misc_seismic, misc_antimicrobial, misc_ecolabel, 
		misc_ecolabel_desc, mapping_code, medgas_oxygen, medgas_nitrogen, medgas_air, medgas_n2o, medgas_vacuum, medgas_wag, medgas_co2, 
		medgas_other, medgas_steam, medgas_natgas, plu_hot_water, plu_drain, plu_cold_water, plu_return, plu_treated_water, plu_relief, 
		plu_chilled_water, network_option, ports, bluetooth, cat6, displayport, dvi, hdmi, wireless,
		serial_name, useful_life, loaded_weight, ship_weight, alternate_asset, GETDATE(), subcategory_domain_id, asset_suffix,
		category_attribute, jsn_id, jsn_domain_id, class, gas_liquid_co2, gas_liquid_nitrogen, gas_instrument_air,
		gas_liquid_propane_gas, gas_methane, gas_butane, gas_propane, gas_hydrogen, gas_acetylene, medgas_high_pressure, misc_shielding_lead_line,
		misc_shielding_magnetic, jsn_suffix, jsn_utility1_ow, jsn_utility1, jsn_utility2_ow, jsn_utility2, jsn_utility3_ow, jsn_utility3,
		jsn_utility4_ow, jsn_utility4, jsn_utility5_ow, jsn_utility5, jsn_utility6_ow, jsn_utility6, jsn_utility7_ow, jsn_utility7, mounting_height,
		CASE WHEN @need_approval = 1 THEN @old_domain_id ELSE NULL END, asset_code, @approval_modify_aw_asset
	FROM assets where asset_id = @old_asset_id and domain_id = @old_domain_id;

	select @old_manufacturer_id = manufacturer_id, @old_manufacturer_domain_id = manufacturer_domain_id, @old_subcategory_domain_id = subcategory_domain_id, @old_subcategory_id = subcategory_id
	FROM #Asset 

	/*********************************************************************************
	* CHECK IF MANUFACTURER, CATEGORY AND SUBCATEGORY ARE FROM A DIFERENT DOMAIN
	**********************************************************************************/
	select @old_manufacturer_description = manufacturer_description, @old_manufacturer_comment = comment 
	from manufacturer where manufacturer_id = @old_manufacturer_id and domain_id = @old_domain_id 
	
	select @sub_category_id = category_id, @sub_description = description, @sub_category_domain_id = category_domain_id, @sub_HVAC = HVAC, @sub_Plumbing = Plumbing, @sub_Gases = Gases, @sub_IT = IT, @sub_Electrical = Electrical, 
		@sub_use_category_settings = use_category_settings, @sub_Support = Support, @sub_Physical = Physical, @sub_Environmental = Environmental, @sub_asset_code = asset_code, @sub_asset_code_domain_id = asset_code_domain_id 
	from assets_subcategory where subcategory_id = @old_subcategory_id and domain_id = @old_subcategory_domain_id

	select @cat_description = description, @cat_HVAC = HVAC, @cat_Plumbing = Plumbing, @cat_Gases = Gases, @cat_IT = IT, @cat_Electrical = Electrical, 
		@cat_Support = Support, @cat_Physical = Physical, @cat_Environmental = Environmental, @cat_asset_code = asset_code, @cat_asset_code_domain_id = asset_code_domain_id  
	from assets_category where category_id = @sub_category_id and domain_id = @sub_category_domain_id  
	--**********************************************************************************************

	EXEC sp_set_session_context 'domain_id', @new_domain_id

	
	INSERT INTO assets(domain_id, asset_code, manufacturer_id, manufacturer_domain_id, asset_description, subcategory_id, height, 
		width, depth, weight, serial_number, min_cost, max_cost, avg_cost, last_cost, default_resp, cut_sheet, date_added, added_by, comment, 
		cad_block, water, plumbing, data, electrical, mobile, blocking, medgas, supports, discontinued, last_budget_update, photo, 
		eq_measurement_id, water_option, plumbing_option, data_option, electrical_option, mobile_option, blocking_option, medgas_option, 
		supports_option, revit, placement, clearance_left, clearance_right, clearance_front, clearance_back, clearance_top, clearance_bottom, 
		volts, phases, hertz, amps, volt_amps, watts, cfm, btus, misc_ase, misc_ada, misc_seismic, misc_antimicrobial, misc_ecolabel, 
		misc_ecolabel_desc, mapping_code, medgas_oxygen, medgas_nitrogen, medgas_air, medgas_n2o, medgas_vacuum, medgas_wag, medgas_co2, 
		medgas_other, medgas_steam, medgas_natgas, plu_hot_water, plu_drain, plu_cold_water, plu_return, plu_treated_water, plu_relief, 
		plu_chilled_water, network_option, ports, bluetooth, cat6, displayport, dvi, hdmi, wireless,
		serial_name, useful_life, loaded_weight, ship_weight, alternate_asset, updated_at, subcategory_domain_id, asset_suffix,
		category_attribute, jsn_id, jsn_domain_id, class, gas_liquid_co2, gas_liquid_nitrogen, gas_instrument_air,
		gas_liquid_propane_gas, gas_methane, gas_butane, gas_propane, gas_hydrogen, gas_acetylene, medgas_high_pressure, misc_shielding_lead_line,
		misc_shielding_magnetic, jsn_suffix, jsn_utility1_ow, jsn_utility1, jsn_utility2_ow, jsn_utility2, jsn_utility3_ow, jsn_utility3,
		jsn_utility4_ow, jsn_utility4, jsn_utility5_ow, jsn_utility5, jsn_utility6_ow, jsn_utility6, jsn_utility7_ow, jsn_utility7, mounting_height, 
		approval_pending_domain, created_from, approval_modify_aw_asset) 
	select * from #Asset
	
	SELECT @new_asset_id = MAX(asset_id) FROM assets where domain_id = @new_domain_id;


	/************************************************************************
	* CHECK IF MANUFACTURER EXISTS IN NEW DOMAIN OR NEED TO BE ADDED
	*************************************************************************/
	IF @old_manufacturer_domain_id != @new_domain_id BEGIN
		select @new_manufacturer_id = manufacturer_id from manufacturer WHERE trim(manufacturer_description) = trim(@old_manufacturer_description) and domain_id = @new_domain_id
		IF @new_manufacturer_id is null AND @old_manufacturer_description IS NOT NULL BEGIN
			INSERT INTO manufacturer(manufacturer_description, domain_id, comment, date_added, added_by)
			VALUES(@old_manufacturer_description, @new_domain_id, @old_manufacturer_comment, GETDATE(), @added_by)
			
			select @new_manufacturer_id = MAX(manufacturer_id) from manufacturer where domain_id = @new_domain_id
		END
	END

	--CATEGORY E SUBCATEGORY
	IF @old_subcategory_domain_id != @new_domain_id BEGIN
		select @new_subcategory_id = subcategory_id from assets_subcategory where domain_id = @new_domain_id and trim(description) = trim(@sub_description)
		select @new_category_id = category_id from assets_category where domain_id = @new_domain_id and trim(description) = trim(@sub_description)
		IF @new_subcategory_id IS NULL AND @sub_description IS NOT NULL BEGIN
			IF @new_category_id is null AND @cat_description is not null BEGIN
				INSERT INTO assets_category(description, domain_id, HVAC, Plumbing, Gases, IT, Electrical, Support, Physical, Environmental, asset_code, asset_code_domain_id, date_added, added_by)
				values(@sub_description, @new_domain_id, @sub_HVAC, @sub_Plumbing, @sub_Gases, @sub_IT, @sub_Electrical, @sub_Support, @sub_Physical, @sub_Environmental, @sub_asset_code, @sub_asset_code_domain_id, GETDATE(), @added_by)
				
				select @new_category_id = MAX(category_id) from assets_category where domain_id = @new_domain_id
			END

			INSERT INTO assets_subcategory(category_id, description, domain_id, category_domain_id, HVAC, Plumbing, Gases, IT, Electrical, use_category_settings, Support, Physical, Environmental, asset_code, asset_code_domain_id, date_added, added_by)
			values(@new_category_id, @sub_description, @new_domain_id, @new_domain_id, @sub_HVAC, @sub_Plumbing, @sub_Gases, @sub_IT, @sub_Electrical, @sub_use_category_settings, @sub_Support, @sub_Physical, @sub_Environmental, @sub_asset_code, @sub_asset_code_domain_id, GETDATE(), @added_by)
			
			select @new_subcategory_id = MAX(subcategory_id) from assets_subcategory where domain_id = @new_domain_id
		END
	END
	--**********************************************************


	--UPDATE ASSETS WITH THE NEW MANUFACTURER AND SUBCATEGORY CREATED FROM OLD DOMAIN TO NEW DOMAIN
	update assets set manufacturer_id = coalesce(@new_manufacturer_id, manufacturer_id), 
	manufacturer_domain_id = CASE WHEN @new_manufacturer_id is not null THEN @new_domain_id ELSE manufacturer_domain_id END, 
	subcategory_id = coalesce(@new_subcategory_id, subcategory_id),
	subcategory_domain_id = CASE WHEN @new_subcategory_id is not null THEN @new_domain_id ELSE subcategory_domain_id END
	where asset_id = @new_asset_id and domain_id = @new_domain_id;

	EXEC sp_set_session_context 'domain_id', @old_domain_id
	
	/* VENDORS */
	INSERT INTO #Asset_vendor
	SELECT vendor_id, min_cost, max_cost, avg_cost, comment, model_number, vendor_domain_id, last_cost 
	FROM assets_vendor WHERE asset_id = @old_asset_id and asset_domain_id = @old_domain_id and vendor_domain_id = @old_domain_id;

	DECLARE vendor_cursor CURSOR LOCAL FOR SELECT vendor_id, min_cost, max_cost, avg_cost, comment, model_number, vendor_domain_id, last_cost
		FROM #Asset_vendor;
	OPEN vendor_cursor
	FETCH NEXT FROM vendor_cursor INTO @vendor_id, @vendor_min_cost, @vendor_max_cost, @vendor_avg_cost, @vendor_comment, @vendor_model_number, 
			@vendor_domain_id, @vendor_last_cost
	WHILE @@FETCH_STATUS = 0
		BEGIN

			IF @vendor_domain_id != 1 AND @new_domain_id = 1 BEGIN
				
				EXEC sp_set_session_context 'domain_id', @old_domain_id
				
				SELECT @vendor_name = [name] FROM vendor WHERE vendor_id = @vendor_id and domain_id = @vendor_domain_id;

				IF NOT EXISTS(SELECT 1 FROM vendor WHERE domain_id = @new_domain_id AND [name] = @vendor_name)
				BEGIN
					EXEC sp_set_session_context 'domain_id', @new_domain_id;
					INSERT INTO vendor([name], territory, hospitals, date_added, added_by, comment, domain_id)
					SELECT [name], territory, hospitals, GETDATE(), @added_by, comment, @new_domain_id
					FROM vendor WHERE vendor_id = @vendor_id and domain_id = @vendor_domain_id
				END

				SET @vendor_domain_id = 1;
				SELECT @vendor_id = vendor_id FROM vendor where domain_id = @new_domain_id and [name] = @vendor_name;
			END
			
			EXEC sp_set_session_context 'domain_id', @new_domain_id
			INSERT INTO assets_vendor(asset_id, vendor_id, min_cost, max_cost, avg_cost, date_added, added_by, comment, model_number, 
				asset_domain_id, vendor_domain_id, last_cost)
			VALUES(@new_asset_id, @vendor_id, @vendor_min_cost, @vendor_max_cost, @vendor_avg_cost, GETDATE(), @added_by, @vendor_comment, @vendor_model_number,
			@new_domain_id, @vendor_domain_id, @vendor_last_cost);

		FETCH NEXT FROM vendor_cursor INTO @vendor_id, @vendor_min_cost, @vendor_max_cost, @vendor_avg_cost, @vendor_comment, @vendor_model_number, 
			@vendor_domain_id, @vendor_last_cost
		END
	CLOSE vendor_cursor;
	DEALLOCATE vendor_cursor;
	

	EXEC sp_set_session_context 'domain_id', @old_domain_id

	DECLARE options_cursor CURSOR LOCAL FOR SELECT asset_option_id, code, description, data_type, unit_budget 
		FROM assets_options WHERE asset_id = @old_asset_id and domain_id = @old_domain_id;
	OPEN options_cursor
	FETCH NEXT FROM options_cursor INTO @id, @code, @description, @data_type, @unit_price;
	WHILE @@FETCH_STATUS = 0
		BEGIN
			EXEC sp_set_session_context 'domain_id', @new_domain_id

			INSERT INTO assets_options(asset_id, code, description, added_by, date_added, domain_id, data_type, unit_budget) 
				VALUES(@new_asset_id, @code, @description, @added_by, GETDATE(), @new_domain_id, @data_type, @unit_price);

			EXEC sp_set_session_context 'domain_id', @old_domain_id

			IF @change_inventories = 1
				BEGIN
					SELECT @new_options_id = MAX(asset_option_id) FROM assets_options WHERE domain_id = @new_domain_id AND asset_id = @new_asset_id;
					DECLARE inventory_cursor CURSOR LOCAL FOR SELECT a.inventory_id FROM project_room_inventory a, inventory_options b 
						WHERE asset_id = @old_asset_id and a.domain_id = @old_domain_id AND a.inventory_id = b.inventory_id AND b.option_id = @id;
					OPEN inventory_cursor
					FETCH NEXT FROM inventory_cursor INTO @old_inventory_id;
					WHILE @@FETCH_STATUS = 0
						BEGIN
							INSERT INTO project_room_inventory (project_id, department_id, room_id, asset_id, status, resp, budget_qty,
								dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, 
								date_added, added_by, comment, lease_qty, cost_center_id, tag, cad_id, asset_domain_id,
								temporary_location, received_date, delivered_date) 
							SELECT project_id, department_id, room_id, @new_asset_id, status, resp, budget_qty, 
								dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, 
								date_added, @added_by, comment, lease_qty, cost_center_id, tag, cad_id, @new_domain_id,
								temporary_location, received_date, delivered_date
							FROM project_room_inventory a, inventory_options b
							WHERE a.inventory_id = @old_inventory_id AND project_id = ANY
								(SELECT project_id FROM project WHERE status IN ('A', 'P') AND domain_id = @new_domain_id);
							SELECT @new_inventory_id = MAX(inventory_id) FROM project_room_inventory where domain_id = @new_domain_id AND asset_id = @new_asset_id;
			
							INSERT INTO inventory_options(inventory_id, option_id, domain_id, unit_price, quantity, inventory_domain_id) 
							SELECT @new_inventory_id, @new_options_id, @new_domain_id, unit_price, quantity, @new_domain_id
								FROM inventory_options WHERE inventory_id = @old_inventory_id AND option_id = @id;
							FETCH NEXT FROM inventory_cursor INTO @old_inventory_id;
						END
					CLOSE inventory_cursor;
					DEALLOCATE inventory_cursor;
				END
			FETCH NEXT FROM options_cursor INTO @id, @code, @description, @data_type, @unit_price;
		END
	CLOSE options_cursor;
	DEALLOCATE options_cursor;
	/* INVENTORY */
	IF @change_inventories = 1
	BEGIN
		
		INSERT INTO project_room_inventory (project_id, department_id, room_id, asset_id, status, resp, budget_qty, 
			dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, 
			date_added, added_by, comment, lease_qty, cost_center_id, tag, cad_id, asset_domain_id, detailed_budget,
			temporary_location, received_date, delivered_date) 
		SELECT project_id, department_id, room_id, @new_asset_id, status, resp, budget_qty, 
			dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, 
			date_added, @added_by, comment, lease_qty, cost_center_id, tag, cad_id, @new_domain_id, detailed_budget,
			temporary_location, received_date, delivered_date
		FROM project_room_inventory  WHERE  asset_id = @old_asset_id and domain_id = @old_domain_id AND (option_ids is null OR option_ids ='') AND
			project_id = ANY (SELECT project_id FROM project WHERE status IN ('A', 'P') AND domain_id = @new_domain_id);
		
		UPDATE inventory_purchase_order SET asset_id = @new_asset_id, asset_domain_id = @new_domain_id 
			WHERE asset_id = @old_asset_id AND asset_domain_id = @old_domain_id AND project_id = ANY(SELECT project_id FROM project WHERE status IN ('A', 'P') AND domain_id = @new_domain_id);
	
		DELETE FROM project_room_inventory WHERE asset_id = @old_asset_id AND domain_id = @old_domain_id  AND project_id = ANY(SELECT project_id FROM project WHERE status IN ('A', 'P') AND domain_id = @new_domain_id);
	END
	
	
	EXEC sp_set_session_context 'domain_id', @new_domain_id
	
	IF @link_duplicated = 1 BEGIN
		INSERT INTO related_assets values(@old_asset_id, @old_domain_id, @new_asset_id, @new_domain_id);
	END

	
	COMMIT TRANSACTION

	SELECT * FROM assets WHERE asset_id = @new_asset_id AND domain_id = @new_domain_id;
	
END


