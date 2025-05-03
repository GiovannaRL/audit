
CREATE VIEW [dbo].[inventory_w_relo_v] AS 

 SELECT 'NEW' AS type,
    a.project_id,
	a.phase_id,
    a.department_id,
    a.room_id,
	pp.description AS phase_description,
	pd.description AS department_description,
	pr.drawing_room_name,
	pr.drawing_room_number,
    a.asset_id,
    a.asset_description,
    c.asset_code,
    a.manufacturer_description,
    a.current_location,
    a.comment,
    COALESCE(a.resp, c.default_resp) AS resp,
    (SUM(a.budget_qty) * MAX(pr.room_quantity)) AS budget_qty,
        CASE
            WHEN MAX(em.eq_unit_desc) = 'per sf' THEN
            CASE
                WHEN SUM(COALESCE(a.budget_qty, 0)) = 0 THEN 0
                ELSE 1
            END
            ELSE (SUM(COALESCE(a.budget_qty, 0)) * MAX(pr.room_quantity))
        END AS budget_qty_sf,
    (SUM(COALESCE(a.lease_qty, 0)) * MAX(pr.room_quantity)) AS lease_qty,
        CASE
            WHEN MAX(em.eq_unit_desc) = 'per sf' THEN
            CASE
                WHEN SUM(COALESCE(a.lease_qty, 0)) = 0 THEN 0
                ELSE 1
            END
            ELSE (SUM(COALESCE(a.lease_qty, 0)) * MAX(pr.room_quantity))
        END AS lease_qty_sf,
    (SUM(COALESCE(a.dnp_qty, 0)) * MAX(pr.room_quantity)) AS dnp_qty,
        CASE
            WHEN MAX(em.eq_unit_desc) = 'per sf' THEN
            CASE
                WHEN SUM(COALESCE(a.dnp_qty, 0)) = 0 THEN 0
                ELSE 1
            END
            ELSE (SUM(COALESCE(a.dnp_qty, 0)) * MAX(pr.room_quantity))
        END AS dnp_qty_sf,
        CASE
            WHEN MAX(a.resp) in ('EXOI', 'EXCI', 'EXVI', 'EXEX') THEN 0
            ELSE SUM(COALESCE(a.unit_budget, 0))
        END AS unit_budget,
        CASE
            WHEN MAX(a.resp) in ('EXOI', 'EXCI', 'EXVI', 'EXEX') THEN 0
            ELSE (SUM(COALESCE(a.total_budget_amt, 0)) * MAX(pr.room_quantity))
        END AS total_budget_amt,
    sum(COALESCE(d.po_qty, 0)) AS po_qty,
    sum(
        CASE
            WHEN em.eq_unit_desc = 'per sf' THEN
            CASE
                WHEN COALESCE(d.po_qty, 0) = 0 THEN 0
                ELSE 1
            END
            ELSE COALESCE(d.po_qty, 0)
        END) AS po_qty_sf,
    sum(COALESCE(d.po_qty, 0) * COALESCE(d.po_unit_amt, 0)) AS total_po_amt,
        CASE
            WHEN min(f.quote_received_date) IS NOT NULL THEN COALESCE(a.unit_budget, 0) * (COALESCE(a.budget_qty, 0) - COALESCE(a.dnp_qty, 0)) - sum(COALESCE(d.po_qty, 0) * COALESCE(d.po_unit_amt, 0))
            ELSE 0
        END AS buyout_delta,
    a.cost_center_id,
    a.domain_id,
	a.asset_domain_id,
    a.cad_id,
	a.tag, a.jsn_code, 
	a.serial_number, a.serial_name
   FROM project_room_inventory a
	 INNER JOIN project_room pr ON a.domain_id = pr.domain_id AND a.project_id = pr.project_id AND a.phase_id = pr.phase_id AND
		a.department_id = pr.department_id AND a.room_id = pr.room_id
     INNER JOIN project_department pd ON a.domain_id = pd.domain_id AND a.project_id = pd.project_id AND a.phase_id = pd.phase_id AND
		a.department_id = pd.department_id
	 INNER JOIN project_phase pp ON a.domain_id = pp.domain_id AND a.project_id = pp.project_id AND a.phase_id = pp.phase_id
     JOIN assets c ON a.asset_id = c.asset_id AND a.asset_domain_id = c.domain_id
     JOIN assets_measurement em ON c.eq_measurement_id = em.eq_unit_measure_id
	 LEFT JOIN inventory_purchase_order d ON a.inventory_id = d.inventory_id
     LEFT JOIN purchase_order f ON d.po_id = f.po_id AND d.project_id = f.project_id
  GROUP BY a.project_id, a.phase_id, a.department_id, a.room_id, pp.description, pd.description, pr.drawing_room_name, pr.drawing_room_number, a.asset_id, a.asset_description, c.asset_code, a.manufacturer_description, a.current_location, a.comment, 
  COALESCE(a.resp, c.default_resp), COALESCE(a.budget_qty, 0), (COALESCE(a.budget_qty, 0) * pr.room_quantity),
        CASE
            WHEN em.eq_unit_desc = 'per sf' THEN
            CASE
                WHEN COALESCE(a.budget_qty, 0) = 0 THEN 0
                ELSE 1
            END
            ELSE (COALESCE(a.budget_qty, 0) * pr.room_quantity)
        END, (COALESCE(a.lease_qty, 0) * pr.room_quantity),
        CASE
            WHEN em.eq_unit_desc = 'per sf' THEN
            CASE
                WHEN COALESCE(a.lease_qty, 0) = 0 THEN 0
                ELSE 1
      END
            ELSE (COALESCE(a.lease_qty, 0) * pr.room_quantity)
        END, 
		COALESCE(a.dnp_qty, 0), (COALESCE(a.dnp_qty, 0) * pr.room_quantity),
        CASE
            WHEN em.eq_unit_desc = 'per sf' THEN
            CASE
                WHEN COALESCE(a.dnp_qty, 0) = 0 THEN 0
                ELSE 1
            END
            ELSE (COALESCE(a.dnp_qty, 0) * pr.room_quantity)
        END,
         CASE
            WHEN a.resp in ('EXOI', 'EXCI', 'EXVI', 'EXEX') THEN 0
            ELSE (COALESCE(a.total_budget_amt, 0) * pr.room_quantity)
        END, a.cost_center_id, a.domain_id, a.asset_domain_id, a.unit_budget, a.cad_id, a.tag, a.jsn_code, a.serial_number, a.serial_name;




