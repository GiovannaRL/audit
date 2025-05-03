CREATE PROCEDURE [dbo].[room_inventory_po](@domain_id INTEGER, @project_id integer, @phase_id integer, @department_id integer) 
--RETURNS TABLE
AS	
BEGIN
	SELECT a.domain_id, a.project_id, a.phase_id, a.department_id, a.room_id, a.drawing_room_name, a.drawing_room_number, 
			a.final_room_name, a.final_room_number,
			SUM(COALESCE(b.total_budget_amt,0)) as total_budget_amt, SUM(COALESCE(b.total_po_amt,0)) as total_po_amt, 
			SUM(COALESCE(b.buyout_delta,0)) as buyout_delta, a.comment, a.room_quantity
			FROM project_room a LEFT OUTER JOIN inventory_po_qty_v b 
			ON(a.domain_id = b.domain_id AND a.project_id = b.project_id AND a.department_id = b.department_id AND a.room_id = b.room_id) 
			WHERE a.domain_id = @domain_id AND a.project_id = @project_id AND a.phase_id = @phase_id AND a.department_id = @department_id
			GROUP BY a.domain_id, a.project_id, a.phase_id, a.department_id, a.room_id, a.drawing_room_name, a.drawing_room_number, a.final_room_name,
			a.final_room_number, a.comment, a.room_quantity ORDER BY a.drawing_room_number, a.drawing_room_name;
END

