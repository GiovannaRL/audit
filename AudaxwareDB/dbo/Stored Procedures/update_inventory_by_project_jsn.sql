CREATE PROCEDURE [dbo].[update_inventory_by_project_jsn](
	@domain_id smallint,
	@project_id int,
	@inventory_id int,
	@jsn_code varchar(10),
	@resp varchar(10))
AS BEGIN
	
	DECLARE @updated_to_id INTEGER, @updated_from_id int, @total int;

	DECLARE update_inventory_cursor CURSOR LOCAL FOR SELECT inventory_id
	FROM project_room_inventory 
	WHERE project_id = @project_id and domain_id = @domain_id and jsn_code = @jsn_code and inventory_id != @inventory_id and COALESCE(jsn_code, '') <> '' and resp = @resp

	OPEN update_inventory_cursor;

	FETCH NEXT FROM update_inventory_cursor INTO @updated_to_id
	WHILE @@FETCH_STATUS = 0
		BEGIN

			exec copy_inventory_item @domain_id, @inventory_id, @updated_to_id, null, null, null, null, @domain_id, 'source', null, 5

			FETCH NEXT FROM update_inventory_cursor INTO @updated_to_id
		 END
	CLOSE update_inventory_cursor;
	DEALLOCATE update_inventory_cursor;
END
