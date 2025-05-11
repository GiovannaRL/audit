CREATE PROCEDURE [dbo].[copy_single_item_inventory]
	@domain_id SMALLINT,
    @curr_project_id INTEGER,
    @curr_phase_id INTEGER,
    @curr_department_id INTEGER,
	@curr_room_id INTEGER,
	@curr_inventory_id INTEGER,
	@new_project_id INTEGER,
	@new_phase_id INTEGER,
	@new_department_id INTEGER,
	@new_room_id INTEGER,
	@quantity INTEGER,
	@copy_options BIT,
	@added_by VARCHAR(50),
	@inventory_source_id INTEGER = null,
	@unit_budget decimal(10,2) = null,
	@with_budget BIT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @new_inventory_id INTEGER;

	INSERT INTO project_room_inventory(domain_id, project_id, phase_id, department_id, room_id,  asset_id, asset_domain_id, status, 
		resp, budget_qty, dnp_qty, unit_budget, buyout_delta, estimated_delivery_date, current_location,
		inventory_type, date_added, added_by, comment, lease_qty, /*cost_center_id,*/ tag, linked_id_template, inventory_source_id,
		asset_description_ow, asset_description, ECN, placement, placement_ow, temporary_location,
		height_ow, height, width_ow, width, depth_ow, depth, mounting_height_ow,
		mounting_height, class_ow, class, jsn_code,
		jsn_utility1, jsn_utility2, jsn_utility3,
		jsn_utility4, jsn_utility5,
		jsn_utility6, jsn_utility7, jsn_ow,
		manufacturer_description, manufacturer_description_ow, serial_number, serial_number_ow, serial_name, serial_name_ow,
		plug_type, plug_type_ow, connection_type, connection_type_ow, lan, lan_ow, network_type, network_type_ow, network_option, network_option_ow
		)
	SELECT @domain_id, @new_project_id, @new_phase_id, @new_department_id, @new_room_id, asset_id, asset_domain_id,
		[status], resp, @quantity, 0, case when @inventory_source_id is null then unit_budget else @unit_budget end, buyout_delta, null, 'Plan', inventory_type, GETDATE(), @added_by,
		comment, lease_qty, /*cost_center_id,*/ tag, null, @inventory_source_id,
		asset_description_ow, asset_description, ECN, placement, placement_ow, temporary_location,
		height_ow, height, width_ow, width, depth_ow, depth, mounting_height_ow,
		mounting_height, class_ow, class, jsn_code,
		jsn_utility1, jsn_utility2, jsn_utility3,
		jsn_utility4, jsn_utility5,
		jsn_utility6, jsn_utility7, jsn_ow,
		manufacturer_description, manufacturer_description_ow, serial_number, serial_number_ow, serial_name, serial_name_ow,
		plug_type, plug_type_ow, connection_type, connection_type_ow, lan, lan_ow, network_type, network_type_ow, network_option, network_option_ow
	FROM project_room_inventory WHERE inventory_id = @curr_inventory_id AND domain_id = @domain_id AND project_id = @curr_project_id 
		AND phase_id = @curr_phase_id AND department_id = @curr_department_id AND room_id = @curr_room_id;

	SELECT @new_inventory_id = MAX(inventory_id) FROM project_room_inventory WHERE domain_id = @domain_id AND project_id = 
		@new_project_id AND phase_id = @new_phase_id AND department_id = @new_department_id AND room_id = @new_room_id;

	IF @inventory_source_id IS NOT NULL BEGIN
		update project_room_inventory set inventory_target_id = @new_inventory_id where inventory_id = @curr_inventory_id AND domain_id = @domain_id
	END

	IF @copy_options = 1
		BEGIN
			INSERT INTO inventory_options(inventory_id, option_id, domain_id, unit_price, quantity, inventory_domain_id) 
			SELECT @new_inventory_id, option_id, domain_id, unit_price, quantity, @domain_id 
			FROM inventory_options WHERE inventory_id = @curr_inventory_id;
		END

	IF @with_budget = 1
		BEGIN
			EXEC copy_inventory_budget_values @domain_id, @curr_project_id, @curr_phase_id, @curr_department_id ,@curr_room_id,	@curr_inventory_id, 
			@domain_id, @new_project_id, @new_phase_id , @new_department_id, @new_room_id, @new_inventory_id;
		END

	SELECT * from asset_inventory 
	WHERE inventory_id = @new_inventory_id AND domain_id = @domain_id AND project_id = @new_project_id;
END
