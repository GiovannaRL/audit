CREATE PROCEDURE [dbo].[get_project_treeview]
	@domain_id int
AS
	SELECT 
p.domain_id, 
p.project_id, 
p.project_description,
CONCAT(
	CASE WHEN p.client_project_number IS NULL OR p.client_project_number = '' THEN '' ELSE CONCAT(p.client_project_number, ' - ')END,
	p.project_description, ' (',	
	CASE 
		WHEN p.status = 'A' THEN 'Active'
		WHEN p.status = 'D' THEN 'Canceled'
		WHEN p.status = 'C' THEN 'Complete'
		WHEN p.status = 'I' THEN 'Inventory'
		WHEN p.status = 'L' THEN 'Locked'
		WHEN p.status = 'P' THEN 'Pending'
		WHEN p.status = 'T' THEN 'Training'
	END,
	CASE WHEN p.locked_comment IS NULL OR DATALENGTH(p.locked_comment) = 0 THEN '' ELSE CONCAT(' - ', p.locked_comment) END,	
	')'
) AS project_name,
pp.phase_id, 
pp.description AS phase_description,pd.department_id,  
pd.description AS department_description, 
pr.room_id, 

CONCAT(
	pr.drawing_room_name,
	CASE WHEN pr.drawing_room_number IS NULL OR DATALENGTH(pr.drawing_room_number) = 0 THEN '' ELSE CONCAT(' - ', pr.drawing_room_number)END,
	CASE WHEN pr.room_quantity > 1 THEN CONCAT(' (', pr.room_quantity, ')') ELSE '' END
) AS room_name,
p.status
FROM project p 
LEFT JOIN project_phase pp ON p.project_id = pp.project_id and p.domain_id = pp.domain_id
LEFT JOIN project_department pd ON pp.project_id = pd.project_id AND pp.phase_id = pd.phase_id and pp.domain_id = pd.domain_id
LEFT JOIN project_room pr ON pd.project_id = pr.project_id AND pd.phase_id = pr.phase_id AND pd.department_id = pr.department_id and pd.domain_id = pr.domain_id
WHERE p.status <> 'R' AND p.project_id <> 1 and p.domain_id = @domain_id
ORDER BY project_name, p.project_id, pp.description, pp.phase_id, pd.description, pd.department_id, room_name --DO NOT CHANGE THE ORDER
