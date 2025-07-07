CREATE PROCEDURE [dbo].[copy_inventory_item]
	@from_domain_id smallint,
	@from_id int, 
	@update_to_id int, 
	@insert_to_project_id int,
	@insert_to_phase_id int,
	@insert_to_department_id int, 
	@insert_to_room_id int,
	@insert_to_domain_id smallint,
	@budget varchar(30),
	@added_by varchar(50),
	@mode int, --1 = template, 2=new_inventory, 3 = clone, 4 = link, 5 = syncronize
	@copy_options bit = 1
AS BEGIN

	DECLARE @sql nvarchar(100), @from_project_id int, @from_cost_center_id int, @current_cost_center_id int, @linked_id_template int;
	DECLARE @inventory_id INT, @asset_id int, @asset_domain_id INT, @status varchar(20), @resp varchar(10),@budget_qty int, @dnp_qty int, 
	@unit_budget numeric(10,2), @buyout_delta numeric(10,2),@estimated_delivery_date date, @current_location varchar(50), @inventory_type VARCHAR(25), 
	@comment varchar(1000),@lease_qty int, @cost_center_id int, @tag varchar(5000),@cad_id varchar(25),@none_option bit, @locked_unit_budget numeric(10,2),
	@locked_budget_qty int, @locked_dnp_qty int, @detailed_budget bit, @locked_room_quantity int, @lead_time int, @clin varchar(50), 
	@inventory_source_id INT, @unit_markup decimal(10,2),@unit_escalation decimal(10,2),@unit_tax decimal(10,2), @unit_install_net decimal(10,2),
	@unit_install_markup decimal(10,2),@unit_freight_net decimal(10,2),@unit_freight_markup decimal(10,2), @options_unit_price numeric(10,2), 
	@asset_profile varchar(3000), @asset_profile_budget varchar(2000), @asset_description_ow BIT, @asset_description VARCHAR(300), 
	@ECN  VARCHAR (50), @placement VARCHAR (20), @placement_ow bit, @biomed_check_required BIT, @temporary_location VARCHAR(200),
	@height_ow BIT, @height VARCHAR(25), @width_ow BIT, @width VARCHAR(25), @depth_ow BIT, @depth VARCHAR(25), @mounting_height_ow BIT, 
	@mounting_height VARCHAR(25), @class_ow BIT, @class INT, @jsn_code VARCHAR(10), @date_added date = GETDATE(), @linked_document int,
	@jsn_utility1 VARCHAR(10), @jsn_utility2 VARCHAR(10), @jsn_utility3 VARCHAR(10), @jsn_utility4 VARCHAR(10), 
	@jsn_utility5 VARCHAR(10), @jsn_utility6 VARCHAR(10), @jsn_utility7 VARCHAR(10), @copy_link uniqueidentifier, @jsn_ow BIT,
	@manufacturer_description VARCHAR (100), @manufacturer_description_ow BIT,
	@model_number VARCHAR (100), @model_number_ow BIT, @model_name  VARCHAR (150),
	@model_name_ow BIT, @cut_sheet_filename VARCHAR(100), @final_disposition VARCHAR(200), @delivered_date date, @received_date date,
	@connection_type VARCHAR(20), @connection_type_ow	BIT, @plug_type VARCHAR(20), @plug_type_ow BIT,
	@lan INT, @lan_ow BIT, @network_type VARCHAR(20), @network_type_ow BIT, @network_option INT, @network_option_ow BIT,
	@ports NUMERIC (10), @ports_ow BIT, @bluetooth BIT, @bluetooth_ow BIT, @cat6 BIT, @cat6_ow BIT, @displayport BIT, @displayport_ow BIT,
	@dvi BIT, @dvi_ow BIT, @hdmi BIT, @hdmi_ow BIT, @wireless BIT, @wireless_ow BIT, @volts VARCHAR(20), @volts_ow BIT, @amps VARCHAR(20), @amps_ow BIT,
	@template bit = 0, @new_inv bit = 0, @clone bit = 0, @link bit = 0, @sync bit = 0;

	IF @mode = 1 BEGIN
		SET @template = 1;
	END
	ELSE IF @mode = 2 BEGIN
		SET @new_inv = 1;
	END
	ELSE IF @mode = 3 BEGIN
		SET @clone = 1;
	END
	ELSE IF @mode = 4 BEGIN
		SET @link = 1;
	END
	ELSE IF @mode = 5 BEGIN
		SET @sync = 1;
	END

	
	--SELECT DATA FROM INVENTORY
	SELECT @from_cost_center_id = cost_center_id, @unit_budget = unit_budget, @asset_id = asset_id, @asset_domain_id = asset_domain_id, @from_project_id = project_id, 
	@budget_qty = budget_qty, @dnp_qty = dnp_qty, @buyout_delta = buyout_delta, @estimated_delivery_date = estimated_delivery_date, 
	@status = status, @resp = resp, @current_location = current_location, @inventory_type = inventory_type, @comment = comment, @lease_qty = lease_qty, 
	@tag = tag, @cad_id = cad_id, @none_option = none_option, @detailed_budget = detailed_budget, @lead_time = lead_time, @clin = clin, 
	@inventory_source_id = inventory_source_id, @unit_markup = unit_markup, @unit_escalation = unit_escalation, @unit_tax = unit_tax, 
	@unit_install_net = unit_install_net, @unit_install_markup = unit_install_markup, @unit_freight_net = unit_freight_net, 
	@unit_freight_markup = unit_freight_markup, @asset_description_ow = asset_description_ow, @asset_description = asset_description, 
	@options_unit_price = options_unit_price, @asset_profile = asset_profile, @asset_profile_budget = asset_profile_budget,
	@ECN = ECN, @placement = placement, @placement_ow = placement_ow, @biomed_check_required = biomed_check_required, @linked_document = linked_document,
	@temporary_location = temporary_location, @height_ow = height_ow, @height = height, @width_ow = width_ow, @width = width, @copy_link = copy_link,
	@depth_ow = depth_ow, @depth = depth, @mounting_height_ow = mounting_height_ow, @mounting_height = mounting_height, @class_ow = class_ow, 
	@class = class, @jsn_code = jsn_code, @jsn_utility1 = jsn_utility1, @jsn_utility2 = jsn_utility2, @jsn_utility3 = jsn_utility3, 
	@jsn_utility4 = jsn_utility4, @jsn_utility5 = jsn_utility5, @jsn_utility6 = jsn_utility6, @jsn_utility7 = jsn_utility7, @jsn_ow = jsn_ow,
	@manufacturer_description = manufacturer_description, @manufacturer_description_ow = manufacturer_description_ow, @model_number = model_number,
	@model_number_ow = model_number_ow, @model_name = model_name, @model_name_ow = model_name_ow, @cut_sheet_filename = cut_sheet_filename,
	@final_disposition = final_disposition, @delivered_date = delivered_date, @received_date = received_date,
	@connection_type = connection_type, @connection_type_ow = connection_type_ow, @plug_type = plug_type, @plug_type_ow = plug_type_ow,
	@lan = lan, @lan_ow = lan_ow, @network_type = network_type, @network_type_ow = network_type_ow, @network_option = network_option, @network_option_ow = network_option_ow,
	@ports = ports, @ports_ow = ports_ow, @bluetooth = bluetooth, @bluetooth_ow = bluetooth_ow, @cat6 = cat6, @cat6_ow = cat6_ow, @displayport = displayport, @displayport_ow = displayport_ow,
	@dvi = dvi, @dvi_ow = dvi_ow, @hdmi = hdmi, @hdmi_ow = hdmi_ow, @wireless = wireless, @wireless_ow = wireless_ow, @volts = volts, @volts_ow = volts_ow, @amps = amps, @amps_ow = amps_ow
	FROM project_room_inventory 
	WHERE inventory_id = @from_id and domain_id = @from_domain_id;

	

	IF @update_to_id is null BEGIN
	
		INSERT INTO project_room_inventory(domain_id, project_id, phase_id, department_id, room_id,  date_added, added_by,
				asset_id, asset_domain_id, status, resp, current_location)
		VALUES(@insert_to_domain_id, @insert_to_project_id, @insert_to_phase_id, @insert_to_department_id, @insert_to_room_id,  GETDATE(), @added_by,
				@asset_id, @asset_domain_id, @status, @resp, @current_location)

		SELECT @update_to_id = MAX(inventory_id) FROM project_room_inventory WHERE domain_id = @insert_to_domain_id AND project_id = @insert_to_project_id
				AND phase_id = @insert_to_phase_id AND department_id = @insert_to_department_id AND room_id = @insert_to_room_id;
	END
	
	
	--SET COST CENTER
	IF @from_project_id <> @insert_to_project_id BEGIN
		SELECT @current_cost_center_id = id FROM cost_center WHERE domain_id = @insert_to_domain_id AND project_id = @insert_to_project_id and is_default = 1;
		SELECT @cost_center_id = id FROM cost_center WHERE domain_id = @insert_to_domain_id AND project_id = @insert_to_project_id and code = (SELECT code FROM cost_center where id = @from_cost_center_id and project_id = @from_project_id and domain_id = @from_domain_id);
		IF @cost_center_id is null BEGIN
			SET @cost_center_id = @current_cost_center_id;
		END
	END
	ELSE BEGIN
		SET @cost_center_id = @from_cost_center_id;
	END
	
	
	SET @linked_id_template = @from_id;
	--SET BUDGET VALUE
	IF @link <> 1 BEGIN 
		SET @linked_id_template = NULL;

		IF lower(@budget) = 'default' BEGIN
			--need to know to project id in order to get this value
			SELECT @budget = default_cost_field FROM project WHERE domain_id = @insert_to_domain_id AND project_id = @insert_to_project_id;
		END
		IF lower(@budget) in('min_cost', 'max_cost', 'avg_cost', 'last_cost') BEGIN
			SET @sql = CONCAT('SELECT @avg_cost = ', + @budget + '  FROM assets 
						WHERE asset_id = ' , @asset_id , ' and domain_id = ' , @asset_domain_id);

			EXEC sp_executesql @sql, N'@avg_cost numeric(10,2) output', @avg_cost = @unit_budget output;

		END
		ELSE IF lower(@budget) != 'source' BEGIN
			SET @unit_budget = 0;
		END
	END

	--ADJUST COPY_LINK IF IT'S EMPTY
	IF @clone = 1 AND @copy_link = CAST(CAST(0 AS BINARY) AS UNIQUEIDENTIFIER) BEGIN
		SET @copy_link = NEWID();
		update project_room_inventory set copy_link = @copy_link where domain_id = @insert_to_domain_id and project_id = @insert_to_project_id  and inventory_id = @update_to_id;
		update project_room_inventory set copy_link = @copy_link where domain_id = @from_domain_id and project_id = @from_project_id and inventory_id = @from_id;
	END

	
	--UPDATE INVENTORY
	--*********************************************************************************************************************
	--* IMPORTANT!!! 
	--* EVERYTIME YOU UPDATE A COLUMN HERE, NEED TO UPDATE "$/XPlanner/Important files/CopyInventoryNotes.xlsx"
	--*********************************************************************************************************************
	UPDATE project_room_inventory SET 
	-- project_id=project_id	,
	-- department_id=department_id	,
	-- room_id=room_id	,
	asset_id = @asset_id	,
	status = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 THEN @status ELSE status END)	,
	resp = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 THEN @resp ELSE resp END)	,
	budget_qty = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @link = 1 THEN @budget_qty ELSE budget_qty END)	,
	dnp_qty = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @link = 1 THEN @dnp_qty ELSE dnp_qty END)	,
	unit_budget = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @sync = 1 THEN @unit_budget ELSE unit_budget END)	,
	-- buyout_delta=buyout_delta	,
	estimated_delivery_date = (CASE WHEN @new_inv = 1 OR @clone = 1 THEN @estimated_delivery_date ELSE estimated_delivery_date END)	,
	current_location = (CASE WHEN @clone = 1 THEN @current_location ELSE current_location END)	,
	inventory_type = @inventory_type	,
	date_added = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 THEN @date_added ELSE date_added END)	,
	added_by = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 THEN @added_by ELSE added_by END)	,
	comment = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 THEN @comment ELSE comment END)	,
	lease_qty = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @link = 1 THEN @lease_qty ELSE lease_qty END)	,
	cost_center_id = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @sync = 1  THEN @cost_center_id ELSE cost_center_id END)	,
	tag = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @link = 1 THEN @tag ELSE tag END)	,
	cad_id = @cad_id	,
	none_option = @none_option	,
	-- domain_id=domain_id	,
	-- phase_id=phase_id	,
	asset_domain_id = @asset_domain_id	,
	-- inventory_id=inventory_id	,
	-- option_ids=option_ids	,
	locked_unit_budget = (CASE WHEN @clone = 1 THEN @locked_unit_budget ELSE locked_unit_budget END)	,
	locked_budget_qty = (CASE WHEN @clone = 1 THEN @locked_budget_qty ELSE locked_budget_qty END)	,
	locked_dnp_qty = (CASE WHEN @clone = 1 THEN @locked_dnp_qty ELSE locked_dnp_qty END)	,
	-- options_unit_price=options_unit_price	,
	-- options_price=options_price	,
	-- asset_total_budget=asset_total_budget	,
	-- total_budget_amt=total_budget_amt	,
	linked_id_template = (CASE WHEN @template = 1 OR @clone = 1 THEN @linked_id_template ELSE linked_id_template END)	,
	-- asset_profile=asset_profile	,
	-- asset_profile_budget=asset_profile_budget	,
	detailed_budget = @detailed_budget	,
	locked_room_quantity = (CASE WHEN @clone = 1 THEN @locked_room_quantity ELSE locked_room_quantity END)	,
	linked_document = (CASE WHEN @clone = 1 THEN @linked_document ELSE linked_document END)	,
	lead_time = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 THEN @lead_time ELSE lead_time END)	,
	clin = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 THEN @clin ELSE clin END)	,
	inventory_source_id = (CASE WHEN @new_inv = 1 OR @clone = 1 THEN @inventory_source_id ELSE inventory_source_id END)	,
	-- inventory_target_id=inventory_target_id	,
	unit_markup = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @sync = 1 THEN @unit_markup ELSE unit_markup END)	,
	unit_escalation = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @sync = 1 THEN @unit_escalation ELSE unit_escalation END)	,
	unit_tax = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @sync = 1 THEN @unit_tax ELSE unit_tax END)	,
	unit_install_net = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @sync = 1 THEN @unit_install_net ELSE unit_install_net END)	,
	unit_install_markup = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @sync = 1 THEN @unit_install_markup ELSE unit_install_markup END)	,
	unit_freight_net = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @sync = 1 THEN @unit_freight_net ELSE unit_freight_net END)	,
	unit_freight_markup = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @sync = 1 THEN @unit_freight_markup ELSE unit_freight_markup END)	,
	-- total_unit_budget=total_unit_budget	,
	-- unit_markup_calc=unit_markup_calc	,
	-- unit_escalation_calc=unit_escalation_calc	,
	-- unit_budget_adjusted=unit_budget_adjusted	,
	-- unit_tax_calc=unit_tax_calc	,
	-- unit_install=unit_install	,
	-- unit_freight=unit_freight	,
	-- unit_budget_total=unit_budget_total	,
	-- total_install_net=total_install_net	,
	-- total_budget_adjusted=total_budget_adjusted	,
	-- total_tax=total_tax	,
	-- total_install=total_install	,
	-- total_freight_net=total_freight_net	,
	-- total_freight=total_freight	,
	-- total_budget=total_budget	,
	-- net_new=net_new	,
	asset_description_ow = @asset_description_ow	,
	asset_description = @asset_description	,
	ECN = (CASE WHEN @clone = 1 OR @link = 1 THEN @ECN ELSE ECN END)	,
	placement_ow = @placement_ow	,
	placement = @placement	,
	biomed_check_required = @biomed_check_required	,
	temporary_location = (CASE WHEN @clone = 1 THEN @temporary_location ELSE temporary_location END)	,
	height_ow = @height_ow	,
	height = @height	,
	width_ow = @width_ow	,
	width = @width	,
	depth_ow = @depth_ow	,
	depth = @depth	,
	mounting_height_ow = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @link = 1 THEN @mounting_height_ow ELSE mounting_height_ow END),
	mounting_height = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @link = 1 THEN @mounting_height ELSE mounting_height END),
	class_ow = @class_ow	,
	class = @class	,
	jsn_code = (CASE WHEN @template = 1 OR @new_inv = 1 OR @clone = 1 OR @link = 1 THEN @jsn_code ELSE jsn_code END)	,
	jsn_utility1 = @jsn_utility1	,
	jsn_utility2 = @jsn_utility2	,
	jsn_utility3 = @jsn_utility3	,
	jsn_utility4 = @jsn_utility4	,
	jsn_utility5 = @jsn_utility5	,
	jsn_utility6 = @jsn_utility6	,
	jsn_utility7 = @jsn_utility7	,
	-- update_trigger=update_trigger	,
	copy_link = (CASE WHEN @clone = 1 THEN @copy_link ELSE copy_link END)	,
	jsn_ow = @jsn_ow	,
	manufacturer_description = @manufacturer_description	,
	manufacturer_description_ow = @manufacturer_description_ow	,
	model_number = @model_number	,
	model_number_ow = @model_number_ow	,
	model_name = @model_name	,
	model_name_ow = @model_name_ow,
	cut_sheet_filename = @cut_sheet_filename,
	--final_disposition = @final_disposition,
	delivered_date = (CASE WHEN @clone = 1 THEN @delivered_date ELSE delivered_date END),
	received_date = (CASE WHEN @clone = 1 THEN @received_date ELSE received_date END),
	connection_type = @connection_type,
	connection_type_ow = @connection_type_ow,
	plug_type = @plug_type,	
	plug_type_ow = @plug_type_ow,
	lan = @lan,
	lan_ow = @lan_ow,
	network_type = @network_type,
	network_type_ow = @network_type_ow,
	network_option = @network_option,
	network_option_ow = @network_option_ow,
	ports = @ports,
	ports_ow = @ports_ow,
	bluetooth = @bluetooth,
	bluetooth_ow = @bluetooth_ow,
	cat6 = @cat6,
	cat6_ow = @cat6_ow,
	displayport = @displayport,
	displayport_ow = @displayport_ow,
	dvi = @dvi,
	dvi_ow = @dvi_ow,
	hdmi = @hdmi,
	hdmi_ow = @hdmi_ow,
	wireless = @wireless,
	wireless_ow = @wireless_ow,
	volts = @volts,
	volts_ow = @volts_ow,
	amps = @amps,
	amps_ow = @amps_ow	
	
	-- IMPORTANT: DO NOT ADD FIELDS HERE, use the excel worksheet in TFS "$/XPlanner/Important files/CopyInventoryNotes.xlsx"
	WHERE domain_id = @insert_to_domain_id and inventory_id = @update_to_id;
	-- TODO(JLT): We have to modify this to eliminate the copy options only for the option not to copy
	-- in general we should always copy
	IF @copy_options = 1 OR (@new_inv <> 1 AND @clone <> 1)  BEGIN
		EXEC copy_options @from_id, @update_to_id, @insert_to_domain_id, @options_unit_price, @asset_profile, @asset_profile_budget, 1;
	END
	 
	return @update_to_id;

END
