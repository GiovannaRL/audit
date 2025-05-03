CREATE PROCEDURE [dbo].[Get_PO_assigned_assets](
	@domain_id SMALLINT,
    @project_id INTEGER,
	@po_id INTEGER)
AS
BEGIN
	SELECT 
		STRING_AGG(CAST(ipo.inventory_id as nvarchar(MAX)), ';') as inventory_ids,	
		
		ipo.po_domain_id, ipo.project_id, ipo.po_id, ipo.asset_domain_id, ipo.asset_id, SUM(COALESCE(ipo.po_qty, 0)) AS po_qty, pri.delivered_date, pri.received_date, pri.current_location, 
		MIN(ipo.po_unit_amt) AS po_unit_amt , ipo.po_status, ipo.date_added, ipo.added_by, ROUND(SUM(COALESCE(ipo.po_qty, 0) * COALESCE(po_unit_amt,0)),2) AS total_po_amt, 
		a.asset_code, pri.asset_description, pri.manufacturer_description, pri.serial_number, pri.serial_name, SUM(COALESCE(pri.budget_qty, 0)) AS budget_qty, pri.jsn_code,
		ROUND(SUM((COALESCE(pri.budget_qty, 0)-COALESCE(pri.dnp_qty,0)) * COALESCE(pri.unit_budget, 0)), 2) AS budget_amt,

	STUFF(( SELECT distinct '; ' + ao.code FROM inventory_options as io
			inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
			inner join inventory_purchase_order ipo1 on ipo1.inventory_id = io.inventory_id and ipo1.po_qty > 0
			inner join project_room_inventory pri1 on ipo1.inventory_id = pri1.inventory_id
		WHERE ipo1.po_domain_id = ipo.po_domain_id AND ipo1.project_id = ipo.project_id AND ipo1.po_id = ipo.po_id
		AND ipo1.asset_domain_id = ipo.asset_domain_id AND ipo.asset_id = ipo1.asset_id AND (ipo.po_status = ipo1.po_status OR
		(ipo.po_status IS NULL AND ipo1.po_status IS NULL) AND COALESCE(pri1.jsn_code, '') = COALESCE(pri.jsn_code, ''))
		FOR XML PATH('')),1 ,1, '')  as option_codes,
		
		 STUFF(( SELECT distinct '; ' + ao.description FROM inventory_options as io
			inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
			inner join inventory_purchase_order ipo1 on ipo1.inventory_id = io.inventory_id and ipo1.po_qty > 0
			inner join project_room_inventory pri1 on ipo1.inventory_id = pri1.inventory_id
		WHERE ipo1.po_domain_id = ipo.po_domain_id AND ipo1.project_id = ipo.project_id AND ipo1.po_id = ipo.po_id
		AND ipo1.asset_domain_id = ipo.asset_domain_id AND ipo.asset_id = ipo1.asset_id AND (ipo.po_status = ipo1.po_status OR
		(ipo.po_status IS NULL AND ipo1.po_status IS NULL) AND COALESCE(pri1.jsn_code, '') = COALESCE(pri.jsn_code, ''))
		FOR XML PATH('')),1 ,1, '')  as option_descriptions,
		
		(select CAST(sum(coalesce(io.unit_price,0)) AS VARCHAR(15)) from inventory_options io 
			inner join inventory_purchase_order ipo1 on ipo1.inventory_id = io.inventory_id and ipo1.po_qty > 0
			inner join project_room_inventory pri1 on ipo1.inventory_id = pri1.inventory_id
		WHERE ipo1.po_domain_id = ipo.po_domain_id AND ipo1.project_id = ipo.project_id AND ipo1.po_id = ipo.po_id
		AND ipo1.asset_domain_id = ipo.asset_domain_id AND ipo.asset_id = ipo1.asset_id AND (ipo.po_status = ipo1.po_status OR
		(ipo.po_status IS NULL AND ipo1.po_status IS NULL) AND COALESCE(pri1.jsn_code, '') = COALESCE(pri.jsn_code, ''))) as option_prices
		
	FROM inventory_purchase_order ipo, project_room_inventory pri, assets a
	WHERE ipo.po_id = @po_id AND ipo.po_domain_id = @domain_id AND ipo.project_id = @project_id AND ipo.inventory_id = pri.inventory_id 
		AND pri.asset_domain_id = a.domain_id AND pri.asset_id = a.asset_id and ipo.po_qty > 0
	GROUP BY ipo.po_domain_id, ipo.project_id, ipo.po_id, ipo.asset_domain_id, ipo.asset_id, ipo.po_status, ipo.date_added, ipo.added_by, 
		a.asset_code, a.domain_id, pri.delivered_date, pri.received_date, pri.current_location, pri.asset_description, pri.manufacturer_description, 
		pri.serial_number, pri.serial_name, pri.jsn_code;	
END