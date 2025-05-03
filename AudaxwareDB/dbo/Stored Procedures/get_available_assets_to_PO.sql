
-- ========================================================================
-- Author:		Camila Silva
-- Create date: 08/29/2016
-- Description:	Get the available assets to assign to a purchase order
-- =========================================================================
CREATE PROCEDURE [dbo].[get_available_assets_to_PO](
	@domain_id INTEGER,
    @project_id INTEGER,
	@phase_id INTEGER = NULL,
    @department_id INTEGER = NULL,
    @room_id INTEGER = NULL,
	@allow_unapproved BIT = 0)
AS
BEGIN

	IF @room_id IS NOT NULL BEGIN
		SELECT 
		STRING_AGG( CAST(a.inventory_id as nvarchar(max)), ';') as inventory_ids,
		a.domain_id,
		a.project_id, 
		a.phase_id, 
		a.department_id,
		a.room_id,
		a.asset_id, 
		f.asset_code, 
		a.serial_number, a.serial_name, a.manufacturer_description, a.asset_description,
		pr.room_quantity as room_count, 
		SUM(a.budget_qty) - sum(COALESCE(A.DNP_QTY,0) * pr.room_quantity) - sum(COALESCE(po_info.po_qty, 0)) AS budget_qty, 
			CASE
				WHEN a.resp in('EXOI', 'EXCI', 'EXVI', 'EXEX') THEN 0
				ELSE sum(coalesce(a.total_budget_amt,0) * pr.room_quantity)
			END AS total_budget_amt,
		a.asset_domain_id, 
		a.jsn_code
		FROM project_room_inventory a
		INNER JOIN project_room PR ON PR.project_id = A.project_id AND PR.domain_id = A.domain_id AND PR.phase_id = A.phase_id AND PR.department_id = A.department_id AND PR.room_id = A.room_id 
		 INNER JOIN assets f ON a.asset_id = f.asset_id AND a.asset_domain_id = f.domain_id
		 INNER JOIN project_department pd ON pd.project_id = a.project_id AND pd.department_id = a.department_id and pd.domain_id = a.domain_id
		 LEFT JOIN (select inventory_id,
			sum(COALESCE(inv_po.po_qty, 0)) as po_qty from inventory_purchase_order as inv_po
			WHERE COALESCE(inv_po.po_qty,0) > 0 
		  GROUP BY inventory_id) as po_info ON a.inventory_id = po_info.inventory_id
		WHERE 
			a.project_id = @project_id AND a.domain_id = @domain_id 
			AND (@phase_id IS NULL OR (a.phase_id = @phase_id AND @department_id IS NULL OR (a.department_id = @department_id AND (@room_id IS NULL OR a.room_id = @room_id)))) 
			AND a.budget_qty - COALESCE(a.dnp_qty,0) - COALESCE(po_info.po_qty,0) > 0 
			AND (
				@allow_unapproved = 1  
				OR (@allow_unapproved = 0 and a.current_location = 'Approved'))
			AND a.resp NOT IN('EXOI', 'EXCI', 'EXVI', 'EXEX')
	  GROUP BY a.domain_id, pr.room_quantity,a.resp, a.project_id, a.phase_id, a.department_id, a.room_id, a.asset_id, 
				a.asset_domain_id, f.asset_code, a.asset_description, a.serial_number, a.serial_name, a.manufacturer_description, a.jsn_code
	END
	ELSE IF @department_id IS NOT NULL BEGIN
		SELECT 
		STRING_AGG( CAST(a.inventory_id as nvarchar(max)), ';') as inventory_ids,
		a.domain_id,
		a.project_id, 
		a.phase_id, 
		a.department_id,
		0 as room_id,
		a.asset_id, 
		f.asset_code, 
		a.serial_number, a.serial_name, a.manufacturer_description, a.asset_description,
		pr.room_quantity as room_count, 
		SUM(a.budget_qty) - sum(COALESCE(A.DNP_QTY,0) * pr.room_quantity) - sum(COALESCE(po_info.po_qty, 0)) AS budget_qty, 
			CASE
				WHEN a.resp in('EXOI', 'EXCI', 'EXVI', 'EXEX') THEN 0
				ELSE sum(coalesce(a.total_budget_amt,0) * pr.room_quantity)
			END AS total_budget_amt,
		a.asset_domain_id,
		a.jsn_code
		FROM project_room_inventory a
		INNER JOIN project_room PR ON PR.project_id = A.project_id AND PR.domain_id = A.domain_id AND PR.phase_id = A.phase_id AND PR.department_id = A.department_id AND PR.room_id = A.room_id 
		 INNER JOIN assets f ON a.asset_id = f.asset_id AND a.asset_domain_id = f.domain_id
		 INNER JOIN project_department pd ON pd.project_id = a.project_id AND pd.department_id = a.department_id and pd.domain_id = a.domain_id
		 LEFT JOIN (select inventory_id,
			sum(COALESCE(inv_po.po_qty, 0)) as po_qty from inventory_purchase_order as inv_po
			WHERE COALESCE(inv_po.po_qty,0) > 0 
		  GROUP BY inventory_id) as po_info ON a.inventory_id = po_info.inventory_id
		WHERE 
			a.project_id = @project_id AND a.domain_id = @domain_id 
			AND (@phase_id IS NULL OR (a.phase_id = @phase_id AND @department_id IS NULL OR (a.department_id = @department_id AND (@room_id IS NULL OR a.room_id = @room_id)))) 
			AND a.budget_qty - COALESCE(a.dnp_qty,0) - COALESCE(po_info.po_qty,0) > 0 
			AND (
				(@allow_unapproved = 1 AND  a.current_location in('Approved','Received','Delivered', 'Plan')) 
				OR (@allow_unapproved = 0 and a.current_location in('Approved','Received','Delivered')))
			AND a.resp NOT IN('EXOI', 'EXCI', 'EXVI', 'EXEX')
	  GROUP BY a.domain_id, pr.room_quantity,a.resp, a.project_id, a.phase_id, a.department_id, a.asset_id, a.asset_domain_id, f.asset_code, a.asset_description, a.serial_number, a.serial_name, a.manufacturer_description, a.jsn_code
	end
	ELSE IF @phase_id IS NOT NULL BEGIN
		SELECT 
		STRING_AGG( CAST(a.inventory_id as nvarchar(max)), ';') as inventory_ids,
		a.domain_id,
		a.project_id, 
		a.phase_id, 
		0 as department_id,
		0 as room_id,
		a.asset_id, 
		f.asset_code, 
		a.serial_number, a.serial_name, a.manufacturer_description, a.asset_description,
		pr.room_quantity as room_count, 
		SUM(a.budget_qty) - sum(COALESCE(A.DNP_QTY,0) * pr.room_quantity) - sum(COALESCE(po_info.po_qty, 0)) AS budget_qty, 
			CASE
				WHEN a.resp in('EXOI', 'EXCI', 'EXVI', 'EXEX') THEN 0
				ELSE sum(coalesce(a.total_budget_amt,0) * pr.room_quantity)
			END AS total_budget_amt,
		a.asset_domain_id,
		a.jsn_code
		FROM project_room_inventory a
		INNER JOIN project_room PR ON PR.project_id = A.project_id AND PR.domain_id = A.domain_id AND PR.phase_id = A.phase_id AND PR.department_id = A.department_id AND PR.room_id = A.room_id 
		 INNER JOIN assets f ON a.asset_id = f.asset_id AND a.asset_domain_id = f.domain_id
		 INNER JOIN project_department pd ON pd.project_id = a.project_id AND pd.department_id = a.department_id and pd.domain_id = a.domain_id
		 LEFT JOIN (select inventory_id,
			sum(COALESCE(inv_po.po_qty, 0)) as po_qty from inventory_purchase_order as inv_po
			WHERE COALESCE(inv_po.po_qty,0) > 0 
		  GROUP BY inventory_id) as po_info ON a.inventory_id = po_info.inventory_id
		WHERE 
			a.project_id = @project_id AND a.domain_id = @domain_id 
			AND (@phase_id IS NULL OR (a.phase_id = @phase_id AND @department_id IS NULL OR (a.department_id = @department_id AND (@room_id IS NULL OR a.room_id = @room_id)))) 
			AND a.budget_qty - COALESCE(a.dnp_qty,0) - COALESCE(po_info.po_qty,0) > 0 
			AND (
				@allow_unapproved = 1  
				OR (@allow_unapproved = 0 and a.current_location = 'Approved'))
			AND a.resp NOT IN('EXOI', 'EXCI', 'EXVI', 'EXEX')
	  GROUP BY a.domain_id, pr.room_quantity,a.resp, a.project_id, a.phase_id, a.asset_id, a.asset_domain_id, f.asset_code, a.asset_description, a.serial_number, a.serial_name, a.manufacturer_description, a.jsn_code
	END
	ELSE BEGIN
		SELECT 
		STRING_AGG( CAST(a.inventory_id as nvarchar(max)), ';') as inventory_ids,
		a.domain_id,
		a.project_id, 
		0 as phase_id,
		0 as department_id, 
		0 as room_id,
		a.asset_id, 
		f.asset_code, 
		a.serial_number, a.serial_name, a.manufacturer_description, a.asset_description,
		pr.room_quantity as room_count, 
		SUM(a.budget_qty) - sum(COALESCE(A.DNP_QTY,0) * pr.room_quantity) - sum(COALESCE(po_info.po_qty, 0)) AS budget_qty, 
			CASE
				WHEN a.resp in('EXOI', 'EXCI', 'EXVI', 'EXEX') THEN 0
				ELSE sum(coalesce(a.total_budget_amt,0) * pr.room_quantity)
			END AS total_budget_amt,
		a.asset_domain_id, 
		a.jsn_code
		FROM project_room_inventory a
		INNER JOIN project_room PR ON PR.project_id = A.project_id AND PR.domain_id = A.domain_id AND PR.phase_id = A.phase_id AND PR.department_id = A.department_id AND PR.room_id = A.room_id 
		 INNER JOIN assets f ON a.asset_id = f.asset_id AND a.asset_domain_id = f.domain_id
		 INNER JOIN project_department pd ON pd.project_id = a.project_id AND pd.department_id = a.department_id and pd.domain_id = a.domain_id
		 LEFT JOIN (select inventory_id,
			sum(COALESCE(inv_po.po_qty, 0)) as po_qty from inventory_purchase_order as inv_po
			WHERE COALESCE(inv_po.po_qty,0) > 0 
		  GROUP BY inventory_id) as po_info ON a.inventory_id = po_info.inventory_id
		 --LEFT JOIN inventory_purchase_order d ON a.inventory_id = d.inventory_id and po_info.po_qty > 0
		 --LEFT JOIN purchase_order e ON d.po_id = e.po_id AND d.project_id = e.project_id
		WHERE 
			a.project_id = @project_id AND a.domain_id = @domain_id 
			AND a.budget_qty - COALESCE(a.dnp_qty,0) - COALESCE(po_info.po_qty,0) > 0 
			AND (
				@allow_unapproved = 1  
				OR (@allow_unapproved = 0 and a.current_location = 'Approved'))
			AND a.resp NOT IN('EXOI', 'EXCI', 'EXVI', 'EXEX')
	  GROUP BY a.domain_id, pr.room_quantity,a.resp, a.project_id, a.asset_id, a.asset_domain_id, f.asset_code, a.asset_description, a.serial_number, a.serial_name, a.manufacturer_description, a.jsn_code
	END

END