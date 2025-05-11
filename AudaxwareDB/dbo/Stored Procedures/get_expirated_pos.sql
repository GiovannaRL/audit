CREATE PROCEDURE [dbo].[get_expirated_pos]
	@userId VARCHAR(128),
	@domainId SMALLINT
AS
	SELECT 
		po.domain_id, po.project_id, po.phase_id, po.department_id, po.room_id, po.po_id, po.description AS po_description,
		p.project_description, pp.description AS phase_description, pd.description AS department_description,
		CASE WHEN po.room_id IS NOT NULL THEN CONCAT(pr.drawing_room_number, ' - ', pr.drawing_room_name) ELSE '' END AS room_description,
		v.name AS vendor_name, po.quote_expiration_date AS expiration_date
	FROM purchase_order po
		INNER JOIN project p ON po.domain_id = p.domain_id AND po.project_id = p.project_id 
		INNER JOIN vendor v ON po.vendor_domain_id = v.domain_id AND po.vendor_id = v.vendor_id
		LEFT JOIN project_phase pp ON po.phase_id IS NOT NULL AND po.domain_id = pp.domain_id AND po.phase_id = pp.phase_id
		LEFT JOIN project_department pd ON po.department_id IS NOT NULL AND pd.domain_id = po.domain_id AND pd.department_id = po.department_id
		LEFT JOIN project_room pr ON po.room_id IS NOT NULL AND po.domain_id = pr.domain_id AND po.room_id = pr.room_id
	WHERE po.domain_id = @domainId AND po.project_id IN
		(SELECT project_id FROM project_user WHERE user_pid = @userId AND project_domain_id = @domainId)
		AND po.quote_expiration_date IS NOT NULL AND po.quote_expiration_date < GETDATE()
