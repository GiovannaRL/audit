/*
PROCEDURE CREATED TO SPLIT INVENTORY FROM PROJECT_ROOM_INVENTORY THAT HAS MORE THEN ONE INVENTORY_PURCHASE_ORDER RELATED
*/

CREATE PROCEDURE [dbo].[split_inventory_purchase_order]
	
AS
	DECLARE @budget_qty INT, @inventory_id INT, @domain_id INT, @used_po_qty INT, @first_time bit, @po_id INT, @po_qty INT, @new_inventory_id INT, @project_id INT, @added_by varchar(50) = 'juliana.barros@audaxware.com';

DECLARE inventories_cursor CURSOR FOR 

	SELECT pri.budget_qty, ipo.inventory_id, ipo.po_domain_id, ipo.project_id
	FROM inventory_purchase_order ipo
	INNER JOIN project_room_inventory pri ON pri.inventory_id = ipo.inventory_id AND pri.domain_id = ipo.po_domain_id 
	INNER JOIN project p ON p.project_id = ipo.project_id AND p.domain_id = ipo.po_domain_id
	WHERE po_qty > 0 
	GROUP BY  ipo.inventory_id, ipo.project_id, po_domain_id, pri.budget_qty
	HAVING count(*) > 1 

OPEN inventories_cursor
FETCH NEXT FROM inventories_cursor INTO @budget_qty, @inventory_id, @domain_id, @project_id
WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @first_time = 1;
		SET @used_po_qty = 0;
		--print 'primeiro looping inventory_id: ' + cast(@inventory_id as varchar(15))
		DECLARE po_cursor CURSOR FOR 
			SELECT po_id, po_qty from inventory_purchase_order WHERE inventory_id = @inventory_id AND po_domain_id = @domain_id AND po_qty > 0;
			
		OPEN po_cursor
		FETCH NEXT FROM po_cursor INTO @po_id, @po_qty
		WHILE @@FETCH_STATUS = 0
			BEGIN
				SET @used_po_qty = @used_po_qty + @po_qty;

				IF @first_time = 1 BEGIN
					SET @first_time = 0;
					--print 'entrou no if'
					--print '@used_po_qty' + cast(@used_po_qty as varchar(10))
					UPDATE project_room_inventory SET budget_qty = @po_qty, dnp_qty = 0 WHERE inventory_id = @inventory_id and domain_id = @domain_id
				END
				ELSE BEGIN
					--print 'entrou no else'
					--print '@used_po_qty' + cast(@used_po_qty as varchar(10))
					--INSERT INTO project_room_inventory(project_id, department_id, room_id, asset_id, status, resp, budget_qty, dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, date_added, added_by, comment, lease_qty, cost_center_id, tag, cad_id, none_option, domain_id, phase_id, asset_domain_id, option_ids, locked_unit_budget, locked_budget_qty, locked_dnp_qty, options_unit_price, linked_id_template, asset_profile, asset_profile_budget, detailed_budget, locked_room_quantity, linked_document, lead_time, clin, inventory_source_id, inventory_target_id, unit_markup, /*unit_escalation,*/ unit_tax, unit_install_net, unit_install_markup, unit_freight_net, unit_freight_markup, /*unit_markup_calc, unit_escalation_calc,*/ unit_budget_adjusted, /*unit_tax_calc,*/ unit_install, unit_freight, unit_budget_total, /*total_install_net, total_budget_adjusted, total_tax, total_install, total_freight_net, total_freight,*/ total_budget, net_new, asset_description_ow, asset_description, ECN, placement_ow, placement, biomed_check_required, temporary_location, height_ow, height, width_ow, width, depth_ow, depth, mounting_height_ow, mounting_height, class_ow, class, jsn_code, jsn_utility1, jsn_utility2, jsn_utility3, jsn_utility4, jsn_utility5, jsn_utility6, jsn_utility7, update_trigger, copy_link, jsn_ow, manufacturer_description, manufacturer_description_ow, serial_number, serial_number_ow, serial_name, serial_name_ow, cut_sheet_filename, final_disposition, delivered_date, received_date, connection_type, connection_type_ow, plug_type, plug_type_ow, lan, lan_ow, network_type, network_type_ow, network_option, network_option_ow, ports, ports_ow, bluetooth, bluetooth_ow, cat6, cat6_ow, displayport, displayport_ow, dvi, dvi_ow, hdmi, hdmi_ow, wireless, wireless_ow, volts, volts_ow, amps, amps_ow, date_modified) 
					--SELECT project_id, department_id, room_id, asset_id, status, resp, @po_qty, dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, GETDATE(), @added_by, comment, lease_qty, cost_center_id, tag, cad_id, none_option, domain_id, phase_id, asset_domain_id, option_ids, locked_unit_budget, locked_budget_qty, locked_dnp_qty, options_unit_price, linked_id_template, asset_profile, asset_profile_budget, detailed_budget, locked_room_quantity, linked_document, lead_time, clin, inventory_source_id, inventory_target_id, unit_markup, /*unit_escalation,*/ unit_tax, unit_install_net, unit_install_markup, unit_freight_net, unit_freight_markup, /*unit_markup_calc, unit_escalation_calc,*/ unit_budget_adjusted, /*unit_tax_calc,*/ unit_install, unit_freight, unit_budget_total, /*total_install_net, total_budget_adjusted, total_tax, total_install, total_freight_net, total_freight,*/ total_budget, net_new, asset_description_ow, asset_description, ECN, placement_ow, placement, biomed_check_required, temporary_location, height_ow, height, width_ow, width, depth_ow, depth, mounting_height_ow, mounting_height, class_ow, class, jsn_code, jsn_utility1, jsn_utility2, jsn_utility3, jsn_utility4, jsn_utility5, jsn_utility6, jsn_utility7, update_trigger, copy_link, jsn_ow, manufacturer_description, manufacturer_description_ow, serial_number, serial_number_ow, serial_name, serial_name_ow, cut_sheet_filename, final_disposition, delivered_date, received_date, connection_type, connection_type_ow, plug_type, plug_type_ow, lan, lan_ow, network_type, network_type_ow, network_option, network_option_ow, ports, ports_ow, bluetooth, bluetooth_ow, cat6, cat6_ow, displayport, displayport_ow, dvi, dvi_ow, hdmi, hdmi_ow, wireless, wireless_ow, volts, volts_ow, amps, amps_ow, date_modified
					INSERT INTO project_room_inventory(project_id, department_id, room_id, asset_id, status, resp, budget_qty, dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, date_added, added_by, comment, lease_qty, cost_center_id, tag, cad_id, none_option, domain_id, phase_id, asset_domain_id, option_ids, locked_unit_budget, locked_budget_qty, locked_dnp_qty, options_unit_price, linked_id_template, asset_profile, asset_profile_budget, detailed_budget, locked_room_quantity, linked_document, lead_time, clin, inventory_source_id, inventory_target_id, unit_markup, unit_escalation, unit_install_net, unit_install_markup, unit_freight_net, unit_freight_markup, asset_description_ow, asset_description, ECN, placement_ow, placement, biomed_check_required, temporary_location, height_ow, height, width_ow, width, depth_ow, depth, mounting_height_ow, mounting_height, class_ow, class, jsn_code, jsn_utility1, jsn_utility2, jsn_utility3, jsn_utility4, jsn_utility5, jsn_utility6, jsn_utility7, update_trigger, copy_link, jsn_ow, manufacturer_description, manufacturer_description_ow, serial_number, serial_number_ow, serial_name, serial_name_ow, cut_sheet_filename, final_disposition, delivered_date, received_date, connection_type, connection_type_ow, plug_type, plug_type_ow, lan, lan_ow, network_type, network_type_ow, network_option, network_option_ow, ports, ports_ow, bluetooth, bluetooth_ow, cat6, cat6_ow, displayport, displayport_ow, dvi, dvi_ow, hdmi, hdmi_ow, wireless, wireless_ow, volts, volts_ow, amps, amps_ow, date_modified) 
					SELECT project_id, department_id, room_id, asset_id, status, resp, @po_qty, dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, GETDATE(), @added_by, comment, lease_qty, cost_center_id, tag, cad_id, none_option, domain_id, phase_id, asset_domain_id, option_ids, locked_unit_budget, locked_budget_qty, locked_dnp_qty, options_unit_price, linked_id_template, asset_profile, asset_profile_budget, detailed_budget, locked_room_quantity, linked_document, lead_time, clin, inventory_source_id, inventory_target_id, unit_markup,unit_escalation, unit_install_net, unit_install_markup, unit_freight_net, unit_freight_markup, asset_description_ow, asset_description, ECN, placement_ow, placement, biomed_check_required, temporary_location, height_ow, height, width_ow, width, depth_ow, depth, mounting_height_ow, mounting_height, class_ow, class, jsn_code, jsn_utility1, jsn_utility2, jsn_utility3, jsn_utility4, jsn_utility5, jsn_utility6, jsn_utility7, update_trigger, copy_link, jsn_ow, manufacturer_description, manufacturer_description_ow, serial_number, serial_number_ow, serial_name, serial_name_ow, cut_sheet_filename, final_disposition, delivered_date, received_date, connection_type, connection_type_ow, plug_type, plug_type_ow, lan, lan_ow, network_type, network_type_ow, network_option, network_option_ow, ports, ports_ow, bluetooth, bluetooth_ow, cat6, cat6_ow, displayport, displayport_ow, dvi, dvi_ow, hdmi, hdmi_ow, wireless, wireless_ow, volts, volts_ow, amps, amps_ow, date_modified	
					FROM project_room_inventory WHERE inventory_id = @inventory_id
					
					SELECT @new_inventory_id = SCOPE_IDENTITY();
					
					UPDATE inventory_purchase_order SET inventory_id = @new_inventory_id WHERE po_id = @po_id AND inventory_id = @inventory_id AND po_domain_id = @domain_id;

					EXEC copy_inventory_document @domain_id, @project_id, null, null, null, null, null,
					null, @inventory_id, @new_inventory_id, @added_by;	
										
				END

				FETCH NEXT FROM po_cursor INTO @po_id, @po_qty;
			END
		CLOSE po_cursor;
		DEALLOCATE po_cursor;

		IF @used_po_qty < @budget_qty BEGIN
			--print 'insert na project_room_inventory o budget restante: ' + cast(@budget_qty-@used_po_qty as varchar(10))
			INSERT INTO project_room_inventory(project_id, department_id, room_id, asset_id, status, resp, budget_qty, dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, date_added, added_by, comment, lease_qty, cost_center_id, tag, cad_id, none_option, domain_id, phase_id, asset_domain_id, option_ids, locked_unit_budget, locked_budget_qty, locked_dnp_qty, options_unit_price, linked_id_template, asset_profile, asset_profile_budget, detailed_budget, locked_room_quantity, linked_document, lead_time, clin, inventory_source_id, inventory_target_id, unit_markup, unit_escalation, unit_install_net, unit_install_markup, unit_freight_net, unit_freight_markup, asset_description_ow, asset_description, ECN, placement_ow, placement, biomed_check_required, temporary_location, height_ow, height, width_ow, width, depth_ow, depth, mounting_height_ow, mounting_height, class_ow, class, jsn_code, jsn_utility1, jsn_utility2, jsn_utility3, jsn_utility4, jsn_utility5, jsn_utility6, jsn_utility7, update_trigger, copy_link, jsn_ow, manufacturer_description, manufacturer_description_ow, serial_number, serial_number_ow, serial_name, serial_name_ow, cut_sheet_filename, final_disposition, delivered_date, received_date, connection_type, connection_type_ow, plug_type, plug_type_ow, lan, lan_ow, network_type, network_type_ow, network_option, network_option_ow, ports, ports_ow, bluetooth, bluetooth_ow, cat6, cat6_ow, displayport, displayport_ow, dvi, dvi_ow, hdmi, hdmi_ow, wireless, wireless_ow, volts, volts_ow, amps, amps_ow, date_modified) 
			SELECT project_id, department_id, room_id, asset_id, status, resp, @budget_qty-@used_po_qty, 0, unit_budget, buyout_delta, estimated_delivery_date, current_location, inventory_type, GETDATE(), @added_by, comment, lease_qty, cost_center_id, tag, cad_id, none_option, domain_id, phase_id, asset_domain_id, option_ids, locked_unit_budget, locked_budget_qty, locked_dnp_qty, options_unit_price, linked_id_template, asset_profile, asset_profile_budget, detailed_budget, locked_room_quantity, linked_document, lead_time, clin, inventory_source_id, inventory_target_id, unit_markup, unit_escalation, unit_install_net, unit_install_markup, unit_freight_net, unit_freight_markup, asset_description_ow, asset_description, ECN, placement_ow, placement, biomed_check_required, temporary_location, height_ow, height, width_ow, width, depth_ow, depth, mounting_height_ow, mounting_height, class_ow, class, jsn_code, jsn_utility1, jsn_utility2, jsn_utility3, jsn_utility4, jsn_utility5, jsn_utility6, jsn_utility7, update_trigger, copy_link, jsn_ow, manufacturer_description, manufacturer_description_ow, serial_number, serial_number_ow, serial_name, serial_name_ow, cut_sheet_filename, final_disposition, delivered_date, received_date, connection_type, connection_type_ow, plug_type, plug_type_ow, lan, lan_ow, network_type, network_type_ow, network_option, network_option_ow, ports, ports_ow, bluetooth, bluetooth_ow, cat6, cat6_ow, displayport, displayport_ow, dvi, dvi_ow, hdmi, hdmi_ow, wireless, wireless_ow, volts, volts_ow, amps, amps_ow, date_modified	
			FROM project_room_inventory WHERE inventory_id = @inventory_id
			
			SELECT @new_inventory_id = SCOPE_IDENTITY();

			EXEC copy_inventory_document @domain_id, @project_id, null, null, null, null, null,
			null, @inventory_id, @new_inventory_id, @added_by;
		END


		FETCH NEXT FROM inventories_cursor INTO  @budget_qty, @inventory_id, @domain_id, @project_id;
	END
CLOSE inventories_cursor;
DEALLOCATE inventories_cursor;
