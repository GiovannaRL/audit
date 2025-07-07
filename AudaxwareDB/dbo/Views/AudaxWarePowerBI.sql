CREATE VIEW [dbo].[AudaxWarePowerBI] as
	SELECT 
		p.project_description as 'Project',
		CASE 
			WHEN p.status = 'A' THEN 'Active' 
			WHEN p.status = 'D' THEN 'Canceled'
			WHEN p.status = 'C' THEN 'Complete'
			WHEN p.status = 'P' THEN 'Pending' 
			WHEN p.status = 'T' THEN 'Training'
			ELSE 'Unknown'
		END as 'Project Status',
		PP.description AS 'Phase',
		PD.description AS 'Department',
		PR.drawing_room_number as 'Room Number',
		PR.drawing_room_name AS 'Room Name',
		A.resp as 'Responsability',
		CASE WHEN A.resp = 'EXOI' OR A.resp = 'EXCI' OR A.resp = 'EXVI' OR A.resp = 'EXEX' THEN 'EXISTING' ELSE 'NEW' END AS 'Resp Type',
		CAST(A.project_id AS VARCHAR(10)) as 'Project ID',
        CAST(A.phase_id AS VARCHAR(10)) as 'Phase ID',
        CAST(a.department_id AS VARCHAR(10)) as 'Department ID',
		CAST(a.room_id AS VARCHAR(10)) as 'Room ID',
  		c.asset_code as 'Asset Code',
		a.cad_id as 'CAD ID',
        a.asset_description as 'Asset Description',
        a.manufacturer_description as 'Manufacturer Description',
        (COALESCE(a.budget_qty,0) * pr.room_quantity) as 'Budget Qty',
        (COALESCE(A.dnp_qty,0) * pr.room_quantity) AS 'Do Not Purchase Qty',
        (COALESCE(A.lease_qty, 0) * pr.room_quantity) AS 'Lease Qty',
        COALESCE(po_info.po_qty, 0) AS 'PO Qty',
		convert(money, CASE
            WHEN a.resp in ('EXOI', 'EXCI', 'EXVI', 'EXEX') THEN 0
            ELSE (coalesce(a.total_budget_amt,0) * pr.room_quantity)
        END) AS 'Total Budget Amount',
        convert(money, COALESCE(po_info.total_po_amt, 0)) AS 'Total PO Amount',
        convert(money, COALESCE(A.unit_budget,0) * COALESCE(po_info.po_qty_with_quote, 0) - COALESCE(po_info.total_po_amt, 0)) AS 'Buyout Delta',
		convert(money, CASE
            WHEN a.resp in ('EXOI', 'EXCI', 'EXVI', 'EXEX') THEN 0
            ELSE COALESCE(a.locked_unit_budget, 0) * (COALESCE(a.locked_budget_qty, 0) - COALESCE(a.locked_dnp_qty, 0))
        END) AS 'Total Locked Cost Amount',
        c.model_number as 'Model Number',
        c.model_name as 'Model Name',
        (CASE WHEN C.discontinued = 1 AND p.status IN('A','P') THEN 'D' ELSE '' END) as Discontinued,
        ((COALESCE(A.budget_qty,0) * pr.room_quantity)-(COALESCE(A.dnp_qty,0) * pr.room_quantity)) AS 'Net New',
		CC.code AS 'Cost Center',
		po_info.status as 'PO Status',
		A.current_location as 'Current Location',
		a.tag as Tag,
		convert(money,a.unit_budget) as 'Unit Budget',
		a.estimated_delivery_date as 'Estimated Delivery Date',
		a.comment as Comment,
		dt.description as 'Department Type',
		pd.area as 'Department Area',
		p.domain_id as 'Domain ID',
		po_info.po_number as 'PO Number',
		po_info.vendor_name as 'PO Vendor',
		convert(money, COALESCE(a.unit_markup, 0))  as 'Unit Markup %', 
		convert(money, COALESCE(a.unit_escalation, 0))  as 'Unit Escalation %', 
		convert(money, COALESCE(a.unit_tax, 0))  as 'Unit Tax %',
		convert(money, COALESCE(a.unit_install_markup, 0))  as 'Unit Install Markup', 
		convert(money, COALESCE(a.unit_install_net, 0))  as 'Unit Install (Net)', 
		convert(money, COALESCE(a.unit_freight_markup, 0))  as 'Unit Freight Markup', 
		convert(money, COALESCE(a.unit_freight_net, 0))  as 'Unit Freight (Net)'
    FROM project_room_inventory a
  		LEFT JOIN (select inventory_id,
			STRING_AGG(po.po_number, ',') as po_number,
			STRING_AGG(v.name, ',') as vendor_name,
			STRING_AGG(
				CASE WHEN po.status IS NULL THEN 'None'
					WHEN po.status = 'Open' THEN PO.status
					WHEN po.status = 'Quote Requested' THEN 'Qreq'
					WHEN po.status = 'Quote Received' THEN 'Qrec'
					WHEN po.status = 'PO Requested' THEN 'PO Req'
					WHEN po.status = 'PO Issued' THEN 'PO Issued'
					END, ',') as Status,
			sum(
			-- We only compute PO amount if we have received a quote
				CASE
					WHEN po.quote_received_date is null THEN
						0
					ELSE
						(COALESCE(inv_po.po_qty, 0) * COALESCE(inv_po.po_unit_amt, 0))
				END
			) as total_po_amt,
			sum(
			-- We only compute PO amount if we have received a quote
				CASE
					WHEN po.quote_received_date is null THEN
						0
					ELSE
						COALESCE(inv_po.po_qty, 0)
				END
			) as po_qty_with_quote,
			sum(COALESCE(inv_po.po_qty, 0)) as po_qty from inventory_purchase_order as inv_po
	 LEFT JOIN purchase_order as po ON inv_po.po_id = po.po_id AND inv_po.project_id = po.project_id  
	 LEFT JOIN vendor as v on v.vendor_id = po.vendor_id and v.domain_id = po.vendor_domain_id
	 where inv_po.po_qty > 0
	 GROUP BY inventory_id) as po_info ON a.inventory_id = po_info.inventory_id
     JOIN assets c ON a.asset_id = c.asset_id AND a.asset_domain_id = c.domain_id
     JOIN assets_measurement em ON c.eq_measurement_id = em.eq_unit_measure_id
	 INNER JOIN project p ON p.project_id = A.project_id and p.domain_id = a.domain_id
	 INNER JOIN project_phase PP ON PP.project_id = A.project_id AND PP.domain_id = A.domain_id AND PP.phase_id = A.phase_id
	 INNER JOIN project_department PD ON PD.project_id = A.project_id AND PD.domain_id = A.domain_id AND PD.phase_id = A.phase_id AND PD.department_id = A.department_id
	 INNER JOIN project_room PR ON PR.project_id = A.project_id AND PR.domain_id = A.domain_id AND PR.phase_id = A.phase_id AND PR.department_id = A.department_id AND PR.room_id = A.room_id 
	 inner join department_type dt on dt.department_type_id = pd.department_type_id and dt.domain_id = pd.department_type_domain_id 
	 LEFT JOIN cost_center CC ON CC.id = A.cost_center_id
	 WHERE a.project_id <> 1 AND 
	 (p.domain_id = CAST(SESSION_CONTEXT(N'domain_id') AS SMALLINT) OR
	 (p.domain_id = CAST(SUBSTRING(CURRENT_USER, CHARINDEX('_', CURRENT_USER)+1, 20) as SMALLINT)))
GO



