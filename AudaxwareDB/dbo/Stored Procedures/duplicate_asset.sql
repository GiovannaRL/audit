
CREATE PROCEDURE [dbo].[duplicate_asset](
   @new_domain_id SMALLINT,
   @old_asset_id INTEGER,
   @change_inventories BIT,
   @added_by VARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @new_asset_id INTEGER, @new_options_id INTEGER, @new_colors_id INTEGER, @new_inventory_id INTEGER, @old_inventory_id INTEGER;
	DECLARE @id INTEGER, @code VARCHAR(50), @description VARCHAR(270), @domain_id SMALLINT,
		@unit_price NUMERIC(10, 2), @data_type VARCHAR(1);

	INSERT INTO assets(domain_id, asset_code, manufacturer_id, manufacturer_domain_id, asset_description, subcategory_id, height, 
		width, depth, weight, serial_number, min_cost, max_cost, avg_cost, last_cost, default_resp, cut_sheet, date_added, added_by, comment, 
		cad_block, water, plumbing, data, electrical, mobile, blocking, medgas, supports, discontinued, last_budget_update, photo, 
		eq_measurement_id, water_option, plumbing_option, data_option, electrical_option, mobile_option, blocking_option, medgas_option, 
		supports_option, revit, placement, clearance_left, clearance_right, clearance_front, clearance_back, clearance_top, clearance_bottom, 
		volts, phases, hertz, amps, volt_amps, watts, cfm, btus, misc_ase, misc_ada, misc_seismic, misc_antimicrobial, misc_ecolabel, 
		misc_ecolabel_desc, mapping_code, medgas_oxygen, medgas_nitrogen, medgas_air, medgas_n2o, medgas_vacuum, medgas_wag, medgas_co2, 
		medgas_other, medgas_steam, medgas_natgas, plu_hot_water, plu_drain, plu_cold_water, plu_return, plu_treated_water, plu_relief, 
		plu_chilled_water, serial_name, useful_life, loaded_weight, ship_weight, alternate_asset, updated_at, subcategory_domain_id,
		category_attribute, asset_suffix, jsn_id, jsn_domain_id, class, gas_liquid_co2, gas_liquid_nitrogen, gas_instrument_air,
		gas_liquid_propane_gas, gas_methane, gas_butane, gas_propane, gas_hydrogen, gas_acetylene, medgas_high_pressure, misc_shielding_lead_line,
		misc_shielding_magnetic, jsn_suffix, jsn_utility1_ow, jsn_utility1, jsn_utility2_ow, jsn_utility2, jsn_utility3_ow, jsn_utility3,
		jsn_utility4_ow, jsn_utility4, jsn_utility5_ow, jsn_utility5, jsn_utility6_ow, jsn_utility6, jsn_utility7_ow, jsn_utility7, mounting_height, created_from, lan, network_type, plug_type, connection_type, network_option,
		ports, bluetooth, cat6, displayport, dvi, hdmi, wireless) 
	SELECT @new_domain_id, CONCAT(asset_code, 'C'), manufacturer_id, manufacturer_domain_id, asset_description, subcategory_id, height, 
		width, depth, weight, serial_number, min_cost, max_cost, avg_cost, last_cost, default_resp, 
		CONCAT(SUBSTRING(cut_sheet, 1, CHARINDEX('.', cut_sheet)-1), 'C', SUBSTRING(cut_sheet, CHARINDEX('.', cut_sheet), 100)), date_added, @added_by, comment, 
		cad_block, water, plumbing, data, electrical, mobile, blocking, medgas, supports, discontinued, last_budget_update, photo, 
		eq_measurement_id, water_option, plumbing_option, data_option, electrical_option, mobile_option, blocking_option, medgas_option, 
		supports_option, revit, placement, clearance_left, clearance_right, clearance_front, clearance_back, clearance_top, clearance_bottom, 
		volts, phases, hertz, amps, volt_amps, watts, cfm, btus, misc_ase, misc_ada, misc_seismic, misc_antimicrobial, misc_ecolabel, 
		misc_ecolabel_desc, mapping_code, medgas_oxygen, medgas_nitrogen, medgas_air, medgas_n2o, medgas_vacuum, medgas_wag, medgas_co2, 
		medgas_other, medgas_steam, medgas_natgas, plu_hot_water, plu_drain, plu_cold_water, plu_return, plu_treated_water, plu_relief, 
		plu_chilled_water, serial_name, useful_life, loaded_weight, ship_weight, alternate_asset, updated_at, subcategory_domain_id,
		category_attribute, asset_suffix, jsn_id, jsn_domain_id, class, gas_liquid_co2, gas_liquid_nitrogen, gas_instrument_air,
		gas_liquid_propane_gas, gas_methane, gas_butane, gas_propane, gas_hydrogen, gas_acetylene, medgas_high_pressure, misc_shielding_lead_line,
		misc_shielding_magnetic, jsn_suffix, jsn_utility1_ow, jsn_utility1, jsn_utility2_ow, jsn_utility2, jsn_utility3_ow, jsn_utility3,
		jsn_utility4_ow, jsn_utility4, jsn_utility5_ow, jsn_utility5, jsn_utility6_ow, jsn_utility6, jsn_utility7_ow, jsn_utility7, mounting_height, asset_code, lan, network_type, plug_type, connection_type, network_option,
		ports, bluetooth, cat6, displayport, dvi, hdmi, wireless
	FROM assets where asset_id = @old_asset_id and domain_id = 1;

	SELECT @new_asset_id = MAX(asset_id) FROM assets where domain_id = @new_domain_id;
	
	/* VENDORS */
	INSERT INTO assets_vendor(asset_id, asset_domain_id, vendor_id, vendor_domain_id, min_cost, max_cost, avg_cost, date_added, added_by, 
		comment, model_number)
		SELECT @new_asset_id, @new_domain_id, vendor_id, vendor_domain_id, min_cost, max_cost, avg_cost, date_added, @added_by, comment, 
			model_number FROM assets_vendor WHERE asset_id = @old_asset_id and asset_domain_id = 1;

	DECLARE options_cursor CURSOR LOCAL FOR SELECT asset_option_id, code, description, data_type, unit_budget 
		FROM assets_options WHERE asset_id = @old_asset_id and domain_id = 1;
	OPEN options_cursor
	FETCH NEXT FROM options_cursor INTO @id, @code, @description, @data_type, @unit_price;
	WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO assets_options(asset_id, code, description, added_by, date_added, domain_id, data_type, unit_budget) 
				VALUES(@new_asset_id, @code, @description, @added_by, GETDATE(), @new_domain_id, @data_type, @unit_price);

			IF @change_inventories = 1
				BEGIN
					SELECT @new_options_id = MAX(asset_option_id) FROM assets_options WHERE domain_id = @new_domain_id AND asset_id = @new_asset_id;

					DECLARE inventory_cursor CURSOR LOCAL FOR SELECT a.inventory_id FROM project_room_inventory a, inventory_options b 
						WHERE asset_id = @old_asset_id and a.domain_id = 1 AND a.inventory_id = b.inventory_id AND b.option_id = @id;
					OPEN inventory_cursor
					FETCH NEXT FROM inventory_cursor INTO @old_inventory_id;
					WHILE @@FETCH_STATUS = 0
						BEGIN
							INSERT INTO project_room_inventory (project_id, department_id, room_id, asset_id, status, resp, budget_qty,
								dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, 
								date_added, added_by, comment, lease_qty, cost_center_id, tag, cad_id, asset_domain_id, temporary_location, received_date, delivered_date) 
							SELECT project_id, department_id, room_id, @new_asset_id, status, resp, budget_qty, 
								dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, 
								date_added, @added_by, comment, lease_qty, cost_center_id, tag, cad_id, @new_domain_id, temporary_location, received_date, delivered_date
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
		FROM project_room_inventory  WHERE  asset_id = @old_asset_id and domain_id = 1 AND (option_ids is null OR option_ids ='') AND
			project_id = ANY (SELECT project_id FROM project WHERE status IN ('A', 'P') AND domain_id = @new_domain_id);

		--UPDATE assets_it_connectivity SET asset_id = @new_asset_id, domain_id = @new_domain_id FROM assets_it_connectivity AS a, 
		--	room_it_connectivity_boxes AS b WHERE b.box_id = a.box_id AND a.asset_id = @old_asset_id AND a.domain_id = 1 AND 
		--	b.project_id = ANY(SELECT project_id FROM project WHERE status IN ('A', 'P') AND domain_id = @new_domain_id);

		--UPDATE assets_it_connectivity SET connected_asset_id = @new_asset_id, connected_domain_id = @new_domain_id 
		--	FROM room_it_connectivity_boxes AS b, assets_it_connectivity AS a WHERE b.box_id = a.box_id AND 
		--	a.connected_asset_id = @old_asset_id AND a.domain_id = 1 AND b.project_id = ANY(SELECT project_id FROM project WHERE status IN ('A', 'P') AND domain_id = @new_domain_id);

		UPDATE inventory_purchase_order SET asset_id = @new_asset_id, asset_domain_id = @new_domain_id 
			WHERE asset_id = @old_asset_id AND asset_domain_id = 1 AND project_id = ANY(SELECT project_id FROM project WHERE status IN ('A', 'P') AND domain_id = @new_domain_id);
	
		DELETE FROM project_room_inventory WHERE asset_id = @old_asset_id AND domain_id = 1  AND project_id = ANY(SELECT project_id FROM project WHERE status IN ('A', 'P') AND domain_id = @new_domain_id);
	END

	INSERT INTO related_assets values(@old_asset_id, 1, @new_asset_id, @new_domain_id);

	SELECT * FROM assets WHERE asset_id = @new_asset_id AND domain_id = @new_domain_id;

END
