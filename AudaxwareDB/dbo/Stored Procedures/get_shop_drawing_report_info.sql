CREATE PROCEDURE [dbo].[get_shop_drawing_report_info]
	@domain_id SMALLINT,
    @project_id INTEGER,
	@type VARCHAR(15)
AS
BEGIN
	SET NOCOUNT ON;

	SET @type = lOWER(@type);

	SELECT pdoc.*, a.asset_code, a.asset_description, pd.description AS department_description,
		pp.description AS phase_description, pr.drawing_room_number AS room_number, pr.drawing_room_name
		AS room_name, p.project_description
	FROM project_documents pdoc
	LEFT JOIN documents_associations da ON pdoc.id = da.document_id
	LEFT JOIN document_types dt ON pdoc.type_id = dt.id
	-- TODO(JLT): This is wrong, the showdrawing should not be associated with the asset catalog, it should
	-- be linked to the room inventory, so this should be the inventory id, so we can get the correct description
	LEFT JOIN assets a ON a.domain_id = da.asset_domain_id AND a.asset_id = da.asset_id
	LEFT JOIN project p ON pdoc.project_domain_id = p.domain_id AND pdoc.project_id = p.project_id
	LEFT JOIN project_phase pp ON pdoc.project_domain_id = pp.domain_id AND pdoc.project_id = pp.project_id
		AND da.phase_id = pp.phase_id
	LEFT JOIN project_department pd ON pdoc.project_domain_id = pd.domain_id AND pdoc.project_id = pd.project_id
		AND da.phase_id = pd.phase_id AND da.department_id = pd.department_id
	LEFT JOIN project_room pr ON pdoc.project_domain_id = pr.domain_id AND pdoc.project_id = pr.project_id
		AND da.phase_id = pr.phase_id AND da.department_id = pr.department_id AND da.room_id = pr.room_id
	WHERE pdoc.project_domain_id = @domain_id AND pdoc.project_id = @project_id AND 
		(@type = 'all' OR LOWER(dt.name) = @type)
	ORDER BY pdoc.filename, asset_code, asset_description, pp.description, pd.description, pr.drawing_room_number,
		pr.drawing_room_name;
END