CREATE VIEW [dbo].[full_project_name] AS 

	SELECT  '{ "domain_id": ' + cast(pri.domain_id as varchar(3)) + ', "room_id": ' + cast(pri.room_id as varchar(10)) + ', "drawing_room_number": "' + pr.drawing_room_number + '", "drawing_room_name":  "' + REPLACE(pr.drawing_room_name, '"', '\"') + '", "room_quantity": ' + cast(pr.room_quantity as varchar(10)) + ', "project_id": ' + cast(pri.project_id as varchar(10)) + ', "phase_id": ' + cast(pri.phase_id as varchar(10)) + ', "department_id": ' + cast(pri.department_id as varchar(10)) + '}' + '||' + 
	p.project_description + ', ' + pp.description + ', ' + pd.description + ', ' + pr.drawing_room_name + case when pr.drawing_room_number is null then '' else ' - ' + pr.drawing_room_number end as project, 
	'{ "domain_id": ' + cast(pri.domain_id as varchar(3)) + ', "room_id": ' + cast(pri.room_id as varchar(10)) + ', "drawing_room_number": "' + pr.drawing_room_number + '", "drawing_room_name":  "' + REPLACE(pr.drawing_room_name, '"', '\"') + '", "room_quantity": ' + cast(pr.room_quantity as varchar(10)) + ', "project_id": ' + cast(pri.project_id as varchar(10)) + ', "phase_id": ' + cast(pri.phase_id as varchar(10)) + ', "department_id": ' + cast(pri.department_id as varchar(10)) + '}' + '||' + 
	pr.drawing_room_number as room,
	pri.inventory_id, pri.inventory_source_id 
	FROM project_room_inventory pri 
	INNER JOIN project_room pr on pr.room_id = pri.room_id and pr.project_id = pri.project_id AND pr.domain_id = pri.domain_id AND pr.phase_id = pri.phase_id AND pr.department_id = pri.department_id
	INNER JOIN project_department pd ON pd.project_id = pri.project_id AND pd.domain_id = pri.domain_id AND pd.phase_id = pri.phase_id AND pd.department_id = pri.department_id
	INNER JOIN project_phase pp ON pp.project_id = pri.project_id AND pp.domain_id = pri.domain_id AND pp.phase_id = pri.phase_id
	INNER JOIN project p on p.project_id = pri.project_id and p.domain_id = pri.domain_id
