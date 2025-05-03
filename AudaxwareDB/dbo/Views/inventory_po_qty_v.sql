CREATE VIEW [dbo].[inventory_po_qty_v] AS 
 
 SELECT 
	STRING_AGG(a.inventory_id, ';') as inventory_ids,
	a.project_id,
    a.phase_id,
    a.department_id,
    a.room_id,
	a.asset_id,
	pr.room_quantity as room_count,
    sum(coalesce(a.budget_qty,0) * pr.room_quantity) AS budget_qty,
        CASE
            WHEN cast(g.eq_unit_desc as varchar) = 'per sf' THEN
            CASE
                WHEN sum(COALESCE(a.budget_qty, 0)) = 0 THEN 0
                ELSE 1
            END
            ELSE sum(coalesce(a.budget_qty,0) * pr.room_quantity)
        END AS budget_qty_sf,
        CASE
            WHEN a.resp in('EXOI', 'EXCI', 'EXVI', 'EXEX') THEN 0
            ELSE sum(coalesce(a.total_budget_amt,0) * pr.room_quantity)
        END AS total_budget_amt,
    sum(COALESCE(A.DNP_QTY,0) * pr.room_quantity) AS dnp_qty,
        CASE
            WHEN cast(g.eq_unit_desc as varchar) = 'per sf' THEN
            CASE
                WHEN sum(COALESCE(a.dnp_qty, 0)) = 0 THEN 0
                ELSE 1
            END
            ELSE sum(COALESCE(A.DNP_QTY,0) * pr.room_quantity)
        END AS dnp_qty_sf,
    sum(COALESCE(po_info.po_qty, 0)) AS po_qty,
    sum(
        CASE
            WHEN cast(g.eq_unit_desc as varchar) = 'per sf' THEN
            CASE
                WHEN COALESCE(po_info.po_qty, 0) = 0 THEN 0
                ELSE 1
            END
            ELSE COALESCE(po_info.po_qty, 0)
        END) AS po_qty_sf,
    sum(po_info.total_po_amt) AS total_po_amt,
        CASE
            WHEN min(po_info.quote_received_date) IS NOT NULL THEN sum(COALESCE(a.unit_budget, 0) * COALESCE(po_info.po_qty, 0) - COALESCE(po_info.po_qty, 0) * COALESCE(po_info.po_unit_amt, 0))
            ELSE 0
        END AS buyout_delta,
    a.current_location,
    a.domain_id,
	a.asset_domain_id,
	a.cost_center_id,
	sum(coalesce(a.total_freight,0)) as total_freight,
	sum(coalesce(a.total_install,0)) as total_install,
	sum(coalesce(a.total_tax,0)) as total_tax,
	sum(coalesce(a.unit_escalation_calc,0)) as unit_escalation_calc,
	sum(coalesce(a.unit_markup_calc,0)) as unit_markup_calc,
	sum(a.total_budget) as total_budget,
	 CASE
        WHEN (min(COALESCE(p.markup, 0)) > sum(COALESCE(a.unit_markup_calc, 0))) THEN min(COALESCE(p.markup, 0))
        ELSE sum(COALESCE(a.unit_markup_calc, 0))
    END AS markup_projected,
	CASE
        WHEN (min(COALESCE(p.escalation, 0)) > sum(COALESCE(a.unit_escalation_calc, 0))) THEN min(COALESCE(p.escalation, 0))
        ELSE sum(COALESCE(a.unit_escalation_calc, 0))
    END AS escalation_projected
   FROM project_room_inventory a
	INNER JOIN project_room PR ON PR.project_id = A.project_id AND PR.domain_id = A.domain_id AND PR.phase_id = A.phase_id AND PR.department_id = A.department_id AND PR.room_id = A.room_id 
	INNER JOIN project p ON p.project_id = a.project_id and p.domain_id = a.domain_id
     INNER JOIN assets f ON a.asset_id = f.asset_id AND a.asset_domain_id = f.domain_id
	 INNER JOIN project_department pd ON pd.project_id = a.project_id AND pd.department_id = a.department_id and pd.domain_id = a.domain_id
	 INNER JOIN responsability r on trim(r.name) = trim(a.resp)
	 LEFT JOIN assets_measurement g ON f.eq_measurement_id = g.eq_unit_measure_id
	LEFT JOIN (select inventory_id,  min(po.quote_received_date) as quote_received_date,
		CASE
			WHEN COALESCE(SUM(inv_po.po_qty), 0) = 0 THEN
				0
			ELSE
				SUM(COALESCE(inv_po.po_unit_amt, 0)*inv_po.po_qty)/sum(inv_po.po_qty)
		END as po_unit_amt,
		sum((COALESCE(inv_po.po_qty, 0) * COALESCE(inv_po.po_unit_amt, 0))) as total_po_amt, sum(COALESCE(inv_po.po_qty, 0)) as po_qty
		from inventory_purchase_order as inv_po
	LEFT JOIN purchase_order as po ON inv_po.po_id = po.po_id AND inv_po.project_id = po.project_id 
	where po.quote_received_date is not null and po.invalid_po = 0 GROUP BY inventory_id) as po_info ON a.inventory_id = po_info.inventory_id
  GROUP BY a.project_id, a.department_id, a.room_id, a.asset_id, pr.room_quantity,
		g.eq_unit_desc, a.resp,
		a.current_location, a.domain_id, a.phase_id,a.unit_budget, a.asset_domain_id, a.cost_center_id

GO











