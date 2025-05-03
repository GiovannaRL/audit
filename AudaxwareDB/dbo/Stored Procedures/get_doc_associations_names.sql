CREATE PROCEDURE [dbo].[get_doc_associations_names]
	@project_domain_id SMALLINT,
    @project_id INTEGER,
	@document_id INTEGER
AS
BEGIN
	SELECT da.*, a.asset_code, pr.drawing_room_number AS room_number, pr.drawing_room_name AS room_name,
		pd.description AS department_description, pp.description AS phase_description
	FROM documents_associations da 
	LEFT JOIN project_room_inventory pri ON da.inventory_id = pri.inventory_id
	LEFT JOIN assets a ON pri.asset_domain_id = a.domain_id AND a.asset_id = pri.asset_id
	LEFT JOIN project_room pr ON da.room_id = pr.room_id AND da.department_id = pr.department_id AND
		da.phase_id = pr.phase_id AND da.project_domain_id = pr.domain_id AND da.project_id = 
		pr.project_id
	LEFT JOIN project_department pd ON da.department_id = pd.department_id AND da.phase_id = pd.phase_id
		AND da.project_id = pd.project_id AND da.project_domain_id = pd.domain_id
	LEFT JOIN project_phase pp ON da.phase_id = pp.phase_id AND da.project_id = pp.project_id AND
		da.project_domain_id = pp.domain_id
	WHERE da.project_domain_id = @project_domain_id AND da.project_id = @project_id AND da.document_id = @document_id
	ORDER BY phase_description, department_description, room_number, room_name, asset_code;

END