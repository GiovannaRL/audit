
CREATE VIEW [dbo].[asset_by_room] AS 
 SELECT 
	a.domain_id AS project_domain_id, 
	d.project_id,
    p.project_description,
    a.description AS phase_description,
    b.description AS dept_description,
    c.drawing_room_name,
    c.drawing_room_number,
    d.resp,
    d.budget_qty_sf AS budget_qty,
    d.dnp_qty_sf AS dnp_qty,
    d.lease_qty_sf AS lease_qty,
    d.po_qty_sf AS po_qty,
    d.comment,
    a.phase_id,
    b.department_id,
	c.room_id,
    (cast(e.asset_code as varchar) + ' - ') + pri.asset_description AS asset_desc,
    e.asset_id,
    e.asset_code,
    (rtrim(pri.manufacturer_description) + ': ') + coalesce(pri.serial_number, '') AS serial_number,
    e.comment AS asset_comment,
    e.weight,
    CASE WHEN TRY_CONVERT(NUMERIC(10, 2), e.weight) IS NOT NULL THEN
    CAST((TRY_CONVERT(NUMERIC(10, 2), e.weight) / 2.2046) as VARCHAR(25))
	ELSE '--' END AS weight_kg,
    (((
        CASE
            WHEN LEFT(pri.height, 1) = '-' THEN 'var ' + cast(abs(pri.height) as varchar)
            ELSE pri.height
        END + ' | ') +
        CASE
            WHEN LEFT(pri.width, 1) = '-' THEN 'var ' + cast(abs(pri.width) as varchar)
            ELSE pri.width
        END) + ' | ') +
        CASE
            WHEN LEFT(pri.depth, 1) = '-' THEN 'var ' + cast(abs(pri.depth) as varchar)
            ELSE pri.depth
        END AS hwd,
    (((CASE
            WHEN LEFT(pri.height, 1) = '-' THEN CONCAT('var ', abs(cast((cast(pri.height as money) / 0.39370) as numeric(10,2))))
			WHEN ISNUMERIC(pri.height) = 0 AND CHARINDEX('-', pri.height) > 0 THEN CONCAT(cast((cast((SUBSTRING(pri.height,1,CHARINDEX('-', pri.height)-1)) as money) / 0.39370) as numeric(10,2)), '') + ' - ' + 
			CONCAT(cast((cast(SUBSTRING(pri.height,CHARINDEX('-', pri.height)+1, LEN(pri.height)) as money) / 0.39370) as numeric(10,2)), '')
            WHEN ISNUMERIC(pri.height) = 1 THEN CONCAT(cast((cast(pri.height as money) / 0.39370) as numeric(10,2)), '')
			ELSE 'INVALID'
        END + ' | ') +
        CASE
            WHEN LEFT(pri.width, 1) = '-' THEN CONCAT('var ', abs(cast((cast(pri.width as money) / 0.39370) as numeric(10,2))))
			WHEN ISNUMERIC(pri.width) = 0 AND CHARINDEX('-', pri.width) > 0 THEN CONCAT(cast((cast((SUBSTRING(pri.width,1,CHARINDEX('-', pri.width)-1)) as money) / 0.39370) as numeric(10,2)), '') + ' - ' + 
			CONCAT(cast((cast(SUBSTRING(pri.width,CHARINDEX('-', pri.width)+1, LEN(pri.width)) as money) / 0.39370) as numeric(10,2)), '')
            WHEN ISNUMERIC(pri.width) = 1 THEN CONCAT(cast((cast(pri.width as money) / 0.39370) as numeric(10,2)), '')
			ELSE 'INVALID'
        END) + ' | ') +
        CASE
            WHEN LEFT(pri.depth, 1) = '-' THEN CONCAT('var ', abs(cast((cast(pri.depth as money)  / 0.39370) as numeric(10,2))))
			WHEN ISNUMERIC(pri.depth) = 0 AND CHARINDEX('-', pri.depth) > 0  THEN CONCAT(cast((cast((SUBSTRING(pri.depth,1,CHARINDEX('-', pri.depth)-1)) as money) / 0.39370) as numeric(10,2)), '') + ' - ' + 
			CONCAT(cast((cast(SUBSTRING(pri.depth,CHARINDEX('-', pri.depth)+1, LEN(pri.depth)) as money) / 0.39370) as numeric(10,2)), '')
            WHEN ISNUMERIC(pri.depth) = 1 THEN CONCAT(cast((cast(pri.depth as money)  / 0.39370) as numeric(10,2)), '')
			ELSE 'INVALID'
        END AS hwd_cm,
        CASE e.water_option
            WHEN 1 THEN 'Y'
            WHEN 2 THEN 'O'
            ELSE '--'
        END AS water_option,
        CASE e.plumbing_option
            WHEN 1 THEN 'Y'
            WHEN 2 THEN 'O'
            ELSE '--'
        END AS plumbing_option,
        CASE e.data_option
            WHEN 1 THEN 'Y'
            WHEN 2 THEN 'O'
            ELSE '--'
        END AS data_option,
        CASE e.electrical_option
            WHEN 1 THEN 'Y'
            WHEN 2 THEN 'O'
            ELSE '--'
        END AS electrical_option,
        CASE e.mobile_option
            WHEN 1 THEN 'Y'
            WHEN 2 THEN 'O'
            ELSE '--'
        END AS mobile_option,
        CASE e.blocking_option
            WHEN 1 THEN 'Y'
            WHEN 2 THEN 'O'
            ELSE '--'
        END AS blocking_option,
        CASE e.medgas_option
            WHEN 1 THEN 'Y'
            WHEN 2 THEN 'O'
            ELSE '--'
        END AS medgas_option,
        CASE e.supports_option
            WHEN 1 THEN 'Y'
            WHEN 2 THEN 'O'
            ELSE '--'
        END AS supports_option,
    em.eq_unit_desc,
    pri.cost_center_id,
    e.domain_id AS asset_domain_id,
    pri.cad_id,
    (COALESCE(pri.cad_id, e.asset_code) + ' - ') + pri.asset_description AS asset_cad_desc
   FROM project_phase a
     JOIN project_department b ON a.project_id = b.project_id AND a.phase_id = b.phase_id and a.domain_id = b.domain_id
     JOIN project_room c ON b.project_id = c.project_id AND b.department_id = c.department_id and c.domain_id = b.domain_id
     JOIN inventory_w_relo_v d ON c.project_id = d.project_id AND c.room_id = d.room_id and c.domain_id = d.domain_id
     JOIN assets e ON d.asset_id = e.asset_id AND d.asset_domain_id = e.domain_id
     JOIN project_room_inventory pri ON pri.room_id = d.room_id AND pri.asset_id = d.asset_id AND pri.domain_id = d.domain_id
     JOIN project p ON p.project_id = a.project_id and a.domain_id = p.domain_id
     LEFT JOIN assets_measurement em ON e.eq_measurement_id = em.eq_unit_measure_id;


