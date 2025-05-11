CREATE PROCEDURE [dbo].[get_possible_associations]
	@domain_id SMALLINT,
    @project_id INTEGER,
	@document_id INTEGER,
    @type VARCHAR(10)
AS
BEGIN
	SET NOCOUNT ON;

	SET @type = LOWER(@type);

	IF @type = 'asset'
		BEGIN
			SELECT DISTINCT a.asset_code, a.asset_id, pri.asset_domain_id, p.project_id, p.project_description, 
				p.domain_id, pp.phase_id, pp.description AS phase_description, pd.department_id, pd.description AS department_description, 
				pr.room_id, pr.drawing_room_number AS room_number, pr.drawing_room_name AS room_name,
				CASE WHEN @document_id = -1 OR NOT EXISTS (SELECT * FROM documents_associations WHERE document_id = @document_id AND 
					project_domain_id = pri.domain_id AND project_id = pri.project_id AND department_id = 
					pri.department_id AND phase_id = pri.phase_id AND room_id = pri.room_id AND asset_domain_id = 
					pri.asset_domain_id AND asset_id = pri.asset_id) THEN 0 ELSE 1 END AS [check]
			FROM project_room_inventory pri
			LEFT JOIN assets a ON pri.asset_domain_id = a.domain_id AND pri.asset_id = a.asset_id
			LEFT JOIN project p ON pri.domain_id = p.domain_id AND pri.project_id = p.project_id
			LEFT JOIN project_phase pp ON pri.project_id = pp.project_id AND pri.domain_id = pp.domain_id
				AND pp.phase_id = pri.phase_id
			LEFT JOIN project_department pd ON pd.domain_id = pri.domain_id AND pd.project_id = pri.project_id
				AND pd.phase_id = pri.phase_id AND pd.department_id = pri.department_id
			LEFT JOIN project_room pr ON pr.domain_id = pri.domain_id AND pr.project_id = pri.project_id
				AND pr.phase_id = pri.phase_id AND pr.department_id = pri.department_id AND pr.room_id
				= pri.room_id
			WHERE pri.domain_id = @domain_id AND pri.project_id = @project_id;
		END
	ELSE IF @type = 'room'
		BEGIN
			SELECT p.project_id, p.project_description, p.domain_id, pp.phase_id, pp.description AS phase_description, 
				pd.department_id, pd.description AS department_description, pr.room_id, 
				pr.drawing_room_number AS room_number, pr.drawing_room_name AS room_name,
				null AS asset_code, null AS asset_id, CAST(-1 AS SMALLINT) AS asset_domain_id,
				CASE WHEN @document_id = -1 OR NOT EXISTS (SELECT * FROM documents_associations WHERE 
					document_id = @document_id AND project_domain_id = pr.domain_id AND project_id = 
					pr.project_id AND department_id = pr.department_id AND phase_id = pr.phase_id AND 
					room_id = pr.room_id) THEN 0 ELSE 1 END AS [check]
			FROM project_room pr 
			LEFT JOIN project p ON p.domain_id = pr.domain_id AND p.project_id = pr.project_id
			LEFT JOIN project_phase pp ON pr.domain_id = pp.domain_id AND pr.project_id = pp.project_id
				AND pr.phase_id = pp.phase_id
			LEFT JOIN project_department pd ON pr.domain_id = pd.domain_id AND pr.project_id =
				pd.project_id AND pr.phase_id = pd.phase_id AND pr.department_id = pd.department_id
			WHERE pr.domain_id = @domain_id AND pr.project_id = @project_id;
		END
	ELSE IF @type = 'department'
		BEGIN
			SELECT p.project_id, p.project_description, p.domain_id, pp.phase_id, pp.description AS phase_description, 
				pd.department_id, pd.description AS department_description, null AS room_id, null AS
				room_number, null AS room_name, null AS asset_code, null AS asset_id, CAST(-1 AS SMALLINT) AS asset_domain_id,
				CASE WHEN @document_id = -1 OR NOT EXISTS (SELECT * FROM documents_associations WHERE 
				document_id = @document_id AND project_domain_id = pd.domain_id AND project_id = 
				pd.project_id AND department_id = pd.department_id) THEN 0 ELSE 1 END AS [check]
			FROM project_department pd 
			LEFT JOIN project p ON p.domain_id = pd.domain_id AND p.project_id = pd.project_id
			LEFT JOIN project_phase pp ON pd.domain_id = pp.domain_id AND pd.project_id = pp.project_id
				AND pd.phase_id = pp.phase_id
			WHERE pd.domain_id = @domain_id AND pd.project_id = @project_id
		END
	ELSE IF @type = 'phase'
		BEGIN
			SELECT p.project_id, p.project_description, p.domain_id, pp.phase_id, pp.description AS phase_description,
				null AS room_id, null AS room_number, null AS room_name, null AS asset_code, null AS 
				asset_id, CAST(-1 AS SMALLINT) AS asset_domain_id, null AS department_id, null AS department_description,
				CASE WHEN @document_id = -1 OR NOT EXISTS (SELECT * FROM documents_associations WHERE 
					document_id = @document_id AND project_domain_id = pp.domain_id AND project_id = 
					pp.project_id AND phase_id = pp.phase_id) THEN 0 ELSE 1 END AS [check]
			FROM project_phase pp
				LEFT JOIN project p ON p.domain_id = pp.domain_id AND p.project_id = pp.project_id
			WHERE pp.domain_id = @domain_id AND pp.project_id = @project_id
		END
END
