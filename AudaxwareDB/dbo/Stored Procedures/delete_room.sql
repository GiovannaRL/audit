CREATE PROCEDURE [dbo].[delete_room]
	@domain_id int,
	@project_id int,
	@phase_id int,
	@department_id int,
	@room_id int
AS
BEGIN
	DELETE FROM inventory_purchase_order WHERE inventory_id IN (SELECT inventory_id FROM project_room_inventory WHERE
		domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id AND department_id = @department_id
		AND room_id = @room_id);

	DELETE FROM purchase_order WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id AND 
		department_id = @department_id AND room_id = @room_id;

	DELETE FROM project_room WHERE domain_id = @domain_id AND project_id = @project_id AND phase_id = @phase_id AND 
		department_id = @department_id AND room_id = @room_id;
END
