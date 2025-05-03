
CREATE PROCEDURE [dbo].[get_asset_rooms](@PROJECT_ID integer, @ASSET_IDS VARCHAR(1000), @PHASE_ID integer, @DEPARTMENT_ID integer, @ROOM_ID integer) 
AS	
BEGIN
	-- Returns the info about all the rooms that one (or more) assets belongs

	SET NOCOUNT ON;
	 
	SELECT pri.serial_number, pri.serial_name, pp.description phase_desc, pri.project_id, pri.department_id, pd.phase_id, pd.description dept_desc, 
	pri.room_id, pri.asset_id, e.asset_code, pri.asset_description, pri.current_location,
	IIF(pr.drawing_room_number IS NULL OR pr.drawing_room_number = '', '', pr.drawing_room_number + '-') + pr.drawing_room_name as drawing_room_name, 
	budget_qty, SUM(lease_qty) lease_qty, COALESCE(PRI.RESP, 'OFOI') RESP, 
	SUM(PRI.DNP_QTY) AS DNP_QTY, SUM(pri.total_budget_amt) AS total_budget_amt,
	CASE WHEN PO.STATUS IS NULL THEN 'None' 
		WHEN PO.STATUS = 'Open' THEN PO.STATUS 
		WHEN PO.STATUS = 'Quote Requested' THEN 'Qreq' 
		WHEN PO.STATUS = 'Quote Received' THEN 'Qrec' 
		WHEN PO.STATUS = 'PO Requested' THEN 'PO Req' 
		WHEN PO.STATUS = 'PO Issued' THEN 'PO Issued' END AS po_status, 
	'' as tag --pri.tag 
	from project_room_inventory pri 
	LEFT JOIN assets e on pri.asset_id = e.asset_id and pri.asset_domain_id = e.domain_id 
	LEFT JOIN project_room pr on pri.project_id = pr.project_id and pri.room_id = pr.room_id 
	LEFT JOIN project_department pd on pri.project_id = pd.project_id and pri.department_id = pd.department_id 
	LEFT JOIN project_phase pp on pd.project_id = pp.project_id and pd.phase_id = pp.phase_id  
	LEFT JOIN INVENTORY_PURCHASE_ORDER IPO ON pri.PROJECT_ID = IPO.PROJECT_ID AND PRI.ASSET_ID = IPO.ASSET_ID and pri.asset_domain_id = ipo.asset_domain_id 
	LEFT JOIN PURCHASE_ORDER PO ON IPO.PO_ID = PO.PO_ID AND IPO.PROJECT_ID = PO.PROJECT_ID 
	WHERE pri.project_id =  @PROJECT_ID
	and CONCAT(pri.asset_id, pri.asset_domain_id) in(SELECT * FROM string_split(@ASSET_IDS, ',')) 
	AND (pp.phase_id = @PHASE_ID OR @PHASE_ID = -1)
	AND (pri.department_id = @DEPARTMENT_ID OR @DEPARTMENT_ID = -1)
	AND (pri.room_id = @ROOM_ID OR @ROOM_ID = -1)
	GROUP BY pri.serial_number, pri.serial_name, pp.description, pri.project_id, pri.department_id, pd.phase_id, pd.description, 
	pri.room_id, pr.drawing_room_number, pr.drawing_room_name, pri.asset_id, e.asset_code, pri.asset_description, 
	budget_qty, COALESCE(PRI.RESP, 'OFOI'), pri.current_location, PO.STATUS
	ORDER BY pp.description, pd.description, pr.drawing_room_number, pr.drawing_room_name
END

