CREATE PROCEDURE [dbo].[copy_inventory](
	@domain_id SMALLINT,
    @project_id INTEGER,
    @phase_id INTEGER,
    @department_id INTEGER,
	@room_id INTEGER,
	@copy_domain_id SMALLINT,
    @copy_project_id INTEGER,
    @copy_phase_id INTEGER,
    @copy_department_id INTEGER,
	@copy_room_id INTEGER,
    @added_by varchar(50),
    @copy_opt_col BIT,
	@linked_id_template int = null,
	@from_project bit = 0,
	@is_move bit = 0,
	@return_var varchar(500) OUT)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @current_cc_id INTEGER, @equivalent_cc_id INTEGER, @new_inventory_id INT, @avg_cost NUMERIC(10, 2), @default_resp varchar(10), @sql nvarchar(500),
	@markup NUMERIC(10,2), @escalation NUMERIC(10,2), @tax NUMERIC(10,2), @freight_markup NUMERIC(10,2), @install_markup NUMERIC(10,2), @default_cost_field varchar(25);

	SET @return_var = '';
	SELECT @current_cc_id = id FROM cost_center WHERE domain_id = @domain_id AND project_id = @project_id and is_default = 1;


	DECLARE @inventory_id INT, @asset_id int, @asset_domain_id INT, @status varchar(20), @resp varchar(10),@budget_qty int, @dnp_qty int, @unit_budget numeric(10,2),
	@buyout_delta numeric(10,2),@estimated_delivery_date date, @current_location varchar(50), @inventory_type VARCHAR(25), @comment varchar(1000),@lease_qty int, 
	@cost_center_id int, @tag varchar(5000),@cad_id varchar(25),@none_option bit, @locked_unit_budget numeric(10,2),@locked_budget_qty int, @locked_dnp_qty int, 
	@detailed_budget bit, @locked_room_quantity int, @lead_time int, 
	@clin varchar(50), @inventory_source_id INT, @unit_markup decimal(10,2),@unit_escalation decimal(10,2),@unit_tax decimal(10,2),
	@unit_install_net decimal(10,2),@unit_install_markup decimal(10,2),@unit_freight_net decimal(10,2),@unit_freight_markup decimal(10,2), 
	@options_unit_price numeric(10,2), @asset_profile varchar(3000), @asset_profile_budget varchar(2000),
	@asset_description_ow BIT, @asset_description VARCHAR(300), @ECN  VARCHAR (50), @placement VARCHAR (20), @placement_ow bit,
	@biomed_check_required BIT, @temporary_location VARCHAR(200),
	@height_ow BIT, @height VARCHAR(25), @width_ow BIT, @width VARCHAR(25), @depth_ow BIT, @depth VARCHAR(25),
	@mounting_height_ow BIT, @mounting_height VARCHAR(25), @class_ow BIT, @class INT,
	@jsn_code VARCHAR(10), 
	@jsn_utility1 VARCHAR(10), @jsn_utility2 VARCHAR(10), 
	@jsn_utility3 VARCHAR(10), @jsn_utility4 VARCHAR(10), 
	@jsn_utility5 VARCHAR(10), @jsn_utility6 VARCHAR(10), 
	@jsn_utility7 VARCHAR(10), @copy_link uniqueidentifier, @jsn_ow BIT,
	@manufacturer_description VARCHAR (100), @manufacturer_description_ow BIT,
	@model_number VARCHAR (100), @model_number_ow BIT, @model_name  VARCHAR (150),
	@model_name_ow BIT, @plug_type VARCHAR (20), @plug_type_ow BIT, @connection_type VARCHAR (20), @connection_type_ow BIT,
	@lan INT, @lan_ow BIT, @network_type VARCHAR(20), @network_type_ow BIT, @network_option INT, @network_option_ow BIT,
	@ports NUMERIC(10), @ports_ow BIT, @bluetooth BIT, @bluetooth_ow BIT, @cat6 BIT, @cat6_ow BIT, @displayport BIT, @displayport_ow BIT,
	@dvi BIT, @dvi_ow BIT, @hdmi BIT, @hdmi_ow BIT, @wireless BIT, @wireless_ow BIT, @volts VARCHAR(10), @volts_ow BIT, @amps VARCHAR(20), @amps_ow BIT;
	-- copy all equipment inventories and options
	DECLARE inventories_cursor CURSOR FOR 
		SELECT inventory_id, asset_id, asset_domain_id, status, resp, budget_qty, dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, 
		current_location, inventory_type, comment, lease_qty, cost_center_id, tag, cad_id, none_option, locked_unit_budget, locked_budget_qty, locked_dnp_qty, 
		detailed_budget, locked_room_quantity, lead_time, clin, inventory_source_id, unit_markup, unit_escalation, unit_tax, 
		unit_install_net, unit_install_markup, unit_freight_net, unit_freight_markup,
		options_unit_price, asset_profile, asset_profile_budget,
		asset_description_ow, asset_description, ECN, placement, placement_ow, biomed_check_required, temporary_location,
		height_ow, height, width_ow, width, depth_ow, depth, mounting_height_ow,
		mounting_height, class_ow, class, jsn_code,
		jsn_utility1, jsn_utility2, jsn_utility3,
		jsn_utility4, jsn_utility5,
		jsn_utility6, jsn_utility7, copy_link, jsn_ow,
		manufacturer_description, manufacturer_description_ow, model_number, model_number_ow, model_name, model_name_ow,
		plug_type, plug_type_ow, connection_type, connection_type_ow, lan, lan_ow, network_type, network_type_ow, network_option, network_option_ow,
		ports, ports_ow, bluetooth, bluetooth_ow, cat6, cat6_ow, displayport, displayport_ow, dvi, dvi_ow, hdmi, hdmi_ow, wireless, wireless_ow,
		volts, volts_ow, amps, amps_ow
		FROM project_room_inventory
		WHERE domain_id = @copy_domain_id AND project_id = @copy_project_id 
			AND phase_id = @copy_phase_id AND department_id = @copy_department_id AND room_id = @copy_room_id

	OPEN inventories_cursor
	FETCH NEXT FROM inventories_cursor INTO @inventory_id, @asset_id, @asset_domain_id, @status, @resp, @budget_qty, @dnp_qty, @unit_budget, @buyout_delta, @estimated_delivery_date, 
		@current_location, @inventory_type, @comment, @lease_qty, @cost_center_id, @tag, @cad_id, @none_option, @locked_unit_budget, @locked_budget_qty, @locked_dnp_qty, 
		@detailed_budget, @locked_room_quantity, @lead_time, @clin, @inventory_source_id, @unit_markup, @unit_escalation, @unit_tax, 
		@unit_install_net, @unit_install_markup, @unit_freight_net, @unit_freight_markup, 
		@options_unit_price, @asset_profile, @asset_profile_budget,
		@asset_description_ow, @asset_description, @ECN, @placement, @placement_ow, @biomed_check_required, @temporary_location,
		@height_ow, @height, @width_ow, @width, @depth_ow, @depth, @mounting_height_ow,
		@mounting_height, @class_ow, @class, @jsn_code,
		@jsn_utility1, @jsn_utility2, @jsn_utility3,
		@jsn_utility4, @jsn_utility5,
		@jsn_utility6, @jsn_utility7, @copy_link, @jsn_ow,
		@manufacturer_description, @manufacturer_description_ow, @model_number, @model_number_ow, @model_name, @model_name_ow,
		@plug_type, @plug_type_ow, @connection_type, @connection_type_ow, @lan, @lan_ow, @network_type, @network_type_ow, @network_option, @network_option_ow,
		@ports, @ports_ow, @bluetooth, @bluetooth_ow, @cat6, @cat6_ow, @displayport, @displayport_ow, @dvi, @dvi_ow, @hdmi, @hdmi_ow, @wireless, @wireless_ow,
		@volts, @volts_ow, @amps, @amps_ow
	WHILE @@FETCH_STATUS = 0
		BEGIN
			
			SET @equivalent_cc_id = NULL;

			IF @cost_center_id IS NOT NULL BEGIN
				SELECT @equivalent_cc_id = id FROM cost_center WHERE domain_id = @domain_id AND project_id = @project_id and code = (SELECT code FROM cost_center where id = @cost_center_id AND project_id =  @copy_project_id);
			END

			IF @equivalent_cc_id is null BEGIN
				SET @equivalent_cc_id = @current_cc_id;
			END

			IF @project_id != @copy_project_id BEGIN
				IF @copy_link = CAST(CAST(0 AS BINARY) AS UNIQUEIDENTIFIER) BEGIN
					SET @copy_link = NEWID();
					update project_room_inventory set copy_link = @copy_link where domain_id = @copy_domain_id and project_id = @copy_project_id  and inventory_id = @inventory_id;
				END;
			END

			INSERT INTO project_room_inventory(domain_id, project_id, phase_id, department_id, room_id,  date_added, added_by, linked_id_template,
				asset_id, asset_domain_id, status, resp, budget_qty, dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, 
				current_location, inventory_type, comment, lease_qty, cost_center_id, tag, cad_id, none_option,  
				detailed_budget, lead_time, clin, inventory_source_id, unit_markup, unit_escalation, unit_tax, 
				unit_install_net, unit_install_markup, unit_freight_net, unit_freight_markup,
				asset_description_ow, asset_description, ECN, placement, placement_ow, biomed_check_required, temporary_location,
				height_ow, height, width_ow, width, depth_ow, depth, mounting_height_ow,
				mounting_height, class_ow, class, jsn_code,
				jsn_utility1, jsn_utility2, jsn_utility3,
				jsn_utility4, jsn_utility5,
				jsn_utility6, jsn_utility7, jsn_ow,
				manufacturer_description, manufacturer_description_ow, model_number, model_number_ow, model_name, model_name_ow,
				copy_link, plug_type, plug_type_ow, connection_type, connection_type_ow, lan, lan_ow, network_type, network_type_ow, network_option, network_option_ow, 
				ports, ports_ow, bluetooth, bluetooth_ow, cat6, cat6_ow, displayport, displayport_ow, dvi, dvi_ow, hdmi, hdmi_ow, wireless, wireless_ow,
				volts, volts_ow, amps, amps_ow
				)
			VALUES(@domain_id, @project_id, @phase_id, @department_id, @room_id, GETDATE(), @added_by, @linked_id_template,
				@asset_id, @asset_domain_id, @status, @resp, @budget_qty, @dnp_qty, @unit_budget, @buyout_delta, @estimated_delivery_date, 
				@current_location, @inventory_type, @comment, @lease_qty, @equivalent_cc_id, @tag, @cad_id, @none_option,
				@detailed_budget, @lead_time, @clin, @inventory_source_id, @unit_markup, @unit_escalation, @unit_tax, 
				@unit_install_net, @unit_install_markup, @unit_freight_net, @unit_freight_markup,
				@asset_description_ow, @asset_description, @ECN, @placement, @placement_ow, @biomed_check_required, @temporary_location,
				@height_ow, @height, @width_ow, @width, @depth_ow, @depth, @mounting_height_ow,
				@mounting_height, @class_ow, @class, @jsn_code,
				@jsn_utility1, @jsn_utility2, @jsn_utility3,
				@jsn_utility4, @jsn_utility5,
				@jsn_utility6, @jsn_utility7, @jsn_ow,
				@manufacturer_description, @manufacturer_description_ow, @model_number, @model_number_ow, @model_name, @model_name_ow,
				@copy_link, @plug_type, @plug_type_ow, @connection_type, @connection_type_ow, @lan, @lan_ow, @network_type, @network_type_ow, @network_option, @network_option_ow,
				@ports, @ports_ow, @bluetooth, @bluetooth_ow, @cat6, @cat6_ow, @displayport, @displayport_ow, @dvi, @dvi_ow, @hdmi, @hdmi_ow, @wireless, @wireless_ow,
				@volts, @volts_ow, @amps, @amps_ow
				)
			
			SELECT @new_inventory_id = MAX(inventory_id) FROM project_room_inventory WHERE domain_id = @domain_id AND project_id = @project_id
				AND phase_id = @phase_id AND department_id = @department_id AND room_id = @room_id;
			
			EXEC copy_inventory_document @domain_id, @project_id, @phase_id, @department_id, @room_id, @copy_phase_id, @copy_department_id,
				@copy_room_id, @inventory_id, @new_inventory_id, @added_by;	

			--VERIFY IF THERE IS ANY INVENTORY LINKED AND CHANGE THE LINK
			IF @is_move = 1 BEGIN
				UPDATE project_room_inventory SET inventory_target_id = @new_inventory_id
				WHERE inventory_target_id = @inventory_id

				--IF EXISTS ANY RELATED PO'S CHANGE THE REFERENCE
				UPDATE inventory_purchase_order SET inventory_id = @new_inventory_id WHERE project_id = @copy_project_id and inventory_id = @inventory_id and po_domain_id = @domain_id
				UPDATE purchase_order set phase_id = @phase_id, department_id = @department_id, room_id = @room_id
					WHERE project_id = @copy_project_id AND phase_id = @copy_phase_id AND department_id = @copy_department_id AND room_id = @copy_room_id AND domain_id = @domain_id
			END

			IF @copy_opt_col = 1 BEGIN
				EXEC copy_options @inventory_id, @new_inventory_id, @domain_id, @options_unit_price, @asset_profile, @asset_profile_budget;
			END

			IF @from_project = 1 BEGIN
				SELECT @markup = markup, @escalation = escalation, @tax = tax, @freight_markup = freight_markup, @install_markup = install_markup, @default_cost_field = coalesce(default_cost_field, 'avg_cost')
				FROM project WHERE project_id = @project_id AND domain_id = @domain_id;

				SELECT @default_resp = default_resp FROM assets where asset_id = @asset_id and domain_id = @asset_domain_id;

				SET @sql = CONCAT('SELECT @default_resp = default_resp, @avg_cost = ', + @default_cost_field + '  FROM assets 
						WHERE asset_id = ' , @asset_id , ' and domain_id = ' , @asset_domain_id);

				EXEC sp_executesql @sql, N'@avg_cost numeric(10,2) output, @default_resp varchar(10) output', @avg_cost = @avg_cost output, @default_resp = @default_resp output;

				UPDATE project_room_inventory set dnp_qty = 0, current_location = 'Plan', lease_qty = 0, resp = @default_resp, estimated_delivery_date = null,
				
				unit_markup = @markup, unit_escalation = @escalation, unit_tax = @tax, unit_freight_markup = @freight_markup, unit_install_markup = @install_markup,
				unit_budget = @avg_cost, comment = null
				WHERE inventory_id = @new_inventory_id and domain_id = @domain_id;
			END

			FETCH NEXT FROM inventories_cursor INTO @inventory_id, @asset_id, @asset_domain_id, @status, @resp, @budget_qty, @dnp_qty, @unit_budget, @buyout_delta, @estimated_delivery_date, 
		@current_location, @inventory_type, @comment, @lease_qty, @cost_center_id, @tag, @cad_id, @none_option, @locked_unit_budget, @locked_budget_qty, @locked_dnp_qty, 
		@detailed_budget, @locked_room_quantity, @lead_time, @clin, @inventory_source_id, @unit_markup, @unit_escalation, @unit_tax, 
		@unit_install_net, @unit_install_markup, @unit_freight_net, @unit_freight_markup, 
		@options_unit_price, @asset_profile, @asset_profile_budget,
		@asset_description_ow, @asset_description, @ECN, @placement, @placement_ow, @biomed_check_required, @temporary_location,
		@height_ow, @height, @width_ow, @width, @depth_ow, @depth, @mounting_height_ow,
		@mounting_height, @class_ow, @class, @jsn_code,
		@jsn_utility1, @jsn_utility2, @jsn_utility3,
		@jsn_utility4, @jsn_utility5,
		@jsn_utility6, @jsn_utility7, @copy_link, @jsn_ow,
		@manufacturer_description, @manufacturer_description_ow, @model_number, @model_number_ow, @model_name, @model_name_ow,
		@plug_type, @plug_type_ow, @connection_type, @connection_type_ow, @lan, @lan_ow, @network_type, @network_type_ow, @network_option, @network_option_ow,
		@ports, @ports_ow, @bluetooth, @bluetooth_ow, @cat6, @cat6_ow, @displayport, @displayport_ow, @dvi, @dvi_ow, @hdmi, @hdmi_ow, @wireless, @wireless_ow,
		@volts, @volts_ow, @amps, @amps_ow;
		END
	CLOSE inventories_cursor;
	DEALLOCATE inventories_cursor;

END