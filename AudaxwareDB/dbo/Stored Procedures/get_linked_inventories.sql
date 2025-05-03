CREATE PROCEDURE [dbo].[get_linked_inventories]
	@inventories_id VARCHAR(5000)
AS
	SELECT a.inventory_id as inventory_id from project_room_inventory a
	join project_room pr on pr.id = a.linked_id_template
	join project_room_inventory t on t.project_id = pr.project_id and t.phase_id = pr.phase_id and t.department_id = pr.department_id and t.room_id = pr.room_id and t.domain_id = pr.domain_id
	where a.asset_id = t.asset_id and a.asset_domain_id = t.asset_domain_id and  t.inventory_id in (SELECT CAST(value AS INTEGER) AS inventory_id FROM STRING_SPLIT(@inventories_id, ';'));
