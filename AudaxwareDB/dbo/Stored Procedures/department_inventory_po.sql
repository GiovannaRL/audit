
CREATE PROCEDURE [dbo].[department_inventory_po](@domain_id INTEGER, @project_id integer, @phase_id integer) 
--RETURNS TABLE
AS	
BEGIN
	SELECT a.domain_id, a.phase_id, a.project_id, a.department_id, a.description, a.department_type_id, a.department_type_domain_id, 
			c.description AS type, a.area, COUNT(DISTINCT(b.room_id)) AS num_rooms, SUM(COALESCE(b.total_budget_amt,0)) AS 
			total_budget_amt, SUM(COALESCE(b.total_po_amt,0)) AS total_po_amt, SUM(COALESCE(b.buyout_delta,0)) AS buyout_delta, 
			a.comment FROM project_department a LEFT OUTER JOIN inventory_po_qty_v b ON 
			(a.domain_id = b.domain_id AND a.project_id = b.project_id AND a.department_id = b.department_id), 
			department_type c WHERE a.department_type_id = c.department_type_id AND c.domain_id = a.department_type_domain_id
			AND a.domain_id = @domain_id AND a.project_id = @project_id AND a.phase_id = @phase_id
			GROUP BY a.domain_id, a.project_id, 
			a.phase_id, a.department_id, a.description,  a.department_type_id, a.department_type_domain_id, c.description, 
			a.area, a.phase_id, a.comment ORDER BY a.description;
END