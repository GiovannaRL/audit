CREATE PROCEDURE [dbo].[edit_multi_asset]
	@inventories_id VARCHAR(5000),
    @resp CHAR(10),
	@current_location VARCHAR(50),
    @cost_center_id INTEGER,
    @estimated_delivery_date DATETIME,
    @comment VARCHAR(1000),
    @tag VARCHAR(5000),
    @cad_id VARCHAR(25),
	@unit_budget NUMERIC(10, 2),
    @none_option BIT,
	@detailed_budget BIT,
	@lead_time int,
	@clin VARCHAR(50),
	@unit_markup NUMERIC(10, 2),
	@unit_freight_net NUMERIC(10, 2),
	@unit_freight_markup NUMERIC(10, 2),
	@unit_escalation NUMERIC(10, 2),
	@unit_install_net NUMERIC(10, 2),
	@unit_install_markup NUMERIC(10, 2),
	@unit_tax NUMERIC(10, 2),
	@ECN VARCHAR(20), 
	@placement VARCHAR(20),
	@placement_ow BIT,
	@temporary_location VARCHAR(200), 
	@biomed_check_required bit,
	@asset_description VARCHAR(300),
	@asset_description_aw BIT,
	@budget_qty INTEGER,
	@lease_qty INTEGER,
	@dnp_qty INTEGER,
	@jsn_ow BIT,
	@manufacturer_description VARCHAR (100),
	@manufacturer_description_ow BIT,
	@serial_number VARCHAR (100),
	@serial_number_ow BIT,
	@serial_name  VARCHAR (150),
	@serial_name_ow BIT,
	@jsn_code varchar(10),
	@jsn_u1 varchar(10),
	@jsn_u2 varchar(10),
	@jsn_u3 varchar(10),
	@jsn_u4 varchar(10),
	@jsn_u5 varchar(10),
	@jsn_u6 varchar(10),
	@jsn_u7 varchar(10),
	@class INT,
	@class_ow BIT,
	@final_disposition VARCHAR(200),
	@delivered_date DATETIME,
	@received_date DATETIME
AS
BEGIN
	
	declare @linked_inventories varchar(max);

	SELECT @linked_inventories = (STUFF((SELECT ';' +  cast(a.inventory_id as varchar(10)) from project_room_inventory a
	join project_room pr on pr.id = a.linked_id_template
	join project_room_inventory t on t.project_id = pr.project_id and t.phase_id = pr.phase_id and t.department_id = pr.department_id and t.room_id = pr.room_id and t.domain_id = pr.domain_id
	where a.asset_id = t.asset_id and a.asset_domain_id = t.asset_domain_id and  t.inventory_id in (SELECT CAST(value AS INTEGER) AS inventory_id FROM STRING_SPLIT(@inventories_id, ';'))
	FOR XML PATH('')), 1,1, ''));

	if count(@linked_inventories) > 0 begin
		set @inventories_id = @inventories_id + ';' + @linked_inventories;
	end

	UPDATE project_room_inventory SET resp = COALESCE(@resp, resp), current_location = 
		COALESCE(@current_location, current_location), cost_center_id = COALESCE(@cost_center_id, cost_center_id), 
		estimated_delivery_date = COALESCE(@estimated_delivery_date, estimated_delivery_date),
		comment = COALESCE(@comment, comment), tag = CASE @tag WHEN '' THEN tag ELSE COALESCE(@tag, tag) END, 
		cad_id = CASE @cad_id WHEN '-1' THEN NULL ELSE COALESCE(@cad_id, cad_id) END, 
		unit_budget = COALESCE(@unit_budget, unit_budget), none_option = COALESCE(@none_option, none_option), detailed_budget = COALESCE(@detailed_budget, detailed_budget),
		lead_time = COALESCE(@lead_time, lead_time), clin = COALESCE(@clin, clin), unit_markup = coalesce(@unit_markup, unit_markup),
		unit_freight_net = coalesce(@unit_freight_net, unit_freight_net), unit_freight_markup = coalesce(@unit_freight_markup, unit_freight_markup),
		unit_escalation = coalesce(@unit_escalation, unit_escalation), unit_install_net = coalesce(@unit_install_net, unit_install_net),
		unit_install_markup = coalesce(@unit_install_markup, unit_install_markup), unit_tax = coalesce(@unit_tax, unit_tax),
		ECN = coalesce(@ECN, ECN), 
		placement = coalesce(@placement, placement), 
		placement_ow = coalesce(@placement_ow, placement_ow), 
		temporary_location = coalesce(@temporary_location, temporary_location),
		biomed_check_required = coalesce(@biomed_check_required, biomed_check_required),
		asset_description = coalesce(@asset_description, asset_description), asset_description_ow = coalesce(@asset_description_aw, asset_description_ow),
		budget_qty = coalesce(@budget_qty, budget_qty),
		lease_qty = coalesce(@lease_qty, lease_qty),
		dnp_qty = coalesce(@dnp_qty, dnp_qty),
		manufacturer_description = coalesce(@manufacturer_description, manufacturer_description),
		manufacturer_description_ow = coalesce(@manufacturer_description_ow	, manufacturer_description_ow),
		serial_number = coalesce(@serial_number, serial_number),
		serial_number_ow = coalesce(@serial_number_ow, serial_number_ow),
		serial_name = coalesce(@serial_name, serial_name),
		serial_name_ow = coalesce(@serial_name_ow, serial_name_ow),
		jsn_ow = coalesce(@jsn_ow, jsn_ow),
		class = coalesce(@class, class),
		class_ow = coalesce(@class_ow, class_ow),
		final_disposition = coalesce(@final_disposition, final_disposition),
		delivered_date = COALESCE(@delivered_date, delivered_date), 
		received_date = COALESCE(@received_date, received_date),
		cut_sheet_filename = CASE WHEN 
			(COALESCE(@asset_description_aw, asset_description_ow) = 1 AND asset_description <> @asset_description)
			OR ((case when placement_ow != 1 and (@placement is null or @placement = '') then 0 else 1 end) = 1 AND placement <> @placement)
			OR (COALESCE(@class, class) = 1 AND class <> @class)
			OR (COALESCE(@jsn_ow, jsn_ow) = 1 AND jsn_code <> @jsn_code)
			OR (COALESCE(@manufacturer_description_ow, manufacturer_description_ow) = 1 AND manufacturer_description <> @manufacturer_description)
			OR (COALESCE(@serial_number_ow, serial_number_ow) = 1 AND serial_number <> @serial_number)
			OR (COALESCE(@serial_name_ow, serial_name_ow) = 1 AND serial_name <> @serial_name)
			THEN null ELSE cut_sheet_filename END
	WHERE inventory_id in (SELECT CAST(value AS INTEGER) AS inventory_id FROM STRING_SPLIT(@inventories_id, ';'));

	IF @jsn_ow = 1
	BEGIN
		UPDATE project_room_inventory SET jsn_code = COALESCE(@jsn_code, jsn_code), 
			jsn_utility1 = COALESCE(@jsn_u1, jsn_utility1), 
			jsn_utility2 = COALESCE(@jsn_u2, jsn_utility2),	jsn_utility3 = COALESCE(@jsn_u3, jsn_utility3), 
			jsn_utility4 = COALESCE(@jsn_u4, jsn_utility4), jsn_utility5 = COALESCE(@jsn_u5, jsn_utility5), 
			jsn_utility6 = COALESCE(@jsn_u6, jsn_utility6), jsn_utility7 = COALESCE(@jsn_u7, jsn_utility7)
		WHERE inventory_id in (SELECT CAST(value AS INTEGER) AS inventory_id FROM STRING_SPLIT(@inventories_id, ';'));
	END


	SELECT * FROM project_room_inventory WHERE inventory_id in (SELECT CAST(value AS INTEGER) AS inventory_id FROM STRING_SPLIT(@inventories_id, ';'));
END