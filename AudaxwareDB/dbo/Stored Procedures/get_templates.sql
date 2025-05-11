CREATE PROCEDURE [dbo].[get_templates](
	@domain_id SMALLINT,
    @show_audaxware bit,
    @project_id INTEGER = null,
    @template_type INTEGER = null)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT pr.id as template_id, pr.domain_id, pr.project_id, pr.phase_id, pr.department_id, pr.room_id, pr.drawing_room_name as description, dt.description as department_type,
           pr.project_id_template, pr.project_domain_id_template, CASE WHEN pr.project_domain_id_template IS NULL THEN p.project_description ELSE p2.project_description END as project_name,
           pr.date_added, pr.comment, d.name as owner
	FROM project_room pr
	INNER JOIN project_department pd ON pd.project_id = pr.project_id and pd.domain_id = pr.domain_id and pd.department_id = pr.department_id
	INNER JOIN project_phase pp ON pp.project_id = pd.project_id and pp.domain_id = pd.domain_id and pp.phase_id = pd.phase_id
	INNER JOIN project p ON p.project_id = pr.project_id and p.domain_id = pr.domain_id
	INNER JOIN department_type dt ON dt.department_type_id =  pr.department_type_id_template and dt.domain_id = pr.department_type_domain_id_template
	INNER JOIN domain d ON d.domain_id = pr.domain_id
	LEFT JOIN project p2 ON p2. project_id = pr.project_id_template and p2.domain_id = pr.project_domain_id_template
	WHERE pr.is_template = 1 
	AND (
		@template_type is null AND @project_id is null AND ( project_domain_id_template = @domain_id or project_domain_id_template is null ) AND (pr.domain_id = @domain_id OR (@show_audaxware = 1 AND pr.domain_id = 1 ) ) --show everything
		OR (@template_type = 1 AND pr.domain_id = 1 AND pr.project_id_template is null AND @show_audaxware = 1) --show global audaxware
		OR (@template_type = 2 AND pr.domain_id = @domain_id AND pr.project_id_template is null)  --show global local enterprise
		OR (@template_type = 3 AND pr.project_id_template = @project_id AND pr.project_domain_id_template = @domain_id) --show only from projects
	)


END